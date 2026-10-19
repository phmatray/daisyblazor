using AngleSharp.Dom;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The variants of the core charts: column and win/loss sparklines with a trend line and a normal
/// range; pies with an "Other" slice, exploded slices and several rings. The geometry is tested in
/// tests/charts-core.test.mjs; these check the spec each sends and how the answer is drawn.
/// </summary>
public class ChartsCoreVariantsTests : BunitContext
{
    private static readonly ChartDataPoint[] _plans =
    [
        new("Free", 620), new("Pro", 300), new("Team", 50), new("Enterprise", 20), new("Education", 10),
    ];

    [Fact]
    public void Sparkline_line_keeps_its_original_spec()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts();
        Render<Sparkline>(ps => ps.Add(p => p.Data, new double[] { 1, 2 }));

        module.Spec().ShouldBe("{\"data\":[1,2],\"pad\":1.5}");
    }

    [Fact]
    public void Sparkline_column_sends_its_kind_and_colours_negative_bars()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(spark: ChartFakes.Spark with
        {
            Line = string.Empty, Area = string.Empty, End = null,
            Bars = [new(0, 2, 2, 15, 10, 3), new(1, 22, 12, 15, 5, -1)],
            Band = new(4, 8),
            Trend = new(10, 20, 70, 4),
        });
        IRenderedComponent<Sparkline> cut = Render<Sparkline>(ps => ps
            .Add(p => p.Data, new double[] { 3, -1 })
            .Add(p => p.Type, SparklineKind.Column)
            .Add(p => p.ShowTrendline, true)
            .Add(p => p.NormalMin, 0)
            .Add(p => p.NormalMax, 2)
            .Add(p => p.NegativeColor, "var(--color-warning)"));

        module.Spec().ShouldBe("{\"data\":[3,-1],\"pad\":1.5,\"kind\":\"column\",\"trend\":true,\"normal\":[0,2]}");
        IReadOnlyList<IElement> rects = cut.FindAll("rect");
        rects.Count.ShouldBe(3);
        rects[0].GetAttribute("fill-opacity").ShouldBe("0.08");
        rects[0].TextContent.ShouldBe("Normal range 0 to 2");
        rects[1].GetAttribute("fill").ShouldBe("var(--color-primary)");
        rects[2].GetAttribute("fill").ShouldBe("var(--color-warning)");
        cut.Find("line").GetAttribute("stroke-dasharray").ShouldBe("3 2");
        cut.FindAll("circle").Count.ShouldBe(0);
    }

    [Fact]
    public void Sparkline_win_loss_ignores_trend_and_range_and_counts_results()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(spark: ChartFakes.Spark with
        {
            Line = string.Empty, Area = string.Empty, End = null,
            Bars = [new(0, 2, 2, 15, 9, 1), new(1, 22, 13, 15, 9, -1)],
        });
        IRenderedComponent<Sparkline> cut = Render<Sparkline>(ps => ps
            .Add(p => p.Data, new double[] { 1, -1, 0 })
            .Add(p => p.Type, SparklineKind.WinLoss)
            .Add(p => p.ShowTrendline, true)
            .Add(p => p.NormalMin, 0)
            .Add(p => p.NormalMax, 1));

        module.Spec().ShouldBe("{\"data\":[1,-1,0],\"pad\":1.5,\"kind\":\"winloss\"}");
        cut.FindAll("rect")[1].GetAttribute("fill").ShouldBe("var(--color-error)");
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Win/loss sparkline");
        cut.Find("desc").TextContent.ShouldBe("1 up, 1 down, 1 even.");
    }

    [Fact]
    public void Pie_merges_small_slices_into_Other_and_lists_them_in_its_tooltip()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(pie: ChartFakes.Pie with
        {
            Slices = [new(0, "M0,0Z", 62, 0, 1), new(1, "M0,0Z", 30, 1, 2), new(2, "M0,0Z", 8, 2, 3)],
        });
        IRenderedComponent<PieChart> cut = Render<PieChart>(ps => ps
            .Add(p => p.Data, _plans)
            .Add(p => p.OthersThreshold, 0.06)
            .Add(p => p.OthersLabel, "Other plans"));

        module.Spec("layout").ShouldBe("{\"values\":[620,300,80],\"size\":200,\"innerRatio\":0,\"centerLabel\":\"\"}");
        cut.FindAll("path title")[2].TextContent.ShouldBe("Other plans: 80 (8%): Team 50, Enterprise 20, Education 10");
        cut.FindAll("li")[2].TextContent.ShouldContain("Other plans");
    }

    [Fact]
    public void Pie_keeps_a_lone_small_slice_and_sends_the_exploded_indices()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(pie: ChartFakes.Pie with
        {
            Slices = [new(0, "M0,0Z", 70, 0, 4.4, 5, -6), new(1, "M0,0Z", 30, 4.4, 6.28)],
        });
        IRenderedComponent<DonutChart> cut = Render<DonutChart>(ps => ps
            .Add(p => p.Data, [new ChartDataPoint("Free", 70), new ChartDataPoint("Pro", 3)])
            .Add(p => p.OthersThreshold, 0.1)
            .Add(p => p.Exploded, ["Pro"])
            .Add(p => p.ExplodeOffset, 0.12));

        module.Spec("layout").ShouldBe("{\"values\":[70,3],\"size\":200,\"innerRatio\":0.6,\"centerLabel\":\"\",\"explode\":[1],\"explodeOffset\":0.12}");
        IReadOnlyList<IElement> paths = cut.FindAll("path");
        paths[0].GetAttribute("transform").ShouldBe("translate(5,-6)");
        paths[1].HasAttribute("transform").ShouldBeFalse();
    }

    [Fact]
    public void Donut_rings_share_one_colour_per_label_and_name_the_ring_in_tooltips()
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(pie: ChartFakes.Pie with
        {
            Slices = [new(0, "M0,0Z", 60, 0, 3), new(1, "M0,0Z", 40, 3, 6.28), new(0, "M0,0Z", 50, 0, 3, Ring: 1), new(1, "M0,0Z", 50, 3, 6.28, Ring: 1)],
        });
        DonutRing[] rings =
        [
            new("2026", [new("Free", 60), new("Pro", 40)]),
            new("2025", [new("Pro", 50), new("Free", 50)]),
        ];
        IRenderedComponent<DonutChart> cut = Render<DonutChart>(ps => ps.Add(p => p.Rings, rings));

        module.Spec("layout").ShouldBe("{\"values\":[],\"size\":200,\"innerRatio\":0.6,\"centerLabel\":\"\",\"rings\":[[60,40],[50,50]]}");
        IReadOnlyList<IElement> paths = cut.FindAll("path");
        paths[0].GetAttribute("fill").ShouldBe("var(--color-primary)");
        paths[2].GetAttribute("fill").ShouldBe("var(--color-secondary)", "Pro keeps its colour in the inner ring");
        paths[3].GetAttribute("fill").ShouldBe("var(--color-primary)");
        paths[2].TextContent.ShouldBe("2025, Pro: 50 (50%)");
        cut.FindAll("li").Count.ShouldBe(2);
    }
    [Fact]
    public void Donut_forwards_ShowPercent_to_the_pie()
    {
        JSInterop.SetupCharts(pie: ChartFakes.Pie with { Slices = [new(0, "M0,0Z", 70, 0, 4.4), new(1, "M0,0Z", 30, 4.4, 6.28)] });
        IRenderedComponent<DonutChart> cut = Render<DonutChart>(ps => ps
            .Add(p => p.Data, [new ChartDataPoint("Free", 70), new ChartDataPoint("Pro", 30)])
            .Add(p => p.ShowPercent, false));

        cut.FindComponent<PieChart>().Instance.ShowPercent.ShouldBeFalse();
        cut.FindAll("path")[0].TextContent.ShouldBe("Free: 70");
    }
}
