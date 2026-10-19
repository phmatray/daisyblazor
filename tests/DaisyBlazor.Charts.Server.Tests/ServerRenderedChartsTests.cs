using System.Reflection;
using AngleSharp.Dom;
using Bunit;
using Bunit.Rendering;
using DaisyBlazor.Charts;
using DaisyBlazor.Charts.Server;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Charts.Server.Tests;

/// <summary>
/// With server rendering registered, every chart example of the docs renders its finished SVG with
/// no JavaScript at all: the HTML a prerender or a static page would send already holds the chart.
/// </summary>
public sealed class ServerRenderedChartsTests : BunitContext, IClassFixture<ServerRenderedChartsTests.EngineFixture>
{
    public ServerRenderedChartsTests(EngineFixture fixture)
    {
        // Strict mode: any JavaScript call fails the test.
        JSInterop.Mode = JSRuntimeMode.Strict;
        Services.AddSingleton<IChartLayoutEngine>(fixture.Engine);
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
    }

    public static TheoryData<string> Examples()
    {
        TheoryData<string> data = [];
        foreach (Type type in typeof(DaisyBlazor.Docs.Examples.Charts.SankeyChartBasicExample).Assembly.GetTypes()
                     .Where(t => t.Namespace == "DaisyBlazor.Docs.Examples.Charts" && typeof(IComponent).IsAssignableFrom(t) && !t.IsAbstract)
                     .OrderBy(t => t.Name))
        {
            data.Add(type.Name);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Examples))]
    public void Example_renders_its_chart_on_the_server(string example)
    {
        Type type = typeof(DaisyBlazor.Docs.Examples.Charts.SankeyChartBasicExample).Assembly
            .GetTypes().Single(t => t.Name == example && t.Namespace == "DaisyBlazor.Docs.Examples.Charts");

        IRenderedComponent<ContainerFragment> cut = Render(builder =>
        {
            builder.OpenComponent(0, type);
            builder.CloseComponent();
        });

        cut.FindAll("[aria-busy=true]").ShouldBeEmpty("a chart is still a placeholder");
        cut.FindAll("[role=alert]").Select(a => a.TextContent).ShouldBeEmpty();
        IReadOnlyList<IElement> svgs = cut.FindAll("svg[role=img]");
        svgs.ShouldNotBeEmpty();
        svgs.ShouldAllBe(svg => svg.QuerySelectorAll("path, rect, circle, line, polygon").Length > 0);
    }

    [Fact]
    public void Category_markers_render_on_the_server()
    {
        IRenderedComponent<DaisyBlazor.Docs.Examples.Charts.LineChartMarkersExample> cut = Render<DaisyBlazor.Docs.Examples.Charts.LineChartMarkersExample>();

        cut.FindAll("line").Single(l => l.QuerySelector("title")?.TextContent == "New pricing: Apr").GetAttribute("stroke").ShouldBe("var(--color-primary)");
        cut.FindAll("rect").Single(r => r.QuerySelector("title")?.TextContent == "Summer campaign: Jun to Aug").GetAttribute("fill-opacity").ShouldBe("0.12");
        string[] labels = cut.FindAll("text").Select(t => t.TextContent).ToArray();
        labels.ShouldContain("New pricing");
        labels.ShouldContain("Summer campaign");
    }

    public sealed class EngineFixture : IDisposable
    {
        public JintChartLayoutEngine Engine { get; } = new();

        public void Dispose() => Engine.Dispose();
    }
}
