namespace DaisyBlazor;

/// <summary>
/// Per-message snackbar configuration, mirroring the MudBlazor options surface used across DaisyBlazor.
/// </summary>
public sealed class SnackbarOptions
{
    /// <summary>How long (ms) the toast stays visible before it auto-dismisses.</summary>
    public int VisibleStateDuration { get; set; } = 5000;

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
