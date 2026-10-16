using DaisyBlazor.Feedback;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>Regression tests for the library-components-5 bug batch.</summary>
public class LibraryComponents5FixTests : BunitContext
{
    [Fact]
    public void Surface_is_not_a_duplicate_of_Dark()
    {
        CssClass.ColorToken(Color.Surface).ShouldBeNull();
        CssClass.ColorToken(Color.Dark).ShouldBe("neutral");

        IRenderedComponent<Navbar> cut = Render<Navbar>(ps => ps.Add(p => p.Color, Color.Surface));
        cut.Find(".navbar").ClassList.ShouldContain("bg-base-100");
    }

    [Fact]
    public void Default_badge_color_is_ghost_so_it_stays_visible()
    {
        CssClass.BadgeColor(Color.Default).ShouldBe("badge-ghost");
        CssClass.BadgeColor(Color.Primary).ShouldBe("badge-primary");
        Render<Chip>(ps => ps.Add(p => p.Text, "Default")).Find(".badge").ClassList.ShouldContain("badge-ghost");
    }

    [Fact]
    public void Dark_text_contrasts_with_the_surface()
    {
        CssClass.TextColor(Color.Dark).ShouldBe("text-base-content");
    }

    [Fact]
    public void Loading_link_button_is_not_navigable()
    {
        IRenderedComponent<Button> cut = Render<Button>(ps => ps
            .Add(p => p.Href, "/billing")
            .Add(p => p.Loading, true));

        var a = cut.Find("a");
        a.HasAttribute("href").ShouldBeFalse();
        a.GetAttribute("aria-disabled").ShouldBe("true");
        a.ClassList.ShouldContain("btn-disabled");
    }

    [Fact]
    public void Large_button_icon_and_spinner_follow_size()
    {
        IRenderedComponent<Button> cut = Render<Button>(ps => ps
            .Add(p => p.Size, Size.Large)
            .Add(p => p.StartIcon, Icons.Material.Filled.Add));
        cut.Find(".material-symbols-rounded, span[aria-hidden]").GetAttribute("style")!.ShouldContain("font-size:1.5rem");

        IRenderedComponent<Button> loading = Render<Button>(ps => ps
            .Add(p => p.Size, Size.Large)
            .Add(p => p.Loading, true));
        loading.Find(".loading").ClassList.ShouldContain("loading-md");
    }

    [Fact]
    public void EmptyState_is_centred_muted_and_hosts_actions()
    {
        IRenderedComponent<EmptyState> cut = Render<EmptyState>(ps => ps
            .Add(p => p.Icon, Icons.Material.Filled.Inbox)
            .Add(p => p.Message, "No tickets")
            .Add(p => p.Subtitle, "New support tickets will show up here.")
            .Add(p => p.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0, "<button>Add ticket</button>"))));

        var ps = cut.FindAll("p");
        ps.ShouldAllBe(p => p.ClassList.Contains("text-center"));
        cut.Markup.ShouldNotContain("text-secondary");
        ps[1].ClassList.ShouldContain("text-base-content/60");
        cut.Find("button").TextContent.ShouldBe("Add ticket");
    }

    [Fact]
    public void Swap_checkbox_is_focusable_and_named()
    {
        IRenderedComponent<Swap> cut = Render<Swap>(ps => ps.Add(p => p.AriaLabel, "Dark mode"));

        var input = cut.Find("input[type=checkbox]");
        input.ClassList.ShouldNotContain("hidden");
        input.GetAttribute("aria-label").ShouldBe("Dark mode");
    }

    [Fact]
    public void ToggleItem_exposes_pressed_state()
    {
        IRenderedComponent<ToggleGroup<string>> cut = Render<ToggleGroup<string>>(ps => ps
            .Add(p => p.Value, "month")
            .Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<ToggleItem<string>>(0);
                b.AddAttribute(1, "Value", "month");
                b.AddAttribute(2, "Text", "Monthly");
                b.CloseComponent();
                b.OpenComponent<ToggleItem<string>>(3);
                b.AddAttribute(4, "Value", "year");
                b.AddAttribute(5, "Text", "Yearly");
                b.CloseComponent();
            })));

        var buttons = cut.FindAll("button");
        buttons[0].GetAttribute("aria-pressed").ShouldBe("true");
        buttons[1].GetAttribute("aria-pressed").ShouldBe("false");
    }
}
