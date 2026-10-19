using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DaisyBlazor.Charts;

/// <summary>
/// The engine of every chart: its geometry is computed by the D3 module bundled in this package
/// (<c>_content/DaisyBlazor.Charts/charts.mjs</c>). After each render whose inputs changed, the chart
/// sends its <see cref="BuildSpec"/> to the layout named by <see cref="Kind"/> and draws the returned
/// <typeparamref name="TLayout"/> as inline SVG, coloured with daisyUI theme tokens. The module loads
/// on first use; until a layout arrives (and under static server rendering, which runs no
/// JavaScript) the chart shows a placeholder of the same size.
/// <para>
/// A chart either lays out in a fixed coordinate space (the default), or opts into measured mode
/// with <see cref="Measured"/>: it renders a box of a CSS size, captures it in
/// <see cref="MeasuredElement"/>, and the layout runs at the box's real pixel size (which the
/// layout echoes as <c>width</c>/<c>height</c>), again whenever the box is resized.
/// </para>
/// </summary>
/// <typeparam name="TLayout">The data the layout returns, deserialized from JSON.</typeparam>
public abstract class D3ChartBase<TLayout> : ComponentBase, IAsyncDisposable
    where TLayout : class
{
    internal const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    /// <summary>Invariant culture for numbers written into SVG attributes.</summary>
    protected static readonly CultureInfo Ci = CultureInfo.InvariantCulture;

    // JSON has no NaN or infinity: such values travel as null, which every layout treats as missing.
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { Converters = { new FiniteDoubleConverter() } };

    private IJSObjectReference? _module;
    private DotNetObjectReference<D3ChartBase<TLayout>>? _self;
    private ElementReference? _observed;
    private string? _specJson;
    private JsonElement _spec;
    private IChartLayoutEngine? _engine;
    private PersistentComponentState? _state;
    private PersistingComponentStateSubscription? _persisting;
    private string? _layoutJson;
    private bool _pending;
    private int _version;

    [Inject] private IJSRuntime JS { get; set; } = null!;

    [Inject] private IServiceProvider Services { get; set; } = null!;

    /// <summary>The layout file in <c>wwwroot/layouts/</c> (without extension).</summary>
    protected abstract string Kind { get; }

    /// <summary>The layout's input. It is serialized to JSON (camelCase); a new layout is requested only when that JSON changes.</summary>
    protected abstract object BuildSpec();

    /// <summary>
    /// True to lay out at the pixel size of <see cref="MeasuredElement"/> instead of a fixed
    /// coordinate space. The element must be rendered (with a CSS size) in every state, placeholder included.
    /// </summary>
    protected virtual bool Measured => false;

    /// <summary>
    /// Size (in pixels) a measured chart is laid out at when it renders on the server, where its box
    /// cannot be measured. The SVG scales to the real box; the browser lays it out again at the true
    /// size once the chart is interactive.
    /// </summary>
    protected virtual (double Width, double Height) ServerSize => (800, 256);

    /// <summary>The measured box, captured with <c>@ref</c> by charts in measured mode.</summary>
    protected ElementReference MeasuredElement;

    /// <summary>The latest layout, or null before the first one arrives.</summary>
    protected TLayout? Layout { get; private set; }

    /// <summary>The layout's error message when it could not be computed (for example a cycle in a Sankey).</summary>
    protected string? Error { get; private set; }

    internal string? ErrorMessage => Error;

    internal bool IsReady => Layout is not null;

    /// <summary>Formats a coordinate for an SVG attribute.</summary>
    protected static string N(double value) => Math.Round(value, 2).ToString(Ci);

    /// <summary>Receives a new layout from measured mode after the chart's box was resized.</summary>
    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public Task OnMeasuredLayout(TLayout layout) => InvokeAsync(() =>
    {
        Layout = layout;
        Error = null;
        StateHasChanged();
    });

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _engine = (IChartLayoutEngine?)Services.GetService(typeof(IChartLayoutEngine));
        _state = (PersistentComponentState?)Services.GetService(typeof(PersistentComponentState));

        // A layout computed while prerendering travels to the interactive render (WebAssembly or a
        // new circuit) through the persisted component state, so the chart never flashes back to
        // its placeholder while the browser lays it out again.
        if (_engine is not null && _state is not null && !RendererInfo.IsInteractive)
        {
            _persisting = _state.RegisterOnPersisting(() =>
            {
                if (_specJson is not null && _layoutJson is not null)
                {
                    _state.PersistAsJson(StateKey(_specJson), _layoutJson);
                }

                return Task.CompletedTask;
            });
        }
    }

    // The key is the layout's input itself (kind and a hash of the spec), so two charts can only
    // share a persisted layout when they would compute the same one.
    private string StateKey(string specJson) => StateKey(Kind, specJson);

    internal static string StateKey(string kind, string specJson) =>
        $"daisyblazor-chart:{kind}:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(specJson)), 0, 12)}";

    internal string? SpecJson => _specJson;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        object spec = BuildSpec();
        string json = JsonSerializer.Serialize(spec, spec.GetType(), _json);
        if (json == _specJson)
        {
            return;
        }

        _specJson = json;
        _spec = JsonDocument.Parse(json).RootElement.Clone();
        _pending = true;

        // With server rendering registered, lay out right away so the first HTML (prerendered or
        // static) already holds the chart. A measured chart is redone in the browser at its true size.
        if (_engine is not null)
        {
            LayoutOnServer(_engine, json);
            _pending = Measured && RendererInfo.IsInteractive;
        }
        else if (_state is not null && _state.TryTakeFromJson(StateKey(json), out string? persisted) && persisted is not null)
        {
            Layout = JsonSerializer.Deserialize<TLayout>(persisted, _json);
            Error = null;
            _pending = Measured;
        }
    }

    private void LayoutOnServer(IChartLayoutEngine engine, string json)
    {
        try
        {
            if (!Measured)
            {
                _layoutJson = engine.Layout(Kind, json);
                Layout = JsonSerializer.Deserialize<TLayout>(_layoutJson, _json);
            }
            else
            {
                (double width, double height) = ServerSize;
                JsonObject spec = JsonNode.Parse(json)!.AsObject();
                spec["width"] = width;
                spec["height"] = height;
                JsonObject result = JsonNode.Parse(engine.Layout(Kind, spec.ToJsonString()))!.AsObject();
                result["width"] = width;
                result["height"] = height;
                _layoutJson = result.ToJsonString();
                Layout = result.Deserialize<TLayout>(_json);
            }

            Error = null;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Layout = null;
            Error = ex.Message.Split('\n')[0];
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_pending)
        {
            return;
        }

        _pending = false;
        int version = ++_version;
        try
        {
            _module ??= await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);
            TLayout? layout;
            if (!Measured)
            {
                layout = await _module.InvokeAsync<TLayout>("layout", Kind, _spec);
            }
            else if (_observed is null)
            {
                _self ??= DotNetObjectReference.Create(this);
                _observed = MeasuredElement;
                layout = await _module.InvokeAsync<TLayout?>("observe", MeasuredElement, Kind, _spec, _self);
            }
            else
            {
                layout = await _module.InvokeAsync<TLayout?>("update", _observed.Value, _spec);
            }

            // Null: the measured box has no size yet; the resize callback brings the layout.
            if (version != _version || layout is null)
            {
                return;
            }

            Layout = layout;
            Error = null;
        }
        catch (JSException ex)
        {
            if (version != _version)
            {
                return;
            }

            Layout = null;
            Error = ex.Message.Split('\n')[0];
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        catch (TaskCanceledException)
        {
            return;
        }

        StateHasChanged();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                if (_observed is { } element)
                {
                    await _module.InvokeVoidAsync("unobserve", element);
                }

                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }

        _self?.Dispose();
        _persisting?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Base for the charts drawn in a fixed-shape coordinate space (<see cref="ViewWidth"/> by
/// <see cref="ViewHeight"/>, from <see cref="AspectRatio"/>) inside a <c>D3ChartFrame</c>: the SVG
/// scales with the wrapper's width and its text scales with it.
/// </summary>
/// <typeparam name="TLayout">The data the layout returns, deserialized from JSON.</typeparam>
public abstract class D3Chart<TLayout> : D3ChartBase<TLayout>
    where TLayout : class
{
    /// <summary>Width of the SVG coordinate space; layouts are computed in these units.</summary>
    protected const double ViewWidth = 640;

    /// <summary>Optional heading rendered above the chart.</summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>CSS width of the chart wrapper.</summary>
    [Parameter] public string? Width { get; set; } = "100%";

    /// <summary>Width divided by height. The chart keeps this shape at any width and its text scales with it.</summary>
    [Parameter] public double AspectRatio { get; set; } = 16d / 9;

    /// <summary>Additional CSS classes merged onto the <c>w-full</c> wrapper.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Inline styles appended after the width on the wrapper.</summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>Attributes splatted onto the wrapper.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }

    /// <summary>Height of the SVG coordinate space, from <see cref="AspectRatio"/>.</summary>
    protected double ViewHeight => Math.Round(ViewWidth / (AspectRatio > 0.1 ? AspectRatio : 16d / 9), 2);

    internal string WrapperClass => CssClass.Merge("w-full", Class);

    internal string WrapperStyle => (string.IsNullOrEmpty(Width) ? string.Empty : $"width:{Width};") + Style;

    internal string ViewBox => $"0 0 {ViewWidth.ToString(Ci)} {ViewHeight.ToString(Ci)}";
}

/// <summary>Writes NaN and infinities as null, since JSON cannot carry them.</summary>
internal sealed class FiniteDoubleConverter : JsonConverter<double>
{
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Null ? double.NaN : reader.GetDouble();

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        if (double.IsFinite(value))
        {
            writer.WriteNumberValue(value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
