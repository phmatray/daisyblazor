using AngleSharp.Dom;
using DaisyBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DaisyBlazor.Components.Tests;

/// <summary>Keyboard, ARIA and layout fixes for Menu, NavMenuLink, Chip, Tabs, Dock, Tooltip and DatePicker.</summary>
public class LibraryComponents8FixTests : BunitContext
{
    public LibraryComponents8FixTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    private IRenderedComponent<Menu> RenderMenu() => Render<Menu>(ps => ps
        .Add(p => p.Label, "Actions")
        .AddChildContent<MenuItem>(item => item.AddChildContent("Rename")));

    [Fact]
    public void Menu_trigger_exposes_its_expanded_state_and_Escape_closes()
    {
        IRenderedComponent<Menu> cut = RenderMenu();
        IElement trigger = cut.Find("button[aria-haspopup=menu]");
        trigger.GetAttribute("aria-expanded").ShouldBe("false");

        trigger.Click();
        cut.Find("button[aria-haspopup=menu]").GetAttribute("aria-expanded").ShouldBe("true");
        cut.FindAll("ul[role=menu]").Count.ShouldBe(1);

        cut.Find("div.dropdown").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        cut.FindAll("ul[role=menu]").ShouldBeEmpty();
        cut.Find("button[aria-haspopup=menu]").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public void MenuItem_without_Href_is_a_focusable_menuitem_button()
    {
        IRenderedComponent<Menu> cut = RenderMenu();
        cut.Find("button[aria-haspopup=menu]").Click();

        IElement item = cut.Find("ul[role=menu] li[role=none] > button[role=menuitem]");
        item.GetAttribute("type").ShouldBe("button");
        cut.Find("ul[role=menu]").HasAttribute("tabindex").ShouldBeFalse();
    }

    [Fact]
    public void Disabled_MenuItem_link_drops_href_and_is_aria_disabled()
    {
        IRenderedComponent<MenuItem> cut = Render<MenuItem>(ps => ps
            .Add(p => p.Href, "/billing")
            .Add(p => p.Target, "_blank")
            .Add(p => p.Disabled, true)
            .AddChildContent("Billing"));

        IElement a = cut.Find("a");
        a.HasAttribute("href").ShouldBeFalse();
        a.HasAttribute("target").ShouldBeFalse();
        a.GetAttribute("aria-disabled").ShouldBe("true");
    }

    [Fact]
    public void Disabled_NavMenuLink_is_aria_disabled()
    {
        IRenderedComponent<NavMenuLink> cut = Render<NavMenuLink>(ps => ps
            .Add(p => p.Href, "/changelog")
            .Add(p => p.Disabled, true)
            .AddChildContent("Changelog"));

        IElement span = cut.Find("span");
        span.GetAttribute("aria-disabled").ShouldBe("true");
        span.GetAttribute("role").ShouldBe("link");
    }

    [Fact]
    public void Chip_close_button_is_named_after_the_chip()
    {
        IRenderedComponent<Chip> cut = Render<Chip>(ps => ps
            .Add(p => p.Text, "Acme Cloud")
            .Add(p => p.OnClose, EventCallback.Factory.Create<Chip>(this, _ => { })));

        cut.Find("button").GetAttribute("aria-label").ShouldBe("Remove Acme Cloud");
    }

    [Fact]
    public void Chip_with_OnClick_and_OnClose_never_nests_buttons()
    {
        int clicks = 0;
        int closes = 0;
        IRenderedComponent<Chip> cut = Render<Chip>(ps => ps
            .Add(p => p.Text, "Staging")
            .Add(p => p.OnClick, EventCallback.Factory.Create<MouseEventArgs>(this, () => clicks++))
            .Add(p => p.OnClose, EventCallback.Factory.Create<Chip>(this, _ => closes++)));

        cut.FindAll("button button").ShouldBeEmpty();
        cut.Find("span.badge").ShouldNotBeNull();

        IReadOnlyList<IElement> buttons = cut.FindAll("span.badge > button");
        buttons.Count.ShouldBe(2);
        buttons[0].Click();
        cut.Find("button[aria-label='Remove Staging']").Click();
        clicks.ShouldBe(1);
        closes.ShouldBe(1);
    }

    [Fact]
    public void Tab_panel_is_shown_by_a_utility_class_not_scoped_css()
    {
        IRenderedComponent<Tabs> cut = Render<Tabs>(ps => ps
            .AddChildContent<Tab>(t => t.Add(p => p.Title, "Overview").AddChildContent("Panel")));

        cut.Find("[role=tabpanel]").ClassList.ShouldContain("block");
    }

    [Fact]
    public void Dock_is_fixed_by_default_and_inline_when_Fixed_is_false()
    {
        Render<Dock>().Find("div.dock").ClassList.ShouldNotContain("relative");
        Render<Dock>(ps => ps.Add(p => p.Fixed, false)).Find("div.dock").ClassList.ShouldContain("relative");
    }

    [Fact]
    public void Tooltip_takes_Color_and_Open_and_exposes_its_text()
    {
        IRenderedComponent<Tooltip> cut = Render<Tooltip>(ps => ps
            .Add(p => p.Text, "Copy link")
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Open, true)
            .AddChildContent("<span>icon</span>"));

        IElement root = cut.Find("div.tooltip");
        root.ClassList.ShouldContain("tooltip-primary");
        root.ClassList.ShouldContain("tooltip-open");
        cut.Find("span.sr-only").TextContent.ShouldBe("Copy link");
    }

    [Fact]
    public void Picker_only_DatePicker_makes_the_native_picker_the_keyboard_path()
    {
        IRenderedComponent<DatePicker> cut = Render<DatePicker>(ps => ps
            .Add(p => p.Label, "Renewal date")
            .Add(p => p.Editable, false));

        IElement picker = cut.Find("input[type=date]");
        picker.HasAttribute("tabindex").ShouldBeFalse();
        picker.HasAttribute("aria-hidden").ShouldBeFalse();
        picker.GetAttribute("aria-label").ShouldBe("Renewal date");
        cut.Find("input[type=text]").GetAttribute("tabindex").ShouldBe("-1");
    }

    [Fact]
    public void Editable_DatePicker_keeps_the_native_picker_out_of_the_tab_order()
    {
        IRenderedComponent<DatePicker> cut = Render<DatePicker>();

        cut.Find("input[type=date]").GetAttribute("tabindex").ShouldBe("-1");
        cut.Find("input[type=text]").HasAttribute("tabindex").ShouldBeFalse();
    }
}
