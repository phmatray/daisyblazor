using System.Globalization;
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
        BunitJSModuleInterop module = this.SetupChartModule();
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
        cut.Markup.ShouldContain($"{new DateOnly(2026, 1, 5).ToString("D", CultureInfo.CurrentCulture)}: 3");
        cut.Markup.ShouldContain("Less");
        cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 640 100");

        string spec = Spec(module, "calendar-heatmap");
        spec.ShouldContain("\"date\":\"2026-01-05\"");
        spec.ShouldContain("\"weekStart\":0");
        spec.ShouldContain("\"levels\":5");
    }

    [Fact]
    public void CalendarHeatmap_names_months_weekdays_and_days_in_the_current_culture()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
        try
        {
            BunitJSModuleInterop module = this.SetupChartModule();
            module.Setup<CalendarHeatmapLayout>("layout", _ => true).SetResult(new CalendarHeatmapLayout(
                [new("2026-01-05", "Mon, Jan 5, 2026", 3, 2, 30, 16, 11)], [new(30, "janv.")], [new(21, "lun.")], [2, 4, 6], 8, 11));

            IRenderedComponent<CalendarHeatmap> cut = Render<CalendarHeatmap>(ps => ps.Add(p => p.Days, [new CalendarDay(new DateOnly(2026, 1, 5), 3)]));

            string spec = Spec(module, "calendar-heatmap");
            spec.ShouldContain("\"monthNames\":[\"janv.\"");
            spec.ShouldContain("\"dayNames\":[\"dim.\",\"lun.\"");
            cut.WaitForAssertion(() => cut.Markup.ShouldContain("lundi 5 janvier 2026: 3"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void HeatmapChart_shades_cells_from_the_theme_colour()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
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
        BunitJSModuleInterop module = this.SetupChartModule();
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
        BunitJSModuleInterop module = this.SetupChartModule();
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
        BunitJSModuleInterop module = this.SetupChartModule();
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
    public void CandlestickChart_without_indicators_or_volume_sends_the_plain_spec()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        module.Setup<CandlestickLayout>("layout", _ => true);

        Render<CandlestickChart>(ps => ps
            .Add(p => p.Candles, [new Candle(new DateTime(2026, 10, 1, 9, 0, 0), 40, 70, 30, 55) { Volume = 1200 }]));

        Spec(module, "candlestick").ShouldBe("{\"candles\":[{\"time\":\"2026-10-01T09:00:00\",\"open\":40,\"high\":70,\"low\":30,\"close\":55}],\"width\":640,\"height\":360}");
    }

    [Fact]
    public void CandlestickChart_draws_indicators_panels_and_volume()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        CandlestickLayout layout = new(
            [new("Oct 1, 2026 09:00", 40, 70, 30, 55, true, 300, 20, 200, 120, 40, 260) { Volume = 1200 }],
            [new(300, "09:00")], [], _plot with { Bottom = 200 })
        {
            Indicators =
            [
                new(0, "sma", -1, "M290,150L310,140", "", "", "", "", [], [47.5]),
                new(1, "bollinger", -1, "M290,150L310,140", "", "M290,100L310,90", "M290,200L310,190", "M290,100L310,90L310,190L290,200Z", [], [47.5]),
                new(2, "rsi", 0, "M290,230L310,220", "", "", "", "", [], [61.25]),
            ],
            Panels =
            [
                new("rsi", 214, 260, [new(250, "30", 30), new(224, "70", 70)], [250, 224], []),
                new("volume", 274, 336, [new(336, "0", 0)], [], [new(290, 280, 20, 56, true, 1200)]),
            ],
        };
        module.Setup<CandlestickLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<CandlestickChart> cut = Render<CandlestickChart>(ps => ps
            .Add(p => p.Candles, [new Candle(new DateTime(2026, 10, 1, 9, 0, 0), 40, 70, 30, 55) { Volume = 1200 }])
            .Add(p => p.ShowVolume, true)
            .Add(p => p.Indicators, [new ChartIndicator(IndicatorKind.Sma, Period: 20), new ChartIndicator(IndicatorKind.Bollinger), new ChartIndicator(IndicatorKind.Rsi)]));

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("M290,150L310,140"));
        string spec = Spec(module, "candlestick");
        spec.ShouldContain("\"volume\":1200");
        spec.ShouldContain("\"indicators\":[{\"kind\":\"sma\",\"period\":20,");
        spec.ShouldContain("{\"kind\":\"bollinger\",\"period\":null,\"stdDev\":2,");
        spec.ShouldContain("\"showVolume\":true");

        // Indicators take palette colours other than the rising and falling ones, in order.
        cut.Find("path[d='M290,150L310,140']").GetAttribute("stroke").ShouldBe("var(--color-primary)");
        cut.Find("path[d='M290,100L310,90L310,190L290,200Z']").GetAttribute("fill").ShouldBe("var(--color-secondary)");
        cut.Find("path[d='M290,230L310,220']").GetAttribute("stroke").ShouldBe("var(--color-accent)");
        cut.FindAll("line[stroke-dasharray='3 3']").Count.ShouldBe(2); // RSI 30 and 70
        cut.Markup.ShouldContain("Oct 1, 2026 09:00: open 40, high 70, low 30, close 55, volume 1,200; SMA 20 47.5; Bollinger 20, 2σ 47.5; RSI 14 61.25");
        cut.Markup.ShouldContain("Volume: 1,200");
        cut.Markup.ShouldContain(">Volume<");
        cut.Markup.ShouldContain(">RSI 14<");
        string legend = cut.Find("svg + div").TextContent;
        legend.ShouldContain("SMA 20");
        legend.ShouldContain("Bollinger 20, 2σ");
        legend.ShouldContain("RSI 14");
    }

    [Fact]
    public void CandlestickChart_names_indicators_by_the_periods_the_layout_uses()
    {
        // The layout takes the default for a period under 1, so the legend must say so too.
        BunitJSModuleInterop module = this.SetupChartModule();
        CandlestickLayout layout = new(
            [new("Oct 1, 2026 09:00", 40, 70, 30, 55, true, 300, 20, 200, 120, 40, 260)],
            [new(300, "09:00")], [], _plot with { Bottom = 200 })
        {
            Indicators =
            [
                new(0, "sma", -1, "M290,150L310,140", "", "", "", "", [], [47.5]),
                new(1, "macd", 0, "M290,230L310,220", "", "", "", "", [], [1.5]),
            ],
            Panels = [new("macd", 214, 260, [new(250, "0", 0)], [237], [])],
        };
        module.Setup<CandlestickLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<CandlestickChart> cut = Render<CandlestickChart>(ps => ps
            .Add(p => p.Candles, [new Candle(new DateTime(2026, 10, 1, 9, 0, 0), 40, 70, 30, 55)])
            .Add(p => p.Indicators, [new ChartIndicator(IndicatorKind.Sma, Period: 0), new ChartIndicator(IndicatorKind.Macd, Period: 0, SlowPeriod: 0, SignalPeriod: -3)]));

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("M290,150L310,140"));
        string legend = cut.Find("svg + div").TextContent;
        legend.ShouldContain("SMA 20");
        legend.ShouldContain("MACD 12, 26, 9");
        legend.ShouldNotContain("SMA 0");
    }

    [Fact]
    public void CandlestickChart_draws_a_close_line_and_area_and_sends_the_style()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        CandlestickLayout layout = new(
            [new("Oct 1, 2026 09:00", 40, 70, 30, 55, true, 300, 20, 200, 120, 40, 260)],
            [new(300, "09:00")], [], _plot)
        {
            PriceLine = "M290,150L310,140",
            PriceArea = "M290,150L310,140L310,336L290,336Z",
        };
        module.Setup<CandlestickLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<CandlestickChart> cut = Render<CandlestickChart>(ps => ps
            .Add(p => p.Candles, [new Candle(new DateTime(2026, 10, 1, 9, 0, 0), 40, 70, 30, 55)])
            .Add(p => p.CandleStyle, CandlestickStyle.Area));

        cut.WaitForAssertion(() => cut.Find("path[d='M290,150L310,140']").GetAttribute("stroke").ShouldBe("var(--color-base-content)"));
        cut.Find("path[d='M290,150L310,140L310,336L290,336Z']").GetAttribute("fill-opacity").ShouldBe("0.15");
        cut.FindAll("svg g line").Count.ShouldBe(0); // no candle marks
        // The hover slot spans the plot, so the tooltip still gives the four values.
        cut.Find("svg g rect").GetAttribute("height").ShouldBe("326");
        cut.Markup.ShouldContain("Oct 1, 2026 09:00: open 40, high 70, low 30, close 55");
        string legend = cut.Find("svg + div").TextContent;
        legend.ShouldContain("Close");
        legend.ShouldNotContain("Rising");
        Spec(module, "candlestick").ShouldBe("{\"candles\":[{\"time\":\"2026-10-01T09:00:00\",\"open\":40,\"high\":70,\"low\":30,\"close\":55,\"volume\":null}],\"width\":640,\"height\":360,\"indicators\":[],\"showVolume\":false,\"style\":\"area\"}");
    }

    [Theory]
    [InlineData(CandlestickStyle.HeikinAshi, 1, 2, "heikinashi")]
    [InlineData(CandlestickStyle.CandleVolume, 1, 2, "candlevolume")]
    [InlineData(CandlestickStyle.EquiVolume, 0, 2, "equivolume")]
    public void CandlestickChart_draws_heikin_ashi_and_volume_width_styles(CandlestickStyle style, int lines, int rects, string name)
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        CandlestickLayout layout = new(
            [new("Oct 1, 2026 09:00", 40, 70, 30, 55, true, 300, 36, 200, 120, 40, 260)],
            [new(300, "09:00")], [], _plot);
        module.Setup<CandlestickLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<CandlestickChart> cut = Render<CandlestickChart>(ps => ps
            .Add(p => p.Candles, [new Candle(new DateTime(2026, 10, 1, 9, 0, 0), 40, 70, 30, 55) { Volume = 1200 }])
            .Add(p => p.CandleStyle, style));

        cut.WaitForAssertion(() => cut.FindAll("svg g rect").Count.ShouldBe(rects));
        cut.FindAll("svg g line").Count.ShouldBe(lines);
        cut.Find("svg g rect").GetAttribute("width").ShouldBe("36"); // the width the layout gave
        cut.Markup.ShouldContain(style == CandlestickStyle.HeikinAshi ? "09:00: Heikin-Ashi open 40" : "09:00: open 40");
        string spec = Spec(module, "candlestick");
        spec.ShouldContain($"\"style\":\"{name}\"");
        spec.ShouldContain("\"volume\":1200");
    }

    [Fact]
    public void CandlestickChart_names_the_price_channel()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        CandlestickLayout layout = new(
            [new("Oct 1, 2026 09:00", 40, 70, 30, 55, true, 300, 20, 200, 120, 40, 260)],
            [new(300, "09:00")], [], _plot)
        {
            Indicators = [new(0, "pricechannel", -1, "M290,150L310,140", "", "M290,100L310,90", "M290,200L310,190", "M290,100L310,90L310,190L290,200Z", [], [50])],
        };
        module.Setup<CandlestickLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<CandlestickChart> cut = Render<CandlestickChart>(ps => ps
            .Add(p => p.Candles, [new Candle(new DateTime(2026, 10, 1, 9, 0, 0), 40, 70, 30, 55)])
            .Add(p => p.Indicators, [new ChartIndicator(IndicatorKind.PriceChannel, Period: 12)]));

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Price channel 12 50"));
        cut.Find("path[d='M290,100L310,90L310,190L290,200Z']").GetAttribute("fill").ShouldBe("var(--color-primary)");
        cut.Find("svg + div").TextContent.ShouldContain("Price channel 12");
        Spec(module, "candlestick").ShouldContain("{\"kind\":\"pricechannel\",\"period\":12,");
    }

    [Fact]
    public void GanttChart_draws_bars_progress_milestones_and_today()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
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
    public void GanttChart_draws_lanes_group_colours_a_legend_and_dates_in_the_current_culture()
    {
        System.Globalization.CultureInfo before = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
            BunitJSModuleInterop module = this.SetupChartModule();
            // Two flights share stand A1 (same y), the second with its own colour; Oct 5 2026 is the tick.
            GanttLayout layout = new(
                [
                    new(false, "AF 1", "A1", 0, false, "x", "x", null, 100, 200, 20, 30, Index: 0),
                    new(false, "LH 1", "A1", 1, false, "x", "x", null, 300, 400, 20, 30, "red", Index: 1),
                ],
                ["AF", "LH"], null, [new(100, "Oct 5", new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc).Subtract(DateTime.UnixEpoch).TotalMilliseconds)], _plot);
            module.Setup<GanttLayout>("layout", _ => true).SetResult(layout);

            IRenderedComponent<GanttChart> cut = Render<GanttChart>(ps => ps
                .Add(p => p.Tasks,
                [
                    new GanttTask("AF 1", new DateTime(2026, 10, 5, 6, 0, 0), new DateTime(2026, 10, 5, 8, 0, 0), "AF", Row: "A1"),
                    new GanttTask("LH 1", new DateTime(2026, 10, 5, 9, 0, 0), new DateTime(2026, 10, 5, 10, 0, 0), "LH", Row: "A1", Color: "red"),
                ])
                .Add(p => p.ShowGroupHeaders, false)
                .Add(p => p.ShowLegend, true)
                .Add(p => p.GroupColors, new Dictionary<string, string> { ["AF"] = "blue" }));

            cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(2));
            cut.FindAll("svg rect").Select(r => r.GetAttribute("fill")).ShouldBe(["blue", "red"]);
            cut.FindAll("svg text").Count(t => t.TextContent == "A1").ShouldBe(1);
            cut.FindAll("svg text").ShouldContain(t => t.TextContent == "5 oct.");
            cut.Markup.ShouldContain("AF 1: 05/10/2026 06:00 – 05/10/2026 08:00");
            cut.FindAll(".flex-wrap span.inline-flex").Count.ShouldBe(2);

            string spec = Spec(module, "gantt");
            spec.ShouldContain("\"row\":\"A1\"");
            spec.ShouldContain("\"headers\":false");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = before;
        }
    }

    [Fact]
    public void BumpChart_draws_lines_points_and_end_labels()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
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
        BunitJSModuleInterop module = this.SetupChartModule();
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

    [Fact]
    public void RangeAreaChart_sends_step()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        module.Setup<RangeAreaLayout>("layout", _ => true).SetResult(new RangeAreaLayout("", "", [], [], [], _plot));

        Render<RangeAreaChart>(ps => ps
            .Add(p => p.Points, [new RangePoint("Mon", 1, 3), new RangePoint("Tue", 2, 4)])
            .Add(p => p.Step, true));

        Spec(module, "range-area").ShouldContain("\"smooth\":true,\"step\":true");
    }

    [Fact]
    public void HeatmapChart_levels_send_the_step_count_and_show_a_stepped_legend()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        HeatmapLayout layout = new(
            [new(100, "Mon", null, "Mon")], [new(200, "09", null, "09"), new(400, "10", null, "10")],
            [new("Mon", "09", 10, 0, 100, 20, 190, 160), new("Mon", "10", 30, 1, 300, 20, 190, 160)], 10, 30, _plot, [20]);
        module.Setup<HeatmapLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<HeatmapChart> cut = Render<HeatmapChart>(ps => ps
            .Add(p => p.Cells, [new HeatmapCell("Mon", "09", 10), new HeatmapCell("Mon", "10", 30)])
            .Add(p => p.Levels, 2));

        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(2));
        Spec(module, "heatmap").ShouldContain("\"levels\":2");
        cut.FindAll(".rounded-sm").Count.ShouldBe(2);
        cut.Markup.ShouldContain("10 – 20");
        cut.Markup.ShouldContain("20 – 30");
        cut.Markup.ShouldNotContain("linear-gradient");
    }
    [Fact]
    public void HeatmapChart_level_legend_keeps_small_steps_apart()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        HeatmapLayout small = new(
            [new(100, "A", null, "A")], [new(200, "c0", null, "c0"), new(400, "c1", null, "c1")],
            [new("A", "c0", 0.001, 0, 100, 20, 190, 160), new("A", "c1", 0.005, 1, 300, 20, 190, 160)], 0.001, 0.005, _plot,
            [0.002, 0.003, 0.004]);
        module.Setup<HeatmapLayout>("layout", _ => true).SetResult(small);

        IRenderedComponent<HeatmapChart> cut = Render<HeatmapChart>(ps => ps
            .Add(p => p.Cells, [new HeatmapCell("A", "c0", 0.001), new HeatmapCell("A", "c1", 0.005)])
            .Add(p => p.Levels, 4));

        cut.WaitForAssertion(() => cut.FindAll(".rounded-sm").Count.ShouldBe(4));
        cut.Markup.ShouldContain("0.001 – 0.002");
        cut.Markup.ShouldContain("0.004 – 0.005");
    }

    [Fact]
    public void HeatmapChart_level_legend_of_a_flat_domain_is_one_step_with_the_value()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        HeatmapLayout flat = new(
            [new(100, "A", null, "A")], [new(200, "c0", null, "c0")], [new("A", "c0", 5, 1, 100, 20, 190, 160)], 5, 5, _plot, []);
        module.Setup<HeatmapLayout>("layout", _ => true).SetResult(flat);

        IRenderedComponent<HeatmapChart> cut = Render<HeatmapChart>(ps => ps
            .Add(p => p.Cells, [new HeatmapCell("A", "c0", 5)])
            .Add(p => p.Levels, 4));

        cut.WaitForAssertion(() => cut.FindAll(".rounded-sm").Count.ShouldBe(1));
        cut.Find(".rounded-sm + span").TextContent.ShouldBe("5");
    }
    [Fact]
    public void CandlestickChart_names_the_close_price_with_CloseLabel()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        CandlestickLayout layout = new(
            [new("Oct 1, 2026 09:00", 40, 70, 30, 55, true, 300, 20, 200, 120, 40, 260)],
            [new(300, "09:00")], [], _plot)
        {
            PriceLine = "M290,150L310,140",
        };
        module.Setup<CandlestickLayout>("layout", _ => true).SetResult(layout);

        IRenderedComponent<CandlestickChart> cut = Render<CandlestickChart>(ps => ps
            .Add(p => p.Candles, [new Candle(new DateTime(2026, 10, 1, 9, 0, 0), 40, 70, 30, 55)])
            .Add(p => p.CandleStyle, CandlestickStyle.Line)
            .Add(p => p.CloseLabel, "Last price"));

        cut.WaitForAssertion(() => cut.Find("svg + div").TextContent.ShouldContain("Last price"));
        cut.Find("svg + div").TextContent.ShouldNotContain("Close");
    }
}
