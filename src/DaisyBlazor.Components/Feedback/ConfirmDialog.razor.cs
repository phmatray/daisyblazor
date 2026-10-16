using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Feedback;

/// <summary>
/// Standard confirm/cancel dialog. Pass <see cref="CancelText"/> for a localized cancel label.
/// </summary>
public partial class ConfirmDialog
{
    [CascadingParameter]
    private IMudDialogInstance? MudDialog { get; set; }

    /// <summary>Body text shown inside the dialog.</summary>
    [Parameter]
    public string ContentText { get; set; } = string.Empty;

    /// <summary>Label of the confirm button. Defaults to <c>"Confirm"</c>; pass a localized string to override.</summary>
    [Parameter]
    public string ButtonText { get; set; } = "Confirm";

    /// <summary>Color applied to the confirm button.</summary>
    [Parameter]
    public Color Color { get; set; } = Color.Primary;

    /// <summary>Label of the cancel button. Defaults to <c>"Cancel"</c>; pass a localized string to override.</summary>
    [Parameter]
    public string? CancelText { get; set; }

    private void Submit() => MudDialog?.Close(DialogResult.Ok(true));
    private void Cancel() => MudDialog?.Cancel();
}
