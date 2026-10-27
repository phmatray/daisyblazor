using Microsoft.AspNetCore.Components;
using System.Globalization;
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

        // A resize lays out again at the new width, wide boxes included (text keeps its size), and a tiny one stops at 280.
        cut.InvokeAsync(() => cut.Instance.OnFrameWidth(1200));
        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 1200 600"));
        cut.InvokeAsync(() => cut.Instance.OnFrameWidth(120));
        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 280 140"));
        module.Invocations["layout"].Count.ShouldBe(3);
        module.Invocations["observe"].Count.ShouldBe(1);
    }

    [Fact]
    public void Frame_charts_lay_out_at_a_width_in_pixels_until_measured()
    {
        // A box with no size yet answers null: the chart keeps the width it was given.
        SetRendererInfo(new RendererInfo("WebAssembly", isInteractive: true));
        BunitJSModuleInterop module = JSInterop.SetupModule(ChartFakes.ModulePath);
        module.Setup<double?>("observe", _ => true).SetResult(null);
        module.Setup<SankeyLayout>("layout", _ => true).SetResult(_layout);

        IRenderedComponent<SankeyChart> cut = Render<SankeyChart>(ps => ps.Add(p => p.Links, _links).Add(p => p.AspectRatio, 2).Add(p => p.Width, "900px"));

        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 900 450"));
    }

    [Fact]
    public void ShowTitle_false_keeps_the_title_as_the_accessible_name_only()
    {
        this.SetupChartModule().Setup<SankeyLayout>("layout", _ => true).SetResult(_layout);

        IRenderedComponent<SankeyChart> cut = Render<SankeyChart>(ps => ps.Add(p => p.Links, _links).Add(p => p.Title, "Flows").Add(p => p.ShowTitle, false));

        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("aria-label").ShouldBe("Flows"));
        cut.FindAll("div.font-medium").ShouldBeEmpty();
    }

    [Fact]
    public void Value_ticks_follow_the_current_culture_and_categories_stay_as_given()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
        try
        {
            IRenderedComponent<ChartAxes> cut = Render<ChartAxes>(ps => ps
                .Add(p => p.Plot, new PlotArea(40, 10, 600, 300))
                .Add(p => p.YTicks, [new AxisTick(300, "−1,500.5"), new AxisTick(200, "1.5k"), new AxisTick(100, "80%"), new AxisTick(50, "14000")])
                .Add(p => p.XTicks, [new AxisTick(100, "2.5"), new AxisTick(200, "12:00")])
                .Add(p => p.XCategories, true));

            NumberFormatInfo nf = CultureInfo.CurrentCulture.NumberFormat;
            string[] texts = [.. cut.FindAll("text").Select(t => t.TextContent)];
            texts.ShouldBe(["2.5", "12:00", $"−1{nf.NumberGroupSeparator}500,5", "1,5k", 0.8.ToString("P0", nf), "14000"]);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
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
