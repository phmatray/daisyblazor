using System.Text.Json;
using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The combo chart: sends its bars and lines to its layout, shows a placeholder until the layout
/// arrives, then draws bars, lines, both value axes, tooltips and a legend from it.
/// </summary>
public class ChartsComboTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly PlotArea _plot = new(40, 10, 590, 336);
    private static readonly AxisTick[] _xTicks = [new(150, "W1", null, "W1"), new(450, "W2", null, "W2")];

    private static readonly ComboLayout _layout = new(_plot, _xTicks,
        [new(336, "0", 0), new(10, "200", 200)],
        [new(336, "0", 0), new(10, "5", 5)],
        [new(0, 0, 120, 60, 140, 80, 196), new(0, 1, 180, 360, 50, 80, 286)],
        [new(0, "M150,200L450,100", [new(0, 2.5, 150, 200), new(1, 4.1, 450, 100)])]);

    private BunitJSModuleInterop _module = null!;

    private JSRuntimeInvocationHandler<ComboLayout> Setup()
    {
        _module = this.SetupChartModule();
        return _module.Setup<ComboLayout>("layout", _ => true);
    }

    private string Spec()
    {
        JSRuntimeInvocation invocation = _module.Invocations["layout"].Single();
        invocation.Arguments[0].ShouldBe("combo");
        return JsonSerializer.Serialize(invocation.Arguments[1], new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    [Fact]
    public void Draws_bars_lines_and_a_secondary_axis()
    {
        JSRuntimeInvocationHandler<ComboLayout> call = Setup();
        IRenderedComponent<ComboChart> cut = Render<ComboChart>(ps => ps
            .Add(p => p.Labels, ["W1", "W2"])
            .Add(p => p.Bars, [new ChartSeries("Deployments", [120, 180])])
            .Add(p => p.Lines, [new ChartSeries("Failure rate", [2.5, 4.1])])
            .Add(p => p.SecondaryAxis, true)
            .Add(p => p.Smooth, true)
            .Add(p => p.SecondaryValueFormat, "{0:0.#}%"));

        cut.Find("[aria-busy=true]").ShouldNotBeNull();
        string spec = Spec();
        spec.ShouldContain("\"secondaryAxis\":true");
        spec.ShouldContain("\"smooth\":true");
        spec.ShouldContain("\"stackBars\":false");
        spec.ShouldContain("\"bars\":[{\"name\":\"Deployments\",\"data\":[120,180]}]");

        call.SetResult(_layout);
        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(2));
        cut.FindAll("svg circle").Count.ShouldBe(2);
        cut.Find("svg path[d='M150,200L450,100']").GetAttribute("stroke").ShouldBe("var(--color-secondary)");
        cut.Find("svg rect").GetAttribute("fill").ShouldBe("var(--color-primary)");
        cut.Markup.ShouldContain("W1 · Deployments: 120");
        cut.Markup.ShouldContain("W2 · Failure rate: 4.1%");
        cut.FindAll("svg text").Select(t => t.TextContent).ShouldContain("5%", "right axis label");
        cut.Markup.ShouldContain("Failure rate (right axis)");
        cut.FindAll(".rounded-sm").Count.ShouldBe(2);
        cut.Find("svg").GetAttribute("aria-label").ShouldBe("Combo chart");
        cut.Find("svg desc").TextContent.ShouldBe("2 categories; grouped bars: Deployments; lines: Failure rate on the right axis.");
    }

    [Fact]
    public void Sends_how_much_wider_the_formatted_tick_labels_are()
    {
        // The layout sizes its margins from D3's labels; the chart draws ValueFormat's, so it says how many characters those add.
        Setup();
        Render<ComboChart>(ps => ps
            .Add(p => p.Labels, ["W1", "W2"])
            .Add(p => p.Bars, [new ChartSeries("Requests", [1200, 1800])])
            .Add(p => p.Lines, [new ChartSeries("Errors", [2.5, 4.1])])
            .Add(p => p.SecondaryAxis, true)
            .Add(p => p.ValueFormat, "{0:#,0.##} requests/day")
            .Add(p => p.SecondaryValueFormat, "{0:0.#}%"));

        string spec = Spec();
        spec.ShouldContain("\"yLabelPad\":13");
        spec.ShouldContain("\"y2LabelPad\":1");
    }

    [Fact]
    public void Series_colours_override_the_palette_and_the_legend_can_be_hidden()
    {
        JSRuntimeInvocationHandler<ComboLayout> call = Setup();
        IRenderedComponent<ComboChart> cut = Render<ComboChart>(ps => ps
            .Add(p => p.Labels, ["W1", "W2"])
            .Add(p => p.Bars, [new ChartSeries("Deployments", [120, double.NaN]) { Color = "oklch(70% 0.1 200)" }])
            .Add(p => p.Lines, [new ChartSeries("Lead time", [2.5, 4.1])])
            .Add(p => p.StackBars, true)
            .Add(p => p.ShowLegend, false));

        Spec().ShouldContain("\"data\":[120,null]");
        call.SetResult(_layout with { Y2Ticks = [] });
        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(2));
        cut.Find("svg rect").GetAttribute("fill").ShouldBe("oklch(70% 0.1 200)");
        cut.Find("svg path[d='M150,200L450,100']").GetAttribute("stroke").ShouldBe("var(--color-secondary)");
        cut.Markup.ShouldContain("W2 · Lead time: 4.1");
        cut.FindAll(".rounded-sm").Count.ShouldBe(0);
    }

    [Fact]
    public void Empty_input_renders_an_empty_chart()
    {
        JSRuntimeInvocationHandler<ComboLayout> call = Setup();
        IRenderedComponent<ComboChart> cut = Render<ComboChart>();
        call.SetResult(new ComboLayout(null, [], [], [], [], []));
        cut.WaitForAssertion(() => cut.FindAll("svg").Count.ShouldBe(1));
        cut.FindAll("svg rect").Count.ShouldBe(0);
    }
}
