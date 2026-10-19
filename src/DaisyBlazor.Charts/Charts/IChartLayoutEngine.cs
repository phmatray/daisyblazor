namespace DaisyBlazor.Charts;

/// <summary>
/// Computes a chart layout without a browser, so a chart can render its full SVG during server
/// prerendering and static server rendering. The layouts are the same D3 modules the browser runs;
/// register an implementation with <c>AddDaisyBlazorChartsServerRendering()</c> from the
/// DaisyBlazor.Charts.Server package.
/// </summary>
public interface IChartLayoutEngine
{
    /// <summary>Runs the layout named <paramref name="kind"/> on a JSON spec and returns the layout as JSON.</summary>
    string Layout(string kind, string specJson);
}
