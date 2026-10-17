using AngleSharp.Dom;
using DaisyBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace DaisyBlazor.Components.Tests;

/// <summary>NavMenuLink renders its own menu &lt;li&gt; so it is valid straight inside a ul.menu (#78).</summary>
public class NavMenuLinkStructureTests : BunitContext
{
    [Fact]
    public void Renders_li_wrapping_the_link()
    {
        IRenderedComponent<NavMenuLink> cut = Render<NavMenuLink>(ps => ps
            .Add(p => p.Href, "/docs")
            .AddChildContent("Docs"));

        IElement li = cut.Find("li");
        li.Children.Length.ShouldBe(1);
        li.FirstElementChild!.TagName.ShouldBe("A");
        li.FirstElementChild!.GetAttribute("href").ShouldBe("/docs");
    }

    [Fact]
    public void Active_link_gets_menu_active_on_the_anchor_not_the_li()
    {
        IRenderedComponent<NavMenuLink> cut = Render<NavMenuLink>(ps => ps
            .Add(p => p.Href, "/")
            .Add(p => p.Match, NavLinkMatch.All)
            .AddChildContent("Home"));

        cut.Find("li > a").ClassList.ShouldContain("menu-active");
        cut.Find("li").ClassList.ShouldNotContain("menu-active");
    }

    [Fact]
    public void Disabled_link_marks_the_li_menu_disabled()
    {
        IRenderedComponent<NavMenuLink> cut = Render<NavMenuLink>(ps => ps
            .Add(p => p.Href, "/changelog")
            .Add(p => p.Disabled, true)
            .AddChildContent("Changelog"));

        cut.Find("li").ClassList.ShouldContain("menu-disabled");
        cut.Find("li > span").GetAttribute("aria-disabled").ShouldBe("true");
    }

    [Fact]
    public void Links_inside_a_menu_and_a_nested_group_never_nest_li_in_li()
    {
        // <ul class="menu"> <NavMenuLink/> <li><details><summary/><ul> <NavMenuLink/> </ul></details></li> </ul>
        RenderFragment menu = b =>
        {
            b.OpenElement(0, "ul");
            b.AddAttribute(1, "class", "menu");
            AddLink(b, 2, "/", "Home");
            b.OpenElement(3, "li");
            b.OpenElement(4, "details");
            b.AddAttribute(5, "open", true);
            b.AddMarkupContent(6, "<summary>Group</summary>");
            b.OpenElement(7, "ul");
            AddLink(b, 8, "/a", "A");
            b.CloseElement();
            b.CloseElement();
            b.CloseElement();
            b.CloseElement();
        };

        IRenderedComponent<IComponent> cut = Render(menu);

        cut.FindAll("li > li").Count.ShouldBe(0, "a li must sit in a ul, never directly in another li");
        cut.FindAll("ul.menu > li > a").Count.ShouldBe(1);
        cut.FindAll("details > ul > li > a").Count.ShouldBe(1);
    }

    private static void AddLink(RenderTreeBuilder b, int seq, string href, string label)
    {
        b.OpenComponent<NavMenuLink>(seq);
        b.AddAttribute(seq, nameof(NavMenuLink.Href), href);
        b.AddAttribute(seq, nameof(NavMenuLink.ChildContent), (RenderFragment)(c => c.AddContent(0, label)));
        b.CloseComponent();
    }
}
