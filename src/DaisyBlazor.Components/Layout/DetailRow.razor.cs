using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Layout;

/// <summary>
/// Single label/value table row meant to be used inside a <see cref="DetailCard"/>.
/// Provide either <see cref="Value"/> for plain text or <see cref="ValueContent"/> for richer rendering.
/// </summary>
public partial class DetailRow
{
    /// <summary>Label shown in the left column (muted text).</summary>
    [Parameter, EditorRequired]
    public string Label { get; set; } = null!;

    /// <summary>Plain-text value shown in the right column when <see cref="ValueContent"/> is null.</summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>Richer right-column content (e.g. status chip, multi-element value). Overrides <see cref="Value"/>.</summary>
    [Parameter]
    public RenderFragment? ValueContent { get; set; }

    /// <summary>Optional CSS class forwarded to the row.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Optional inline style forwarded to the row.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Unmatched attributes splatted onto the row.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
