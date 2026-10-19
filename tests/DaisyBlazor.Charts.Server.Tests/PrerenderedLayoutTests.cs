using Bunit;
using Bunit.Rendering;
using Bunit.TestDoubles;
using DaisyBlazor.Charts;
using DaisyBlazor.Charts.Server;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Charts.Server.Tests;

/// <summary>
/// A layout computed while prerendering is handed to the interactive render through the persisted
/// component state, so a WebAssembly chart shows its SVG at once instead of a placeholder.
/// </summary>
public sealed class PrerenderedLayoutTests : IClassFixture<PrerenderedLayoutTests.EngineFixture>
{
    private static readonly SankeyLink[] _links = [new("Trial", "Paid", 40), new("Trial", "Expired", 60)];

    private readonly JintChartLayoutEngine _engine;

    public PrerenderedLayoutTests(EngineFixture fixture) => _engine = fixture.Engine;

    [Fact]
    public void The_prerendered_layout_is_restored_without_laying_out_again()
    {
        string key;
        string layout;
        using (BunitContext server = new())
        {
            server.Services.AddSingleton<IChartLayoutEngine>(_engine);
            BunitPersistentComponentState state = server.AddBunitPersistentComponentState();
            server.SetRendererInfo(new RendererInfo("Static", isInteractive: false));
            IRenderedComponent<SankeyChart> prerendered = Prerender(server, RenderMode.InteractiveWebAssembly);

            state.TriggerOnPersisting();

            key = D3ChartBase<SankeyLayout>.StateKey("sankey", prerendered.Instance.SpecJson!);
            state.TryTake(key, out string? persisted).ShouldBeTrue();
            layout = persisted!;
        }

        using BunitContext browser = new();
        browser.JSInterop.Mode = JSRuntimeMode.Strict;
        // The chart only measures its box, which is as wide as the frame or wider.
        BunitJSModuleInterop module = browser.JSInterop.SetupModule("./_content/DaisyBlazor.Charts/charts.mjs");
        module.Setup<double?>("observe", _ => true).SetResult(800);
        BunitPersistentComponentState restored = browser.AddBunitPersistentComponentState();
        restored.Persist(key, layout);
        browser.SetRendererInfo(new RendererInfo("WebAssembly", isInteractive: true));

        IRenderedComponent<SankeyChart> cut = browser.Render<SankeyChart>(ps => ps.Add(p => p.Links, _links));

        cut.FindAll("[aria-busy=true]").ShouldBeEmpty();
        cut.FindAll("svg rect").Count.ShouldBe(3);
        module.Invocations["observe"].Count.ShouldBe(1);
        module.Invocations["layout"].ShouldBeEmpty();
    }

    [Fact]
    public void A_chart_with_other_data_does_not_take_the_persisted_layout()
    {
        using BunitContext browser = new();
        browser.JSInterop.Mode = JSRuntimeMode.Loose;
        BunitPersistentComponentState restored = browser.AddBunitPersistentComponentState();
        restored.Persist(D3ChartBase<SankeyLayout>.StateKey("sankey", "{}"), "{\"nodes\":[],\"links\":[]}");
        browser.SetRendererInfo(new RendererInfo("WebAssembly", isInteractive: true));

        IRenderedComponent<SankeyChart> cut = browser.Render<SankeyChart>(ps => ps.Add(p => p.Links, _links));

        cut.FindAll("[aria-busy=true]").ShouldNotBeEmpty();
    }

    [Fact]
    public void Two_identical_charts_persist_one_layout_without_failing()
    {
        using BunitContext server = ServerContext(out BunitPersistentComponentState state);
        IRenderedComponent<ContainerFragment> page = server.Render(builder =>
        {
            for (int i = 0; i < 2; i++)
            {
                builder.OpenComponent<SankeyChart>(i * 3);
                builder.AddAttribute((i * 3) + 1, nameof(SankeyChart.Links), _links);
                builder.AddComponentRenderMode(RenderMode.InteractiveWebAssembly);
                builder.CloseComponent();
            }
        });

        Should.NotThrow(() => state.TriggerOnPersisting());
        string key = D3ChartBase<SankeyLayout>.StateKey("sankey", page.FindComponents<SankeyChart>()[0].Instance.SpecJson!);
        state.TryTake(key, out string? _).ShouldBeTrue();
    }

    [Fact]
    public void An_interactive_server_chart_persists_nothing()
    {
        using BunitContext server = ServerContext(out BunitPersistentComponentState state);
        IRenderedComponent<SankeyChart> prerendered = Prerender(server, RenderMode.InteractiveServer);

        state.TriggerOnPersisting();

        state.TryTake(D3ChartBase<SankeyLayout>.StateKey("sankey", prerendered.Instance.SpecJson!), out string? _).ShouldBeFalse();
    }

    [Fact]
    public void An_interactive_server_chart_lays_out_a_narrow_box_on_the_server()
    {
        using BunitContext circuit = new();
        circuit.Services.AddSingleton<IChartLayoutEngine>(_engine);
        circuit.JSInterop.Mode = JSRuntimeMode.Strict;
        BunitJSModuleInterop module = circuit.JSInterop.SetupModule("./_content/DaisyBlazor.Charts/charts.mjs");
        module.Setup<double?>("observe", _ => true).SetResult(360);
        circuit.SetRendererInfo(new RendererInfo("Server", isInteractive: true));

        IRenderedComponent<SankeyChart> cut = circuit.Render<SankeyChart>(ps => ps.Add(p => p.Links, _links));

        cut.WaitForAssertion(() => cut.Find("svg").GetAttribute("viewBox").ShouldBe("0 0 360 202.5"));
        cut.Instance.SpecJson!.ShouldContain("\"width\":360,\"height\":202.5");
        cut.FindAll("svg rect").Count.ShouldBe(3);
        module.Invocations["layout"].ShouldBeEmpty();
    }

    private BunitContext ServerContext(out BunitPersistentComponentState state)
    {
        BunitContext server = new();
        server.Services.AddSingleton<IChartLayoutEngine>(_engine);
        state = server.AddBunitPersistentComponentState();
        server.SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        return server;
    }

    private static IRenderedComponent<SankeyChart> Prerender(BunitContext server, IComponentRenderMode mode)
    {
        IRenderedComponent<ContainerFragment> page = server.Render(builder =>
        {
            builder.OpenComponent<SankeyChart>(0);
            builder.AddAttribute(1, nameof(SankeyChart.Links), _links);
            builder.AddComponentRenderMode(mode);
            builder.CloseComponent();
        });
        return page.FindComponent<SankeyChart>();
    }

    public sealed class EngineFixture : IDisposable
    {
        public JintChartLayoutEngine Engine { get; } = new();

        public void Dispose() => Engine.Dispose();
    }
}
