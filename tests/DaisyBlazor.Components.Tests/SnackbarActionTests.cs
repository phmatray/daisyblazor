using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Components.Tests;

/// <summary>Snackbar house duration (#106) and action button, progress bar, focus return (#112).</summary>
public class SnackbarActionTests : BunitContext
{
    private readonly SnackbarService _snackbar = new();

    public SnackbarActionTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<ISnackbar>(_snackbar);
    }

    [Fact]
    public void Provider_duration_is_the_default_and_a_message_duration_overrides_it()
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>(ps => ps.Add(p => p.VisibleStateDuration, 50));
        cut.InvokeAsync(() =>
        {
            _snackbar.Add("Quick");
            _snackbar.Add("Sticky", configure: o => o.VisibleStateDuration = 60_000);
        });

        cut.WaitForAssertion(() => _snackbar.Shown.Select(m => m.Message).ShouldBe(["Sticky"]), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Action_button_runs_its_callback_closes_the_toast_and_returns_focus()
    {
        int undone = 0;
        ElementReference opener = new("opener", new WebElementReferenceContext(JSInterop.JSRuntime));
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>();
        cut.InvokeAsync(() => _snackbar.Add("Song deleted", Severity.Info, o =>
        {
            o.Action = "Undo";
            o.OnAction = () => { undone++; return Task.CompletedTask; };
            o.ShowProgress = true;
            o.ReturnFocus = opener;
        }));

        cut.WaitForAssertion(() => cut.Find(".snackbar-progress"));
        cut.Find(".alert").FocusIn();
        cut.Find(".snackbar-progress").GetAttribute("style")!.ShouldContain("animation-play-state:paused");
        cut.FindAll("button").Single(b => b.TextContent.Trim() == "Undo").Click();

        undone.ShouldBe(1);
        cut.WaitForAssertion(() => cut.FindAll(".alert").Count.ShouldBe(0));
        JSInterop.VerifyFocusAsyncInvoke().Arguments[0].ShouldBe(opener);
    }
}
