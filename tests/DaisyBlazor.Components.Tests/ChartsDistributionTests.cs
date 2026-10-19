using System.Text.Json;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The distribution charts: each sends its data to its layout, shows a placeholder until the layout
/// arrives, then draws marks, axes and tooltips from it; empty or non-finite input never throws.
/// </summary>
public class ChartsDistributionTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly PlotArea _plot = new(40, 26, 624, 318);
    private static readonly AxisTick[] _xTicks = [new(40, "0"), new(624, "100")];
    private static readonly AxisTick[] _yTicks = [new(318, "0"), new(26, "50")];
    private static readonly BoxPositions _pos = new(300, 250, 200, 150, 100);

    private static readonly ValueGroup[] _groups =
    [
        new("EU", [1, 2, 3, 4, 100, double.NaN]),
        new("US", [5, 6, double.PositiveInfinity]),
    ];

    private BunitJSModuleInterop _module = null!;

    private JSRuntimeInvocationHandler<TLayout> Setup<TLayout>()
    {
        _module = JSInterop.SetupModule(ModulePath);
        return _module.Setup<TLayout>("layout", _ => true);
    }

    private string Spec(string kind)
    {
        JSRuntimeInvocation invocation = _module.Invocations["layout"].Single();
        invocation.Arguments[0].ShouldBe(kind);
        return JsonSerializer.Serialize(invocation.Arguments[1], new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    [Fact]
    public void Scatter_draws_a_dot_and_a_hover_cell_per_point()
    {
        JSRuntimeInvocationHandler<ScatterLayout> call = Setup<ScatterLayout>();
        ScatterPoint[] points = [new(1, 2, Series: "Pro", Label: "Larkspur"), new(3, 4, Series: "Starter"), new(double.NaN, 1)];

        IRenderedComponent<ScatterChart> cut = Render<ScatterChart>(ps => ps
            .Add(p => p.Points, points).Add(p => p.XTitle, "Seats").Add(p => p.YScale, AxisScale.Log));

        cut.Find("[aria-busy=true]").ShouldNotBeNull();
        string spec = Spec("scatter");
        spec.ShouldContain("\"yLog\":true");
        spec.ShouldContain("\"series\":\"Pro\"");
        spec.ShouldNotContain("NaN");

        call.SetResult(new ScatterLayout(_plot, _xTicks, _yTicks, ["Pro", "Starter"],
        [
            new(1, 2, null, "Larkspur", 0, 100, 200, 4, "M0,0L10,0L10,10Z"),
            new(3, 4, null, null, 1, 300, 100, 4, "M10,0L20,0L20,10Z"),
        ]));
        cut.WaitForAssertion(() => cut.FindAll("svg circle").Count.ShouldBe(2));
        cut.FindAll("svg path.dbc-cell").Count.ShouldBe(2);
        cut.Markup.ShouldContain("Larkspur · Pro: Seats 1, y 2");
        cut.Markup.ShouldContain("var(--color-secondary)");
        cut.FindAll(".rounded-sm").Count.ShouldBe(2, "legend for two series");
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Scatter plot");
    }

    [Fact]
    public void Scatter_draws_series_lines_and_trend_lines_with_their_equation()
    {
        JSRuntimeInvocationHandler<ScatterLayout> call = Setup<ScatterLayout>();
        IRenderedComponent<ScatterChart> cut = Render<ScatterChart>(ps => ps
            .Add(p => p.Points, [new ScatterPoint(1, 2, Series: "Pro"), new ScatterPoint(3, 5, Series: "Pro")])
            .Add(p => p.ShowLine, true).Add(p => p.Trendline, ChartTrendline.Linear));

        string spec = Spec("scatter");
        spec.ShouldContain("\"showLine\":true");
        spec.ShouldContain("\"trendline\":\"linear\"");

        call.SetResult(new ScatterLayout(_plot, _xTicks, _yTicks, ["Pro"],
            [new(1, 2, null, null, 0, 100, 200, 4, "M0,0Z"), new(3, 5, null, null, 0, 300, 100, 4, "M1,1Z")],
            [new(0, "M100,200L300,100")],
            [new(0, "linear", 0.5, 1.5, 1, "M100,200L300,100")]));
        cut.WaitForAssertion(() => cut.FindAll("svg path[stroke-dasharray]").Count.ShouldBe(1));
        cut.FindAll("svg path[d='M100,200L300,100']").Count.ShouldBe(3, "series line, trend hit area and dashed trend");
        cut.Markup.ShouldContain("Pro linear trend: y = 1.5x + 0.5, R² = 1");
        cut.Find("svg").QuerySelector("desc")!.TextContent.ShouldContain("with a linear trend line per series");
    }

    [Theory]
    [InlineData("exponential", 2, 0.5, "y = 2·e^(0.5x)")]
    [InlineData("logarithmic", -1, 3, "y = 3·ln(x) − 1")]
    [InlineData("power", 2, 1.25, "y = 2·x^1.25")]
    public void Scatter_trend_equations_follow_their_kind(string kind, double a, double b, string equation)
    {
        JSRuntimeInvocationHandler<ScatterLayout> call = Setup<ScatterLayout>();
        IRenderedComponent<ScatterChart> cut = Render<ScatterChart>(ps => ps
            .Add(p => p.Points, [new ScatterPoint(1, 2)]).Add(p => p.Trendline, Enum.Parse<ChartTrendline>(kind, ignoreCase: true)));

        call.SetResult(new ScatterLayout(_plot, _xTicks, _yTicks, [""], [], Trends: [new(0, kind, a, b, 0.9, "M0,0L1,1")]));
        cut.WaitForAssertion(() => cut.Markup.ShouldContain(equation));
        cut.Markup.ShouldContain("R² = 0.9");
    }

    [Fact]
    public void Contour_draws_bands_relative_to_the_plot()
    {
        JSRuntimeInvocationHandler<ContourLayout> call = Setup<ContourLayout>();
        IRenderedComponent<ContourChart> cut = Render<ContourChart>(ps => ps
            .Add(p => p.Points, [new ScatterPoint(1, 2), new ScatterPoint(2, 3)]).Add(p => p.Levels, 4).Add(p => p.Bandwidth, 15));

        string spec = Spec("contour");
        spec.ShouldContain("\"thresholds\":4");
        spec.ShouldContain("\"bandwidth\":15");

        call.SetResult(new ContourLayout(_plot, _xTicks, _yTicks, [new(0.001, "M0,0L5,5Z"), new(0.002, "M1,1L4,4Z")], []));
        cut.WaitForAssertion(() => cut.FindAll("svg g[transform] path").Count.ShouldBe(2));
        cut.Find("svg g[transform]").GetAttribute("transform").ShouldBe("translate(40,26)");
        cut.Markup.ShouldContain("Density level 2 of 2");
    }

    [Fact]
    public void Hexbin_draws_each_bin_with_its_count()
    {
        JSRuntimeInvocationHandler<HexbinLayout> call = Setup<HexbinLayout>();
        IRenderedComponent<HexbinChart> cut = Render<HexbinChart>(ps => ps
            .Add(p => p.Points, [new ScatterPoint(1, 2)]).Add(p => p.Radius, 9).Add(p => p.CountFormat, "{0} requests"));

        Spec("hexbin").ShouldContain("\"radius\":9");

        call.SetResult(new HexbinLayout(_plot, _xTicks, _yTicks, "M0,-9L7.79,-4.5Z", 5, [new(100, 100, 1), new(120, 100, 5)]));
        cut.WaitForAssertion(() => cut.FindAll("svg g[clip-path] path").Count.ShouldBe(2));
        cut.Markup.ShouldContain("5 requests");
        cut.Markup.ShouldContain("translate(120,100)");
        cut.FindAll("clipPath").Count.ShouldBe(1);
    }

    [Fact]
    public void BoxPlot_draws_box_whiskers_median_and_outliers()
    {
        JSRuntimeInvocationHandler<BoxPlotLayout> call = Setup<BoxPlotLayout>();
        IRenderedComponent<BoxPlotChart> cut = Render<BoxPlotChart>(ps => ps
            .Add(p => p.Groups, _groups).Add(p => p.ValueTitle, "Response time (ms)"));

        string spec = Spec("boxplot");
        spec.ShouldContain("\"values\":[1,2,3,4,100]");
        spec.ShouldContain("\"horizontal\":false");

        call.SetResult(new BoxPlotLayout(_plot, _xTicks, _yTicks, false, 30,
            [new("EU", 0, 5, 1, 2, 3, 4, 4, 120, _pos, [new(100, 40)])]));
        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(1));
        cut.Find("svg rect").GetAttribute("y").ShouldBe("150");
        cut.Find("svg rect").GetAttribute("height").ShouldBe("100");
        cut.FindAll("svg circle").Count.ShouldBe(1);
        cut.Markup.ShouldContain("EU: median 3, quartiles 2 to 4");
        cut.Markup.ShouldContain("Response time (ms)");
    }

    [Fact]
    public void BoxPlot_horizontal_swaps_the_axes()
    {
        JSRuntimeInvocationHandler<BoxPlotLayout> call = Setup<BoxPlotLayout>();
        IRenderedComponent<BoxPlotChart> cut = Render<BoxPlotChart>(ps => ps.Add(p => p.Groups, _groups).Add(p => p.Horizontal, true));

        Spec("boxplot").ShouldContain("\"horizontal\":true");
        call.SetResult(new BoxPlotLayout(_plot, _xTicks, _yTicks, true, 30,
            [new("EU", 0, 5, 1, 2, 3, 4, 4, 120, new(100, 150, 200, 250, 300), [])]));
        cut.WaitForAssertion(() => cut.Find("svg rect").GetAttribute("x").ShouldBe("150"));
        cut.Find("svg rect").GetAttribute("width").ShouldBe("100");
        cut.Find("svg rect").GetAttribute("y").ShouldBe("90");
    }

    [Fact]
    public void Violin_draws_outline_and_quartiles()
    {
        JSRuntimeInvocationHandler<ViolinLayout> call = Setup<ViolinLayout>();
        IRenderedComponent<ViolinChart> cut = Render<ViolinChart>(ps => ps.Add(p => p.Groups, _groups).Add(p => p.Bandwidth, double.NaN));

        Spec("violin").ShouldContain("\"bandwidth\":0");
        call.SetResult(new ViolinLayout(_plot, _xTicks, _yTicks, false, [new("EU", 0, 5, 2, 3, 4, 120, "M100,100L140,200Z", _pos)]));
        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(1));
        cut.FindAll("svg circle").Count.ShouldBe(1);
        cut.Markup.ShouldContain("EU: median 3, quartiles 2 to 4 (n = 5)");
    }

    [Fact]
    public void Histogram_draws_bars_and_the_median()
    {
        JSRuntimeInvocationHandler<HistogramLayout> call = Setup<HistogramLayout>();
        IRenderedComponent<HistogramChart> cut = Render<HistogramChart>(ps => ps
            .Add(p => p.Values, [1, 2, double.NaN, 3]).Add(p => p.Bins, 12).Add(p => p.ShowMedian, true));

        string spec = Spec("histogram");
        spec.ShouldContain("\"values\":[1,2,3]");
        spec.ShouldContain("\"bins\":12");

        call.SetResult(new HistogramLayout(_plot, _xTicks, _yTicks,
            [new(0, 2, 2, 40, 300, 100, 318), new(2, 4, 1, 300, 600, 200, 318)], new(2, 300)));
        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(2));
        cut.Markup.ShouldContain("0 to 2: 2");
        cut.Markup.ShouldContain("Median 2");
        cut.Find("svg line[stroke-dasharray]").GetAttribute("x1").ShouldBe("300");
    }

    [Fact]
    public void Beeswarm_draws_one_dot_per_value()
    {
        JSRuntimeInvocationHandler<BeeswarmLayout> call = Setup<BeeswarmLayout>();
        IRenderedComponent<BeeswarmChart> cut = Render<BeeswarmChart>(ps => ps.Add(p => p.Groups, _groups).Add(p => p.Radius, 3));

        Spec("beeswarm").ShouldContain("\"radius\":3");
        call.SetResult(new BeeswarmLayout(_plot, _xTicks, _yTicks, 3, [new("US", 1, 200, [new(5, 100, 200), new(6, 110, 197)])]));
        cut.WaitForAssertion(() => cut.FindAll("svg circle").Count.ShouldBe(2));
        cut.Markup.ShouldContain("US: 6");
        cut.Markup.ShouldContain("var(--color-secondary)");
    }

    [Fact]
    public void Ridgeline_draws_a_curve_and_baseline_per_group()
    {
        JSRuntimeInvocationHandler<RidgelineLayout> call = Setup<RidgelineLayout>();
        IRenderedComponent<RidgelineChart> cut = Render<RidgelineChart>(ps => ps.Add(p => p.Groups, _groups).Add(p => p.Overlap, 1));

        Spec("ridgeline").ShouldContain("\"overlap\":1");
        call.SetResult(new RidgelineLayout(_plot, _xTicks, [new(100, "EU")], [new("EU", 0, 5, 100, "M40,100L600,100Z", "M40,100L600,100")]));
        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(3));
        cut.Markup.ShouldContain("EU: 5 values");
        cut.Markup.ShouldContain(">EU<");
    }

    [Fact]
    public void Empty_input_is_sent_and_drawn_without_error()
    {
        JSRuntimeInvocationHandler<BoxPlotLayout> call = Setup<BoxPlotLayout>();
        IRenderedComponent<BoxPlotChart> cut = Render<BoxPlotChart>();

        Spec("boxplot").ShouldContain("\"groups\":[]");
        call.SetResult(new BoxPlotLayout(_plot, [], [], false, 30, []));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.FindAll("svg rect").Count.ShouldBe(0);
        cut.FindAll("[role=alert]").Count.ShouldBe(0);
    }

    [Fact]
    public void Empty_scatter_and_histogram_render_frames()
    {
        JSRuntimeInvocationHandler<ScatterLayout> scatter = Setup<ScatterLayout>();
        IRenderedComponent<ScatterChart> s = Render<ScatterChart>(ps => ps.Add(p => p.Points, [new ScatterPoint(double.NaN, double.NegativeInfinity)]));
        Spec("scatter").ShouldContain("\"points\":[]");
        scatter.SetResult(new ScatterLayout(_plot, [], [], [], []));
        s.WaitForAssertion(() => s.FindAll("svg").Count.ShouldBe(1));
        s.FindAll(".rounded-sm").Count.ShouldBe(0);
    }

    [Fact]
    public void Scatter_sends_smooth_and_draws_error_bars_with_their_tooltip()
    {
        JSRuntimeInvocationHandler<ScatterLayout> call = Setup<ScatterLayout>();
        IRenderedComponent<ScatterChart> cut = Render<ScatterChart>(ps => ps
            .Add(p => p.Points, [new ScatterPoint(2, 10) { XError = 0.5, YError = 3 }, new ScatterPoint(4, 20)])
            .Add(p => p.ShowLine, true).Add(p => p.Smooth, true).Add(p => p.YTitle, "Latency"));

        string spec = Spec("scatter");
        spec.ShouldContain("\"smooth\":true");
        spec.ShouldContain("\"xError\":0.5,\"yError\":3");
        spec.ShouldContain("\"highDensity\":false");

        call.SetResult(new ScatterLayout(_plot, _xTicks, _yTicks, [""],
        [
            new(2, 10, null, null, 0, 100, 200, 4, "M0,0Z", XWhisker: new(0.5, 90, 110), YWhisker: new(3, 230, 170)),
            new(4, 20, null, null, 0, 300, 100, 4, "M1,1Z"),
        ]));
        cut.WaitForAssertion(() => cut.FindAll("svg path[fill=none][stroke-width='1.25']").Count.ShouldBe(2));
        cut.Markup.ShouldContain("M90,200H110M90,196v8M110,196v8");
        cut.Markup.ShouldContain("M100,230V170M96,230h8M96,170h8");
        cut.Markup.ShouldContain("x 2 ± 0.5, Latency 10 ± 3");
        cut.Find("svg").QuerySelector("desc")!.TextContent.ShouldContain("with error bars");
    }

    [Fact]
    public void Scatter_high_density_draws_one_path_per_count_level_with_a_count_legend()
    {
        JSRuntimeInvocationHandler<ScatterLayout> call = Setup<ScatterLayout>();
        IRenderedComponent<ScatterChart> cut = Render<ScatterChart>(ps => ps
            .Add(p => p.Points, [new ScatterPoint(1, 2, Label: "dropped"), new ScatterPoint(3, 4)])
            .Add(p => p.HighDensity, true).Add(p => p.CellSize, 4).Add(p => p.CountFormat, "{0:N0} requests"));

        string spec = Spec("scatter");
        spec.ShouldContain("\"highDensity\":true");
        spec.ShouldContain("\"cellSize\":4");
        spec.ShouldNotContain("dropped", customMessage: "only X, Y and Series are sent");

        // Counts 1, 1 and 36 (max): levels 1 (1 point) and 6 (26–36).
        call.SetResult(new ScatterLayout(_plot, _xTicks, _yTicks, [""], [], Density: [new(0, [0, 0, 1, 2, 1, 1, 10, 5, 36])], CellSize: 4));
        cut.WaitForAssertion(() => cut.FindAll("svg path[fill^='color-mix']").Count.ShouldBe(2));
        cut.FindAll("svg circle").Count.ShouldBe(0);
        cut.Markup.ShouldContain("M40,26h4v4h-4zM48,30h4v4h-4z");
        cut.Markup.ShouldContain("M80,46h4v4h-4z");
        cut.Markup.ShouldContain("26–36 requests per cell");
        cut.FindAll(".rounded-sm").Count.ShouldBe(6, "one legend key per count level");
        cut.Find("svg").QuerySelector("desc")!.TextContent.ShouldContain("small cell");
    }

    [Fact]
    public void Scatter_stacked_quadrants_draw_reference_lines_labels_and_totals()
    {
        JSRuntimeInvocationHandler<ScatterLayout> call = Setup<ScatterLayout>();
        IRenderedComponent<ScatterChart> cut = Render<ScatterChart>(ps => ps
            .Add(p => p.Points, [new ScatterPoint(1, 2, Series: "Compute"), new ScatterPoint(1, 3, Series: "Storage")])
            .Add(p => p.Stacked, true).Add(p => p.XReference, 50).Add(p => p.YReference, 25)
            .Add(p => p.QuadrantLabels, ["Grow", "Fix", "Retire", "Keep"]));

        string spec = Spec("scatter");
        spec.ShouldContain("\"stacked\":true");
        spec.ShouldContain("\"xRef\":50,\"yRef\":25");

        call.SetResult(new ScatterLayout(_plot, _xTicks, _yTicks, ["Compute", "Storage"],
            [new(1, 2, null, null, 0, 100, 200, 4, "M0,0Z", Total: 2), new(1, 3, null, null, 1, 100, 150, 4, "M1,1Z", Total: 5)],
            XRef: 332, YRef: 172));
        cut.WaitForAssertion(() => cut.FindAll("svg line[stroke-dasharray]").Count.ShouldBe(2));
        cut.Find("svg line[stroke-dasharray][x1='332']").GetAttribute("y2").ShouldBe("318");
        cut.Find("svg line[stroke-dasharray][y1='172']").GetAttribute("x2").ShouldBe("624");
        string[] labels = cut.FindAll("svg text").Select(t => t.TextContent).ToArray();
        labels.ShouldContain("Grow");
        labels.ShouldContain("Retire");
        cut.Markup.ShouldContain("Storage: x 1, y 3 (total 5)");
        cut.Find("svg").QuerySelector("desc")!.TextContent.ShouldContain("series stacked");
    }

    [Fact]
    public void Scatter_defaults_send_no_new_options()
    {
        Setup<ScatterLayout>();
        Render<ScatterChart>(ps => ps.Add(p => p.Points, [new ScatterPoint(1, 2)]));

        string spec = Spec("scatter");
        spec.ShouldContain("\"smooth\":false,\"highDensity\":false");
        spec.ShouldContain("\"stacked\":false,\"xRef\":null,\"yRef\":null");
        spec.ShouldContain("\"xError\":null,\"yError\":null");
    }

    [Fact]
    public void Contour_value_mode_fills_value_bands_with_lines_labels_and_a_legend()
    {
        JSRuntimeInvocationHandler<ContourLayout> call = Setup<ContourLayout>();
        IRenderedComponent<ContourChart> cut = Render<ContourChart>(ps => ps
            .Add(p => p.Points, [new ScatterPoint(1, 2) { Value = 40 }, new ScatterPoint(2, 3) { Value = 60 }, new ScatterPoint(3, 3)])
            .Add(p => p.Mode, ContourMode.Value).Add(p => p.ShowLines, true).Add(p => p.ShowPoints, true).Add(p => p.ValueTitle, "Latency (ms)"));

        string spec = Spec("contour");
        spec.ShouldContain("\"mode\":\"value\"");
        spec.ShouldContain("\"showLines\":true");
        spec.ShouldContain("{\"x\":2,\"y\":3,\"value\":60}");
        spec.ShouldNotContain("\"x\":3", customMessage: "points without a value are skipped");

        call.SetResult(new ContourLayout(_plot, _xTicks, _yTicks, [new(40, "M0,0L5,5Z"), new(50, "M1,1L4,4Z")],
            [new(1, 2, 100, 200, 40)], Max: 60, Labels: [new(50, 30, 40)]));
        cut.WaitForAssertion(() => cut.FindAll("svg g[transform] path").Count.ShouldBe(2));
        IReadOnlyList<AngleSharp.Dom.IElement> bands = cut.FindAll("svg g[transform] path");
        bands[0].GetAttribute("fill").ShouldContain("10%");
        bands[1].GetAttribute("fill").ShouldContain("100%");
        bands[0].GetAttribute("stroke").ShouldBe("none");
        bands[1].GetAttribute("stroke").ShouldBe("var(--color-base-content)");
        cut.Markup.ShouldContain("Latency (ms) 40–50");
        cut.Markup.ShouldContain("Latency (ms) 50–60");
        cut.Markup.ShouldContain(">40–50</span>", customMessage: "short legend keys");
        cut.FindAll("svg g[transform] text").Single().TextContent.ShouldBe("50");
        cut.Markup.ShouldContain("x 1, y 2: Latency (ms) 40");
        cut.FindAll(".rounded-sm").Count.ShouldBe(2);
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Value contour chart");
    }

    [Fact]
    public void Contour_density_spec_is_unchanged_by_value_mode()
    {
        Setup<ContourLayout>();
        Render<ContourChart>(ps => ps.Add(p => p.Points, [new ScatterPoint(1, 2) { Value = 9 }]));

        Spec("contour").ShouldBe("{\"points\":[{\"x\":1,\"y\":2}],\"width\":640,\"height\":360,\"xTitle\":null,\"yTitle\":null,\"bandwidth\":20,\"thresholds\":10,\"showPoints\":false}");
    }
}
