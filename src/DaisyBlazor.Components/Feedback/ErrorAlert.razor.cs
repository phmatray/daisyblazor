using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Feedback;

/// <summary>
/// Filled error <see cref="Alert"/>; renders nothing when <see cref="Message"/> is null or blank.
/// </summary>
public partial class ErrorAlert
{
    /// <summary>Error text. When null, empty or whitespace, the alert is not rendered.</summary>
    [Parameter]
    public string? Message { get; set; }

    /// <summary>Extra CSS classes applied to the alert. No default margin: space it from the layout (gap, Stack).</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Inline CSS style applied to the alert.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Extra attributes passed through to the alert element.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
