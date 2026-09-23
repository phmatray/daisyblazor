using DaisyBlazor.Layout;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Tests for <see cref="SectionCard"/> and the two Layout components built on it.
/// </summary>
/// <remarks>
/// SectionCard exists because the "Paper + icon/title header + rule + free-form body" shape was
/// being hand-assembled in application code — 59 times across 52 files in the reference consumer,
/// because <see cref="DetailCard"/> hardcoded its body to a table. These tests pin the parts that
/// made hand-rolling necessary: a free-form body, a header-actions slot, and a toggleable rule.
/// </remarks>
public class SectionCardTests : BunitContext
{
    [Fact]
    public void Renders_title_icon_and_free_form_body()
    {
        IRenderedComponent<SectionCard> cut = Render<SectionCard>(ps => ps
            .Add(p => p.Title, "Passenger")
            .Add(p => p.Icon, "person")
            .AddChildContent("<form id=\"free\">anything</form>"));

        cut.Markup.ShouldContain("Passenger");
        cut.Markup.ShouldContain("person");
        // The whole point: the body is NOT forced into a table.
        cut.Find("#free").ShouldNotBeNull();
        cut.Markup.ShouldNotContain("<table");
    }

    [Fact]
    public void Omitting_icon_renders_header_without_one()
    {
        IRenderedComponent<SectionCard> cut = Render<SectionCard>(ps => ps
            .Add(p => p.Title, "No icon")
            .AddChildContent("body"));

        cut.Markup.ShouldContain("No icon");
        cut.FindAll(".material-symbols-outlined").Count.ShouldBe(0);
    }

    [Fact]
    public void Header_actions_render_alongside_the_title()
    {
        IRenderedComponent<SectionCard> cut = Render<SectionCard>(ps => ps
            .Add(p => p.Title, "Zones")
            .Add(p => p.HeaderActions, (RenderFragment)(b => b.AddMarkupContent(0, "<button id=\"refresh\">R</button>")))
            .AddChildContent("body"));

        cut.Find("#refresh").ShouldNotBeNull();
        cut.Markup.ShouldContain("justify-between");
    }

    [Fact]
    public void Divider_is_on_by_default_and_can_be_turned_off()
    {
        IRenderedComponent<SectionCard> withRule = Render<SectionCard>(ps => ps
            .Add(p => p.Title, "T").AddChildContent("b"));
        withRule.Markup.ShouldContain("divider");

        IRenderedComponent<SectionCard> withoutRule = Render<SectionCard>(ps => ps
            .Add(p => p.Title, "T")
            .Add(p => p.ShowDivider, false)
            .AddChildContent("b"));
        withoutRule.Markup.ShouldNotContain("divider");
    }

    [Fact]
    public void Defaults_to_elevation_2_and_p4_and_appends_caller_classes()
    {
        IRenderedComponent<SectionCard> cut = Render<SectionCard>(ps => ps
            .Add(p => p.Title, "T")
            .Add(p => p.Class, "mb-4")
            .AddChildContent("b"));

        string cls = cut.Find("div").GetAttribute("class")!;
        cls.ShouldContain("p-4");
        cls.ShouldContain("mb-4");
        // Elevation 2 maps to the "shadow" step in Paper.
        cls.ShouldContain("shadow");
    }

    [Fact]
    public void DetailCard_still_renders_a_table_body_and_keeps_its_own_defaults()
    {
        IRenderedComponent<DetailCard> cut = Render<DetailCard>(ps => ps
            .Add(p => p.Title, "Flight")
            .Add(p => p.Icon, "flight")
            .AddChildContent("<tr><td>x</td></tr>"));

        cut.Markup.ShouldContain("Flight");
        cut.Find("table").ShouldNotBeNull();
        // DetailCard deliberately has no rule under its header.
        cut.Markup.ShouldNotContain("divider");
    }

    [Fact]
    public void PageHeader_renders_an_icon_when_given_one()
    {
        IRenderedComponent<PageHeader> cut = Render<PageHeader>(ps => ps
            .Add(p => p.Title, "Flights")
            .Add(p => p.Icon, "flight_takeoff"));

        cut.Markup.ShouldContain("flight_takeoff");
        cut.Markup.ShouldContain("items-center");
    }

    [Fact]
    public void PageHeader_without_an_icon_is_unchanged()
    {
        IRenderedComponent<PageHeader> cut = Render<PageHeader>(ps => ps
            .Add(p => p.Title, "Flights"));

        cut.Markup.ShouldContain("Flights");
        cut.Markup.ShouldNotContain("gap-2");
    }
}
