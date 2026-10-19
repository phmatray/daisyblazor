using DaisyBlazor.Charts;
using DaisyBlazor.Charts.Server;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registers server rendering for DaisyBlazor charts.</summary>
public static class DaisyBlazorChartsServerExtensions
{
    /// <summary>
    /// Lets every DaisyBlazor chart compute its layout on the server, so prerendered and statically
    /// rendered pages contain the finished SVG. Interactive charts still lay out again in the browser.
    /// </summary>
    public static IServiceCollection AddDaisyBlazorChartsServerRendering(this IServiceCollection services)
    {
        services.AddSingleton<IChartLayoutEngine, JintChartLayoutEngine>();
        return services;
    }
}
