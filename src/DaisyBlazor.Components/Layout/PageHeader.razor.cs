using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Layout;

/// <summary>
/// Page-level header row: title + optional subtitle on the left, optional action slot on the right.
/// </summary>
public partial class PageHeader
{
    /// <summary>Main page title: an <c>h1</c> by default (see <see cref="HeadingLevel"/>), styled by <see cref="TitleTypo"/>.</summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Heading level (1-6) of the title element. Defaults to 1: a page header is the page's main
    /// heading. It only picks the element; the size comes from <see cref="TitleTypo"/>.
    /// </summary>
    [Parameter]
    public int HeadingLevel { get; set; } = 1;

    /// <summary>Text style of the title, whatever its <see cref="HeadingLevel"/>. Defaults to <see cref="Typo.h4"/>, the size page headers always had.</summary>
    [Parameter]
    public Typo TitleTypo { get; set; } = Typo.h4;

    /// <summary>Optional secondary line beneath the title.</summary>
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Colour of <see cref="Subtitle"/>. Defaults to <see cref="Color.Surface"/> (muted base content);
    /// use <see cref="Color.Dark"/> for full contrast or <see cref="Color.Inherit"/> to follow the parent.
    /// </summary>
    [Parameter]
    public Color SubtitleColor { get; set; } = Color.Surface;

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

    /// <summary>Extra CSS classes on the title heading (e.g. a display face, size or leading).</summary>
    [Parameter]
    public string? TitleClass { get; set; }

    /// <summary>Extra CSS classes on the subtitle line.</summary>
    [Parameter]
    public string? SubtitleClass { get; set; }

    /// <summary>Optional inline style forwarded to the root container.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Unmatched attributes splatted onto the root container.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}
