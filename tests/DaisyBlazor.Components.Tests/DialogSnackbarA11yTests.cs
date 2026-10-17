using DaisyBlazor.Feedback;
using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Accessibility guards for DialogProvider (#47) and SnackbarProvider (#48).
/// </summary>
public class DialogSnackbarA11yTests : BunitContext
{
    private readonly DialogService _dialogs = new();
    private readonly SnackbarService _snackbar = new();

    public DialogSnackbarA11yTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IDialogService>(_dialogs);
        Services.AddSingleton<ISnackbar>(_snackbar);
    }

    [Fact]
    public async Task Dialog_is_labelled_by_its_title_and_promoted_to_modal()
    {
        IRenderedComponent<DialogProvider> cut = Render<DialogProvider>();
        await cut.InvokeAsync(() => _dialogs.ShowAsync<Dialog>("Edit item"));

        cut.WaitForAssertion(() =>
        {
            string labelledBy = cut.Find("[role=dialog]").GetAttribute("aria-labelledby")!;
            cut.Find($"#{labelledBy}").TextContent.ShouldBe("Edit item");
        });

        JSInterop.VerifyInvoke("import");
    }

    [Fact]
    public async Task Escape_cancels_the_dialog_unless_disabled()
    {
        IRenderedComponent<DialogProvider> cut = Render<DialogProvider>();
        IDialogReference kept = await cut.InvokeAsync(() =>
            _dialogs.ShowAsync<Dialog>("Kept", options: new DialogOptions { CloseOnEscapeKey = false }));
        cut.Find("dialog").KeyDown(key: "Escape");
        kept.Result.IsCompleted.ShouldBeFalse();

        await cut.InvokeAsync(() => ((DialogReference)kept).Cancel());
        IDialogReference closed = await cut.InvokeAsync(() => _dialogs.ShowAsync<Dialog>("Closed"));
        cut.WaitForAssertion(() => cut.FindAll("dialog").Count.ShouldBe(1));
        cut.Find("dialog").KeyDown(key: "Escape");

        (await closed.Result).Canceled.ShouldBeTrue();
    }

    [Fact]
    public async Task Confirmations_use_alertdialog_and_close_label_is_localisable()
    {
        IRenderedComponent<DialogProvider> cut = Render<DialogProvider>(ps => ps.Add(p => p.CloseLabel, "Fermer"));
        await cut.InvokeAsync(() => _dialogs.ShowAsync<ConfirmDialog>(
            "Delete?", options: new DialogOptions { CloseButton = true }));

        cut.WaitForAssertion(() => cut.FindAll("[role=alertdialog]").Count.ShouldBe(1));
        cut.Find("button[aria-label]").GetAttribute("aria-label").ShouldBe("Fermer");
    }

    [Fact]
    public void Snackbar_has_single_live_region_and_localisable_close()
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>(ps => ps.Add(p => p.CloseLabel, "Fermer"));
        cut.InvokeAsync(() => _snackbar.Add("Boom", Severity.Error));

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Boom"));
        cut.FindAll("[role=alert]").Count.ShouldBe(0);
        cut.FindAll("[role=status]").Count.ShouldBe(0);
        cut.FindAll("[aria-live]").Count.ShouldBe(1);
        cut.Find("[aria-live]").GetAttribute("aria-live").ShouldBe("assertive");
        cut.Find("button").GetAttribute("aria-label").ShouldBe("Fermer");
    }

    [Fact]
    public async Task Snackbar_pauses_auto_hide_while_hovered()
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>();
        await cut.InvokeAsync(() => _snackbar.Add("Saved", Severity.Success, o => o.VisibleStateDuration = 1000)); // long enough to hover before it expires under CI load
        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Saved"));

        cut.Find(".alert").MouseEnter();
        await Task.Delay(1500, Xunit.TestContext.Current.CancellationToken);
        _snackbar.Shown.Count.ShouldBe(1);

        cut.Find(".alert").MouseLeave();
        cut.WaitForAssertion(() => _snackbar.Shown.Count.ShouldBe(0), TimeSpan.FromSeconds(5));
    }
}
