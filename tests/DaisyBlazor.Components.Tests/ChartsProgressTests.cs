using System.Text.Json;
using DaisyBlazor.Charts;
using Microsoft.JSInterop;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The composition and progress charts: each shows a placeholder, sends its inputs to its D3 layout,
/// draws the returned geometry in theme colours, and draws nothing for an empty layout.
/// </summary>
public class ChartsProgressTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    private (BunitJSModuleInterop Module, JSRuntimeInvocationHandler<TLayout> Call) Setup<TLayout>()
    {
        BunitJSModuleInterop module = JSInterop.SetupModule(ModulePath);
        return (module, module.Setup<TLayout>("layout", _ => true));
    }

    private static (string Kind, string Spec) Sent(BunitJSModuleInterop module)
    {
        JSRuntimeInvocation invocation = module.Invocations["layout"].Single();
        return ((string)invocation.Arguments[0]!, JsonSerializer.Serialize(invocation.Arguments[1], _json));
    }

    [Fact]
    public void Waterfall_draws_signed_steps_in_success_and_error()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<WaterfallLayout> call) = Setup<WaterfallLayout>();
        WaterfallStep[] steps = [new("Open", 100, WaterfallStepKind.Start), new("New", 40), new("Lost", -25), new("Close", 0, WaterfallStepKind.End)];

        IRenderedComponent<WaterfallChart> cut = Render<WaterfallChart>(ps => ps.Add(p => p.Steps, steps));
        cut.Find("[aria-busy=true]").ShouldNotBeNull();

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("waterfall");
        spec.ShouldContain("\"kind\":\"start\"");
        spec.ShouldContain("\"kind\":\"end\"");
        spec.ShouldContain("\"horizontal\":false");

        call.SetResult(new WaterfallLayout(
            [
                new("Open", "total", 100, 100, 50, 100, 60, 200, 80, 94, "middle"),
                new("New", "increase", 40, 140, 150, 20, 60, 80, 180, 14, "middle"),
                new("Lost", "decrease", -25, 115, 250, 20, 60, 50, 280, 14, "middle"),
                new("Close", "total", 115, 115, 350, 70, 60, 230, 380, 64, "middle"),
            ],
            [new(110, 100, 150, 100)],
            [new(300, "0", 0), new(100, "100", 100)],
            new(48, 20, 632, 300),
            300));

        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(4));
        cut.Markup.ShouldContain("var(--color-success)");
        cut.Markup.ShouldContain("var(--color-error)");
        cut.Markup.ShouldContain("+40");
        cut.Markup.ShouldContain("-25");
        cut.Markup.ShouldContain("Lost: -25 (total 115)");
    }

    [Fact]
    public void Funnel_prints_the_conversion_from_the_previous_stage()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<FunnelLayout> call) = Setup<FunnelLayout>();
        ChartDataPoint[] stages = [new("Trial", 1000), new("Paid", 250)];

        IRenderedComponent<FunnelChart> cut = Render<FunnelChart>(ps => ps.Add(p => p.Data, stages).Add(p => p.Shape, FunnelShape.Pyramid));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("funnel");
        spec.ShouldContain("\"shape\":\"pyramid\"");

        call.SetResult(new FunnelLayout(
        [
            new(0, "Trial", 1000, null, 1, 4, 170, "M150,4L490,4L400,170L240,170Z", 87, 140, 500),
            new(1, "Paid", 250, 0.25, 0.25, 174, 356, "M240,174L400,174L350,356L290,356Z", 265, 140, 500),
        ]));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(2));
        cut.Markup.ShouldContain("25% of previous");
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Pyramid chart");
    }

    [Fact]
    public void Gauge_names_its_value_and_draws_bands_and_marker()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<GaugeLayout> call) = Setup<GaugeLayout>();
        GaugeBand[] bands = [new(99, 99.9, "var(--color-error)", "Below SLA"), new(99.9, 100)];

        IRenderedComponent<GaugeChart> cut = Render<GaugeChart>(ps => ps
            .Add(p => p.Title, "Uptime").Add(p => p.Value, 99.95).Add(p => p.Min, 99).Add(p => p.Max, 100)
            .Add(p => p.Marker, 99.9).Add(p => p.Bands, bands).Add(p => p.Color, "var(--color-success)"));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("gauge");
        spec.ShouldContain("\"marker\":99.9");
        spec.ShouldContain("\"startAngle\":-120");
        spec.ShouldContain("\"height\":512");

        call.SetResult(new GaugeLayout(320, 280, 200, "M0,-200A200,200Z", "M0,-200A200,200Z",
            [new(0, 99, 99.9, "M1Z"), new(1, 99.9, 100, "M2Z")], new(99.9, 1, 2, 3, 4), [new(99, 100, 400, "end"), new(100, 540, 400, "start")],
            99.95, 0.95, 17.8, 56, 300));

        cut.WaitForAssertion(() => cut.FindAll("svg line").Count.ShouldBe(1));
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Uptime: 99.95, target 99.9 (scale 99 to 100)");
        cut.Markup.ShouldContain("var(--color-success)");
        cut.Markup.ShouldContain("Below SLA: 99 to 99.9");
        cut.Markup.ShouldContain("var(--color-secondary)");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Gauge_falls_back_to_primary_for_an_empty_colour(string color)
    {
        (_, JSRuntimeInvocationHandler<GaugeLayout> call) = Setup<GaugeLayout>();
        IRenderedComponent<GaugeChart> cut = Render<GaugeChart>(ps => ps.Add(p => p.Value, 40).Add(p => p.Color, color));

        call.SetResult(new GaugeLayout(320, 280, 200, "M0,-200A200,200Z", "M0,-200A200,200Z", [], null, [], 40, 0.4, 17.8, 56, 300));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBeGreaterThan(0));
        cut.FindAll("svg path").Select(p => p.GetAttribute("fill")).ShouldContain("var(--color-primary)");
        cut.FindAll("svg path").Select(p => p.GetAttribute("fill")).ShouldNotContain(color);
    }

    [Fact]
    public void An_axis_tick_without_a_label_reads_as_empty()
    {
        new AxisTick(1, null!).Label.ShouldBe(string.Empty);
        JsonSerializer.Deserialize<AxisTick>("""{"pos":1,"label":null}""", _json)!.Label.ShouldBe(string.Empty);
    }

    [Fact]
    public void Gauge_with_an_empty_layout_draws_no_arc()
    {
        (_, JSRuntimeInvocationHandler<GaugeLayout> call) = Setup<GaugeLayout>();
        IRenderedComponent<GaugeChart> cut = Render<GaugeChart>(ps => ps.Add(p => p.Min, 5).Add(p => p.Max, 5));

        call.SetResult(new GaugeLayout(0, 0, 0, "", "", [], null, [], 0, 0, 12, 12, 0));

        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.FindAll("svg path").Count.ShouldBe(0);
    }

    [Fact]
    public void Linear_gauge_sends_its_orientation_and_draws_pointer_and_ticks()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<LinearGaugeLayout> call) = Setup<LinearGaugeLayout>();

        IRenderedComponent<LinearGaugeChart> cut = Render<LinearGaugeChart>(ps => ps
            .Add(p => p.Value, 412).Add(p => p.Max, 500).Add(p => p.Horizontal, false).Add(p => p.AspectRatio, 0.5).Add(p => p.ValueFormat, "{0} GB"));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("linear-gauge");
        spec.ShouldContain("\"vertical\":true");
        spec.ShouldContain("\"height\":1280");

        call.SetResult(new LinearGaugeLayout(new(300, 40, 70, 1200), new(321, 260, 28, 980), [], new("M1,2L3,4L5,6Z", 420, 270, "start"), null,
            [new(0, 290, 1240, 280, 1240, 250, 1250, "end"), new(500, 290, 40, 280, 40, 250, 50, "end")], 412, 42));

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("412 GB"));
        cut.Markup.ShouldContain("500 GB");
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Gauge: 412 GB (scale 0 GB to 500 GB)");
    }

    [Fact]
    public void Radial_bar_labels_each_ring_with_its_value()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<RadialBarLayout> call) = Setup<RadialBarLayout>();
        ChartDataPoint[] items = [new("Compute", 72), new("Seats", 93, "var(--color-warning)")];

        IRenderedComponent<RadialBarChart> cut = Render<RadialBarChart>(ps => ps.Add(p => p.Data, items).Add(p => p.ValueFormat, "{0}%"));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("radial-bar");
        spec.ShouldContain("\"max\":100");
        spec.ShouldContain("\"height\":640");

        call.SetResult(new RadialBarLayout(320, 320,
        [
            new(0, "Compute", 72, 0.72, "M1Z", "M2Z", 312, 40, 17.8),
            new(1, "Seats", 93, 0.93, "M3Z", "M4Z", 312, 100, 17.8),
        ]));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(4));
        cut.Markup.ShouldContain("Compute 72%");
        cut.Markup.ShouldContain("var(--color-warning)");
    }

    [Fact]
    public void Polar_area_draws_a_wedge_and_label_per_item()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<PolarAreaLayout> call) = Setup<PolarAreaLayout>();
        ChartDataPoint[] items = [new("Login", 142), new("API", 88)];

        IRenderedComponent<PolarAreaChart> cut = Render<PolarAreaChart>(ps => ps.Add(p => p.Data, items));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("polar-area");
        spec.ShouldContain("\"label\":\"Login\"");

        call.SetResult(new PolarAreaLayout(320, 320, 200, 17.8, [new(100, 168)],
            [new(0, "Login", 142, "M0,0Z", 500, 320, "start"), new(1, "API", 88, "M0,0Z", 140, 320, "end")]));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(2));
        cut.FindAll("svg circle").Count.ShouldBe(1);
        cut.Markup.ShouldContain("Login 142");
        cut.Markup.ShouldContain("API: 88");
    }

    [Fact]
    public void Pareto_fades_bars_past_the_vital_few_and_marks_the_threshold()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<ParetoLayout> call) = Setup<ParetoLayout>();
        ChartDataPoint[] items = [new("DNS", 30), new("Timeout", 50), new("Other", 20)];

        IRenderedComponent<ParetoChart> cut = Render<ParetoChart>(ps => ps.Add(p => p.Data, items));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("pareto");
        spec.ShouldContain("\"threshold\":0.8");

        call.SetResult(new ParetoLayout(
            [
                new("Timeout", 50, 0.5, 0.5, 60, 20, 100, 280, 110, 170),
                new("DNS", 30, 0.3, 0.8, 260, 130, 100, 170, 310, 76),
                new("Other", 20, 0.2, 1, 460, 190, 100, 110, 510, 12),
            ],
            "M110,170L310,76L510,12", [new(300, "0", 0), new(12, "50", 50)], [new(300, "0", 0), new(76, "0.8", 0.8), new(12, "1", 1)],
            new(48, 12, 594, 300), 76, 2));

        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(3));
        cut.FindAll("svg rect[fill-opacity='0.4']").Count.ShouldBe(1);
        cut.FindAll("svg circle").Count.ShouldBe(3);
        cut.Markup.ShouldContain("Timeout: 50 (50%, cumulative 50%)");
        cut.Markup.ShouldContain("var(--color-warning)");
        cut.Find("svg desc").TextContent.ShouldContain("The first 2 reach 80% of the total.");
    }

    [Fact]
    public void Marimekko_draws_cells_column_labels_and_a_segment_legend()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<MarimekkoLayout> call) = Setup<MarimekkoLayout>();
        MarimekkoCell[] cells = [new("EU", "Free", 30), new("EU", "Pro", 10), new("US", "Free", 20), new("US", "Pro", 40)];

        IRenderedComponent<MarimekkoChart> cut = Render<MarimekkoChart>(ps => ps.Add(p => p.Cells, cells));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("marimekko");
        spec.ShouldContain("\"column\":\"EU\",\"segment\":\"Free\",\"value\":30");

        call.SetResult(new MarimekkoLayout(
            [new("EU", 40, 0.4, 44, 276, 160), new("US", 60, 0.6, 278, 634, 456)],
            [
                new(0, 0, 30, 0.75, 44, 85, 232, 230), new(0, 1, 10, 0.25, 44, 6, 232, 77),
                new(1, 0, 20, 1 / 3d, 278, 214, 356, 101), new(1, 1, 40, 2 / 3d, 278, 6, 356, 206),
            ],
            ["Free", "Pro"], [new(320, "0", 0), new(6, "1", 1)], new(44, 6, 634, 320)));

        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(4));
        cut.Markup.ShouldContain("75%");
        cut.Markup.ShouldContain("var(--color-primary-content)");
        cut.Markup.ShouldContain("EU, Free: 30 (75% of EU)");
        cut.FindAll("span.rounded-sm").Count.ShouldBe(2);
    }

    [Fact]
    public void Waffle_colours_cells_by_item_and_leaves_the_rest_faint()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<WaffleLayout> call) = Setup<WaffleLayout>();
        ChartDataPoint[] items = [new("Active", 412), new("Gone", 0), new("Invited", 96)];

        IRenderedComponent<WaffleChart> cut = Render<WaffleChart>(ps => ps.Add(p => p.Data, items).Add(p => p.Total, 600));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("waffle");
        spec.ShouldContain("\"total\":600");
        spec.ShouldNotContain("Gone");

        call.SetResult(new WaffleLayout(50,
            [new("Active", 412, 0.687, 1), new("Invited", 96, 0.16, 1)],
            [new(0, 0, 0), new(60, 0, 1), new(120, 0, -1)]));

        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(3));
        cut.Find("svg rect[fill-opacity='0.12']").ShouldNotBeNull();
        cut.Markup.ShouldContain("var(--color-secondary)");
        cut.Markup.ShouldContain("Active 68.7%");
        cut.Markup.ShouldContain("Rest: 92");
    }

    [Fact]
    public void Charts_with_no_data_draw_an_empty_svg()
    {
        (_, JSRuntimeInvocationHandler<WaterfallLayout> call) = Setup<WaterfallLayout>();
        IRenderedComponent<WaterfallChart> cut = Render<WaterfallChart>();

        call.SetResult(new WaterfallLayout([], [], [], null, 0));

        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.FindAll("svg rect").Count.ShouldBe(0);
    }

    [Fact]
    public void Polar_area_with_series_draws_a_wind_rose_with_a_legend()
    {
        (BunitJSModuleInterop module, JSRuntimeInvocationHandler<PolarAreaLayout> call) = Setup<PolarAreaLayout>();

        IRenderedComponent<PolarAreaChart> cut = Render<PolarAreaChart>(ps => ps
            .Add(p => p.Labels, ["North", "East"])
            .Add(p => p.Series, [new ChartSeries("Reads", [4, 1]), new ChartSeries("Writes", [5, double.NaN])]));

        (string kind, string spec) = Sent(module);
        kind.ShouldBe("polar-area");
        spec.ShouldContain("\"labels\":[\"North\",\"East\"]");
        spec.ShouldContain("\"series\":[[4,1],[5,null]]");
        spec.ShouldNotContain("items");

        call.SetResult(new PolarAreaLayout(320, 320, 200, 17.8, [new(4, 100)],
            [new(0, "North", 9, "M0,0Z", 320, 100, "middle"), new(1, "East", 1, "M0,0Z", 540, 320, "start")],
            [new(0, 0, 4, "M0,-1Z"), new(0, 1, 5, "M0,-2Z"), new(1, 0, 1, "M1,0Z")]));

        cut.WaitForAssertion(() => cut.FindAll("svg path").Count.ShouldBe(3));
        cut.Markup.ShouldContain("North, Writes: 5");
        cut.Markup.ShouldContain("North 9");
        cut.FindAll("svg path")[1].GetAttribute("fill").ShouldBe("var(--color-secondary)");
        cut.FindAll(".flex-wrap span.inline-flex").Count.ShouldBe(2);
        cut.Find("desc").TextContent.ShouldBe("North: Reads 4, Writes 5; East: Reads 1, Writes no value");
    }

    [Fact]
    public void Funnel_rectangle_sends_its_shape()
    {
        (BunitJSModuleInterop module, _) = Setup<FunnelLayout>();
        Render<FunnelChart>(ps => ps.Add(p => p.Data, [new ChartDataPoint("Trial", 100), new ChartDataPoint("Paid", 20)]).Add(p => p.Shape, FunnelShape.Rectangle));

        (_, string spec) = Sent(module);
        spec.ShouldContain("\"shape\":\"rectangle\"");
    }
}
