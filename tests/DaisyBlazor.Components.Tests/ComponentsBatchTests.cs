using DaisyBlazor.Layout;

namespace DaisyBlazor.Components.Tests;

/// <summary>Regression tests for the #24/#52/#54/#55 component batch.</summary>
public class ComponentsBatchTests : BunitContext
{
    [Fact]
    public void TextField_label_points_at_the_input()
    {
        IRenderedComponent<TextField> cut = Render<TextField>(ps => ps.Add(p => p.Label, "Name"));

        string id = cut.Find("input").GetAttribute("id")!;
        id.ShouldNotBeNullOrEmpty();
        cut.Find("label.label").GetAttribute("for").ShouldBe(id);
    }

    [Fact]
    public void TextField_honours_a_user_supplied_id()
    {
        IRenderedComponent<TextField> cut = Render<TextField>(ps => ps
            .Add(p => p.Label, "Name")
            .AddUnmatched("id", "my-field"));

        cut.Find("input").GetAttribute("id").ShouldBe("my-field");
        cut.Find("label.label").GetAttribute("for").ShouldBe("my-field");
    }

    [Fact]
    public void Select_label_points_at_the_select()
    {
        IRenderedComponent<Select<string>> cut = Render<Select<string>>(ps => ps.Add(p => p.Label, "Pick"));

        cut.Find("label.label").GetAttribute("for").ShouldBe(cut.Find("select").GetAttribute("id"));
    }

    [Fact]
    public void NumericField_label_points_at_the_input()
    {
        IRenderedComponent<NumericField<int>> cut = Render<NumericField<int>>(ps => ps.Add(p => p.Label, "Qty"));

        cut.Find("label.label").GetAttribute("for").ShouldBe(cut.Find("input").GetAttribute("id"));
    }

    [Fact]
    public void Disabled_button_link_is_not_navigable()
    {
        IRenderedComponent<Button> cut = Render<Button>(ps => ps
            .Add(p => p.Href, "/go")
            .Add(p => p.Disabled, true));

        AngleSharp.Dom.IElement a = cut.Find("a");
        a.HasAttribute("href").ShouldBeFalse();
        a.GetAttribute("aria-disabled").ShouldBe("true");
        a.GetAttribute("tabindex").ShouldBe("-1");
        a.ClassList.ShouldContain("btn-disabled");
    }

    [Fact]
    public void Enabled_button_link_keeps_its_href()
    {
        IRenderedComponent<Button> cut = Render<Button>(ps => ps.Add(p => p.Href, "/go"));

        cut.Find("a").GetAttribute("href").ShouldBe("/go");
        cut.Find("a").HasAttribute("aria-disabled").ShouldBeFalse();
    }

    [Fact]
    public void PageHeader_defaults_to_h4_and_honours_HeadingLevel()
    {
        Render<PageHeader>(ps => ps.Add(p => p.Title, "T")).FindAll("h4").Count.ShouldBe(1);

        IRenderedComponent<PageHeader> h1 = Render<PageHeader>(ps => ps
            .Add(p => p.Title, "T")
            .Add(p => p.HeadingLevel, 1));
        h1.FindAll("h1").Count.ShouldBe(1);
        h1.FindAll("h4").Count.ShouldBe(0);
    }

    [Fact]
    public void Inline_badge_renders_without_the_indicator_wrapper()
    {
        IRenderedComponent<Badge> cut = Render<Badge>(ps => ps
            .Add(p => p.Inline, true)
            .Add(p => p.Content, "New"));

        cut.FindAll(".indicator").Count.ShouldBe(0);
        cut.Find("span.badge").TextContent.ShouldBe("New");
    }
}
