using AngleSharp.Dom;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Stacked mode of AreaChart/LineChart. The running totals, banded fills, missing-as-zero and the
/// axis max are computed by the line layout and asserted in tests/charts-core.test.mjs; these check
/// the spec, the band fills and that tooltips keep each series' own value.
/// </summary>
public class StackedAreaChartTests : BunitContext
{
    private static List<ChartSeries> Plans(double[] a, double[] b) => new()
    {
        new ChartSeries("A", a),
        new ChartSeries("B", b),
    };

    // B drawn on the running total A+B (50, 100) at the top of the plot.
    private static readonly LineLayout _stacked = ChartFakes.Line with
    {
        Series =
        [
            new("M181,139L483,93", "M181,139L483,93L483,231L181,231Z", [new(0, 181, 139, false), new(1, 483, 93, false)]),
            new("M181,116L483,8", "M181,116L483,8L483,93L181,139Z", [new(0, 181, 116, false), new(1, 483, 8, false)]),
        ],
    };

    private (IRenderedComponent<AreaChart> Cut, BunitJSModuleInterop Module) RenderArea(List<ChartSeries> series, bool? stacked)
    {
        BunitJSModuleInterop module = JSInterop.SetupCharts(line: _stacked);
        IRenderedComponent<AreaChart> cut = Render<AreaChart>(ps =>
        {
            ps.Add(p => p.Series, series).Add(p => p.ShowPoints, true);
            if (stacked is not null)
            {
                ps.Add(p => p.Stacked, stacked.Value);
            }
        });
        return (cut, module);
    }

    [Fact]
    public void Stacked_sends_the_flag_and_fills_each_band_near_opaque()
    {
        (IRenderedComponent<AreaChart> cut, BunitJSModuleInterop module) = RenderArea(Plans([20, 40], [30, 60]), stacked: true);

        module.Spec().ShouldBe("{\"series\":[[20,40],[30,60]],\"labels\":[],\"stacked\":true,\"smooth\":false,\"showGrid\":true}");
        string[] d = cut.FindAll("svg path").Select(p => p.GetAttribute("d")!).ToArray(); // area A, line A, area B, line B
        d[2].ShouldBe("M181,116L483,8L483,93L181,139Z");
        cut.Find("svg path").GetAttribute("fill-opacity").ShouldBe("0.7");
    }

    [Fact]
    public void Stacked_tooltips_show_each_series_own_value()
    {
        (IRenderedComponent<AreaChart> cut, _) = RenderArea(Plans([20, 40], [30, 60]), stacked: true);

        IElement marker = cut.FindAll("circle").Single(e => e.QuerySelector("title")!.TextContent == "B · 2: 60");
        marker.GetAttribute("cy").ShouldBe("8");
    }

    [Fact]
    public void Stacked_sends_missing_values_as_gaps_for_the_layout_to_count_as_zero()
    {
        (IRenderedComponent<AreaChart> cut, BunitJSModuleInterop module) = RenderArea(Plans([20, double.NaN], [40]), stacked: true);

        module.Spec().ShouldContain("\"series\":[[20,null],[40]]");
        cut.Markup.ShouldNotContain("NaN");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public void Unstacked_fills_stay_translucent(bool? stacked)
    {
        (IRenderedComponent<AreaChart> cut, BunitJSModuleInterop module) = RenderArea(Plans([20, 40], [30, 60]), stacked);

        module.Spec().ShouldContain("\"stacked\":false");
        cut.Find("svg path").GetAttribute("fill-opacity").ShouldBe("0.15");
    }
}
