using AngleSharp.Dom;
using DaisyBlazor;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>Accessibility and layout fixes for Menu, Fab, Timeline, Tabs, Dock, Loading and ColorPicker.</summary>
public class LibraryComponents7FixTests : BunitContext
{
    public LibraryComponents7FixTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void Menu_icon_activator_has_an_accessible_name_and_expanded_state()
    {
        IRenderedComponent<Menu> cut = Render<Menu>(ps => ps.Add(p => p.Icon, "more_vert"));

        IElement btn = cut.Find("button");
        btn.GetAttribute("aria-label").ShouldBe("Open menu");
        btn.GetAttribute("aria-expanded").ShouldBe("false");

        btn.Click();
        cut.Find("button").GetAttribute("aria-expanded").ShouldBe("true");
    }

    [Fact]
    public void Menu_icon_activator_uses_AriaLabel_when_set()
    {
        IRenderedComponent<Menu> cut = Render<Menu>(ps => ps
            .Add(p => p.Icon, "more_vert")
            .Add(p => p.AriaLabel, "Row actions"));

        cut.Find("button").GetAttribute("aria-label").ShouldBe("Row actions");
    }

    [Fact]
    public void Menu_Open_renders_expanded_and_reports_changes()
    {
        bool? reported = null;
        IRenderedComponent<Menu> cut = Render<Menu>(ps => ps
            .Add(p => p.Label, "Actions")
            .Add(p => p.Open, true)
            .Add(p => p.OpenChanged, EventCallback.Factory.Create<bool>(this, v => reported = v))
            .AddChildContent("<li>Item</li>"));

        cut.Find(".dropdown").ClassList.ShouldContain("dropdown-open");
        cut.FindAll("ul.dropdown-content").Count.ShouldBe(1);

        cut.Find("button").Click();
        reported.ShouldBe(false);
        cut.FindAll("ul.dropdown-content").Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(Origin.CenterLeft, Origin.TopLeft, "dropdown-left")]
    [InlineData(Origin.CenterRight, Origin.TopLeft, "dropdown-right")]
    [InlineData(Origin.BottomLeft, Origin.TopRight, "dropdown-end")]
    [InlineData(Origin.BottomLeft, Origin.TopCenter, "dropdown-center")]
    [InlineData(Origin.BottomLeft, Origin.BottomLeft, "dropdown-top")]
    public void Menu_origins_map_to_dropdown_placement(Origin anchor, Origin transform, string expected)
    {
        IRenderedComponent<Menu> cut = Render<Menu>(ps => ps
            .Add(p => p.Label, "Actions")
            .Add(p => p.AnchorOrigin, anchor)
            .Add(p => p.TransformOrigin, transform));

        cut.Find(".dropdown").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void Menu_Size_applies_to_the_label_button()
    {
        IRenderedComponent<Menu> cut = Render<Menu>(ps => ps
            .Add(p => p.Label, "Actions")
            .Add(p => p.Size, Size.Small));

        cut.Find("button").ClassList.ShouldContain("btn-sm");
    }

    [Fact]
    public void Fab_trigger_exposes_expanded_state_and_close_is_hidden_from_assistive_tech()
    {
        IRenderedComponent<Fab> cut = Render<Fab>(ps => ps.Add(p => p.AriaLabel, "Actions"));

        IElement trigger = cut.Find("[role=button]");
        trigger.GetAttribute("aria-haspopup").ShouldBe("true");
        trigger.GetAttribute("aria-expanded").ShouldBe("false");
        cut.Find(".fab-close").GetAttribute("aria-hidden").ShouldBe("true");

        cut.Find(".fab").FocusIn();
        cut.Find("[role=button]").GetAttribute("aria-expanded").ShouldBe("true");
        cut.Find(".fab").FocusOut();
        cut.Find("[role=button]").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public void Fab_Fixed_false_positions_it_absolute()
    {
        Render<Fab>().Find(".fab").ClassList.ShouldNotContain("absolute");
        Render<Fab>(ps => ps.Add(p => p.Fixed, false)).Find(".fab").ClassList.ShouldContain("absolute");
    }

    [Fact]
    public void TimelineItem_hides_outer_lines_and_has_a_fixed_middle_column()
    {
        IRenderedComponent<TimelineItem> cut = Render<TimelineItem>(ps => ps.Add(p => p.Size, Size.Large));

        IReadOnlyList<IElement> hrs = cut.FindAll("hr");
        hrs.Count.ShouldBe(2);
        hrs[0].ClassList.ShouldContain("[li:first-child>&]:hidden");
        hrs[1].ClassList.ShouldContain("[li:last-child>&]:hidden");
        cut.Find(".timeline-middle").ClassList.ShouldContain("w-5");
    }

    [Theory]
    [InlineData(TabsVariant.Lift)]
    [InlineData(TabsVariant.Box)]
    public void Tabs_lift_and_box_panels_get_a_bordered_surface(TabsVariant variant)
    {
        IRenderedComponent<Tabs> cut = Render<Tabs>(ps => ps
            .Add(p => p.Variant, variant)
            .AddChildContent<Tab>(t => t.Add(p => p.Title, "Overview").AddChildContent("Body")));

        IElement panel = cut.Find("[role=tabpanel]");
        panel.ClassList.ShouldContain("border-base-300");
        panel.ClassList.ShouldContain("bg-base-100");
        panel.ClassList.ShouldContain("rounded-box");
    }

    [Fact]
    public void Active_DockItem_sets_aria_current()
    {
        Render<DockItem>(ps => ps.Add(p => p.Href, "/home").Add(p => p.Active, true))
            .Find("a").GetAttribute("aria-current").ShouldBe("page");
        Render<DockItem>(ps => ps.Add(p => p.Active, true))
            .Find("button").GetAttribute("aria-current").ShouldBe("true");
        Render<DockItem>().Find("button").HasAttribute("aria-current").ShouldBeFalse();
    }

    [Fact]
    public void Loading_is_a_labelled_status()
    {
        IElement span = Render<Loading>().Find(".loading");
        span.GetAttribute("role").ShouldBe("status");
        span.GetAttribute("aria-label").ShouldBe("Loading");

        Render<Loading>(ps => ps.AddUnmatched("aria-label", "Loading tasks"))
            .Find(".loading").GetAttribute("aria-label").ShouldBe("Loading tasks");
    }

    [Fact]
    public void ColorPicker_label_points_at_the_colour_input()
    {
        IRenderedComponent<ColorPicker> cut = Render<ColorPicker>(ps => ps
            .Add(p => p.Label, "Brand colour")
            .Add(p => p.Text, "#e4572e"));

        string? id = cut.Find("input[type=color]").Id;
        id.ShouldNotBeNullOrEmpty();
        cut.Find("label.label").GetAttribute("for").ShouldBe(id);
        cut.FindAll("label").Count.ShouldBe(1);
    }
}
