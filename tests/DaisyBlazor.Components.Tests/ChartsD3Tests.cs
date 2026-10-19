using DaisyBlazor.Charts;
using Microsoft.JSInterop;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// The D3-backed charts: they show a placeholder, ask the bundled module for a layout once rendered,
/// draw what it returns, and show the layout's error instead of throwing.
/// </summary>
public class ChartsD3Tests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Charts/charts.mjs";

    private static readonly SankeyLink[] _links =
    [
        new("Organic", "Starter", 30),
        new("Starter", "Active", 30),
    ];

    private static readonly SankeyLayout _layout = new(
        [new("Organic", 0, 30, 1, 1, 13, 200), new("Starter", 1, 30, 300, 1, 312, 200), new("Active", 2, 30, 627, 1, 639, 200)],
        [new(0, 1, 30, 199, "M13,100C150,100,150,100,300,100"), new(1, 2, 30, 199, "M312,100C470,100,470,100,627,100")]);

    [Fact]
    public void Sankey_shows_a_placeholder_until_the_layout_arrives()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        JSRuntimeInvocationHandler<SankeyLayout> call = module.Setup<SankeyLayout>("layout", _ => true);

        IRenderedComponent<SankeyChart> cut = Render<SankeyChart>(ps => ps.Add(p => p.Links, _links));

        cut.Find("[aria-busy=true]").ShouldNotBeNull();
        cut.FindAll("svg").Count.ShouldBe(0);

        call.SetResult(_layout);
        cut.WaitForAssertion(() => cut.FindAll("svg rect").Count.ShouldBe(3));
        cut.FindAll("svg path").Count.ShouldBe(2);
        cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 640 360");
        cut.Markup.ShouldContain("Organic 30");
        cut.Markup.ShouldContain("Organic → Starter: 30");
        cut.Markup.ShouldContain("var(--color-primary)");
    }

    [Fact]
    public void Sankey_sends_its_links_and_size_to_the_sankey_layout()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        module.Setup<SankeyLayout>("layout", _ => true).SetResult(_layout);

        Render<SankeyChart>(ps => ps.Add(p => p.Links, _links).Add(p => p.AspectRatio, 2));

        JSRuntimeInvocation invocation = module.Invocations["layout"].Single();
        invocation.Arguments[0].ShouldBe("sankey");
        string spec = System.Text.Json.JsonSerializer.Serialize(invocation.Arguments[1], new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        spec.ShouldContain("\"height\":320");
        spec.ShouldContain("\"source\":\"Organic\"");
        spec.ShouldContain("\"align\":\"justify\"");
    }

    [Fact]
    public void Sankey_asks_again_only_when_its_inputs_change()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        module.Setup<SankeyLayout>("layout", _ => true).SetResult(_layout);

        IRenderedComponent<SankeyChart> cut = Render<SankeyChart>(ps => ps.Add(p => p.Links, _links));
        cut.Render(ps => ps.Add(p => p.Title, "Signups"));
        module.Invocations["layout"].Count.ShouldBe(1);

        cut.Render(ps => ps.Add(p => p.Links, [.. _links, new SankeyLink("Starter", "Churned", 5)]));
        module.Invocations["layout"].Count.ShouldBe(2);
    }

    [Fact]
    public void Sankey_shows_the_layout_error()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        module.Setup<SankeyLayout>("layout", _ => true).SetException(new JSException("circular link\n    at stack"));

        IRenderedComponent<SankeyChart> cut = Render<SankeyChart>(ps => ps.Add(p => p.Links, _links));

        cut.WaitForAssertion(() => cut.Find("[role=alert]").TextContent.ShouldContain("circular link"));
        cut.Find("[role=alert]").TextContent.ShouldNotContain("at stack");
    }

    [Fact]
    public void Frame_charts_lay_out_at_the_width_of_a_narrow_box()
    {
        BunitJSModuleInterop module = this.SetupChartModule(frameWidth: 360);
        module.Setup<SankeyLayout>("layout", _ => true).SetResult(_layout);

        IRenderedComponent<SankeyChart> cut = Render<SankeyChart>(ps => ps.Add(p => p.Links, _links).Add(p => p.AspectRatio, 2));

        // Measured before the first layout, so a phone gets one layout, at its own width.
        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 360 180"));
        module.Spec("layout").ShouldContain("\"width\":360,\"height\":180");
        module.Invocations["layout"].Count.ShouldBe(1);

        // A resize lays out again; a box wider than the frame keeps the 640 frame, and a tiny one stops at 280.
        cut.InvokeAsync(() => cut.Instance.OnFrameWidth(1200));
        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 640 320"));
        cut.InvokeAsync(() => cut.Instance.OnFrameWidth(900));
        cut.InvokeAsync(() => cut.Instance.OnFrameWidth(120));
        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 280 140"));
        module.Invocations["layout"].Count.ShouldBe(3);
        module.Invocations["observe"].Count.ShouldBe(1);
    }

    [Fact]
    public async Task Frame_charts_stop_observing_their_box_when_disposed()
    {
        BunitJSModuleInterop module = this.SetupChartModule();
        module.Setup<SankeyLayout>("layout", _ => true).SetResult(_layout);
        module.SetupVoid("unobserve");
        IRenderedComponent<SankeyChart> cut = Render<SankeyChart>(ps => ps.Add(p => p.Links, _links));
        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 640 360"));

        await DisposeComponentsAsync();

        module.Invocations["unobserve"].Count.ShouldBe(1);
    }
}
