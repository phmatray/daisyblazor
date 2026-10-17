using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// #97: closing a service dialog from the provider's own close button or Escape must not re-open
/// it (showModal) while dialog.js is still closing it, or focus ends up lost instead of on the opener.
/// </summary>
public class DialogProviderFocusTests : BunitContext
{
    private readonly DialogService _dialogs = new();

    public DialogProviderFocusTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IDialogService>(_dialogs);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Closing_from_the_provider_does_not_reopen_the_dialog(bool viaCloseButton)
    {
        BunitJSModuleInterop module = JSInterop.SetupModule("./_content/DaisyBlazor.Components/dialog.js");
        module.SetupVoid("open", _ => true).SetVoidResult();
        JSRuntimeInvocationHandler close = module.SetupVoid("close", _ => true); // left pending, like a real round trip

        IRenderedComponent<DialogProvider> cut = Render<DialogProvider>();
        IDialogReference reference = await cut.InvokeAsync(() =>
            _dialogs.ShowAsync<Dialog>("Edit item", options: new DialogOptions { CloseButton = true }));
        cut.WaitForAssertion(() => module.VerifyInvoke("open"));

        if (viaCloseButton)
        {
            cut.Find("button[aria-label]").Click();
        }
        else
        {
            cut.Find("dialog").KeyDown(key: "Escape");
        }

        module.VerifyInvoke("close");
        module.VerifyInvoke("open", calledTimes: 1);

        await cut.InvokeAsync(() => close.SetVoidResult());
        cut.WaitForAssertion(() => cut.FindAll("dialog").Count.ShouldBe(0));
        (await reference.Result).Canceled.ShouldBeTrue();
        module.VerifyInvoke("open", calledTimes: 1);
    }
}
