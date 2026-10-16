using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace DaisyBlazor.Components.Tests;

/// <summary>Regression tests for the library-components-4 bug bucket.</summary>
public class LibraryComponents4FixTests : BunitContext
{
    private static RenderFragment Items(params string[] values) => b =>
    {
        int seq = 0;
        foreach (string v in values)
        {
            b.OpenComponent<ToggleItem<string>>(seq++);
            b.AddAttribute(seq++, nameof(ToggleItem<string>.Value), v);
            b.AddAttribute(seq++, nameof(ToggleItem<string>.Text), v);
            b.CloseComponent();
        }
    };

    [Fact]
    public void ToggleGroup_moves_highlight_off_previous_item()
    {
        IRenderedComponent<ToggleGroup<string>> cut = Render<ToggleGroup<string>>(ps => ps
            .Add(p => p.Value, "month")
            .Add(p => p.ChildContent, Items("week", "month")));

        cut.FindAll("button")[0].Click();

        IReadOnlyList<AngleSharp.Dom.IElement> buttons = cut.FindAll("button");
        buttons[0].ClassList.ShouldContain("btn-primary");
        buttons[1].ClassList.ShouldNotContain("btn-primary");
    }

    [Theory]
    [InlineData(Color.Dark, "btn-neutral")]
    [InlineData(Color.Surface, "btn-active")] // Surface carries no colour modifier (see CssClass)
    [InlineData(Color.Default, "btn-active")]
    public void ToggleGroup_maps_every_color(Color color, string expected)
    {
        IRenderedComponent<ToggleGroup<string>> cut = Render<ToggleGroup<string>>(ps => ps
            .Add(p => p.Value, "a")
            .Add(p => p.Color, color)
            .Add(p => p.ChildContent, Items("a")));

        cut.Find("button").ClassList.ShouldContain(expected);
    }

    [Fact]
    public void Pagination_maps_dark_to_neutral()
    {
        IRenderedComponent<Pagination> cut = Render<Pagination>(ps => ps
            .Add(p => p.Count, 3)
            .Add(p => p.Color, Color.Dark));

        cut.FindAll(".btn-neutral").Count.ShouldBe(1);
    }

    [Fact]
    public void ToggleGroup_multi_selection_toggles_values_and_shows_check_mark()
    {
        IEnumerable<string?>? values = null;
        IRenderedComponent<ToggleGroup<string>> cut = Render<ToggleGroup<string>>(ps => ps
            .Add(p => p.SelectionMode, SelectionMode.MultiSelection)
            .Add(p => p.CheckMark, true)
            .Add(p => p.ValuesChanged, v => values = v)
            .Add(p => p.ChildContent, Items("a", "b")));

        cut.FindAll("button")[0].Click();
        cut.FindAll("button")[1].Click();
        values.ShouldBe(new[] { "a", "b" });
        cut.FindAll("button[aria-pressed=true]").Count.ShouldBe(2);
        cut.Markup.ShouldContain("check");

        cut.FindAll("button")[0].Click();
        values.ShouldBe(new[] { "b" });
    }

    [Fact]
    public void ToggleGroup_toggle_selection_clears_on_second_click()
    {
        string? value = "x";
        IRenderedComponent<ToggleGroup<string>> cut = Render<ToggleGroup<string>>(ps => ps
            .Add(p => p.SelectionMode, SelectionMode.ToggleSelection)
            .Add(p => p.Value, "a")
            .Add(p => p.ValueChanged, v => value = v)
            .Add(p => p.ChildContent, Items("a")));

        cut.Find("button").Click();
        value.ShouldBeNull();
    }

    [Fact]
    public void Mockups_draw_a_border_width()
    {
        Render<MockupBrowser>().Find(".mockup-browser").ClassList.ShouldContain("border");
        Render<MockupWindow>().Find(".mockup-window").ClassList.ShouldContain("border");
    }

    [Fact]
    public void MockupBrowser_toolbar_replaces_url()
    {
        IRenderedComponent<MockupBrowser> cut = Render<MockupBrowser>(ps => ps
            .Add(p => p.Url, "acme.cloud")
            .Add(p => p.Toolbar, "<span id=\"tb\">custom</span>"));

        cut.Find("#tb").ShouldNotBeNull();
        cut.Markup.ShouldNotContain("acme.cloud");
    }

    [Fact]
    public void ButtonGroup_cascades_variant_color_size()
    {
        IRenderedComponent<ButtonGroup> cut = Render<ButtonGroup>(ps => ps
            .Add(p => p.Variant, Variant.Outlined)
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.Size, Size.Small)
            .Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<Button>(0);
                b.AddAttribute(1, nameof(Button.ChildContent), (RenderFragment)(c => c.AddContent(0, "One")));
                b.CloseComponent();
            })));

        AngleSharp.Dom.IElement btn = cut.Find("button");
        btn.ClassList.ShouldContain("btn-outline");
        btn.ClassList.ShouldContain("btn-primary");
        btn.ClassList.ShouldContain("btn-sm");
    }

    [Fact]
    public void ButtonGroup_without_override_keeps_child_styles()
    {
        IRenderedComponent<ButtonGroup> cut = Render<ButtonGroup>(ps => ps
            .Add(p => p.Color, Color.Primary)
            .Add(p => p.OverrideStyles, false)
            .Add(p => p.ChildContent, (RenderFragment)(b =>
            {
                b.OpenComponent<Button>(0);
                b.AddAttribute(1, nameof(Button.Color), Color.Error);
                b.CloseComponent();
            })));

        cut.Find("button").ClassList.ShouldContain("btn-error");
    }

    [Fact]
    public void Diff_is_focusable_and_labels_panes()
    {
        IRenderedComponent<Diff> cut = Render<Diff>(ps => ps
            .Add(p => p.Item1Label, "Before")
            .Add(p => p.Item2Label, "After"));

        cut.Find(".diff").GetAttribute("tabindex").ShouldBe("0");
        AngleSharp.Dom.IElement item1 = cut.Find(".diff-item-1");
        item1.GetAttribute("tabindex").ShouldBe("0");
        item1.GetAttribute("aria-label").ShouldBe("Before");
        cut.Find(".diff-item-2").GetAttribute("aria-label").ShouldBe("After");
    }

    [Fact]
    public void Diff_without_labels_drops_unnamed_img_role()
    {
        Render<Diff>().FindAll("[role=img]").Count.ShouldBe(0);
    }

    [Fact]
    public void FileInput_label_is_associated()
    {
        IRenderedComponent<FileInput> cut = Render<FileInput>(ps => ps.Add(p => p.Label, "Screenshots"));

        string? id = cut.Find("input[type=file]").GetAttribute("id");
        id.ShouldNotBeNullOrEmpty();
        cut.Find("label").GetAttribute("for").ShouldBe(id);
        cut.Find("label").TextContent.ShouldBe("Screenshots");
    }
}
