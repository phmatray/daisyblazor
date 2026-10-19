using System.Text.Json;
using DaisyBlazor.Charts;
using Microsoft.JSInterop;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The "over time" D3 charts: each shows a placeholder, sends its data to its layout, and draws the
/// layout it gets back; empty input still produces a layout request and an empty chart.
/// </summary>
public class ChartsTimeTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly PlotArea _plot = new(40, 10, 616, 336);

    private static string Spec(BunitJSModuleInterop module, string kind)
    {
        JSRuntimeInvocation invocation = module.Invocations["layout"].Single();
        invocation.Arguments[0].ShouldBe(kind);
        return JsonSerializer.Serialize(invocation.Arguments[1], new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    [Fact]
    public void CalendarHeatmap_draws_a_cell_per_day_and_sends_calendar_dates()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        JSRuntimeInvocationHandler<CalendarHeatmapLayout> call = module.Setup<CalendarHeatmapLayout>("layout", _ => true);
        CalendarHeatmapLayout layout = new(
            [new("2026-01-05", "Mon, Jan 5, 2026", 3, 2, 30, 16, 11), new("2026-01-06", "Tue, Jan 6, 2026", 0, 0, 30, 29, 11)],
            [new(30, "Jan")], [new(21, "Mon")], [2, 4, 6], 8, 11);

        IRenderedComponent<CalendarHeatmap> cut = Render<CalendarHeatmap>(ps => ps
            .Add(p => p.Days, [new CalendarDay(new DateOnly(2026, 1, 5), 3)])
            .Add(p => p.FirstDayOfWeek, DayOfWeek.Sunday));

        cut.Find("[aria-busy=true]").ShouldNotBeNull();
        call.SetResult(layout);
        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(2));
        cut.Markup.ShouldContain("Mon, Jan 5, 2026: 3");
        cut.Markup.ShouldContain("Less");
        cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 640 100");

        string spec = Spec(module, "calendar-heatmap");
        spec.ShouldContain("\"date\":\"2026-01-05\"");
        spec.ShouldContain("\"weekStart\":0");
        spec.ShouldContain("\"levels\":5");
    }

    [Fact]
    public void HeatmapChart_shades_cells_from_the_theme_colour()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        HeatmapLayout layout = new(
            [new(100, "Mon", null, "Mon")], [new(200, "09", null, "09"), new(400, "10", null, "10")],
            [new("Mon", "09", 10, 0, 100, 20, 190, 160), new("Mon", "10", 30, 1, 300, 20, 190, 160)], 10, 30, _plot);
        module.Setup<HeatmapLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<HeatmapChart> cut = Render<HeatmapChart>(ps => ps
            .Add(p => p.Cells, [new HeatmapCell("Mon", "09", 10), new HeatmapCell("Mon", "10", 30)])
            .Add(p => p.ShowValues, true));

        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(2));
        cut.Markup.ShouldContain("Mon, 10: 30");
        cut.Markup.ShouldContain("color-mix(in oklab, var(--color-primary) 100%, var(--color-base-100))");
        cut.Markup.ShouldContain("var(--color-primary-content)");
        Spec(module, "heatmap").ShouldContain("\"row\":\"Mon\"");
    }

    [Fact]
    public void RangeAreaChart_draws_band_line_and_axes()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        RangeAreaLayout layout = new("M40,100L616,120L616,200L40,180Z", "M40,150L616,160",
            [new("Oct 1, 2026", 100, 300, 150, 40, 40, 328), new("Oct 2, 2026", 120, 280, null, 616, 328, 616)],
            [new(40, "Oct"), new(616, "Oct 2")], [new(336, "0"), new(10, "300")], _plot);
        module.Setup<RangeAreaLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<RangeAreaChart> cut = Render<RangeAreaChart>(ps => ps
            .Add(p => p.Points, [new RangePoint(new DateTime(2026, 10, 1), 100, 300, 150), new RangePoint(new DateTime(2026, 10, 2), 120, 280)])
            .Add(p => p.BandLabel, "p50–p95"));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(2));
        cut.Markup.ShouldContain("Oct 1, 2026: p50–p95 100–300, Center 150");
        cut.Markup.ShouldContain("Oct 2");
        cut.Markup.ShouldContain("p50–p95");

        string spec = Spec(module, "range-area");
        spec.ShouldContain("\"date\":\"2026-10-01T00:00:00\"");
        spec.ShouldContain("\"center\":null");
    }

    [Fact]
    public void StreamChart_sends_its_offset_and_draws_a_layer_per_series()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        StreamLayout layout = new(
            [new(0, "Billing", "M40,100L616,100L616,200Z", 300, 150, true), new(1, "Search", "M40,200L616,200L616,300Z", 300, 250, false)],
            [new("W1", 40, 328, [10, 30]), new("W2", 328, 616, [20, 0])],
            [new(40, "W1"), new(616, "W2")], [], _plot);
        module.Setup<StreamLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<StreamChart> cut = Render<StreamChart>(ps => ps
            .Add(p => p.Series, [new ChartSeries("Billing", [10, 20]), new ChartSeries("Search", [30, double.NaN])])
            .Add(p => p.Labels, ["W1", "W2"])
            .Add(p => p.Offset, StreamOffset.Percent));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(2));
        cut.Markup.ShouldContain("W1: Billing 10, Search 30");
        cut.Markup.ShouldContain("var(--color-primary-content)");

        string spec = Spec(module, "stream");
        spec.ShouldContain("\"offset\":\"percent\"");
        spec.ShouldContain("\"values\":[30,null]");
    }

    [Theory]
    [InlineData(CandlestickStyle.Candle, 1, 2)]
    [InlineData(CandlestickStyle.Ohlc, 3, 1)]
    [InlineData(CandlestickStyle.HighLow, 1, 1)]
    public void CandlestickChart_draws_each_style(CandlestickStyle style, int lines, int rects)
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        CandlestickLayout layout = new(
            [new("Oct 1, 2026 09:00", 40, 70, 30, 55, true, 300, 20, 200, 120, 40, 260)],
            [new(300, "09:00")], [], _plot);
        module.Setup<CandlestickLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<CandlestickChart> cut = Render<CandlestickChart>(ps => ps
            .Add(p => p.Candles, [new Candle(new DateTime(2026, 10, 1, 9, 0, 0), 40, 70, 30, 55)])
            .Add(p => p.CandleStyle, style));

        cut.WaitForAssertion(() => cut.FindAll("svg g line").Count.ShouldBe(lines));
        cut.FindAll("svg g rect").Count.ShouldBe(rects);
        cut.Markup.ShouldContain("var(--color-success)");
        cut.Markup.ShouldContain("Oct 1, 2026 09:00: open 40, high 70, low 30, close 55");
        Spec(module, "candlestick").ShouldContain("\"time\":\"2026-10-01T09:00:00\"");
    }

    [Fact]
    public void GanttChart_draws_bars_progress_milestones_and_today()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        GanttLayout layout = new(
            [
                new(true, "Plan", "Plan", 0, false, null, null, null, 0, 0, 20, 30),
                new(false, "Design", "Design", 0, false, "Oct 1, 2026", "Oct 10, 2026", 0.5, 100, 300, 60, 30),
                new(false, "Launch", "Launch", -1, true, "Oct 20, 2026", "Oct 20, 2026", null, 500, 502, 100, 30),
            ],
            ["Plan"], new AxisTick(250, "Oct 8, 2026"), [new(100, "Oct 1")], _plot);
        module.Setup<GanttLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<GanttChart> cut = Render<GanttChart>(ps => ps
            .Add(p => p.Tasks, [new GanttTask("Design", new DateTime(2026, 10, 1), new DateTime(2026, 10, 10), "Plan", 0.5), new GanttTask("Launch", new DateTime(2026, 10, 20))])
            .Add(p => p.Today, new DateTime(2026, 10, 8)));

        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(2)); // track + progress
        cut.FindAll("svg path").Count.ShouldBe(1); // the milestone diamond
        cut.Markup.ShouldContain("Design: Oct 1, 2026 – Oct 10, 2026");
        cut.Markup.ShouldContain("Today");

        string spec = Spec(module, "gantt");
        spec.ShouldContain("\"today\":\"2026-10-08T00:00:00\"");
        spec.ShouldContain("\"end\":null");
        spec.ShouldContain("\"group\":\"Plan\"");
    }

    [Fact]
    public void BumpChart_draws_lines_points_and_end_labels()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        BumpLayout layout = new(
            [
                new(0, "Starter", "Starter", "M80,100C200,100,200,200,320,200", [new("Q1", 1, 40, 80, 100), new("Q2", 2, 30, 320, 200)], new(80, 100), new(320, 200)),
                new(1, "Pro", "Pro", "M80,200C200,200,200,100,320,100", [new("Q1", 2, 20, 80, 200), new("Q2", 1, 50, 320, 100)], new(80, 200), null),
            ],
            [new(80, "Q1"), new(320, "Q2")], 2, _plot);
        module.Setup<BumpLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<BumpChart> cut = Render<BumpChart>(ps => ps
            .Add(p => p.Series, [new ChartSeries("Starter", [40, 30]), new ChartSeries("Pro", [20, 50])])
            .Add(p => p.Labels, ["Q1", "Q2"]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle").Count.ShouldBe(4));
        cut.Markup.ShouldContain("Pro, Q2: #1 (50)");
        cut.FindAll("svg text").Count(t => t.TextContent == "Starter").ShouldBe(2);
        cut.FindAll("svg text").Count(t => t.TextContent == "Pro").ShouldBe(1);
        Spec(module, "bump").ShouldContain("\"valuesAreRanks\":false");
    }

    [Fact]
    public void Time_charts_with_no_data_still_ask_for_a_layout_and_draw_an_empty_chart()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        module.Setup<GanttLayout>("layout", _ => true).SetResult(new GanttLayout([], [], null, [], new PlotArea(0, 0, 0, 0)));
        module.Setup<CalendarHeatmapLayout>("layout", _ => true).SetResult(new CalendarHeatmapLayout([], [], [], [], 0, 0));

        IRenderedComponent<GanttChart> gantt = Render<GanttChart>();
        IRenderedComponent<CalendarHeatmap> calendar = Render<CalendarHeatmap>();

        gantt.WaitForAssertion(() => gantt.Find("svg").GetAttribute("aria-label").ShouldBe("Gantt chart"));
        gantt.Find("desc").TextContent.ShouldBe("No tasks.");
        calendar.WaitForAssertion(() => calendar.FindAll("svg rect").Count.ShouldBe(0));
        calendar.Markup.ShouldNotContain("Less");
        module.Invocations["layout"].Count.ShouldBe(2);
    }
}
