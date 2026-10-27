using DaisyBlazor.Data;
using Microsoft.Extensions.DependencyInjection;
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
    public void Button_takes_the_app_default_variant_unless_it_sets_one()
    {
        Services.AddSingleton(new DaisyBlazorOptions { DefaultButtonVariant = Variant.Filled });
        Render<Button>(ps => ps.Add(p => p.Color, Color.Primary)).Find("button").ClassList.ShouldNotContain("btn-ghost");
        Render<Button>(ps => ps.Add(p => p.Variant, Variant.Text)).Find("button").ClassList.ShouldContain("btn-ghost");
    }

    [Fact]
    public void Built_in_labels_come_from_the_app_strings_unless_the_parameter_is_set()
    {
        new BunitContext().Render<Loading>().Find("span").GetAttribute("aria-label").ShouldBe("Loading");

        Services.AddSingleton(new DaisyBlazorOptions { Strings = new DaisyBlazorStrings { Loading = "Chargement", Pagination = "Pages" } });
        Render<Loading>().Find("span").GetAttribute("aria-label").ShouldBe("Chargement");
        Render<Loading>(ps => ps.Add(p => p.AriaLabel, "Envoi")).Find("span").GetAttribute("aria-label").ShouldBe("Envoi");
        Render<Pagination>(ps => ps.Add(p => p.Count, 3)).Find("nav").GetAttribute("aria-label").ShouldBe("Pages");
    }

    [Fact]
    public void KpiCard_accent_and_elevation_are_configurable()
    {
        string Style(KpiAccent accent) => Render<KpiCard>(ps => ps
            .Add(p => p.Label, "L").Add(p => p.Value, "1").Add(p => p.Accent, accent).Add(p => p.Elevation, 0))
            .Find("div").GetAttribute("style")!;

        Style(KpiAccent.None).ShouldNotContain("border");
        Style(KpiAccent.Top).ShouldContain("border-top: 4px solid");
        Style(KpiAccent.Start).ShouldContain("border-inline-start: 4px solid");
    }

    [Fact]
    public void PageHeader_is_an_h1_with_the_h4_look_and_the_level_never_changes_the_size()
    {
        IRenderedComponent<PageHeader> page = Render<PageHeader>(ps => ps.Add(p => p.Title, "T"));
        page.Find("h1").ClassName.ShouldBe(Render<Typography>(ps => ps.Add(p => p.Typo, Typo.h4)).Find("h4").ClassName);

        IRenderedComponent<PageHeader> h3 = Render<PageHeader>(ps => ps
            .Add(p => p.Title, "T")
            .Add(p => p.HeadingLevel, 3)
            .Add(p => p.TitleTypo, Typo.h6));
        h3.FindAll("h1").Count.ShouldBe(0);
        h3.Find("h3").ClassName.ShouldBe(Render<Typography>(ps => ps.Add(p => p.Typo, Typo.h6)).Find("h6").ClassName);
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
