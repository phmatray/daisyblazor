using AngleSharp.Dom;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Regression tests for the chart rendering fixes: nice axis ticks, aspect-ratio-safe text,
/// zero-crossing bars, gaps for non-finite values, accessible descriptions and tooltips.
/// </summary>
public class LibraryChartsFixTests : BunitContext
{
    private static List<ChartSeries> One(params double[] data) => new() { new ChartSeries("S", data) };

    [Fact]
    public void LineChart_rounds_the_axis_to_nice_ticks()
    {
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(12, 30, 44, 20)));

        string text = cut.Markup;
        foreach (string tick in new[] { ">0<", ">10<", ">20<", ">30<", ">40<", ">50<" })
        {
            text.ShouldContain(tick);
        }

        text.ShouldNotContain(">11<");
        text.ShouldNotContain(">44<");
    }

    [Fact]
    public void BarChart_formats_large_ticks_compactly()
    {
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("Free", 48200), new("Pro", 12000) }));

        cut.Markup.ShouldContain(">50k<");
        cut.Markup.ShouldNotContain("12050");
    }

    [Fact]
    public void Chart_text_is_not_inside_the_stretched_svg()
    {
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(1, 2, 3))
            .Add(p => p.Labels, new[] { "Jan", "Feb", "Mar" }));

        IElement svg = cut.Find("svg");
        svg.GetAttribute("preserveAspectRatio").ShouldBe("none");
        svg.QuerySelectorAll("text").Length.ShouldBe(0);
        cut.Markup.ShouldContain("Jan");
        foreach (IElement path in svg.QuerySelectorAll("path[stroke]"))
        {
            path.GetAttribute("vector-effect").ShouldBe("non-scaling-stroke");
        }
    }

    [Fact]
    public void Horizontal_BarChart_keeps_long_category_labels_whole()
    {
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Horizontal, true)
            .Add(p => p.Data, new List<ChartDataPoint> { new("North America", 10), new("Asia Pacific", 5) }));

        cut.FindAll("span").Select(s => s.TextContent).ShouldContain("North America");
    }

    [Fact]
    public void BarChart_draws_negative_bars_from_the_zero_line()
    {
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("Up", 10), new("Down", -10) }));

        // Axis -10..10: zero sits at y=50, both bars are half the plot tall.
        IReadOnlyList<IElement> rects = cut.FindAll("rect");
        rects.Count.ShouldBe(2);
        rects[0].GetAttribute("y").ShouldBe("0");
        rects[0].GetAttribute("height").ShouldBe("50");
        rects[1].GetAttribute("y").ShouldBe("50");
        rects[1].GetAttribute("height").ShouldBe("50");
    }

    [Fact]
    public void LineChart_with_negative_data_fills_to_zero()
    {
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(-20, -10, -40))
            .Add(p => p.Area, true));

        // Axis -40..0: zero is the top of the plot (y=0), so the area closes there.
        string area = cut.FindAll("path")[0].GetAttribute("d")!;
        area.ShouldEndWith(" 0 Z");
        cut.Markup.ShouldContain(">0<");
    }

    [Fact]
    public void LineChart_breaks_the_line_at_NaN_instead_of_emitting_it()
    {
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(1, 2, double.NaN, 3, 4)));

        cut.Markup.ShouldNotContain("NaN");
        string line = cut.Find("path").GetAttribute("d")!;
        line.Split('M', StringSplitOptions.RemoveEmptyEntries).Length.ShouldBe(2);
    }

    [Fact]
    public void Single_point_series_still_shows_a_marker_without_ShowPoints()
    {
        IRenderedComponent<AreaChart> cut = Render<AreaChart>(ps => ps
            .Add(p => p.Series, One(5)));

        cut.FindAll("span[title]").Count.ShouldBe(1);
    }

    [Fact]
    public void LineChart_has_a_slot_for_every_label()
    {
        IRenderedComponent<LineChart> cut = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(1, 2))
            .Add(p => p.Labels, new[] { "A", "B", "C", "D" }));

        // Four slots of 25%: the last label is centred at 87.5%, inside the plot.
        cut.Markup.ShouldContain("left:87.5%");
    }

    [Fact]
    public void Sparkline_flat_data_is_centred_and_NaN_leaves_a_gap()
    {
        IRenderedComponent<Sparkline> flat = Render<Sparkline>(ps => ps
            .Add(p => p.Data, new double[] { 3, 3, 3 })
            .Add(p => p.ShowEndDot, false));
        flat.Find("path").GetAttribute("d")!.ShouldContain(" 15");

        IRenderedComponent<Sparkline> gap = Render<Sparkline>(ps => ps
            .Add(p => p.Data, new double[] { 5, 6, double.NaN, 7, 8 })
            .Add(p => p.ShowEndDot, false));
        string d = gap.Find("path").GetAttribute("d")!;
        d.Split('M', StringSplitOptions.RemoveEmptyEntries).Length.ShouldBe(2);
        // No value was plotted at the bottom edge (y=28) as a fake zero except the true minimum.
        d.ShouldNotContain("NaN");
    }

    [Fact]
    public void Sparkline_dot_is_not_distorted_or_clipped()
    {
        IRenderedComponent<Sparkline> cut = Render<Sparkline>(ps => ps
            .Add(p => p.Data, new double[] { 1, 2, 3 }));

        cut.FindAll("circle").Count.ShouldBe(0);
        cut.Find("svg").GetAttribute("style")!.ShouldContain("overflow:visible");
        foreach (IElement path in cut.FindAll("path"))
        {
            path.GetAttribute("vector-effect").ShouldBe("non-scaling-stroke");
        }
    }

    [Fact]
    public void Pie_legend_uses_flex_gap_not_space_y()
    {
        IRenderedComponent<PieChart> cut = Render<PieChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("A", 1), new("B", 2) }));

        cut.Find("ul").ClassList.ShouldContain("gap-3");
        cut.Find("ul").ClassList.ShouldNotContain("space-y-1");
    }

    [Fact]
    public void PieChart_renders_CenterLabel_as_a_ring_and_describes_slices()
    {
        IRenderedComponent<PieChart> cut = Render<PieChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("Free", 30), new("Pro", 10) })
            .Add(p => p.InnerRadiusRatio, 0.5)
            .Add(p => p.CenterLabel, "40"));

        cut.Find("text").TextContent.ShouldBe("40");
        IElement desc = cut.Find("desc");
        cut.Find("svg").GetAttribute("aria-describedby").ShouldBe(desc.Id);
        desc.TextContent.ShouldContain("Free: 30 (75%)");
        cut.FindAll("path > title").Count.ShouldBe(2);
    }

    [Fact]
    public void Line_and_bar_charts_expose_values_and_tooltips()
    {
        IRenderedComponent<LineChart> line = Render<LineChart>(ps => ps
            .Add(p => p.Series, One(120, 140))
            .Add(p => p.Labels, new[] { "Jan", "Feb" }));
        line.Find("desc").TextContent.ShouldContain("Jan 120");
        line.FindAll("span[title]").Select(s => s.GetAttribute("title")).ShouldContain("Feb: 140");

        IRenderedComponent<BarChart> bar = Render<BarChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("Free", 48200) }));
        bar.Find("rect > title").TextContent.ShouldBe("Free: 48,200");
        bar.Find("desc").TextContent.ShouldContain("Free 48,200");
    }

    [Fact]
    public void Donut_center_label_shrinks_for_long_text_and_ratio_zero_is_a_pie()
    {
        IRenderedComponent<DonutChart> cut = Render<DonutChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("A", 1), new("B", 1) })
            .Add(p => p.InnerRadiusRatio, 0.35)
            .Add(p => p.CenterLabel, "€12,480"));
        double size = double.Parse(cut.Find("text").GetAttribute("font-size")!, System.Globalization.CultureInfo.InvariantCulture);
        size.ShouldBeLessThan(22);

        IRenderedComponent<DonutChart> pie = Render<DonutChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint> { new("A", 1), new("B", 1) })
            .Add(p => p.InnerRadiusRatio, 0));
        // A pie wedge starts at the centre.
        pie.Find("path").GetAttribute("d")!.ShouldStartWith("M 100 100");
    }

    [Fact]
    public void Horizontal_BarChart_without_grid_has_no_bottom_label_band()
    {
        IRenderedComponent<BarChart> cut = Render<BarChart>(ps => ps
            .Add(p => p.Horizontal, true)
            .Add(p => p.ShowGrid, false)
            .Add(p => p.Data, new List<ChartDataPoint> { new("A", 1) }));

        cut.Markup.ShouldNotContain("height:calc(1.2em + 6px)");
    }
}
