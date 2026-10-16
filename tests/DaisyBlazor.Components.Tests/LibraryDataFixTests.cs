using DaisyBlazor.Data;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Pins the DataGrid / DataTable / FilterPanel / KpiCard visual and accessibility fixes.
/// </summary>
public class LibraryDataFixTests : BunitContext
{
    public record Account(string Name, int Seats);

    private static readonly Account[] Accounts =
        Enumerable.Range(1, 6).Select(i => new Account($"Acme {i}", i)).ToArray();

    private static RenderFragment NameColumn => b =>
    {
        b.OpenComponent<PropertyColumn<Account, string>>(0);
        b.AddAttribute(1, "Property", (System.Linq.Expressions.Expression<Func<Account, string>>)(a => a.Name));
        b.AddAttribute(2, "Title", "Name");
        b.AddAttribute(3, "Sortable", true);
        b.CloseComponent();
    };

    [Fact]
    public void DataGrid_hover_uses_daisyui5_row_hover()
    {
        IRenderedComponent<DataGrid<Account>> cut = Render<DataGrid<Account>>(ps => ps
            .Add(p => p.Items, Accounts)
            .Add(p => p.Hover, true)
            .Add(p => p.Columns, NameColumn));

        cut.FindAll("tbody tr").ShouldAllBe(tr => tr.ClassList.Contains("row-hover"));
    }

    [Fact]
    public void Sortable_header_is_a_button_with_aria_sort()
    {
        IRenderedComponent<DataGrid<Account>> cut = Render<DataGrid<Account>>(ps => ps
            .Add(p => p.Items, Accounts)
            .Add(p => p.Columns, NameColumn));

        cut.Find("th").GetAttribute("aria-sort").ShouldBe("none");
        cut.Find("th button[type=button]").Click();
        cut.Find("th").GetAttribute("aria-sort").ShouldBe("ascending");
        cut.Find("th button").Click();
        cut.Find("th").GetAttribute("aria-sort").ShouldBe("descending");
    }

    [Fact]
    public void DataTable_pages_by_first_page_size_option()
    {
        IRenderedComponent<DataTable<Account>> cut = Render<DataTable<Account>>(ps => ps
            .Add(p => p.Items, Accounts)
            .Add(p => p.PageSizeOptions, new[] { 5, 10 })
            .Add(p => p.Columns, NameColumn));

        cut.FindAll("tbody tr").Count.ShouldBe(5);
        cut.Markup.ShouldContain("1-5");
    }

    [Fact]
    public void DataTable_page_size_survives_a_parent_rerender()
    {
        IRenderedComponent<DataTable<Account>> cut = Render<DataTable<Account>>(ps => ps
            .Add(p => p.Items, Accounts)
            .Add(p => p.PageSizeOptions, new[] { 5, 10 })
            .Add(p => p.Columns, NameColumn));

        cut.Find("select").Change("10");
        cut.Render(ps => ps.Add(p => p.Loading, false));

        cut.FindAll("tbody tr").Count.ShouldBe(6);
    }

    [Fact]
    public void DataTable_actions_column_has_a_header_and_muted_empty_state()
    {
        IRenderedComponent<DataTable<Account>> cut = Render<DataTable<Account>>(ps => ps
            .Add(p => p.Items, Array.Empty<Account>())
            .Add(p => p.Columns, NameColumn)
            .Add(p => p.Actions, (RenderFragment<Account>)(_ => b => b.AddContent(0, "edit"))));

        cut.FindAll("th")[1].TextContent.Trim().ShouldBe("Actions");
        cut.Markup.ShouldContain("text-base-content/60");
        cut.Markup.ShouldNotContain("text-secondary");
    }

    [Fact]
    public void Pager_select_is_labelled_and_tracks_sort_reset()
    {
        Account[] many = Enumerable.Range(1, 30).Select(i => new Account($"Acme {i:00}", i)).ToArray();
        IRenderedComponent<DataGrid<Account>> cut = Render<DataGrid<Account>>(ps => ps
            .Add(p => p.Items, many)
            .Add(p => p.Columns, NameColumn)
            .Add(p => p.PagerContent, (RenderFragment)(b =>
            {
                b.OpenComponent<DataGridPager<Account>>(0);
                b.CloseComponent();
            })));

        cut.Find("label select").ShouldNotBeNull();

        cut.FindAll(".join button")[1].Click();
        cut.Markup.ShouldContain("11-20");

        // Sorting resets to page 0; a pager with no complex parameters must follow.
        cut.Find("th button").Click();
        cut.Markup.ShouldContain("1-10");
    }

    [Fact]
    public void FilterPanel_shows_one_clear_action_and_no_baked_margin()
    {
        IRenderedComponent<FilterPanel> cut = Render<FilterPanel>(ps => ps
            .Add(p => p.ActiveFilters, new[] { new ActiveFilter("plan", "Plan: Pro") })
            .Add(p => p.OnApply, () => { })
            .Add(p => p.OnClear, () => { })
            .AddChildContent("filters"));

        cut.Markup.ShouldNotContain("Reset");
        cut.Markup.ShouldNotContain("mb-4");
    }

    [Fact]
    public void FilterPanel_reset_raises_its_own_callback()
    {
        bool reset = false, cleared = false;
        IRenderedComponent<FilterPanel> cut = Render<FilterPanel>(ps => ps
            .Add(p => p.ActiveFilters, new[] { new ActiveFilter("plan", "Plan: Pro") })
            .Add(p => p.OnApply, () => { })
            .Add(p => p.OnClear, () => cleared = true)
            .Add(p => p.OnReset, () => reset = true)
            .AddChildContent("filters"));

        cut.FindAll("button").First(b => b.TextContent.Contains("Reset")).Click();

        reset.ShouldBeTrue();
        cleared.ShouldBeFalse();
    }

    [Fact]
    public void KpiCard_value_is_not_drawn_in_the_accent_colour()
    {
        IRenderedComponent<KpiCard> cut = Render<KpiCard>(ps => ps
            .Add(p => p.Label, "Open incidents")
            .Add(p => p.Value, "42")
            .Add(p => p.Color, "var(--color-warning)"));

        cut.Markup.ShouldContain("border-left: 4px solid var(--color-warning)");
        cut.Markup.ShouldNotContain("color: var(--color-warning)");
    }
}
