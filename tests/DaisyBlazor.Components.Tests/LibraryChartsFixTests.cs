using AngleSharp.Dom;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Regression tests for the chart rendering fixes: nice axis ticks, true-size text, zero-crossing
/// bars, gaps for non-finite values, accessible descriptions and tooltips. The geometry moved to the
/// D3 layouts, so the numbers (tick values, compact format, gap and zero-line paths, slots, label
/// fitting) are asserted in tests/charts-core.test.mjs; these check what the components send to the
/// layouts and how they draw the answer.
/// </summary>
public class LibraryChartsFixTests : BunitContext
{
    private static List<ChartSeries> One(params double[] data) => new() { new ChartSeries("S", data) };

    [Fact]
    public void LineChart_draws_the_nice_ticks_of_its_layout()
    {
        // charts-core.test.mjs: 12, 30, 44, 20 gives the ticks 0, 10, … 50.
        BunitJSModuleInterop module = JSInterop.SetupCharts(line: ChartFakes.Line with
        {
            YTicks = [.. new[] { "0", "10", "20", "30", "40", "50" }.Select((t, i) => new AxisTick(231 - i * 44.6, t))],
        });
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(12, 30, 44, 20)));

        foreach (string tick in new[] { ">0<", ">10<", ">20<", ">30<", ">40<", ">50<" })
        {
            cut.Markup.ShouldContain(tick);
        }

        cut.FindAll("svg > line").Count.ShouldBe(6);
        module.Invocations["observe"].Single().Arguments[1].ShouldBe("line");
        module.Spec().ShouldContain("\"series\":[[12,30,44,20]]");
    }

    [Fact]
    public void BarChart_draws_compact_ticks_from_its_layout()
    {
        // charts-core.test.mjs: 48200 gives a top tick of "50k".
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("Free", 48200), new("Pro", 12000) }));

        cut.Markup.ShouldContain(">50k<");
        cut.Markup.ShouldNotContain("12050");
        module.Spec().ShouldContain("\"categories\":[\"Free\",\"Pro\"],\"series\":[[48200,12000]]");
    }

    [Fact]
    public void Chart_text_is_drawn_at_the_measured_size_not_stretched()
    {
        JSInterop.SetupCharts();
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(1, 2, 3))
            .Add(p => p.Labels, new[] { "Jan", "Feb", "Mar" }));

        // The viewBox is the box's own pixel size, so a unit is a CSS pixel and text keeps its size.
        IElement svg = cut.Find("svg");
        svg.GetAttribute("viewBox").ShouldBe("0 0 640 256");
        svg.HasAttribute("preserveAspectRatio").ShouldBeFalse();
        svg.QuerySelectorAll("text").Select(t => t.TextContent).ShouldContain("Jan");
        svg.QuerySelector("text")!.GetAttribute("font-size").ShouldBe("11");
    }

    [Fact]
    public void Measured_charts_keep_their_CSS_height_while_loading()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ChartFakes.ModulePath);
        JSRuntimeInvocationHandler<LineLayout> observe = module.Setup<LineLayout>("observe", _ => true);
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(1, 2))
            .Add(p => p.Height, "11rem"));

        cut.Find("[aria-busy=true]").ParentElement!.GetAttribute("style")!.ShouldContain("height:11rem");
        cut.FindAll("svg").Count.ShouldBe(0);

        observe.SetResult(ChartFakes.Line);
        cut.WaitForAssertion(() => cut.Find("svg").ParentElement!.GetAttribute("style")!.ShouldContain("height:11rem"));
    }

    [Fact]
    public void Measured_charts_redraw_on_resize_and_update_on_new_data()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        module.Setup<LineLayout>("update", _ => true).SetResult(ChartFakes.Line with { Width = 500 });
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps.Add(p => p.Series, One(1, 2)));

        // The module calls back after a resize.
        cut.InvokeAsync(() => cut.Instance.OnMeasuredLayout(ChartFakes.Line with { Width = 320, Height = 200 }));
        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 320 200"));

        // New inputs go through update() on the observed box, not a second observer.
        cut.Render(ps => ps.Add(p => p.Series, One(3, 4)));
        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 500 256"));
        module.Invocations["observe"].Count.ShouldBe(1);
        module.Spec("update").ShouldContain("[[3,4]]");

        // A title change does not lay out again.
        cut.Render(ps => ps.Add(p => p.Title, "Signups"));
        module.Invocations["update"].Count.ShouldBe(1);
    }

    [Fact]
    public async Task Measured_charts_stop_observing_when_disposed()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        Render<Sparkline>(ps => ps.Add(p => p.Data, new double[] { 1, 2 }));

        await DisposeComponentsAsync();

        module.Invocations["unobserve"].Count.ShouldBe(1);
    }

    [Fact]
    public void Horizontal_BarChart_keeps_long_category_labels_whole()
    {
        // charts-core.test.mjs: the gutter fits "North America"; only names wider than 40% of the box are cut.
        BunitJSModuleInterop module = JSInterop.SetupCharts(bar: ChartFakes.Bar with
        {
            XTicks = [new(320, "0", 0), new(600, "10", 10)],
            YTicks = [new(60, "North America", null, "North America"), new(180, "A very long reg…", null, "A very long region name")],
        });
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Horizontal, true)
            .Add(p => p.Data, new List<ChartDataPoint> { new("North America", 10), new("A very long region name", 5) }));

        cut.FindAll("text").Select(s => s.TextContent).ShouldContain("North America");
        cut.FindAll("g > title").Select(t => t.TextContent).ShouldBe(["A very long region name"]);
        module.Spec().ShouldContain("\"horizontal\":true");
    }

    [Fact]
    public void BarChart_draws_the_bars_of_its_layout()
    {
        // charts-core.test.mjs: a negative bar runs down from the zero line.
        JSInterop.SetupCharts(bar: ChartFakes.Bar with
        {
            Bars = [new(0, 0, 100, 8, 160, 111.5), new(1, 0, 400, 119.5, 160, 111.5)],
        });
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("Up", 10), new("Down", -10) }));

        IReadOnlyList<IElement> rects = cut.FindAll("rect");
        rects.Count.ShouldBe(2);
        rects[1].GetAttribute("y").ShouldBe("119.5");
        rects[1].GetAttribute("height").ShouldBe("111.5");
        rects[1].GetAttribute("fill").ShouldBe("var(--color-secondary)");
    }

    [Fact]
    public void LineChart_sends_NaN_as_a_gap_and_never_emits_it()
    {
        // charts-core.test.mjs: a null breaks the line into separate runs.
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(1, 2, double.NaN, 3, double.PositiveInfinity)));

        module.Spec().ShouldContain("[[1,2,null,3,null]]");
        cut.Markup.ShouldNotContain("NaN");
        cut.Find("desc").TextContent.ShouldContain("3 no data");
    }

    [Fact]
    public void Isolated_points_still_get_a_marker_without_ShowPoints()
    {
        JSInterop.SetupCharts(line: ChartFakes.Line with
        {
            Series = [new("M181,100Z", "", [new(0, 181, 100, true)]), new("M181,50L483,40", "", [new(0, 181, 50, false), new(1, 483, 40, false)])],
        });
        IRenderedComponent<AreaChart> cut = Render<AreaChart>(ps => ps
            .Add(p => p.Series, new List<ChartSeries> { new("A", [5]), new("B", [1, 2]) }));

        cut.FindAll("circle").Count.ShouldBe(1);
        cut.Find("circle > title").TextContent.ShouldBe("A · 1: 5");
    }

    [Fact]
    public void Sparkline_sends_NaN_as_a_gap()
    {
        // charts-core.test.mjs: flat data runs through the middle; null leaves a gap.
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        Render<Sparkline>(ps => ps
            .Add(p => p.Data, new double[] { 5, 6, double.NaN, 7, 8 })
            .Add(p => p.StrokeWidth, 3));

        module.Spec().ShouldBe("{\"data\":[5,6,null,7,8],\"pad\":2}");
    }

    [Fact]
    public void Sparkline_sits_inline_with_a_round_end_dot()
    {
        JSInterop.SetupCharts();
        IRenderedComponent<Sparkline> cut = Render<Sparkline>(ps => ps
            .Add(p => p.Data, new double[] { 1, 2, 3 })
            .Add(p => p.Color, "var(--color-success)"));

        IElement box = cut.Find("span");
        box.ClassList.ShouldContain("inline-block");
        box.GetAttribute("style")!.ShouldBe("overflow:visible;width:5rem;height:1.5rem;");
        cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 80 24");
        IElement dot = cut.Find("circle");
        dot.GetAttribute("cx").ShouldBe("78");
        dot.GetAttribute("r").ShouldBe("3");
        dot.GetAttribute("fill").ShouldBe("var(--color-success)");
        cut.Find("desc").TextContent.ShouldBe("3 values from 1 to 3; low 1, high 3.");

        IRenderedComponent<Sparkline> plain = Render<Sparkline>(ps => ps
            .Add(p => p.Data, new double[] { 1, 2, 3 })
            .Add(p => p.ShowEndDot, false)
            .Add(p => p.Area, true));
        plain.FindAll("circle").Count.ShouldBe(0);
        plain.FindAll("path").Count.ShouldBe(2);
    }

    [Fact]
    public void Pie_legend_uses_flex_gap_not_space_y()
    {
        JSInterop.SetupCharts();
        IRenderedComponent<PieChart> cut = Render<PieChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("A", 1), new("B", 2) }));

        cut.Find("ul").ClassList.ShouldContain("gap-3");
        cut.Find("ul").ClassList.ShouldNotContain("space-y-1");
        cut.Find("ul").TextContent.ShouldContain("70%");
    }

    [Fact]
    public void PieChart_renders_CenterLabel_as_a_ring_and_describes_slices()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(pie: ChartFakes.Pie with
        {
            InnerRadius = 47,
            CenterFontSize = 22,
            Slices = [new(0, "M0,-94A94,94,0,1,1,-94,0L-47,0A47,47,0,1,0,0,-47Z", 75, 0, 4.71), new(1, "M-94,0A94,94,0,0,1,0,-94L0,-47A47,47,0,0,0,-47,0Z", 25, 4.71, 6.28)],
        });
        IRenderedComponent<PieChart> cut = Render<PieChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("Free", 30), new("Pro", 10) })
            .Add(p => p.InnerRadiusRatio, 0.5)
            .Add(p => p.CenterLabel, "40"));

        cut.Find("text").TextContent.ShouldBe("40");
        IElement desc = cut.Find("desc");
        cut.Find("svg").GetAttribute("aria-describedby").ShouldBe(desc.Id);
        desc.TextContent.ShouldContain("Free: 30 (75%)");
        cut.FindAll("path > title").Count.ShouldBe(2);
        module.Spec("layout").ShouldBe("{\"values\":[30,10],\"size\":200,\"innerRatio\":0.5,\"centerLabel\":\"40\"}");
    }

    [Fact]
    public void Line_and_bar_charts_expose_values_and_tooltips()
    {
        JSInterop.SetupCharts();
        IRenderedComponent<LineChart> line = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(120, 140))
            .Add(p => p.Labels, new[] { "Jan", "Feb" }));
        line.Find("desc").TextContent.ShouldContain("Jan 120");
        line.FindAll("circle > title").Select(s => s.TextContent).ShouldContain("Feb: 140");

        IRenderedComponent<BarChart> bar = Render<BarChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("Free", 48200) }));
        bar.Find("rect > title").TextContent.ShouldBe("Free: 48,200");
        bar.Find("desc").TextContent.ShouldContain("Free 48,200");
    }

    [Fact]
    public void Donut_sends_its_hole_and_label_and_uses_the_fitted_font_size()
    {
        // charts-core.test.mjs: a long label in a thick ring shrinks below 22; ratio 0 draws wedges from the centre.
        BunitJSModuleInterop module = JSInterop.SetupCharts(pie: ChartFakes.Pie with { InnerRadius = 32.9, CenterFontSize = 13.3 });
        IRenderedComponent<DonutChart> cut = Render<DonutChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("A", 1), new("B", 1) })
            .Add(p => p.InnerRadiusRatio, 0.35)
            .Add(p => p.CenterLabel, "€12,480"));

        cut.Find("text").GetAttribute("font-size").ShouldBe("13.3");
        module.Spec("layout").ShouldContain("\"innerRatio\":0.35,\"centerLabel\":\"\\u20AC12,480\"");
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Donut chart");
    }

    [Fact]
    public void Pie_without_positive_values_draws_an_empty_outline()
    {
        JSInterop.SetupCharts(pie: ChartFakes.Pie with { Slices = [] });
        IRenderedComponent<PieChart> cut = Render<PieChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("A", 0), new("B", double.NaN) }));

        cut.FindAll("circle").Count.ShouldBe(1);
        cut.Find("desc").TextContent.ShouldBe("No data.");
        cut.FindAll("ul").Count.ShouldBe(0);
    }

    [Fact]
    public void BarChart_sends_stacked_percent_and_stack_groups_and_tooltips_show_the_share()
    {
        // charts-core.test.mjs: stacks side by side, each to 100%.
        BunitJSModuleInterop module = JSInterop.SetupCharts(bar: ChartFakes.Bar with
        {
            Bars = [new(0, 0, 100, 8, 80, 111.5), new(0, 1, 100, 119.5, 80, 111.5), new(0, 2, 180, 8, 80, 223)],
        });
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Series, new List<ChartSeries>
            {
                new("Pro", [10]) { Stack = "EU" },
                new("Team", [30]) { Stack = "EU" },
                new("Pro", [20]) { Stack = "US" },
            })
            .Add(p => p.Labels, new[] { "Q1" })
            .Add(p => p.Percent, true));

        module.Spec().ShouldContain("\"stacked\":false,\"percent\":true,\"stacks\":[0,0,1]");
        cut.FindAll("rect title").Select(t => t.TextContent).ShouldBe(["Pro · Q1: 10 (25%)", "Team · Q1: 30 (75%)", "Pro · Q1: 20 (100%)"]);
    }

    [Fact]
    public void BarChart_stacked_keeps_each_series_own_value_in_tooltips()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(bar: ChartFakes.Bar with
        {
            Bars = [new(0, 0, 100, 100, 160, 131), new(0, 1, 100, 50, 160, 50)],
        });
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Series, new List<ChartSeries> { new("Pro", [12]), new("Team", [-4]) })
            .Add(p => p.Labels, new[] { "Q1" })
            .Add(p => p.Stacked, true));

        module.Spec().ShouldContain("\"stacked\":true,\"percent\":false,\"stacks\":[0,0]");
        cut.FindAll("rect title").Select(t => t.TextContent).ShouldBe(["Pro · Q1: 12", "Team · Q1: -4"]);
    }

    [Fact]
    public void LineChart_sends_step_and_percent_and_tooltips_show_the_share()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, new List<ChartSeries> { new("Pro", [30, 10]), new("Team", [10, 10]) })
            .Add(p => p.Labels, new[] { "Jan", "Feb" })
            .Add(p => p.Percent, true)
            .Add(p => p.Step, true));

        module.Spec().ShouldContain("\"stacked\":false,\"percent\":true,\"smooth\":false,\"step\":true");
        cut.FindAll("circle title")[0].TextContent.ShouldBe("Pro · Jan: 30 (75%)");
    }

    [Fact]
    public void AreaChart_forwards_step_and_percent_and_fills_like_a_stack()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        IRenderedComponent<AreaChart> cut = Render<AreaChart>(ps => ps
            .Add(p => p.Series, One(1, 2))
            .Add(p => p.Percent, true)
            .Add(p => p.Step, true));

        module.Spec().ShouldContain("\"percent\":true,\"smooth\":false,\"step\":true");
        cut.Find("path[fill-opacity]").GetAttribute("fill-opacity").ShouldBe("0.7");
    }
}
