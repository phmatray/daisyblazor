using System.Text.Json;
using DaisyBlazor.Charts;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Stands in for the bundled chart module (charts.mjs) in bUnit: the measured charts (line, bar,
/// spark) get their layout from observe(), the pie from layout(). The geometry itself is tested
/// against the real layouts in tests/charts-core.test.mjs.
/// </summary>
internal static class ChartFakes
{
    public const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly JsonSerializerOptions _web = new(JsonSerializerDefaults.Web);

    public static readonly LineLayout Line = new(640, 256, new PlotArea(30, 8, 634, 231),
        [new(181, "Jan", null, "Jan"), new(483, "Feb", null, "Feb")], [new(231, "0", 0), new(8, "50", 50)],
        [new("M181,100L483,50", "M181,100L483,50L483,231L181,231Z", [new(0, 181, 100, false), new(1, 483, 50, false)])]);

    public static readonly BarLayout Bar = new(640, 256, new PlotArea(30, 8, 634, 231),
        [new(181, "Free", null, "Free"), new(483, "Pro", null, "Pro")], [new(231, "0", 0), new(8, "50k", 50000)],
        [new(0, 0, 100, 20, 160, 211), new(1, 0, 400, 180, 160, 51)]);

    public static readonly PieLayout Pie = new(100, 100, 94, 0,
        [new(0, "M0,-94A94,94,0,1,1,-89.4,29.05L0,0Z", 70, 0, 4.4), new(1, "M-89.4,29.05A94,94,0,0,1,0,-94L0,0Z", 30, 4.4, 6.28)], 4);

    public static readonly SparkLayout Spark = new(80, 24, "M2,22L40,12L78,2", "M2,22L40,12L78,2L78,22L40,22L2,22Z", new ChartPoint(78, 2));

    /// <summary>Sets up the chart module to answer every chart kind with the given (or default) layouts.</summary>
    public static BunitJSModuleInterop SetupCharts(this BunitJSInterop js, LineLayout? line = null, BarLayout? bar = null, PieLayout? pie = null, SparkLayout? spark = null)
    {
        BunitJSModuleInterop module = js.SetupModule(ModulePath);
        module.Setup<LineLayout>("observe", i => Kind(i) == "line").SetResult(line ?? Line);
        module.Setup<BarLayout>("observe", i => Kind(i) == "bar").SetResult(bar ?? Bar);
        module.Setup<SparkLayout>("observe", i => Kind(i) == "spark").SetResult(spark ?? Spark);
        module.Setup<PieLayout>("layout", i => (string?)i.Arguments[0] == "pie").SetResult(pie ?? Pie);
        module.SetupVoid("unobserve");
        return module;
    }

    /// <summary>
    /// Sets up the chart module for an interactive render, where a fixed-frame chart first measures
    /// its box: observe() with no kind answers <paramref name="frameWidth"/> (by default wider than the 640 frame).
    /// </summary>
    public static BunitJSModuleInterop SetupChartModule(this BunitContext context, double frameWidth = 800)
    {
        context.SetRendererInfo(new RendererInfo("WebAssembly", isInteractive: true));
        BunitJSModuleInterop module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<double?>("observe", i => i.Arguments[1] is null).SetResult(frameWidth);
        return module;
    }

    /// <summary>The JSON spec the chart sent with its last call to <paramref name="method"/>.</summary>
    public static string Spec(this BunitJSModuleInterop module, string method = "observe")
    {
        JSRuntimeInvocation call = module.Invocations[method].Last();
        object? spec = method switch { "observe" => call.Arguments[2], _ => call.Arguments[1] };
        return JsonSerializer.Serialize(spec, _web);
    }

    private static string? Kind(JSRuntimeInvocation call) => (string?)call.Arguments[1];
}
