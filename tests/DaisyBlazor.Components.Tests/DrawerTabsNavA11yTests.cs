using AngleSharp.Dom;
using DaisyBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace DaisyBlazor.Components.Tests;

/// <summary>Keyboard and ARIA behaviour of Drawer (#51), Tabs (#53) and NavMenuLink (#50).</summary>
public class DrawerTabsNavA11yTests : BunitContext
{
    public DrawerTabsNavA11yTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void NavMenuLink_marks_the_current_page_with_menu_active_and_aria_current()
    {
        IRenderedComponent<NavMenuLink> cut = Render<NavMenuLink>(ps => ps
            .Add(p => p.Href, "/")
            .Add(p => p.Match, NavLinkMatch.All)
            .AddChildContent("Home"));

        IElement a = cut.Find("a");
        a.ClassList.ShouldContain("menu-active");
        a.ClassList.ShouldNotContain("active");
        a.GetAttribute("aria-current").ShouldBe("page");
    }

    private IRenderedComponent<Drawer> RenderDrawer(bool open = false) => Render<Drawer>(ps => ps
        .Add(p => p.Id, "nav")
        .Add(p => p.Open, open)
        .Add(p => p.ToggleContent, "Menu")
        .Add(p => p.SideContent, "<a href=\"/x\">X</a>"));

    [Fact]
    public void Drawer_toggle_is_a_button_with_aria_expanded_and_controls()
    {
        IRenderedComponent<Drawer> cut = RenderDrawer();

        IElement toggle = cut.Find("button.drawer-button");
        toggle.GetAttribute("aria-expanded").ShouldBe("false");
        toggle.GetAttribute("aria-controls").ShouldBe("nav-side");
        cut.Find(".drawer-side").Id.ShouldBe("nav-side");
        // The hidden checkbox is no longer a second, invisible tab stop.
        cut.Find("input.drawer-toggle").GetAttribute("tabindex").ShouldBe("-1");

        toggle.Click();

        cut.Find("button.drawer-button").GetAttribute("aria-expanded").ShouldBe("true");
        cut.Instance.Open.ShouldBeTrue();
    }

    [Fact]
    public void Drawer_closed_side_is_inert_and_open_side_is_not()
    {
        RenderDrawer(open: false).Find(".drawer-side").HasAttribute("inert").ShouldBeTrue();
        RenderDrawer(open: true).Find(".drawer-side").HasAttribute("inert").ShouldBeFalse();
    }

    [Fact]
    public void Drawer_responsive_open_class_keeps_side_reachable()
    {
        IRenderedComponent<Drawer> cut = Render<Drawer>(ps => ps.Add(p => p.Class, "lg:drawer-open"));

        cut.Find(".drawer-side").HasAttribute("inert").ShouldBeFalse();
    }

    [Fact]
    public void Drawer_Escape_closes_and_focus_moves_in_then_back_to_toggle()
    {
        bool? raised = null;
        IRenderedComponent<Drawer> cut = Render<Drawer>(ps => ps
            .Add(p => p.ToggleContent, "Menu")
            .Add(p => p.OpenChanged, v => raised = v));

        cut.Find("button.drawer-button").Click();
        JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBeOfType<ElementReference>();

        cut.Find(".drawer-side").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        raised.ShouldBe(false);
        cut.Instance.Open.ShouldBeFalse();
        JSInterop.VerifyFocusAsyncInvoke(calledTimes: 2);
    }

    private IRenderedComponent<Tabs> RenderTabs() => Render<Tabs>(ps => ps
        .AddChildContent<Tab>(t => t.Add(x => x.Title, "One").AddChildContent("1"))
        .AddChildContent<Tab>(t => t.Add(x => x.Title, "Two").Add(x => x.Disabled, true).AddChildContent("2"))
        .AddChildContent<Tab>(t => t.Add(x => x.Title, "Three").AddChildContent("3")));

    private static string[] TabIndexes(IRenderedComponent<Tabs> cut) =>
        cut.FindAll("[role=tab]").Select(t => t.GetAttribute("tabindex") ?? "").ToArray();

    [Fact]
    public void Tabs_use_roving_tabindex()
    {
        TabIndexes(RenderTabs()).ShouldBe(["0", "-1", "-1"]);
    }

    [Theory]
    [InlineData("ArrowRight", 2)] // skips the disabled tab
    [InlineData("ArrowLeft", 2)]  // wraps from the first tab to the last
    [InlineData("End", 2)]
    [InlineData("Home", 0)]
    public void Tabs_arrow_home_end_move_selection_and_focus(string key, int expected)
    {
        IRenderedComponent<Tabs> cut = RenderTabs();

        cut.Find("[role=tablist]").KeyDown(new KeyboardEventArgs { Key = key });

        cut.Instance.ActiveIndex.ShouldBe(expected);
        cut.FindAll("[role=tab]")[expected].GetAttribute("aria-selected").ShouldBe("true");
        TabIndexes(cut)[expected].ShouldBe("0");
        JSInterop.VerifyFocusAsyncInvoke();
        IElement panel = cut.Find("[role=tabpanel]");
        panel.GetAttribute("aria-labelledby").ShouldBe(cut.FindAll("[role=tab]")[expected].Id);
    }
}
