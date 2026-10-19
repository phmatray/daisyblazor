using System.Text.Json;
using AngleSharp.Dom;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Trend lines: polynomial and moving average on the scatter chart, and every kind on the line,
/// area and bar charts (one dashed line per series, with its equation or period in the tooltip).
/// </summary>
public class ChartsTrendlineTests : BunitContext
{
    private static readonly PlotArea _plot = new(40, 26, 624, 318);
    private static readonly AxisTick[] _ticks = [new(40, "0"), new(624, "100")];

    private static List<ChartSeries> Two() => [new("Requests", [10, 20, 30, 40]), new("Errors", [4, 3, 2, 1])];

    [Fact]
    public void LineChart_sends_the_trend_and_draws_a_dashed_line_per_series_with_its_equation()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(line: ChartFakes.Line with
        {
            Trends = [new(0, "linear", 10, 10, 1, "M181,100L483,50"), new(1, "polynomial", 5, -1.5, 0.98, "M181,200L483,210", [5, -1.5, 0.25])],
        });
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, Two())
            .Add(p => p.Trendline, ChartTrendline.Polynomial)
            .Add(p => p.TrendlineOrder, 3));

        string spec = module.Spec();
        spec.ShouldContain("\"trendline\":\"polynomial\",\"trendlineOrder\":3");
        spec.ShouldNotContain("trendlinePeriod");

        cut.FindAll("svg path[stroke-dasharray='6 4']").Count.ShouldBe(2);
        cut.FindAll("svg path[stroke-dasharray='6 4']")[1].GetAttribute("stroke").ShouldBe(ChartPalette.At(1));
        cut.Markup.ShouldContain("Requests linear trend: y = 10x + 10, R² = 1");
        cut.Markup.ShouldContain("Errors polynomial trend: y = 0.25x^2 − 1.5x + 5, R² = 0.98");
    }

    [Fact]
    public void LineChart_sends_no_trend_by_default_or_when_stacked()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        Render<LineChart>(ps => ps.Add(p => p.Series, Two()));
        module.Spec().ShouldNotContain("trendline");

        IRenderedComponent<LineChart> stacked = Render<LineChart>(ps => ps
            .Add(p => p.Series, Two()).Add(p => p.Stacked, true).Add(p => p.Trendline, ChartTrendline.Linear));
        module.Spec().ShouldNotContain("trendline", customMessage: "Trendline is ignored while stacked");
        stacked.FindAll("svg path[stroke-dasharray='6 4']").ShouldBeEmpty();
    }

    [Fact]
    public void AreaChart_forwards_the_trend()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        Render<AreaChart>(ps => ps
            .Add(p => p.Series, Two())
            .Add(p => p.Trendline, ChartTrendline.MovingAverage)
            .Add(p => p.TrendlinePeriod, 2));

        string spec = module.Spec();
        spec.ShouldContain("\"trendline\":\"movingaverage\",\"trendlinePeriod\":2");
        spec.ShouldNotContain("trendlineOrder");
    }

    [Fact]
    public void BarChart_draws_a_moving_average_with_its_period_also_horizontal()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(bar: ChartFakes.Bar with
        {
            Trends = [new(0, "movingaverage", 0, 0, 0, "M181,120L483,60", Period: 2)],
        });
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("Free", 48200), new("Pro", 12000) })
            .Add(p => p.Horizontal, true)
            .Add(p => p.Trendline, ChartTrendline.MovingAverage)
            .Add(p => p.TrendlinePeriod, 2));

        string spec = module.Spec();
        spec.ShouldContain("\"horizontal\":true");
        spec.ShouldContain("\"trendline\":\"movingaverage\",\"trendlinePeriod\":2");

        // A flat bar list cycles colours per bar, so its trend takes the text colour.
        IElement dashed = cut.Find("svg path[stroke-dasharray='6 4']");
        dashed.GetAttribute("d").ShouldBe("M181,120L483,60");
        dashed.GetAttribute("stroke").ShouldBe("var(--color-base-content)");
        cut.Markup.ShouldContain("Moving average trend: 2-point moving average");
        cut.Markup.ShouldNotContain("R²");
    }

    [Fact]
    public void BarChart_sends_no_trend_when_percent()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        Render<BarChart>(ps => ps
            .Add(p => p.Series, Two()).Add(p => p.Percent, true).Add(p => p.Trendline, ChartTrendline.Linear));
        module.Spec().ShouldNotContain("trendline");
    }

    [Fact]
    public void Scatter_sends_the_order_or_period_and_describes_the_new_kinds()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        JSRuntimeInvocationHandler<ScatterLayout> call = module.Setup<ScatterLayout>("layout", _ => true);
        IRenderedComponent<ScatterChart> cut = Render<ScatterChart>(ps => ps
            .Add(p => p.Points, [new ScatterPoint(1, 2, Series: "EU"), new ScatterPoint(2, 3, Series: "US")])
            .Add(p => p.Trendline, ChartTrendline.MovingAverage)
            .Add(p => p.TrendlinePeriod, 4));

        string spec = JsonSerializer.Serialize(module.Invocations["layout"].Last().Arguments[1], new JsonSerializerOptions(JsonSerializerDefaults.Web));
        spec.ShouldContain("\"trendline\":\"movingaverage\"");
        spec.ShouldContain("\"trendlinePeriod\":4");
        spec.ShouldNotContain("trendlineOrder");

        call.SetResult(new ScatterLayout(_plot, _ticks, _ticks, ["EU", "US"], [],
            Trends: [new(0, "movingaverage", 0, 0, 0, "M0,0L1,1", Period: 4), new(1, "polynomial", 1, 2, 0.5, "M0,0L2,2", [1, -2, 3])]));
        cut.WaitForAssertion(() => cut.Markup.ShouldContain("EU moving average trend: 4-point moving average"));
        cut.Markup.ShouldContain("US polynomial trend: y = 3x^2 − 2x + 1, R² = 0.5");
        cut.Find("svg").QuerySelector("desc")!.TextContent.ShouldContain("with a moving average trend line per series");
    }

    [Fact]
    public void Scatter_with_a_straight_trend_sends_the_same_spec_as_before()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        module.Setup<ScatterLayout>("layout", _ => true);
        Render<ScatterChart>(ps => ps.Add(p => p.Points, [new ScatterPoint(1, 2)]).Add(p => p.Trendline, ChartTrendline.Linear));

        string spec = JsonSerializer.Serialize(module.Invocations["layout"].Last().Arguments[1], new JsonSerializerOptions(JsonSerializerDefaults.Web));
        spec.ShouldContain("\"showLine\":false,\"trendline\":\"linear\",\"smooth\":false");
        spec.ShouldNotContain("trendlineOrder");
        spec.ShouldNotContain("trendlinePeriod");
    }
}
