using Microsoft.AspNetCore.Components;

namespace DaisyBlazor;

/// <summary>
/// Feature home-page scaffold: title, optional subtitle, optional header actions,
/// nav-cards grid, optional quick-access button strip, and an additional-content slot.
/// Wraps body content with an <c>AuthorizeView</c> when <see cref="RequireAccessCheck"/> is true.
/// </summary>
public partial class FeatureHomePage
{
    /// <summary>Page title.</summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = null!;

    /// <summary>Optional secondary line beneath the title.</summary>
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Feature icon. When set, the page renders a gradient <see cref="FeatureHero"/>
    /// header instead of the plain title row.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Optional uppercase eyebrow (section name) shown above the title in the hero.</summary>
    [Parameter]
    public string? Eyebrow { get; set; }

    /// <summary>Hero gradient start colour. Only used when <see cref="Icon"/> is set.</summary>
    [Parameter]
    public string GradientStart { get; set; } = "#1976d2";

    /// <summary>Hero gradient end colour. Only used when <see cref="Icon"/> is set.</summary>
    [Parameter]
    public string GradientEnd { get; set; } = "#42a5f5";

    /// <summary>Optional stat strip rendered inside the hero (only when <see cref="Icon"/> is set).</summary>
    [Parameter]
    public RenderFragment? HeroStats { get; set; }

    /// <summary>Optional right-aligned action area in the header row.</summary>
    [Parameter]
    public RenderFragment? HeaderActions { get; set; }

    /// <summary>Optional error banner rendered above the nav cards.</summary>
    [Parameter]
    public string? Error { get; set; }

    /// <summary>Grid of feature nav cards (use <see cref="FeatureNavCard"/>).</summary>
    [Parameter]
    public RenderFragment? NavCards { get; set; }

    /// <summary>Optional title rendered above the <see cref="NavCards"/> grid.</summary>
    [Parameter]
    public string? NavCardsTitle { get; set; }

    /// <summary>Optional title above the quick-access strip.</summary>
    [Parameter]
    public string? QuickAccessTitle { get; set; }

    /// <summary>Optional quick-access strip (use <see cref="QuickAccessButton"/>).</summary>
    [Parameter]
    public RenderFragment? QuickAccessButtons { get; set; }

    /// <summary>Optional content rendered below the quick-access strip.</summary>
    [Parameter]
    public RenderFragment? AdditionalContent { get; set; }

    /// <summary>When true, wraps the body in an <c>AuthorizeView</c>.</summary>
    [Parameter]
    public bool RequireAccessCheck { get; set; }

    /// <summary>When false (with <see cref="RequireAccessCheck"/>), shows the access-denied slot.</summary>
    [Parameter]
    public bool IsAccessGranted { get; set; } = true;

    /// <summary>Optional custom access-denied content; defaults to a warning alert.</summary>
    [Parameter]
    public RenderFragment? AccessDeniedContent { get; set; }

    /// <summary>Text shown in the default access-denied warning alert (when no <see cref="AccessDeniedContent"/> is supplied).</summary>
    [Parameter]
    public string AccessDeniedText { get; set; } = "Access denied.";

    private bool HasHeaderActions => HeaderActions is not null;

    private bool UseHero => Icon is not null;

    private string HeroIcon => Icon!;
}
