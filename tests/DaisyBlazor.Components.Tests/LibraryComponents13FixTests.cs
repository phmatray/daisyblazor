using System.Globalization;
using DaisyBlazor.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace DaisyBlazor.Components.Tests;

/// <summary>Regression tests for the library-components-13 bucket (RadialProgress, Table, Switch, Badge, Avatar).</summary>
public class LibraryComponents13FixTests : BunitContext
{
    [Fact]
    public void RadialProgress_is_culture_invariant_rounded_and_clamped()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
        try
        {
            AngleSharp.Dom.IElement el = Render<RadialProgress>(ps => ps.Add(p => p.Value, 44.859813)).Find(".radial-progress");
            el.GetAttribute("style")!.ShouldContain("--value:44.859813;");
            el.GetAttribute("aria-valuenow").ShouldBe("44.859813");
            el.TextContent.Trim().ShouldBe("45%");

            AngleSharp.Dom.IElement over = Render<RadialProgress>(ps => ps.Add(p => p.Value, 150)).Find(".radial-progress");
            over.GetAttribute("aria-valuenow").ShouldBe("100");
            over.TextContent.Trim().ShouldBe("100%");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void RadialProgress_hides_the_cap_at_zero_and_scales_its_label()
    {
        IRenderedComponent<RadialProgress> cut = Render<RadialProgress>(ps => ps.Add(p => p.Value, 0));
        cut.Find(".radial-progress").GetAttribute("style")!.ShouldContain("--thickness:0px;");
        cut.Find(".radial-progress span").GetAttribute("style")!.ShouldContain("var(--size)");
    }

    [Fact]
    public void RadialProgress_has_an_accessible_name_and_visible_dark_color()
    {
        AngleSharp.Dom.IElement el = Render<RadialProgress>(ps => ps
            .Add(p => p.Value, 10)
            .Add(p => p.AriaLabel, "Storage used")
            .Add(p => p.Color, Color.Dark)).Find(".radial-progress");

        el.GetAttribute("aria-label").ShouldBe("Storage used");
        el.ClassList.ShouldContain("text-base-content");
        el.ClassList.ShouldNotContain("text-neutral");
    }

    private RenderFragment<int> Row => i => b => { b.OpenElement(0, "td"); b.AddContent(1, i); b.CloseElement(); };

    [Fact]
    public void Table_rows_activate_with_the_keyboard()
    {
        int? clicked = null;
        IRenderedComponent<Table<int>> cut = Render<Table<int>>(ps => ps
            .Add(p => p.Items, new[] { 1, 2, 3 })
            .Add(p => p.RowTemplate, Row)
            .Add(p => p.OnRowClick, (TableRowClickEventArgs<int> e) => clicked = e.Item));

        AngleSharp.Dom.IElement row = cut.FindAll("tbody tr")[1];
        row.GetAttribute("tabindex").ShouldBe("0");
        row.KeyDown(new KeyboardEventArgs { Key = "Enter" });
        clicked.ShouldBe(2);
    }

    [Fact]
    public void Table_keeps_page_size_across_rerenders_and_clamps_page_when_items_shrink()
    {
        IRenderedComponent<Table<int>> cut = Render<Table<int>>(ps => ps
            .Add(p => p.Items, Enumerable.Range(1, 30))
            .Add(p => p.RowsPerPage, 10)
            .Add(p => p.RowTemplate, Row));

        cut.InvokeAsync(() => cut.Instance.SetPageSize(5));
        cut.Render(ps => ps.Add(p => p.Items, Enumerable.Range(1, 30)));
        cut.Instance.PageSize.ShouldBe(5);
        cut.FindAll("tbody tr").Count.ShouldBe(5);

        cut.InvokeAsync(() => cut.Instance.SetPage(5));
        cut.Render(ps => ps.Add(p => p.Items, Enumerable.Range(1, 7)));
        cut.Instance.CurrentPage.ShouldBe(1);
        cut.FindAll("tbody tr").Count.ShouldBe(2);
    }

    [Fact]
    public void ReadOnly_switch_cancels_clicks_and_announces_as_switch()
    {
        bool raised = false;
        IRenderedComponent<Switch> cut = Render<Switch>(ps => ps
            .Add(p => p.ReadOnly, true)
            .Add(p => p.ValueChanged, (bool _) => raised = true));

        AngleSharp.Dom.IElement input = cut.Find("input");
        input.GetAttribute("role").ShouldBe("switch");
        input.GetAttribute("aria-readonly").ShouldBe("true");
        cut.Markup.ShouldContain("blazor:onclick:preventDefault");
        input.Change(true);
        raised.ShouldBeFalse();
    }

    [Fact]
    public void Inline_badge_honours_dot_and_bordered()
    {
        AngleSharp.Dom.IElement el = Render<Badge>(ps => ps
            .Add(p => p.Inline, true)
            .Add(p => p.Dot, true)
            .Add(p => p.Bordered, true)
            .AddChildContent("New")).Find(".badge");

        el.TextContent.ShouldBeEmpty();
        el.ClassList.ShouldContain("border-2");
    }

    [Theory]
    [InlineData(150L)]
    [InlineData(150.5)]
    [InlineData("150")]
    public void Badge_max_caps_any_numeric_content(object content)
    {
        Render<Badge>(ps => ps.Add(p => p.Content, content)).Find(".badge").TextContent.Trim().ShouldBe("99+");
    }

    [Fact]
    public void Badge_overlap_pulls_the_badge_inward()
    {
        Render<Badge>(ps => ps.Add(p => p.Content, 3).Add(p => p.Overlap, true))
            .Find(".badge").GetAttribute("style")!.ShouldContain("--indicator-x");
    }

    [Fact]
    public void Avatar_applies_variant_shape_and_class_to_the_sized_element()
    {
        IRenderedComponent<Avatar> cut = Render<Avatar>(ps => ps
            .Add(p => p.Variant, Variant.Outlined)
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Square, true)
            .Add(p => p.Class, "w-24 ring-2")
            .AddChildContent("AS"));

        AngleSharp.Dom.IElement inner = cut.Find(".avatar > div");
        inner.ClassList.ShouldContain("w-24");
        inner.ClassList.ShouldContain("ring-2");
        inner.ClassList.ShouldNotContain("w-12");
        inner.ClassList.ShouldContain("rounded-none");
        inner.ClassList.ShouldContain("border-2");
        inner.ClassList.ShouldContain("text-primary");
        inner.ClassList.ShouldNotContain("bg-primary");
    }

    [Fact]
    public void Avatar_image_without_alt_is_decorative()
    {
        Render<Avatar>(ps => ps.Add(p => p.Image, "/a.png")).Find("img").GetAttribute("alt").ShouldBe(string.Empty);
    }
}
