using AngleSharp.Dom;
using DaisyBlazor;
using DaisyBlazor.Charts;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Smoke tests asserting that DaisyBlazor components emit the expected daisyUI classes.
/// Doubles as a guard that the component APIs render without throwing.
/// </summary>
public class ComponentRenderTests : BunitContext
{
    [Fact]
    public void Button_renders_btn_with_color_and_content()
    {
        IRenderedComponent<Button> cut = Render<Button>(ps => ps
            .Add(p => p.Color, Color.Primary)
            .AddChildContent("Save"));

        cut.Markup.ShouldContain("btn");
        cut.Markup.ShouldContain("btn-primary");
        cut.Markup.ShouldContain("Save");
    }

    [Fact]
    public void Button_with_href_renders_anchor()
    {
        IRenderedComponent<Button> cut = Render<Button>(ps => ps
            .Add(p => p.Href, "/go")
            .AddChildContent("Link"));

        cut.Find("a").GetAttribute("href").ShouldBe("/go");
    }

    [Theory]
    [InlineData(Severity.Info, "alert-info")]
    [InlineData(Severity.Success, "alert-success")]
    [InlineData(Severity.Warning, "alert-warning")]
    [InlineData(Severity.Error, "alert-error")]
    public void Alert_renders_severity_class(Severity severity, string expected)
    {
        IRenderedComponent<Alert> cut = Render<Alert>(ps => ps
            .Add(p => p.Severity, severity)
            .AddChildContent("msg"));

        cut.Markup.ShouldContain("alert");
        cut.Markup.ShouldContain(expected);
    }

    [Theory]
    [InlineData(LoadingType.Spinner, "loading-spinner")]
    [InlineData(LoadingType.Dots, "loading-dots")]
    [InlineData(LoadingType.Ring, "loading-ring")]
    [InlineData(LoadingType.Bars, "loading-bars")]
    [InlineData(LoadingType.Infinity, "loading-infinity")]
    public void Loading_renders_animation_class(LoadingType type, string expected)
    {
        IRenderedComponent<Loading> cut = Render<Loading>(ps => ps.Add(p => p.Type, type));

        cut.Markup.ShouldContain("loading");
        cut.Markup.ShouldContain(expected);
    }

    [Fact]
    public void Kbd_renders_kbd_class()
    {
        IRenderedComponent<Kbd> cut = Render<Kbd>(ps => ps.AddChildContent("Ctrl"));

        cut.Find("kbd").ClassList.ShouldContain("kbd");
        cut.Markup.ShouldContain("Ctrl");
    }

    [Fact]
    public void RadialProgress_sets_value_custom_property()
    {
        IRenderedComponent<RadialProgress> cut = Render<RadialProgress>(ps => ps.Add(p => p.Value, 70));

        cut.Markup.ShouldContain("radial-progress");
        string? style = cut.Find("div").GetAttribute("style");
        style.ShouldNotBeNull();
        style.ShouldContain("--value:70");
    }

    [Fact]
    public void Range_renders_range_with_color_and_value()
    {
        IRenderedComponent<Range> cut = Render<Range>(ps => ps
            .Add(p => p.Value, 50)
            .Add(p => p.Color, Color.Success));

        IElement input = cut.Find("input");
        input.ClassList.ShouldContain("range");
        input.ClassList.ShouldContain("range-success");
        input.GetAttribute("value").ShouldBe("50");
    }

    [Fact]
    public void Status_renders_status_with_color()
    {
        IRenderedComponent<Status> cut = Render<Status>(ps => ps.Add(p => p.Color, Color.Success));

        cut.Markup.ShouldContain("status");
        cut.Markup.ShouldContain("status-success");
    }

    [Fact]
    public void Badge_renders_badge_class()
    {
        IRenderedComponent<Badge> cut = Render<Badge>(ps => ps.AddChildContent("9"));

        cut.Markup.ShouldContain("badge");
    }

    [Fact]
    public void Tabs_marks_active_tab()
    {
        IRenderedComponent<Tabs> cut = Render<Tabs>(ps => ps
            .Add(p => p.ActiveIndex, 1)
            .AddChildContent<Tab>(tab => tab.Add(t => t.Title, "One").AddChildContent("first"))
            .AddChildContent<Tab>(tab => tab.Add(t => t.Title, "Two").AddChildContent("second")));

        cut.Markup.ShouldContain("tab-active");
        cut.Markup.ShouldContain("tab-content");
    }

