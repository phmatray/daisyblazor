using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Data;

/// <summary>
/// KPI tile: caption + large value + optional subtitle, with an optional coloured accent stripe.
/// </summary>
public partial class KpiCard
{
    /// <summary>Caption shown above the value. Already-resolved text (pass <c>@L["…"]</c> at the call site to localize).</summary>
    [Parameter, EditorRequired]
    public string Label { get; set; } = null!;

    /// <summary>Main metric value.</summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = null!;

    /// <summary>
    /// CSS color of the accent stripe (see <see cref="Accent"/>) (the value text keeps the base text colour for contrast). Defaults to the active daisyUI
    /// theme's <c>--color-primary</c> token so the tile stays on-palette; pass any CSS colour to override.
    /// </summary>
    [Parameter]
    public string Color { get; set; } = "var(--color-primary)";

    /// <summary>Where the coloured stripe is drawn: on the start edge (the default), on top, or not at all.</summary>
    [Parameter]
    public KpiAccent Accent { get; set; } = KpiAccent.Start;

    /// <summary>Shadow depth of the tile, as on <c>Paper</c> (0 for none). Defaults to 2.</summary>
    [Parameter]
    public int Elevation { get; set; } = 2;

    /// <summary>Optional smaller caption beneath the value.</summary>
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>Additional CSS classes applied to the tile.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Inline style appended to the tile.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Arbitrary attributes splatted onto the tile.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }
}

/// <summary>Where a <see cref="KpiCard"/> draws its coloured stripe.</summary>
public enum KpiAccent
{
    /// <summary>No stripe.</summary>
    None,

    /// <summary>A stripe on the start edge (left in left-to-right text).</summary>
    Start,

    /// <summary>A stripe along the top edge.</summary>
    Top,
}
