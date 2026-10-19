using Bunit;
using Bunit.TestDoubles;
using DaisyBlazor.Charts;
using DaisyBlazor.Charts.Server;
using Microsoft.AspNetCore.Components;
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
    public void The_prerendered_layout_is_restored_without_javascript()
    {
        string key;
        string layout;
        using (BunitContext server = new())
        {
            server.Services.AddSingleton<IChartLayoutEngine>(_engine);
            BunitPersistentComponentState state = server.AddBunitPersistentComponentState();
            server.SetRendererInfo(new RendererInfo("Static", isInteractive: false));
            IRenderedComponent<SankeyChart> prerendered = server.Render<SankeyChart>(ps => ps.Add(p => p.Links, _links));

            state.TriggerOnPersisting();

            key = D3ChartBase<SankeyLayout>.StateKey("sankey", prerendered.Instance.SpecJson!);
            state.TryTake(key, out string? persisted).ShouldBeTrue();
            layout = persisted!;
        }

        using BunitContext browser = new();
        browser.JSInterop.Mode = JSRuntimeMode.Strict;
        BunitPersistentComponentState restored = browser.AddBunitPersistentComponentState();
        restored.Persist(key, layout);
        browser.SetRendererInfo(new RendererInfo("WebAssembly", isInteractive: true));

        IRenderedComponent<SankeyChart> cut = browser.Render<SankeyChart>(ps => ps.Add(p => p.Links, _links));

        cut.FindAll("[aria-busy=true]").ShouldBeEmpty();
        cut.FindAll("svg rect").Count.ShouldBe(3);
        browser.JSInterop.Invocations.ShouldBeEmpty();
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

    public sealed class EngineFixture : IDisposable
    {
        public JintChartLayoutEngine Engine { get; } = new();

        public void Dispose() => Engine.Dispose();
    }
}
