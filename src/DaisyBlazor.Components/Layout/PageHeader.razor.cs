using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Layout;

/// <summary>
/// Page-level header row: title + optional subtitle on the left, optional action slot on the right.
/// </summary>
public partial class PageHeader
{
    /// <summary>Main page title; its element and size follow <see cref="HeadingLevel"/> (h4 by default).</summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = null!;

    /// <summary>Heading level (1-6) of the title element. Defaults to 4, the historical output.</summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 4;

    /// <summary>Optional secondary line beneath the title.</summary>
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Optional Material symbol ligature shown to the left of <see cref="Title"/>. When omitted the
    /// title renders exactly as before, so adding this parameter cannot shift an existing header.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Optional right-aligned content (typically action buttons).</summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

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
