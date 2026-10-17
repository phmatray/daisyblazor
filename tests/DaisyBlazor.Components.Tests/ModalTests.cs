namespace DaisyBlazor.Components.Tests;

/// <summary>Declarative Modal (#67): inline markup on the DialogProvider's accessible path.</summary>
public class ModalTests : BunitContext
{
    public ModalTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Closed_modal_renders_nothing()
    {
        IRenderedComponent<Modal> cut = Render<Modal>(ps => ps.Add(p => p.Title, "Edit"));
        cut.FindAll("dialog").Count.ShouldBe(0);
    }

    [Fact]
    public void Open_modal_is_labelled_by_its_title_and_promoted_with_dialog_js()
    {
        IRenderedComponent<Modal> cut = Render<Modal>(ps => ps
            .Add(p => p.Open, true)
            .Add(p => p.Title, "Invite member")
            .AddChildContent("<p>Body</p>"));

        string labelledBy = cut.Find("[role=dialog]").GetAttribute("aria-labelledby")!;
        cut.Find($"#{labelledBy}").TextContent.ShouldBe("Invite member");
        cut.Find("[role=dialog]").GetAttribute("aria-modal").ShouldBe("true");
        cut.Markup.ShouldContain("<p>Body</p>");
        JSInterop.VerifyInvoke("import");
        JSInterop.VerifyInvoke("open");
    }

    [Fact]
    public void Escape_closes_once_and_raises_OpenChanged_and_OnClose()
    {
        List<bool> changes = [];
        int closes = 0;
        IRenderedComponent<Modal> cut = Render<Modal>(ps => ps
            .Add(p => p.Open, true)
            .Add(p => p.OpenChanged, v => changes.Add(v))
            .Add(p => p.OnClose, () => closes++));

        cut.Find("dialog").KeyDown(key: "Escape");

        changes.ShouldBe([false]);
        closes.ShouldBe(1);
        cut.FindAll("dialog").Count.ShouldBe(0);
        JSInterop.VerifyInvoke("close");
    }

    [Fact]
    public void Escape_and_backdrop_can_be_disabled_and_alertdialog_set()
    {
        int closes = 0;
        IRenderedComponent<Modal> cut = Render<Modal>(ps => ps
            .Add(p => p.Open, true)
            .Add(p => p.AlertDialog, true)
            .Add(p => p.CloseOnEscapeKey, false)
            .Add(p => p.BackdropClick, false)
            .Add(p => p.OnClose, () => closes++));

        cut.Find("dialog").KeyDown(key: "Escape");
        cut.Find("form.modal-backdrop").Click();

        closes.ShouldBe(0);
        cut.FindAll("[role=alertdialog]").Count.ShouldBe(1);
    }

    [Fact]
    public void Close_button_is_localisable_and_parent_close_restores_focus()
    {
        IRenderedComponent<Modal> cut = Render<Modal>(ps => ps
            .Add(p => p.Open, true)
            .Add(p => p.CloseButton, true)
            .Add(p => p.CloseLabel, "Fermer"));
        cut.Find("button[aria-label]").GetAttribute("aria-label").ShouldBe("Fermer");

        cut.Render(ps => ps.Add(p => p.Open, false));

        JSInterop.VerifyInvoke("close");
        cut.FindAll("dialog").Count.ShouldBe(0);
    }

    [Fact]
    public void BodyClass_replaces_the_default_body_wrapper_classes()
    {
        IRenderedComponent<Modal> cut = Render<Modal>(ps => ps
            .Add(p => p.Open, true)
            .Add(p => p.BodyClass, "contents")
            .AddChildContent("<p id=\"b\">Body</p>"));

        cut.Find("#b").ParentElement!.ClassName.ShouldBe("contents");
    }
}
