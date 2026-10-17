using AngleSharp.Dom;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>Stacked mode of AreaChart/LineChart: running totals, banded fills and the axis max.</summary>
public class StackedAreaChartTests : BunitContext
{
    private static List<ChartSeries> Plans(double[] a, double[] b) => new()
    {
        new ChartSeries("A", a),
        new ChartSeries("B", b),
    };

    private IRenderedComponent<AreaChart> RenderArea(List<ChartSeries> series, bool? stacked) =>
        Render<AreaChart>(ps =>
        {
            ps.Add(p => p.Series, series).Add(p => p.ShowPoints, true);
            if (stacked is not null)
            {
                ps.Add(p => p.Stacked, stacked.Value);
            }
        });

    private static string[] Paths(IRenderedComponent<AreaChart> cut) =>
        cut.FindAll("svg path").Select(p => p.GetAttribute("d")!).ToArray();

    [Fact]
    public void Stacked_draws_each_series_on_the_running_total()
    {
        // Totals: A = 20, 40; A+B = 50, 100 -> nice axis 0..100. Two slots: x = 25 and 75.
        IRenderedComponent<AreaChart> cut = RenderArea(Plans([20, 40], [30, 60]), stacked: true);

        string[] d = Paths(cut); // area A, line A, area B, line B
        d[0].ShouldBe("M 25 80 L 75 60 L 75 100 L 25 100 Z");
        d[1].ShouldBe("M 25 80 L 75 60");
        d[2].ShouldBe("M 25 50 L 75 0 L 75 60 L 25 80 Z");
        d[3].ShouldBe("M 25 50 L 75 0");
        cut.Markup.ShouldContain(">100<");
    }

    [Fact]
    public void Stacked_tooltips_show_each_series_own_value()
    {
        IRenderedComponent<AreaChart> cut = RenderArea(Plans([20, 40], [30, 60]), stacked: true);

        IElement marker = cut.FindAll("span[title]").Single(e => e.GetAttribute("title") == "B · 2: 60");
        marker.GetAttribute("style")!.ShouldContain("top:0%");
    }

    [Fact]
    public void Stacked_treats_missing_values_as_zero()
    {
        // A is NaN at index 1 and B is short: totals A = 20, 0; A+B = 60, 0 (axis 0..60).
        IRenderedComponent<AreaChart> cut = RenderArea(Plans([20, double.NaN], [40]), stacked: true);

        cut.Markup.ShouldNotContain("NaN");
        Paths(cut)[3].ShouldBe("M 25 0 L 75 100");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public void Unstacked_output_is_unchanged(bool? stacked)
    {
        // Axis follows the largest single value (60), and every fill closes at zero.
        IRenderedComponent<AreaChart> cut = RenderArea(Plans([20, 40], [30, 60]), stacked);

        string[] d = Paths(cut);
        d[2].ShouldBe("M 25 50 L 75 0 L 75 100 L 25 100 Z");
        d[3].ShouldBe("M 25 50 L 75 0");
        cut.Markup.ShouldNotContain(">100<");
        cut.Find("svg path").GetAttribute("fill-opacity").ShouldBe("0.15");
    }
}
