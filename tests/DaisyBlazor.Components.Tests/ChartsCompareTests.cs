using System.Text.Json;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The "compare" D3 charts: each shows a placeholder, sends its data to its layout, draws the layout
/// it gets back, and copes with empty or non-finite input.
/// </summary>
public class ChartsCompareTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly JsonSerializerOptions _web = new(JsonSerializerDefaults.Web);

    private static readonly PlotArea _plot = new(80, 4, 630, 334);
    private static readonly AxisTick[] _ticks = [new(80, "0", 0), new(355, "50", 50)];

    private (BunitJSModuleInterop Module, JSRuntimeInvocationHandler<T> Call) Setup<T>()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        return (module, module.Setup<T>("layout", _ => true));
    }

    private static (string Kind, string Spec) Sent(BunitJSModuleInterop module)
    {
        JSRuntimeInvocation invocation = module.Invocations["layout"].Single();
        return ((string)invocation.Arguments[0]!, JsonSerializer.Serialize(invocation.Arguments[1], _web));
    }

    [Fact]
    public void Radar_draws_rings_spokes_and_series()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<RadarLayout> call) = Setup<RadarLayout>();
        IRenderedComponent<RadarChart> cut = Render<RadarChart>(ps => ps
            .Add(p => p.Labels, ["Seats", "Storage", "Support"])
            .Add(p => p.Series, [new ChartSeries("Team", [5, double.NaN, 3]), new ChartSeries("Scale", [9, 8, 7])])
            .Add(p => p.Grid, RadarGrid.Circle));

        cut.Find("[aria-busy=true]").ShouldNotBeNull();
        (string kind, string spec) = Sent(module);
        kind.ShouldBe("radar");
        spec.ShouldContain("\"axes\":[\"Seats\",\"Storage\",\"Support\"]");
        spec.ShouldContain("[5,null,3]");
        spec.ShouldContain("\"grid\":\"circle\"");
        spec.ShouldContain("\"height\":480");

        call.SetResult(new RadarLayout(320, 240, 150,
            [new("Seats", 320, 90, 320, 80, "middle"), new("Storage", 450, 315, 465, 330, "start"), new("Support", 190, 315, 175, 330, "end")],
            [new(5, "5", 75, "M245,240Z"), new(10, "10", 150, "M170,240Z")],
            [
                new("M320,165Z", [new(320, 165, 5), new(320, 240, null), new(255, 277, 3)]),
                new("M320,105Z", [new(320, 105, 9), new(424, 300, 8), new(229, 292, 7)]),
            ]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle").Count.ShouldBe(5));
        cut.FindAll("svg line").Count.ShouldBe(3);
        cut.Markup.ShouldContain("Team, Seats: 5");
        cut.Markup.ShouldNotContain("Team, Storage");
        cut.Markup.ShouldContain("Storage no value");
        cut.Markup.ShouldContain("var(--color-secondary)");
        cut.FindAll(".flex-wrap span.inline-flex").Count.ShouldBe(2);
    }

    [Fact]
    public void Bullet_draws_bands_bar_and_target()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<BulletLayout> call) = Setup<BulletLayout>();
        IRenderedComponent<BulletChart> cut = Render<BulletChart>(ps => ps
            .Add(p => p.Items, [new BulletItem("Active seats", 820, 900, [500, 750, 1000]), new BulletItem("Broken", double.NaN)])
            .Add(p => p.Horizontal, false));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("bullet");
        spec.ShouldContain("\"ranges\":[500,750,1000]");
        spec.ShouldContain("\"value\":null");
        spec.ShouldContain("\"vertical\":true");

        call.SetResult(new BulletLayout([
            new("Active seats", 820, 900, new(180, 10, 220, 310),
                [new(180, 10, 40, 100, 0), new(180, 110, 40, 100, 1), new(180, 210, 40, 100, 2)],
                new(193, 40, 13, 294), new(186, 30, 214, 30), [new(200, "Active seats")], [new(334, "0", 0)]),
        ]));

        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(4));
        cut.Markup.ShouldContain("Active seats: 820 (target 900)");
        cut.Markup.ShouldContain("fill-opacity=\"0.2\"");
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Bullet chart");
        cut.Find("desc").TextContent.ShouldBe("Active seats: 820 of a 900 target");
    }

    [Fact]
    public void Dumbbell_draws_two_dots_per_row_and_a_legend()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<DumbbellLayout> call) = Setup<DumbbellLayout>();
        IRenderedComponent<DumbbellChart> cut = Render<DumbbellChart>(ps => ps
            .Add(p => p.Items, [new DumbbellItem("Europe", 240, 180)])
            .Add(p => p.StartLabel, "Before")
            .Add(p => p.EndLabel, "After")
            .Add(p => p.ValueFormat, "{0:N0} ms"));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("dumbbell");
        spec.ShouldContain("\"startText\":\"240 ms\"");

        call.SetResult(new DumbbellLayout(_plot, _ticks, [new(166, "Europe")], 334, 80, 0,
            [new("Europe", 240, 180, 500, 300, 166, new("240 ms", 508, 170, "start"), new("180 ms", 292, 170, "end"))]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle").Count.ShouldBe(2));
        cut.Markup.ShouldContain("Europe: Before 240 ms, After 180 ms");
        cut.Markup.ShouldContain(">240 ms</text>");
        cut.FindAll(".flex-wrap span.inline-flex").Count.ShouldBe(2);
    }

    [Fact]
    public void Slope_draws_lines_with_labels_at_both_ends()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<SlopeLayout> call) = Setup<SlopeLayout>();
        IRenderedComponent<SlopeChart> cut = Render<SlopeChart>(ps => ps
            .Add(p => p.Items, [new SlopeItem("Support", 31, 52), new SlopeItem("Platform", 44, 39)])
            .Add(p => p.StartLabel, "Q1")
            .Add(p => p.EndLabel, "Q4")
            .Add(p => p.ColorByChange, true));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("slope");
        spec.ShouldContain("\"startLabel\":\"Q1\"");

        call.SetResult(new SlopeLayout([new("Q1", 120, 16, "middle"), new("Q4", 520, 16, "middle")], 120, 520, 36, 470,
        [
            new("Support", 31, 52, 120, 300, 520, 100, new("Support 31", 110, 304, "end"), new("52 Support", 530, 104, "start")),
            new("Platform", 44, 39, 120, 200, 520, 250, new("Platform 44", 110, 204, "end"), new("39 Platform", 530, 254, "start")),
        ]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle").Count.ShouldBe(4));
        cut.Markup.ShouldContain(">Support 31</text>");
        cut.Markup.ShouldContain(">39 Platform</text>");
        cut.Markup.ShouldContain(">Q4</text>");
        cut.Markup.ShouldContain("var(--color-success)");
        cut.Markup.ShouldContain("var(--color-error)");
    }

    [Fact]
    public void Lollipop_draws_stems_and_dots_in_the_point_colour()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<LollipopLayout> call) = Setup<LollipopLayout>();
        IRenderedComponent<LollipopChart> cut = Render<LollipopChart>(ps => ps
            .Add(p => p.Data, [new ChartDataPoint("Support", 52), new ChartDataPoint("Billing", 41, "var(--color-warning)")])
            .Add(p => p.Sort, LollipopSort.Ascending));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("lollipop");
        spec.ShouldContain("\"sort\":\"ascending\"");
        spec.ShouldContain("\"text\":\"52\"");

        call.SetResult(new LollipopLayout(_plot, _ticks, [new(96, "Billing"), new(246, "Support")], 334, 80, 0,
        [
            new("Billing", 41, 1, 80, 96, 400, 96, new("41", 408, 100, "start")),
            new("Support", 52, 0, 80, 246, 500, 246, new("52", 508, 250, "start")),
        ]));

        cut.WaitForAssertion(() => cut.FindAll("svg circle").Count.ShouldBe(2));
        cut.Find("svg circle").GetAttribute("fill").ShouldBe("var(--color-warning)");
        cut.Markup.ShouldContain("Support: 52");
        cut.FindAll("svg g[transform]").Count.ShouldBe(0);
        cut.FindAll("svg text").Count.ShouldBe(6);
    }

    [Fact]
    public void Parallel_draws_an_axis_per_dimension_and_a_line_per_item()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ParallelLayout> call) = Setup<ParallelLayout>();
        IRenderedComponent<ParallelCoordinatesChart> cut = Render<ParallelCoordinatesChart>(ps => ps
            .Add(p => p.Dimensions, ["Seats", "Storage"])
            .Add(p => p.Items, [new ParallelItem("Starter", [5, double.PositiveInfinity]), new ParallelItem("Team", [25, 100])])
            .Add(p => p.Highlight, "Team"));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("parallel");
        spec.ShouldContain("\"values\":[5,null]");

        call.SetResult(new ParallelLayout(
            [
                new("Seats", 40, 30, 352, new("Seats", 40, 16, "middle"), [new(352, "0", 0)]),
                new("Storage", 600, 30, 352, new("Storage", 600, 16, "middle"), [new(30, "100", 100)]),
            ],
            [new("Starter", 0, [5, null], "M40,300"), new("Team", 1, [25, 100], "M40,200L600,30")]));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(2));
        IReadOnlyList<AngleSharp.Dom.IElement> paths = cut.FindAll("svg path");
        paths[0].GetAttribute("stroke-opacity").ShouldBe("0.35");
        paths[1].GetAttribute("d").ShouldBe("M40,200L600,30");
        cut.Markup.ShouldContain("Starter: Seats 5, Storage no value");
        cut.FindAll(".flex-wrap span.inline-flex").Count.ShouldBe(2);
    }

    [Fact]
    public void RangeBar_draws_a_bar_per_item_coloured_by_group()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<RangeBarLayout> call) = Setup<RangeBarLayout>();
        IRenderedComponent<RangeBarChart> cut = Render<RangeBarChart>(ps => ps
            .Add(p => p.Items, [new RangeBarItem("Europe", 120, 410, "Before"), new RangeBarItem("Europe", 95, 300, "After")])
            .Add(p => p.GroupColors, new Dictionary<string, string> { ["After"] = "var(--color-success)" }));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("rangebar");
        spec.ShouldContain("\"group\":\"Before\"");

        call.SetResult(new RangeBarLayout(_plot, _ticks, [new(166, "Europe")], 334, 80, 0, ["Before", "After"],
        [
            new("Europe", "Before", 0, 120, 410, 150, 60, 300, 100, null, null),
            new("Europe", "After", 1, 95, 300, 120, 170, 200, 100, null, null),
        ]));

        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(2));
        cut.FindAll("svg rect")[0].GetAttribute("fill").ShouldBe("var(--color-primary)");
        cut.FindAll("svg rect")[1].GetAttribute("fill").ShouldBe("var(--color-success)");
        cut.Markup.ShouldContain("Europe, After: 95 to 300");
        cut.FindAll(".flex-wrap span.inline-flex").Count.ShouldBe(2);
    }

    [Fact]
    public void Empty_input_draws_an_empty_svg_without_throwing()
    {
        (BunitJSModuleInterop _, JSRuntimeInvocationHandler<SlopeLayout> slope) = Setup<SlopeLayout>();
        IRenderedComponent<SlopeChart> cut = Render<SlopeChart>();
        slope.SetResult(new SlopeLayout([], 0, 0, 0, 0, []));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.FindAll("svg line").Count.ShouldBe(0);
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Slope chart");
    }

    [Fact]
    public void Charts_ask_again_only_when_their_data_changes()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<LollipopLayout> call) = Setup<LollipopLayout>();
        call.SetResult(new LollipopLayout(_plot, [], [], 334, 80, 0, []));

        IRenderedComponent<LollipopChart> cut = Render<LollipopChart>(ps => ps.Add(p => p.Data, [new ChartDataPoint("A", 1)]));
        cut.Render(ps => ps.Add(p => p.Title, "NPS"));
        module.Invocations["layout"].Count.ShouldBe(1);

        cut.Render(ps => ps.Add(p => p.Horizontal, false));
        module.Invocations["layout"].Count.ShouldBe(2);
    }
}
