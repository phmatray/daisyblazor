using DaisyBlazor.Feedback;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Tests for <see cref="AccessDenied"/>.
/// </summary>
/// <remarks>
/// Added because a reference consumer had hand-written this exact panel and used it on
/// <b>104 pages out of 104</b> — its single most-used local component — while the kit already
/// exposed a <c>FeatureHomePage.AccessDeniedContent</c> slot with nothing to put in it.
/// </remarks>
public class AccessDeniedTests : BunitContext
{
    [Fact]
    public void Renders_a_lock_and_a_default_title_with_no_parameters()
    {
        IRenderedComponent<AccessDenied> cut = Render<AccessDenied>();

        cut.Markup.ShouldContain("Access denied");
        cut.Markup.ShouldContain("lock");
    }

    [Fact]
    public void Renders_the_supplied_title_and_message()
    {
        IRenderedComponent<AccessDenied> cut = Render<AccessDenied>(ps => ps
            .Add(p => p.Title, "Accès refusé")
            .Add(p => p.Message, "Vous n'avez pas le rôle requis."));

        cut.Markup.ShouldContain("Accès refusé");
        cut.Markup.ShouldContain("Vous n'avez pas le rôle requis.");
    }

    [Fact]
    public void Omits_the_message_when_none_is_given()
    {
        // The title renders as <h5> (Typo.h5) and the message as <p> (Typo.body1),
        // so counting <p> isolates the message.
        IRenderedComponent<AccessDenied> without = Render<AccessDenied>(ps => ps
            .Add(p => p.Title, "Nope"));
        without.FindAll("p").Count.ShouldBe(0);
        without.Find("h5").TextContent.Trim().ShouldBe("Nope");

        IRenderedComponent<AccessDenied> with = Render<AccessDenied>(ps => ps
            .Add(p => p.Title, "Nope")
            .Add(p => p.Message, "Because reasons."));
        with.FindAll("p").Count.ShouldBe(1);
    }

    [Fact]
    public void Action_button_appears_only_when_ActionText_is_set()
    {
        IRenderedComponent<AccessDenied> without = Render<AccessDenied>();
        without.FindAll("a, button").Count.ShouldBe(0);

        IRenderedComponent<AccessDenied> with = Render<AccessDenied>(ps => ps
            .Add(p => p.ActionText, "Back to home")
            .Add(p => p.ActionHref, "/home"));

        with.Find("a").GetAttribute("href").ShouldBe("/home");
        with.Markup.ShouldContain("Back to home");
    }

    [Fact]
    public async Task Action_can_be_a_callback_instead_of_a_link()
    {
        bool clicked = false;

        IRenderedComponent<AccessDenied> cut = Render<AccessDenied>(ps => ps
            .Add(p => p.ActionText, "Request access")
            .Add(p => p.ActionHref, null)
            .Add(p => p.OnAction, EventCallback.Factory.Create(this, () => clicked = true)));

        await cut.Find("button").ClickAsync(new());
        clicked.ShouldBeTrue();
    }

    [Fact]
    public void Icon_can_be_replaced_or_dropped()
    {
        IRenderedComponent<AccessDenied> custom = Render<AccessDenied>(ps => ps
            .Add(p => p.Icon, "shield"));
        custom.Markup.ShouldContain("shield");

        IRenderedComponent<AccessDenied> none = Render<AccessDenied>(ps => ps
            .Add(p => p.Icon, null));
        none.FindAll(".material-symbols-outlined").Count.ShouldBe(0);
    }

    [Fact]
    public void ChildContent_renders_between_the_message_and_the_action()
    {
        IRenderedComponent<AccessDenied> cut = Render<AccessDenied>(ps => ps
            .Add(p => p.Message, "You need one of:")
            .Add(p => p.ActionText, "Back")
            .AddChildContent("<ul id=\"roles\"><li>Flights.Viewer</li></ul>"));

        cut.Find("#roles").ShouldNotBeNull();
        cut.Markup.IndexOf("You need one of:", StringComparison.Ordinal)
            .ShouldBeLessThan(cut.Markup.IndexOf("id=\"roles\"", StringComparison.Ordinal));
        cut.Markup.IndexOf("id=\"roles\"", StringComparison.Ordinal)
            .ShouldBeLessThan(cut.Markup.IndexOf("Back", StringComparison.Ordinal));
    }

    [Fact]
    public void MinHeight_and_caller_classes_reach_the_container()
    {
        IRenderedComponent<AccessDenied> cut = Render<AccessDenied>(ps => ps
            .Add(p => p.MinHeight, "60vh")
            .Add(p => p.Class, "my-panel"));

        string markup = cut.Markup;
        markup.ShouldContain("min-height: 60vh");
        markup.ShouldContain("my-panel");
    }
}
