using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DaisyBlazor.Charts;

/// <summary>
/// Base for the charts whose geometry is computed by the D3 module bundled in this package
/// (<c>_content/DaisyBlazor.Charts/charts.mjs</c>). After each render whose inputs changed, the chart
/// sends its <see cref="BuildSpec"/> to the layout named by <see cref="Kind"/> and draws the returned
/// <typeparamref name="TLayout"/> as inline SVG, coloured with daisyUI theme tokens. The module loads
/// on first use; until a layout arrives (and under static server rendering, which runs no
/// JavaScript) the chart shows a placeholder of the same size.
/// </summary>
/// <typeparam name="TLayout">The data the layout returns, deserialized from JSON.</typeparam>
public abstract class D3Chart<TLayout> : ComponentBase, IAsyncDisposable
    where TLayout : class
{
    internal const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    /// <summary>Width of the SVG coordinate space; layouts are computed in these units.</summary>
    protected const double ViewWidth = 640;

    /// <summary>Invariant culture for numbers written into SVG attributes.</summary>
    protected static readonly CultureInfo Ci = CultureInfo.InvariantCulture;

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    private IJSObjectReference? _module;
    private string? _specJson;
    private object? _spec;
    private bool _pending;
    private int _version;

    [Inject] private IJSRuntime JS { get; set; } = null!;

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

    /// <summary>The layout file in <c>wwwroot/layouts/</c> (without extension).</summary>
    protected abstract string Kind { get; }

    /// <summary>The layout's input. It is serialized to JSON (camelCase); a new layout is requested only when that JSON changes.</summary>
    protected abstract object BuildSpec();

    /// <summary>The latest layout, or null before the first one arrives.</summary>
    protected TLayout? Layout { get; private set; }

    /// <summary>The layout's error message when it could not be computed (for example a cycle in a Sankey).</summary>
    protected string? Error { get; private set; }

    /// <summary>Height of the SVG coordinate space, from <see cref="AspectRatio"/>.</summary>
    protected double ViewHeight => Math.Round(ViewWidth / (AspectRatio > 0.1 ? AspectRatio : 16d / 9), 2);

    internal string WrapperClass => CssClass.Merge("w-full", Class);

    internal string WrapperStyle => (string.IsNullOrEmpty(Width) ? string.Empty : $"width:{Width};") + Style;

    internal string ViewBox => $"0 0 {ViewWidth.ToString(Ci)} {ViewHeight.ToString(Ci)}";

    internal string? ErrorMessage => Error;

    internal bool IsReady => Layout is not null;

    /// <summary>Formats a coordinate for an SVG attribute.</summary>
    protected static string N(double value) => Math.Round(value, 2).ToString(Ci);

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        object spec = BuildSpec();
        string json = JsonSerializer.Serialize(spec, spec.GetType(), _json);
        if (json != _specJson)
        {
            _specJson = json;
            _spec = spec;
            _pending = true;
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
            TLayout layout = await _module.InvokeAsync<TLayout>("layout", Kind, _spec);
            if (version != _version)
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
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }

        GC.SuppressFinalize(this);
    }
}
