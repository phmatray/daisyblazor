using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Layout;

/// <summary>
/// Standard detail-page scaffold: back link, title, optional status slot, main content,
/// optional sidebar, optional related-content area.
/// </summary>
public partial class DetailLayout
{
    /// <summary>Page title.</summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = null!;

    /// <summary>Optional secondary line beneath the title.</summary>
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>Route the back link points to.</summary>
    [Parameter, EditorRequired]
    public string BackRoute { get; set; } = null!;

    /// <summary>Label of the back button. Defaults to <c>"Back"</c>; pass a localized string to override.</summary>
    [Parameter]
    public string? BackLabel { get; set; }

    /// <summary>Optional element rendered next to the title (typically a status chip).</summary>
    [Parameter]
    public RenderFragment? StatusSlot { get; set; }

    /// <summary>Primary content of the detail page.</summary>
    [Parameter, EditorRequired]
    public RenderFragment MainContent { get; set; } = null!;

    /// <summary>Optional sidebar (right column on md+ viewports).</summary>
    [Parameter]
    public RenderFragment? Sidebar { get; set; }

    /// <summary>Optional full-width section rendered below the main grid.</summary>
    [Parameter]
    public RenderFragment? RelatedContent { get; set; }

    /// <summary>Optional CSS class forwarded to the root container.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Optional inline style forwarded to the root container.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Unmatched attributes splatted onto the root container.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
