using Microsoft.AspNetCore.Components;

namespace DaisyBlazor;

/// <summary>
/// Per-message snackbar configuration, mirroring the MudBlazor options surface used across DaisyBlazor.
/// </summary>
public sealed class SnackbarOptions
{
    private int? _visibleStateDuration;

    /// <summary>
    /// How long (ms) the toast stays visible before it auto-dismisses. When not set, the toast uses
    /// <see cref="SnackbarProvider.VisibleStateDuration"/> (5000 by default).
    /// </summary>
    public int VisibleStateDuration
    {
        get => _visibleStateDuration ?? 5000;
        set => _visibleStateDuration = value;
    }

    internal bool HasVisibleStateDuration => _visibleStateDuration is not null;

    /// <summary>Label of an action button on the toast (e.g. "Undo"); no button when null.</summary>
    public string? Action { get; set; }

    /// <summary>Raised when the <see cref="Action"/> button is clicked; the toast then closes.</summary>
    public Func<Task>? OnAction { get; set; }

    /// <summary>Tooltip of the <see cref="Action"/> button, e.g. "Undo (Ctrl+Z)".</summary>
    public string? ActionTitle { get; set; }

    /// <summary>Keyboard shortcut that also runs the action, announced through <c>aria-keyshortcuts</c> (e.g. "Control+Z"). The app handles the key itself.</summary>
    public string? ActionKeyShortcuts { get; set; }

    /// <summary>Move keyboard focus to the <see cref="Action"/> button when the toast appears. Pair it with <see cref="ReturnFocus"/>.</summary>
    public bool FocusAction { get; set; }

    /// <summary>Icon shown instead of the severity icon (an <c>Icons.Material.*</c> name).</summary>
    public string? Icon { get; set; }

    /// <summary>Show no icon on the toast.</summary>
    public bool HideIcon { get; set; }

    /// <summary>Show a bar with the time left before the toast auto-dismisses (it pauses with the countdown).</summary>
    public bool ShowProgress { get; set; }

    /// <summary>
    /// Element that gets focus back when the toast closes while it held focus (e.g. after its action
    /// button was clicked); otherwise focus would fall to the page body.
    /// </summary>
    public ElementReference? ReturnFocus { get; set; }

    /// <summary>Show a manual close button on the toast.</summary>
    public bool ShowCloseIcon { get; set; } = true;

    /// <summary>When true, an identical message will not be added again while it is still shown.</summary>
    public bool PreventDuplicates { get; set; } = true;

    /// <summary>
    /// Alert style of this toast: Filled is solid, Outlined adds <c>alert-outline</c>, Text adds <c>alert-soft</c>.
    /// When null (default), the toast uses <see cref="SnackbarProvider.Variant"/>.
    /// </summary>
    public Variant? Variant { get; set; }

    /// <summary>Extra CSS classes merged onto this toast's alert element, after <see cref="SnackbarProvider.AlertClass"/>.</summary>
    public string? Class { get; set; }
}
