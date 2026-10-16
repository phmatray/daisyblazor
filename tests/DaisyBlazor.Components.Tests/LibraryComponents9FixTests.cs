using AngleSharp.Dom;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Regression tests for the library-components-9 bucket: Status, Step, ListItem, Tab, Navbar, Dialog, Radio.
/// </summary>
public class LibraryComponents9FixTests : BunitContext
{
    public LibraryComponents9FixTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    public enum Plan { Free, Team, Enterprise }

    [Fact]
    public void Animated_status_overlays_both_layers_in_one_grid_cell()
    {
        IRenderedComponent<Status> cut = Render<Status>(ps => ps
            .Add(p => p.Animated, true)
            .Add(p => p.AriaLabel, "Online"));

        cut.Markup.ShouldNotContain("[col-start-1]");
        IReadOnlyList<IElement> layers = cut.FindAll("span.status");
        layers.Count.ShouldBe(2);
        layers.ShouldAllBe(l => l.GetAttribute("style")!.Contains("grid-area:1/1"));
        layers[0].GetAttribute("aria-hidden").ShouldBe("true");
        cut.Find("span.inline-grid").GetAttribute("role").ShouldBe("img");
    }

    [Fact]
    public void Status_gets_a_role_only_when_labelled()
    {
        Render<Status>(ps => ps.Add(p => p.AriaLabel, "Online")).Find("span").GetAttribute("role").ShouldBe("img");
        Render<Status>().Find("span").HasAttribute("role").ShouldBeFalse();
    }

    [Fact]
    public void Active_step_shares_the_completed_colour_and_is_aria_current()
    {
        IElement done = Render<Step>(ps => ps.Add(p => p.Completed, true)).Find("li");
        IElement active = Render<Step>(ps => ps.Add(p => p.Active, true)).Find("li");

        done.ClassList.ShouldContain("step-primary");
        done.GetAttribute("data-content").ShouldBe("✓");
        done.HasAttribute("aria-current").ShouldBeFalse();
        active.ClassList.ShouldContain("step-primary");
        active.ClassList.ShouldNotContain("step-secondary");
        active.HasAttribute("data-content").ShouldBeFalse();
        active.GetAttribute("aria-current").ShouldBe("step");
    }

    [Fact]
    public void Disabled_link_item_is_not_activatable_and_not_double_dimmed()
    {
        IElement a = Render<ListItem>(ps => ps
            .Add(p => p.Href, "/billing")
            .Add(p => p.Disabled, true)
            .Add(p => p.Text, "Billing")).Find("a");

        a.HasAttribute("href").ShouldBeFalse();
        a.GetAttribute("aria-disabled").ShouldBe("true");
        a.ClassList.ShouldContain("menu-disabled");
        a.ClassList.ShouldNotContain("opacity-50");
    }

    [Fact]
    public void Tab_class_style_and_attributes_reach_the_tab_button()
    {
        IRenderedComponent<Tabs> cut = Render<Tabs>(ps => ps
            .AddChildContent<Tab>(t => t
                .Add(x => x.Title, "One")
                .Add(x => x.Class, "font-bold")
                .Add(x => x.Style, "min-width:8rem")
                .AddUnmatched("data-test", "one")
                .AddChildContent("1")));

        IElement tab = cut.Find("[role=tab]");
        tab.ClassList.ShouldContain("font-bold");
        tab.ClassList.ShouldContain("tab");
        tab.GetAttribute("style").ShouldBe("min-width:8rem");
        tab.GetAttribute("data-test").ShouldBe("one");
    }

    [Theory]
    [InlineData(Color.Primary, "primary")]
    [InlineData(Color.Secondary, "secondary")]
    public void Navbar_color_sets_matching_content_colour(Color color, string token)
    {
        IElement nav = Render<Navbar>(ps => ps.Add(p => p.Color, color)).Find(".navbar");
        nav.ClassList.ShouldContain($"bg-{token}");
        nav.ClassList.ShouldContain($"text-{token}-content");
    }

    [Fact]
    public void Dialog_renders_class_style_and_attributes_on_a_root_wrapper()
    {
        IRenderedComponent<Dialog> cut = Render<Dialog>(ps => ps
            .Add(p => p.Title, "Deploy?")
            .Add(p => p.Class, "modal-box")
            .Add(p => p.Style, "max-width:30rem")
            .AddUnmatched("data-test", "dlg"));

        IElement root = cut.Find("[data-test=dlg]");
        root.ClassList.ShouldContain("modal-box");
        root.GetAttribute("style").ShouldBe("max-width:30rem");
        root.QuerySelector("h3")!.ClassList.ShouldNotContain("modal-box");
    }

    [Fact]
    public void Radio_of_value_type_uses_Value_when_Option_is_not_supplied()
    {
        IRenderedComponent<RadioGroup<Plan>> cut = Render<RadioGroup<Plan>>(ps => ps
            .Add(p => p.Value, Plan.Team)
            .AddChildContent<Radio<Plan>>(ip => ip.Add(p => p.Value, Plan.Free))
            .AddChildContent<Radio<Plan>>(ip => ip.Add(p => p.Value, Plan.Team)));

        IReadOnlyList<IElement> radios = cut.FindAll("input[type=radio]");
        radios[0].HasAttribute("checked").ShouldBeFalse();
        radios[1].HasAttribute("checked").ShouldBeTrue();
    }

    [Fact]
    public void Radio_of_int_still_honours_Option()
    {
        IRenderedComponent<RadioGroup<int>> cut = Render<RadioGroup<int>>(ps => ps
            .Add(p => p.Value, 0)
            .AddChildContent<Radio<int>>(ip => ip.Add(p => p.Option, 0))
            .AddChildContent<Radio<int>>(ip => ip.Add(p => p.Value, 5)));

        IReadOnlyList<IElement> radios = cut.FindAll("input[type=radio]");
        radios[0].HasAttribute("checked").ShouldBeTrue();
        radios[1].HasAttribute("checked").ShouldBeFalse();
    }
}
