using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
    private bool _restoreTried;
    private bool _pending;
    private int _version;

    [Inject] private IJSRuntime JS { get; set; } = null!;

    [Inject] private IServiceProvider Services { get; set; } = null!;

    /// <summary>The layout file in <c>wwwroot/layouts/</c> (without extension).</summary>
    protected abstract string Kind { get; }

    /// <summary>The layout's input. It is serialized to JSON (camelCase); a new layout is requested only when that JSON changes.</summary>
    protected abstract object BuildSpec();

    // The spec with the trend line's keys (trendline, and the order or period its kind takes); the
    // spec itself when there is none, so the spec (and the layout cache key) of a chart without a trend stays as it was.
    private protected static object WithTrendline(object spec, ChartTrendline trendline, int order, int period)
    {
        if (trendline == ChartTrendline.None)
        {
            return spec;
        }

        System.Text.Json.Nodes.JsonObject node = JsonSerializer.SerializeToNode(spec, spec.GetType(), _json)!.AsObject();
        node["trendline"] = ChartText.TrendKind(trendline);
        if (trendline == ChartTrendline.Polynomial)
        {
            node["trendlineOrder"] = order;
        }
        else if (trendline == ChartTrendline.MovingAverage)
        {
            node["trendlinePeriod"] = period;
        }

        return node;
    }

    // The spec with the category markers as indices ({ at, to, label }; at is null when a category is
    // unknown, so the layout drops the marker and keeps the indices); the spec itself when there are none.
    private protected static object WithMarkers(object spec, IReadOnlyList<ChartMarker>? markers, IReadOnlyList<string> categories)
    {
        if (markers is not { Count: > 0 })
        {
            return spec;
        }

        int Find(string? category) => category is null ? -1 : categories.ToList().IndexOf(category);
        JsonObject node = spec as JsonObject ?? JsonSerializer.SerializeToNode(spec, spec.GetType(), _json)!.AsObject();
        node["markers"] = JsonSerializer.SerializeToNode(markers.Select(m =>
        {
            int at = Find(m.Category), to = Find(m.To);
            bool known = at >= 0 && (m.To is null || to >= 0);
            return new { At = known ? at : (int?)null, To = known && to >= 0 ? to : (int?)null, m.Label };
        }).ToArray(), _json);
        return node;
    }

    /// <summary>
    /// A cheap change check run before <see cref="BuildSpec"/> on every parameter set: return true
    /// when none of the spec's inputs changed (compared by reference or value) since the last call,
    /// and once a spec exists it is neither built nor serialized again. For charts whose spec is
    /// costly to serialize (a map's geometry); the default, false, always builds the spec and compares its JSON.
    /// </summary>
    protected virtual bool InputsUnchanged() => false;

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

    /// <summary>
    /// The wrapper of a fixed-frame chart, captured by <c>D3ChartFrame</c>: its width is watched so the
    /// frame can follow a narrow box (see <see cref="FitFrame"/>).
    /// </summary>
    internal ElementReference Frame { get; set; }

    /// <summary>Sets the frame for a box of this width; true when the frame changed.</summary>
    private protected virtual bool FitFrame(double width) => false;

    /// <summary>Receives the new width of a fixed-frame chart's box after it was resized.</summary>
    [JSInvokable, EditorBrowsable(EditorBrowsableState.Never)]
    public Task OnFrameWidth(double width) => InvokeAsync(() =>
    {
        if (FitFrame(width))
        {
            RefreshSpec();
            StateHasChanged();
        }
    });

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

        // A layout computed while prerendering travels to a WebAssembly (or Auto) render through the
        // persisted component state, so the chart never flashes back to its placeholder while the
        // browser lays it out again. An Interactive Server circuit has the engine and lays out itself,
        // so nothing is persisted for it.
        if (_engine is not null && _state is not null && !RendererInfo.IsInteractive
            && AssignedRenderMode is InteractiveWebAssemblyRenderMode or InteractiveAutoRenderMode)
        {
            _persisting = _state.RegisterOnPersisting(() =>
            {
                if (_specJson is not null && _layoutJson is not null)
                {
                    try
                    {
                        _state.PersistAsJson(StateKey(_specJson), _layoutJson);
                    }
                    catch (ArgumentException)
                    {
                        // An identical chart on the page already persisted this very layout.
                    }
                }

                return Task.CompletedTask;
            }, AssignedRenderMode);
        }
    }

    // The key is the layout's input itself (kind and a hash of the spec), so two charts can only
    // share a persisted layout when they would compute the same one.
    private string StateKey(string specJson) => StateKey(Kind, specJson);

    internal static string StateKey(string kind, string specJson) =>
        $"daisyblazor-chart:{kind}:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(specJson)), 0, 12)}";

    internal string? SpecJson => _specJson;

    /// <inheritdoc />
    protected override void OnParametersSet() => RefreshSpec();

    private void RefreshSpec()
    {
        // Always asked, so the chart records its inputs from the first spec on.
        if (InputsUnchanged() && _specJson is not null)
        {
            return;
        }

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
        else if (!_restoreTried && _state is not null && TryRestore(json, out string? persisted))
        {
            Layout = JsonSerializer.Deserialize<TLayout>(persisted, _json);
            Error = null;
            _pending = Measured;
        }
    }

    // Persisted state only exists for the first render, so only the first spec is looked up.
    private bool TryRestore(string json, [NotNullWhen(true)] out string? persisted)
    {
        _restoreTried = true;
        return _state!.TryTakeFromJson(StateKey(json), out persisted) && persisted is not null;
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
            _layoutJson = null;
            Error = ex.Message.Split('\n')[0];
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // A fixed-frame chart measures its box once interactive (and again on resize), before its
        // first layout, so a narrow box is laid out once, at its own width.
        bool watchFrame = firstRender && !Measured && Frame.Id is not null && RendererInfo.IsInteractive;
        if (!_pending && !watchFrame)
        {
            return;
        }

        int version = ++_version;
        try
        {
            _module ??= await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);
            if (watchFrame)
            {
                _self ??= DotNetObjectReference.Create(this);
                _observed = Frame;
                double? width = await _module.InvokeAsync<double?>("observe", Frame, null, null, _self);
                if (width is { } w && FitFrame(w))
                {
                    RefreshSpec();
                    StateHasChanged(); // lays out (or, with the server engine, draws) on the next render
                    return;
                }

                if (!_pending)
                {
                    return;
                }
            }

            _pending = false;
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
/// <see cref="ViewHeight"/>, from <see cref="AspectRatio"/>) inside a <c>D3ChartFrame</c>. Once
/// interactive, the chart lays out again at its box's width (and whenever the box is resized), so its
/// text keeps its nominal size in a narrow column as on a full-width card. Before that (prerendering,
/// static server rendering) it lays out at 640 units, or at <see cref="Width"/> when that is in
/// px or rem, and the SVG scales to the box.
/// </summary>
/// <typeparam name="TLayout">The data the layout returns, deserialized from JSON.</typeparam>
public abstract class D3Chart<TLayout> : D3ChartBase<TLayout>
    where TLayout : class
{
    private const double FrameWidth = 640;
    private const double MinFrameWidth = 280;
    private bool _measured;

    /// <summary>
    /// Width of the SVG coordinate space; layouts are computed in these units. It is the box's own
    /// width (at least 280) once measured, so text keeps its true size; until then 640, or
    /// <see cref="Width"/> when that is given in px or rem.
    /// </summary>
    protected double ViewWidth { get; private set; } = FrameWidth;

    /// <summary>Optional heading rendered above the chart; it is also the chart's accessible name.</summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>Draw <c>Title</c> as a visible heading above the chart (the default); false keeps it only as the chart's accessible name, for a chart inside a card that already has a heading.</summary>
    [Parameter] public bool ShowTitle { get; set; } = true;

    /// <summary>CSS width of the chart wrapper.</summary>
    [Parameter] public string? Width { get; set; } = "100%";

    /// <summary>Width divided by height. The chart keeps this shape at any width; its text keeps its size.</summary>
    [Parameter] public double AspectRatio { get; set; } = 16d / 9;

    /// <summary>Additional CSS classes merged onto the <c>w-full</c> wrapper.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Inline styles appended after the width on the wrapper.</summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>Attributes splatted onto the wrapper.</summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }

    /// <summary>
    /// Height of each row in px, for a chart that lays its categories out as rows (horizontal bars, a
    /// Gantt's lanes, a heatmap's rows…): its height then follows its number of rows instead of
    /// <see cref="AspectRatio"/>, so a long list keeps readable rows at any width. A chart, or an
    /// orientation, without rows ignores it.
    /// </summary>
    [Parameter] public double? RowHeight { get; set; }

    /// <summary>
    /// Height of the SVG coordinate space: from <see cref="RowHeight"/> and the chart's
    /// <see cref="RowBands"/> when both are known, else from <see cref="AspectRatio"/>.
    /// </summary>
    protected virtual double ViewHeight => RowHeight is > 0 && RowBands is { Count: > 0 } rows
        ? Math.Round((RowHeight.Value * (rows.Count + rows.Padding)) + rows.Chrome, 2)
        : Math.Round(ViewWidth / (AspectRatio > 0.1 ? AspectRatio : 16d / 9), 2);

    /// <summary>
    /// The rows this chart's layout bands its categories into right now, or null when it has none
    /// (no row layout, or the other orientation). A chart that has rows reports them from the same
    /// filtering its layout applies, with the constants of that layout's frame.
    /// </summary>
    private protected virtual ChartRows? RowBands => null;

    internal string WrapperClass => CssClass.Merge("w-full", Class);

    internal string WrapperStyle => (string.IsNullOrEmpty(Width) ? string.Empty : $"width:{Width};") + Style;

    internal double FrameViewWidth => ViewWidth;

    internal string ViewBox => $"0 0 {ViewWidth.ToString(Ci)} {ViewHeight.ToString(Ci)}";

    // The view box's own shape, so a chart that sets its height reserves that space while it loads.
    internal string PlaceholderStyle => $"aspect-ratio:{ViewWidth.ToString(Ci)} / {ViewHeight.ToString(Ci)};";

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        // Until the box is measured, a width in pixels is the best guess of it (server layout).
        if (!_measured)
        {
            ViewWidth = ChartText.Pixels(Width) is { } px ? Math.Max(MinFrameWidth, px) : FrameWidth;
        }

        base.OnParametersSet();
    }

    private protected override bool FitFrame(double width)
    {
        _measured = true;
        double fit = Math.Max(Math.Round(width), MinFrameWidth);
        if (fit == ViewWidth)
        {
            return false;
        }

        ViewWidth = fit;
        return true;
    }
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

/// <summary>
/// The rows a chart's layout bands its categories into: their <paramref name="Count"/>, the band
/// padding the layout adds on top, in rows (d3's band step is span ÷ (count + padding)), and the
/// <paramref name="Chrome"/>: the height the layout keeps outside the bands for its axis, titles and
/// margins. With <see cref="D3Chart{TLayout}.RowHeight"/> the chart is RowHeight × (Count + Padding)
/// + Chrome tall, so each band step is RowHeight.
/// </summary>
internal readonly record struct ChartRows(int Count, double Padding, double Chrome)
{
    /// <summary>_shared.mjs LABEL_ROW (FONT × 1.2 + 6): the tick-label row under a horizontal plot.</summary>
    internal const double LabelRow = (11 * 1.2) + 6;

    /// <summary>_compare.mjs categoryFrame() laid out horizontally: bands over [4, height − 22].</summary>
    internal const double CategoryChrome = 26;

    /// <summary>The padding term of a band with paddingInner <paramref name="inner"/> and paddingOuter <paramref name="outer"/>.</summary>
    internal static double Band(double inner, double outer) => (2 * outer) - inner;

    /// <summary>_distribution.mjs frame() with no y title: 10 above the plot, a tick row (24) or a tick row and the value title (42) under it.</summary>
    internal static double DistributionChrome(string? valueTitle) => 10 + (string.IsNullOrEmpty(valueTitle) ? 24 : 42);
}