    [Fact]
    public void Tabs_expose_WAI_ARIA_selected_state_and_tab_panel_linkage()
    {
        IRenderedComponent<Tabs> cut = Render<Tabs>(ps => ps
            .Add(p => p.ActiveIndex, 1)
            .AddChildContent<Tab>(tab => tab.Add(t => t.Title, "One").AddChildContent("first"))
            .AddChildContent<Tab>(tab => tab.Add(t => t.Title, "Two").AddChildContent("second")));

        IReadOnlyList<IElement> tabs = cut.FindAll("[role=tab]");
        tabs.Count.ShouldBe(2);
        // role=tab REQUIRES aria-selected; exactly the active (index 1) tab is selected.
        tabs[0].GetAttribute("aria-selected").ShouldBe("false");
        tabs[1].GetAttribute("aria-selected").ShouldBe("true");

        // The active tab is linked to the panel, and the panel is labelled back by it.
        IElement panel = cut.Find("[role=tabpanel]");
        string? panelId = panel.GetAttribute("id");
        panelId.ShouldNotBeNullOrEmpty();
        tabs[1].GetAttribute("aria-controls").ShouldBe(panelId);
        panel.GetAttribute("aria-labelledby").ShouldBe(tabs[1].GetAttribute("id"));
    }

    [Fact]
    public void FeatureHomePage_renders_NavCardsTitle_above_nav_cards()
    {
        IRenderedComponent<FeatureHomePage> cut = Render<FeatureHomePage>(ps => ps
            .Add(p => p.Title, "Operations")
            .Add(p => p.NavCardsTitle, "Quick access")
            .Add(p => p.NavCards, "<div class=\"probe-card\">card</div>"));

        cut.Markup.ShouldContain("Quick access");
        cut.Markup.ShouldContain("probe-card");
    }

    [Fact]
    public void FeatureHomePage_omits_NavCardsTitle_when_not_provided()
    {
        IRenderedComponent<FeatureHomePage> cut = Render<FeatureHomePage>(ps => ps
            .Add(p => p.Title, "Operations")
            .Add(p => p.NavCards, "<div class=\"probe-card\">card</div>"));

        cut.Markup.ShouldContain("probe-card");
        cut.Markup.ShouldNotContain("Quick access");
    }
}

/// <summary>Tests for the dependency-free SVG charts.</summary>
public class ChartRenderTests : BunitContext
{
    [Fact]
    public void Sparkline_renders_svg_path()
    {
        IRenderedComponent<Sparkline> cut = Render<Sparkline>(ps => ps
            .Add(p => p.Data, new double[] { 1, 4, 2, 6, 3, 7 }));

        cut.Find("svg").ShouldNotBeNull();
        cut.Markup.ShouldContain("<path");
    }

    [Fact]
    public void PieChart_renders_a_slice_per_point()
    {
        IRenderedComponent<PieChart> cut = Render<PieChart>(ps => ps
            .Add(p => p.Data, new List<ChartDataPoint>
            {
                new("A", 30),
                new("B", 50),
                new("C", 20),
            }));

        cut.Find("svg").ShouldNotBeNull();
        cut.Markup.ShouldContain("<path");
    }

    // Guards against ".ToString(...)" leaking as literal SVG attribute text when the call is
    // placed outside the Razor @(...) expression (e.g. @((a - b)).ToString(_ci) instead of
    // @((a - b).ToString(_ci))), which produces invalid attributes like x2="588.ToString(...)".
    [Theory]
    [InlineData("LineChart")]
    [InlineData("DonutChart")]
    [InlineData("BarChart")]
    [InlineData("PieChart")]
    [InlineData("AreaChart")]
    public void Charts_do_not_leak_ToString_into_markup(string chart)
    {
        string markup = chart switch
        {
            "LineChart" => Render<LineChart>(ps => ps
                .Add(p => p.Series, new List<ChartSeries> { new("S", new double[] { 1, 5, 3, 8 }) })).Markup,
            "AreaChart" => Render<AreaChart>(ps => ps
                .Add(p => p.Series, new List<ChartSeries> { new("S", new double[] { 1, 5, 3, 8 }) })).Markup,
            "BarChart" => Render<BarChart>(ps => ps
                .Add(p => p.Data, new List<ChartDataPoint> { new("A", 3), new("B", 7) })).Markup,
            "PieChart" => Render<PieChart>(ps => ps
                .Add(p => p.Data, new List<ChartDataPoint> { new("A", 3), new("B", 7) })).Markup,
            _ => Render<DonutChart>(ps => ps
                .Add(p => p.Data, new List<ChartDataPoint> { new("A", 3), new("B", 7) })).Markup,
        };

        markup.ShouldNotContain("ToString");
    }
}
