using AngleSharp.Dom;
using DaisyBlazor.Feedback;
using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Components.Tests;

/// <summary>Pagination, Alert, Footer, ErrorAlert and Snackbar icon fixes.</summary>
public class LibraryComponents11FixTests : BunitContext
{
    private string[] PageLabels(int count, int selected, int boundary = 1, int middle = 3)
    {
        IRenderedComponent<Pagination> cut = Render<Pagination>(ps => ps
            .Add(p => p.Count, count)
            .Add(p => p.Selected, selected)
            .Add(p => p.BoundaryCount, boundary)
            .Add(p => p.MiddleCount, middle));
        // Page buttons and ellipsis spans, skipping the prev/next buttons (which carry aria-label).
        return cut.FindAll("nav > *")
            .Where(e => !e.HasAttribute("aria-label"))
            .Select(e => e.TextContent.Trim())
            .ToArray();
    }

    [Theory]
    [InlineData(5, 4, "1 2 3 4 5")]
    [InlineData(5, 2, "1 2 3 4 5")]
    [InlineData(10, 4, "1 2 3 4 5 … 10")]
    [InlineData(10, 6, "1 … 5 6 7 … 10")]
    public void Pagination_one_page_gap_shows_the_page_instead_of_an_ellipsis(int count, int selected, string expected) =>
        string.Join(' ', PageLabels(count, selected)).ShouldBe(expected);

    [Fact]
    public void Pagination_ellipsis_is_a_hidden_span_not_a_button()
    {
        IRenderedComponent<Pagination> cut = Render<Pagination>(ps => ps
            .Add(p => p.Count, 20)
            .Add(p => p.Selected, 10));
        IReadOnlyList<IElement> gaps = cut.FindAll("nav > span");
        gaps.Count.ShouldBe(2);
        gaps.ShouldAllBe(g => g.GetAttribute("aria-hidden") == "true");
        cut.FindAll("button").ShouldAllBe(b => b.TextContent.Trim() != "…");
    }

    [Fact]
    public void Pagination_default_selected_is_neutral_and_root_scrolls()
    {
        IRenderedComponent<Pagination> cut = Render<Pagination>(ps => ps
            .Add(p => p.Count, 5)
            .Add(p => p.Selected, 3));
        cut.Find("[aria-current=page]").ClassList.ShouldContain("btn-neutral");
        cut.Find("nav").ClassList.ShouldContain("overflow-x-auto");
    }

    [Fact]
    public void Pagination_first_and_last_buttons_use_single_glyphs()
    {
        IRenderedComponent<Pagination> cut = Render<Pagination>(ps => ps
            .Add(p => p.Count, 5)
            .Add(p => p.ShowFirstButton, true)
            .Add(p => p.ShowLastButton, true));
        cut.Find("[aria-label='First page']").TextContent.Trim().ShouldBe("first_page");
        cut.Find("[aria-label='Last page']").TextContent.Trim().ShouldBe("last_page");
    }

    [Theory]
    [InlineData(Severity.Normal, "status")]
    [InlineData(Severity.Info, "status")]
    [InlineData(Severity.Success, "status")]
    [InlineData(Severity.Warning, "alert")]
    [InlineData(Severity.Error, "alert")]
    public void Alert_role_follows_severity(Severity severity, string role) =>
        Render<Alert>(ps => ps.Add(p => p.Severity, severity))
            .Find("div").GetAttribute("role").ShouldBe(role);

    [Fact]
    public void Alert_role_can_be_overridden() =>
        Render<Alert>(ps => ps.Add(p => p.Role, "alert"))
            .Find("div").GetAttribute("role").ShouldBe("alert");

    [Theory]
    [InlineData(Variant.Outlined)]
    [InlineData(Variant.Text)]
    public void Outlined_and_soft_alerts_keep_base_content_text_and_tint_the_icon(Variant variant)
    {
        IRenderedComponent<Alert> cut = Render<Alert>(ps => ps
            .Add(p => p.Severity, Severity.Warning)
            .Add(p => p.Variant, variant));
        cut.Find("div").ClassList.ShouldContain("text-base-content");
        cut.Find("div > span[aria-hidden]").ClassList.ShouldContain("text-warning");
    }

    [Fact]
    public void Filled_alert_keeps_daisy_content_colour() =>
        Render<Alert>(ps => ps.Add(p => p.Severity, Severity.Warning))
            .Find("div").ClassList.ShouldNotContain("text-base-content");

    [Theory]
    [InlineData(Color.Primary, "text-primary-content")]
    [InlineData(Color.Dark, "text-neutral-content")]
    public void Footer_color_sets_matching_content_colour(Color color, string expected) =>
        Render<Footer>(ps => ps.Add(p => p.Color, color))
            .Find("footer").ClassList.ShouldContain(expected);

    [Theory]
    [InlineData(null, "footer-horizontal")]   // horizontal + center = row dense: stacked
    [InlineData(false, "footer-horizontal")]
    [InlineData(true, "footer-vertical")]     // vertical + center = column dense: side by side
    public void Footer_center_stacks_unless_horizontal(bool? horizontal, string expected) =>
        Render<Footer>(ps => ps
                .Add(p => p.Center, true)
                .Add(p => p.Horizontal, horizontal))
            .Find("footer").ClassList.ShouldContain(expected);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ErrorAlert_renders_nothing_for_blank_message(string? message) =>
        Render<ErrorAlert>(ps => ps.Add(p => p.Message, message)).Markup.Trim().ShouldBeEmpty();

    [Fact]
    public void ErrorAlert_has_no_default_margin_and_passes_style_and_attributes()
    {
        IElement alert = Render<ErrorAlert>(ps => ps
                .Add(p => p.Message, "Boom")
                .Add(p => p.Style, "color:red")
                .AddUnmatched("data-test", "x"))
            .Find("div");
        alert.ClassList.ShouldNotContain("mb-4");
        alert.GetAttribute("style").ShouldBe("color:red");
        alert.GetAttribute("data-test").ShouldBe("x");
    }

    [Fact]
    public void Snackbar_uses_the_alert_icon_for_errors()
    {
        SnackbarService snackbar = new();
        Services.AddSingleton<ISnackbar>(snackbar);
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>();
        cut.InvokeAsync(() => snackbar.Add("Failed", Severity.Error));
        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Failed"));
        string toastIcon = cut.Find(".alert-error span[aria-hidden]").TextContent.Trim();
        string alertIcon = Render<Alert>(ps => ps.Add(p => p.Severity, Severity.Error))
            .Find("span[aria-hidden]").TextContent.Trim();
        toastIcon.ShouldBe(alertIcon);
    }
}
