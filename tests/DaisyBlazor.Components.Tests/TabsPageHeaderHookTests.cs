using AngleSharp.Dom;
using DaisyBlazor;
using DaisyBlazor.Layout;

namespace DaisyBlazor.Components.Tests;

/// <summary>Tabs tablist label (#103) and PageHeader title/subtitle class hooks (#104).</summary>
public class TabsPageHeaderHookTests : BunitContext
{
    public TabsPageHeaderHookTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Tabs_labels_its_tablist()
    {
        IRenderedComponent<Tabs> cut = Render<Tabs>(ps => ps
            .Add(p => p.AriaLabel, "Song sections")
            .Add(p => p.AriaLabelledBy, "heading")
            .AddChildContent<Tab>(t => t.Add(x => x.Title, "One").AddChildContent("1")));

        IElement tablist = cut.Find("[role=tablist]");
        tablist.GetAttribute("aria-label").ShouldBe("Song sections");
        tablist.GetAttribute("aria-labelledby").ShouldBe("heading");
    }

    [Fact]
    public void PageHeader_forwards_title_and_subtitle_classes()
    {
        IRenderedComponent<PageHeader> cut = Render<PageHeader>(ps => ps
            .Add(p => p.Title, "Library")
            .Add(p => p.Subtitle, "All songs")
            .Add(p => p.TitleClass, "font-display")
            .Add(p => p.SubtitleClass, "italic"));

        cut.Find("h1").ClassList.ShouldContain("font-display");
        cut.Find(".italic").TextContent.Trim().ShouldBe("All songs");
    }
}
