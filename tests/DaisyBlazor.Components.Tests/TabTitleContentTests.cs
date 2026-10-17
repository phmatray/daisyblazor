using AngleSharp.Dom;
using Bunit;

namespace DaisyBlazor.Components.Tests;

/// <summary>Tab.TitleContent (#66): rich tab labels, with Title kept as the accessible name.</summary>
public class TabTitleContentTests : BunitContext
{
    public TabTitleContentTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void TitleContent_renders_in_the_button_and_Title_becomes_its_aria_label()
    {
        IRenderedComponent<Tabs> cut = Render<Tabs>(ps => ps
            .AddChildContent<Tab>(t => t
                .Add(x => x.Title, "Members, 12 pending")
                .Add(x => x.TitleContent, "<span class=\"badge\">12</span>")
                .AddChildContent("body")));

        IElement tab = cut.Find("[role=tab]");
        tab.QuerySelector(".badge")!.TextContent.ShouldBe("12");
        tab.TextContent.ShouldNotContain("Members");
        tab.GetAttribute("aria-label").ShouldBe("Members, 12 pending");
    }

    [Fact]
    public void Title_only_renders_text_without_aria_label()
    {
        IRenderedComponent<Tabs> cut = Render<Tabs>(ps => ps
            .AddChildContent<Tab>(t => t.Add(x => x.Title, "Overview")));

        IElement tab = cut.Find("[role=tab]");
        tab.TextContent.Trim().ShouldBe("Overview");
        tab.HasAttribute("aria-label").ShouldBeFalse();
    }

    [Fact]
    public void TitleContent_without_Title_takes_its_name_from_content_and_user_aria_label_wins()
    {
        IRenderedComponent<Tabs> cut = Render<Tabs>(ps => ps
            .AddChildContent<Tab>(t => t.Add(x => x.TitleContent, "Inbox"))
            .AddChildContent<Tab>(t => t
                .Add(x => x.Title, "Ignored")
                .Add(x => x.TitleContent, "<i></i>")
                .AddUnmatched("aria-label", "Custom")));

        IReadOnlyList<IElement> tabs = cut.FindAll("[role=tab]");
        tabs[0].HasAttribute("aria-label").ShouldBeFalse();
        tabs[0].TextContent.Trim().ShouldBe("Inbox");
        tabs[1].GetAttribute("aria-label").ShouldBe("Custom");
    }
}
