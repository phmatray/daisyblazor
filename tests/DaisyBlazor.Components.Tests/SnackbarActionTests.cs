using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Components.Tests;

/// <summary>Snackbar house duration (#106), action button, progress bar, focus return (#112) and single-run action (#121).</summary>
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

    [Fact]
    public void Action_runs_once_and_the_toast_closes_before_the_callback_finishes()
    {
        int runs = 0;
        TaskCompletionSource pending = new();
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>();
        cut.InvokeAsync(() => _snackbar.Add("Project archived", Severity.Info, o =>
        {
            o.Action = "Undo";
            o.OnAction = () => { runs++; return pending.Task; };
        }));

        AngleSharp.Dom.IElement undo = cut.WaitForElement("button.btn-sm");
        undo.Click();
        cut.FindAll(".alert").Count.ShouldBe(0);
        cut.FindAll("button.btn-sm").ShouldBeEmpty();

        pending.SetResult();
        runs.ShouldBe(1);
        _snackbar.Shown.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(Origin.BottomRight, "toast-bottom toast-end")]
    [InlineData(Origin.TopCenter, "toast-top toast-center")]
    [InlineData(Origin.CenterLeft, "toast-middle toast-start")]
    public void Position_places_the_toast_stack(Origin position, string classes)
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>(ps => ps.Add(p => p.Position, position));

        foreach (string c in classes.Split(' '))
        {
            cut.Find(".toast").ClassList.ShouldContain(c);
        }
    }

    [Fact]
    public void Escape_closes_the_focused_toast()
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>();
        cut.InvokeAsync(() => _snackbar.Add("Invoice sent"));

        cut.WaitForElement(".alert").KeyDown(new KeyboardEventArgs { Key = "Escape" });

        _snackbar.Shown.ShouldBeEmpty();
    }

    [Fact]
    public void Action_button_carries_its_title_and_shortcut_and_can_take_focus()
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>();
        cut.InvokeAsync(() => _snackbar.Add("Project archived", Severity.Info, o =>
        {
            o.Action = "Undo";
            o.ActionTitle = "Undo (Ctrl+Z)";
            o.ActionKeyShortcuts = "Control+Z";
            o.FocusAction = true;
        }));

        AngleSharp.Dom.IElement undo = cut.WaitForElement("button.btn-sm");
        undo.GetAttribute("title").ShouldBe("Undo (Ctrl+Z)");
        undo.GetAttribute("aria-keyshortcuts").ShouldBe("Control+Z");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke());
    }

    [Fact]
    public void Icon_can_be_replaced_or_hidden()
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>();
        cut.InvokeAsync(() =>
        {
            _snackbar.Add("Custom", configure: o => o.Icon = Icons.Material.Filled.Cloud);
            _snackbar.Add("Plain", configure: o => o.HideIcon = true);
        });

        cut.WaitForAssertion(() => cut.FindAll(".alert").Count.ShouldBe(2));
        cut.FindAll(".alert")[0].InnerHtml.ShouldContain("cloud");
        cut.FindAll(".alert")[1].InnerHtml.ShouldNotContain("notifications");
    }
}
