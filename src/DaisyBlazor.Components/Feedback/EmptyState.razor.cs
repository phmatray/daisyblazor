using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Feedback;

/// <summary>
/// Centered empty-state placeholder with optional icon and subtitle.
/// </summary>
public partial class EmptyState
{
    /// <summary>Primary message displayed to the user.</summary>
    [Parameter, EditorRequired]
    public string Message { get; set; } = null!;

    /// <summary>Optional Material Symbols icon ligature shown above the message.</summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Optional secondary line shown beneath the message.</summary>
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>Optional actions (for example an "Add" button) rendered centred beneath the text.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
