using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Data;

/// <summary>
/// A filled <see cref="Chip"/> variant for status indicators.
/// </summary>
/// <remarks>
/// <para>
/// Supply <see cref="Status"/> plus a <see cref="Map"/> and the chip resolves its own colour and
/// icon; that is the point of the component, and why <see cref="Color"/> is no longer required.
/// Previously it demanded the colour — i.e. the answer to the only hard part — so it added nothing
/// over a plain <see cref="Chip"/>.
/// </para>
/// <para>
/// <b>Labels stay with the caller.</b> Real status labels are localized and this library has no
/// localizer, so <see cref="Map"/> covers colour and icon only. <see cref="Label"/> falls back to
/// <c>Status.ToString()</c> purely as a convenience for prototypes and non-localized apps.
/// </para>
/// <para>
/// Precedence is explicit-wins throughout, so every pre-existing usage renders unchanged:
/// <see cref="Color"/> beats the map, <see cref="Icon"/> beats the map, <see cref="Label"/> beats
/// the status text.
/// </para>
/// </remarks>
public partial class StatusChip
{
    /// <summary>
    /// Text displayed inside the chip. Optional when <see cref="Status"/> is supplied, in which
    /// case it falls back to <c>Status.ToString()</c>.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// The status value to render. Combined with <see cref="Map"/> it drives the colour and icon.
    /// </summary>
    [Parameter]
    public object? Status { get; set; }

    /// <summary>
    /// Declarative status → colour/icon map. Build one per status type with
    /// <see cref="StatusMap.For{TStatus}"/> and reuse it across list, card and detail views.
    /// </summary>
    [Parameter]
    public StatusMap? Map { get; set; }

    /// <summary>
    /// Colour applied to the chip. Overrides <see cref="Map"/> when set; leave unset to let the
    /// map decide.
    /// </summary>
    [Parameter]
    public Color? Color { get; set; }

    /// <summary>Chip size; defaults to <see cref="DaisyBlazor.Size.Small"/>.</summary>
    [Parameter]
    public Size Size { get; set; } = Size.Small;

    /// <summary>
    /// Optional leading icon (a Material Symbols ligature). Overrides <see cref="Map"/> when set.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Additional CSS classes applied to the chip.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Inline style applied to the chip.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Arbitrary attributes splatted onto the chip.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }

    private StatusStyle _resolved => Map?.Resolve(Status) ?? new StatusStyle(DaisyBlazor.Color.Default);

    private Color _color => Color ?? _resolved.Color;

    private string? _icon => Icon ?? _resolved.Icon;

    private string? _label => Label ?? Status?.ToString();
}
