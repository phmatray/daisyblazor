using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>Regression tests for the library-components-12 bucket (Select, tables, Rating, ListRow,
/// Typography, ExpansionPanel, Hero).</summary>
public class LibraryComponents12FixTests : BunitContext
{
    private IRenderedComponent<Select<string>> RenderSelect(Action<ComponentParameterCollectionBuilder<Select<string>>> extra) =>
        Render<Select<string>>(ps =>
        {
            extra(ps);
            ps.AddChildContent<SelectItem<string>>(ip => ip.Add(p => p.Value, "eu").AddChildContent("Europe"));
            ps.AddChildContent<SelectItem<string>>(ip => ip
                .Add(p => p.Value, "na")
                .Add(p => p.Class, "font-bold")
                .Add(p => p.Style, "color:red")
                .AddUnmatched("data-region", "na")
                .AddChildContent("North America"));
        });

    [Fact]
    public void MultiSelection_is_a_listbox_not_a_select_strip()
    {
        IElement select = RenderSelect(ps => ps.Add(p => p.MultiSelection, true)).Find("select");

        select.ClassList.ShouldNotContain("select");
        select.ClassList.ShouldContain("textarea");
        select.GetAttribute("size").ShouldBe("2");
    }

    [Fact]
    public void Single_select_drives_selection_through_value()
    {
        IRenderedComponent<Select<string>> cut = RenderSelect(ps => ps.Add(p => p.Value, "na"));
        cut.Find("select").GetAttribute("value").ShouldBe("1");

        cut.Render(ps => ps.Add(p => p.Value, "eu"));
        cut.Find("select").GetAttribute("value").ShouldBe("0");
    }

    [Fact]
    public void Unmatched_value_shows_an_empty_placeholder_instead_of_the_first_option()
    {
        IRenderedComponent<Select<string>> cut = RenderSelect(ps => ps.Add(p => p.Value, null));

        IElement first = cut.FindAll("option")[0];
        first.GetAttribute("value").ShouldBe("");
        first.HasAttribute("selected").ShouldBeTrue();
        first.HasAttribute("hidden").ShouldBeTrue();
        cut.Find("select").GetAttribute("value").ShouldBe("");

        // A matching value removes the placeholder.
        cut.Render(ps => ps.Add(p => p.Value, "eu"));
        cut.FindAll("option").Count.ShouldBe(2);
    }

    [Fact]
    public void Required_sets_the_attribute_and_items_forward_class_style_and_attributes()
    {
        IRenderedComponent<Select<string>> cut = RenderSelect(ps => ps.Add(p => p.Value, "eu").Add(p => p.Required, true));

        cut.Find("select").HasAttribute("required").ShouldBeTrue();
        IElement na = cut.Find("option[data-region=na]");
        na.ClassList.ShouldContain("font-bold");
        na.GetAttribute("style").ShouldBe("color:red");
    }

    [Fact]
    public void SimpleTable_striped_hover_uses_base_300_and_fixed_header_pins_rows()
    {
        IElement table = Render<SimpleTable>(ps => ps
            .Add(p => p.Hover, true)
            .Add(p => p.Striped, true)
            .Add(p => p.FixedHeader, true)).Find("table");

        table.ClassList.ShouldContain("[&_tbody_tr:hover]:bg-base-300");
        table.ClassList.ShouldContain("table-pin-rows");
    }

    [Fact]
    public void Table_hover_puts_row_hover_on_rows()
    {
        IRenderedComponent<Table<string>> cut = Render<Table<string>>(ps => ps
            .Add(p => p.Items, new[] { "a", "b" })
            .Add(p => p.Hover, true)
            .Add(p => p.Striped, true)
            .Add(p => p.RowTemplate, item => b => b.AddMarkupContent(0, $"<td>{item}</td>")));

        cut.FindAll("tbody tr").ShouldAllBe(tr => tr.ClassList.Contains("row-hover"));
        cut.Find("table").ClassName!.ShouldNotContain("hover:");
    }

    [Fact]
    public void Rating_clicking_the_current_star_clears_it_and_radios_are_labelled()
    {
        int? raised = null;
        IRenderedComponent<Rating> cut = Render<Rating>(ps => ps
            .Add(p => p.Value, 3)
            .Add(p => p.ValueChanged, v => raised = v));

        cut.Find("div.rating").GetAttribute("role").ShouldBe("radiogroup");
        IReadOnlyList<IElement> stars = cut.FindAll("input.mask");
        stars[2].GetAttribute("aria-label").ShouldBe("3 of 5");

        stars[2].Click();
        raised.ShouldBe(0);
        cut.Find("input.rating-hidden").HasAttribute("checked").ShouldBeTrue();
    }

    [Fact]
    public void Rating_clicking_another_star_does_not_clear()
    {
        int? raised = null;
        IRenderedComponent<Rating> cut = Render<Rating>(ps => ps
            .Add(p => p.Value, 3)
            .Add(p => p.ValueChanged, v => raised = v));

        cut.FindAll("input.mask")[3].Click();
        raised.ShouldBeNull();
    }

    [Fact]
    public void ListRow_trailing_content_is_a_plain_right_column()
    {
        IRenderedComponent<ListRow> cut = Render<ListRow>(ps => ps
            .Add(p => p.Title, "Billing API")
            .Add(p => p.TrailingContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span id=\"t\">ok</span>"))));

        cut.Markup.ShouldNotContain("list-col-wrap");
        cut.Find("#t").ParentElement!.ParentElement!.ClassList.ShouldContain("list-row");
    }

    [Theory]
    [InlineData("center", "text-center")]
    [InlineData("Right", "text-right")]
    [InlineData("start", "text-left")]
    public void Typography_align_maps_to_text_alignment(string align, string expected)
    {
        IElement h = Render<Typography>(ps => ps.Add(p => p.Typo, Typo.h2).Add(p => p.Align, align)).Find("h2");

        h.ClassList.ShouldContain(expected);
        h.ClassList.ShouldNotContain("font-heading");
    }

    [Fact]
    public void ExpansionPanels_dense_and_elevation_reach_the_panels()
    {
        IRenderedComponent<ExpansionPanels> cut = Render<ExpansionPanels>(ps => ps
            .Add(p => p.Dense, true)
            .Add(p => p.Elevation, 4)
            .AddChildContent<ExpansionPanel>(pp => pp.Add(p => p.Text, "Plan")));

        cut.Find("details").ClassList.ShouldContain("shadow-md");
        cut.Find("summary").ClassList.ShouldContain("py-2");
    }

    [Fact]
    public void ExpansionPanel_user_toggle_raises_expanded_changed()
    {
        bool? raised = null;
        IRenderedComponent<ExpansionPanel> cut = Render<ExpansionPanel>(ps => ps
            .Add(p => p.Text, "Plan")
            .Add(p => p.IsExpandedChanged, v => raised = v));

        // Initially closed: the user opening it is the first toggle.
        cut.Find("details").TriggerEvent("ontoggle", EventArgs.Empty);
        raised.ShouldBe(true);
        cut.Find("details").HasAttribute("open").ShouldBeTrue();
    }

    [Fact]
    public void ExpansionPanel_toggle_caused_by_initial_open_is_not_reported()
    {
        bool? raised = null;
        IRenderedComponent<ExpansionPanel> cut = Render<ExpansionPanel>(ps => ps
            .Add(p => p.IsExpanded, true)
            .Add(p => p.IsExpandedChanged, v => raised = v));

        cut.Find("details").TriggerEvent("ontoggle", EventArgs.Empty); // browser opening it on insert
        raised.ShouldBeNull();
        cut.Find("details").TriggerEvent("ontoggle", EventArgs.Empty); // user closes it
        raised.ShouldBe(false);
    }

    [Fact]
    public void Hero_min_height_can_be_cleared_and_background_url_is_escaped()
    {
        IElement root = Render<Hero>(ps => ps
            .Add(p => p.MinHeight, null)
            .Add(p => p.BackgroundImage, "a\"b.png")).Find("div.hero");

        root.ClassList.ShouldNotContain("min-h-screen");
        root.GetAttribute("style").ShouldBe("background-image:url(\"a\\\"b.png\");");
        root.QuerySelector(".hero-overlay").ShouldNotBeNull();
    }
}
