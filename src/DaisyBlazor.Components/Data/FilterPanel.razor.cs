using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Data;

/// <summary>Tag rendered above the filter panel for each currently-applied filter.</summary>
/// <param name="Key">Stable identifier so the panel can ask the caller to remove this specific filter.</param>
/// <param name="Label">Localized text displayed inside the chip.</param>
public record ActiveFilter(string Key, string Label);

/// <summary>
/// Standard filter container: title row, child filter inputs, active-filter chips, and apply/clear buttons.
/// </summary>
public partial class FilterPanel
{
    /// <summary>Filter inputs (typically <c>Item</c> + form controls).</summary>
    [Parameter, EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;

    /// <summary>Optional list of currently-active filters rendered as removable chips.</summary>
    [Parameter]
    public IReadOnlyList<ActiveFilter>? ActiveFilters { get; set; }

    /// <summary>Invoked when the user clicks the apply button.</summary>
    [Parameter, EditorRequired]
    public EventCallback OnApply { get; set; }

    /// <summary>Invoked when the user clicks the footer clear button.</summary>
    [Parameter, EditorRequired]
    public EventCallback OnClear { get; set; }

    /// <summary>
    /// Invoked by the header reset button. That button only renders when this is set, so the panel
    /// never shows two controls that do the same thing; the footer clear button raises <see cref="OnClear"/>.
    /// </summary>
    [Parameter]
    public EventCallback OnReset { get; set; }

    /// <summary>Invoked when the user closes a single active-filter chip.</summary>
    [Parameter]
    public EventCallback<ActiveFilter> OnRemoveFilter { get; set; }

    /// <summary>Panel title. Defaults to <c>"Filters"</c>; pass a localized string to override.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>Label of the header reset button (shown when filters are active and <see cref="OnReset"/> is set). Defaults to <c>"Reset"</c>.</summary>
    [Parameter]
    public string ResetLabel { get; set; } = "Reset";

    /// <summary>Prefix shown before the active-filter chips. Defaults to <c>"Active filters:"</c>.</summary>
    [Parameter]
    public string ActiveFiltersLabel { get; set; } = "Active filters:";

    /// <summary>Label of the clear button in the footer. Defaults to <c>"Clear"</c>.</summary>
    [Parameter]
    public string ClearLabel { get; set; } = "Clear";

    /// <summary>Label of the apply button in the footer. Defaults to <c>"Filter"</c>.</summary>
    [Parameter]
    public string ApplyLabel { get; set; } = "Filter";

    /// <summary>Additional CSS classes applied to the panel.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Inline style applied to the panel.</summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>Arbitrary attributes splatted onto the panel.</summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? UserAttributes { get; set; }

    private bool HasActiveFilters => ActiveFilters is { Count: > 0 };
}
