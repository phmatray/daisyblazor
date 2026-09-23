using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Layout;

/// <summary>
/// Paper surface with an icon + title header, an optional rule beneath it, optional
/// right-aligned header actions, and a free-form body.
/// </summary>
/// <remarks>
/// <para>
/// This is the general form of <see cref="DetailCard"/>. Reach for <c>SectionCard</c> whenever the
/// body is arbitrary content — a form, prose, a chart, a list. Reach for <see cref="DetailCard"/>
/// only when the body is a label/value table of <see cref="DetailRow"/>; it is now implemented on
/// top of this component, so the two stay visually identical by construction.
/// </para>
/// <para>
/// Defaults are tuned to the most common real-world usage: <see cref="Elevation"/> 2,
/// <c>p-4</c> padding, an <see cref="Typo.h6"/> title, and <see cref="ShowDivider"/> on. Outer
/// spacing is deliberately left to the caller — pass <c>Class="mb-4"</c> to separate stacked cards.
/// </para>
/// </remarks>
public partial class SectionCard
{
    /// <summary>Section title rendered in the header.</summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Optional Material symbol ligature shown to the left of <see cref="Title"/>
    /// (e.g. <c>Icons.Material.Filled.Person</c>). Omit for a text-only header.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Section body — any content.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Optional controls aligned to the right of the title, such as a refresh button or a filter
    /// toggle. When supplied, the header becomes a space-between row.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderActions { get; set; }

    /// <summary>
    /// Draws a rule between the header and the body. Defaults to <c>true</c>, matching the
    /// prevailing convention; set <c>false</c> for a tighter card.
    /// </summary>
    [Parameter]
    public bool ShowDivider { get; set; } = true;

    /// <summary>Typography scale for the title. Defaults to <see cref="Typo.h6"/>.</summary>
    [Parameter]
    public Typo TitleTypo { get; set; } = Typo.h6;

    /// <summary>Paper elevation. Defaults to 2.</summary>
    [Parameter]
    public int Elevation { get; set; } = 2;

    /// <summary>Extra CSS classes appended to the surface, after the default <c>p-4</c>.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Optional inline style forwarded to the surface.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Unmatched attributes splatted onto the surface.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
