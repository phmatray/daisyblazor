using DaisyBlazor.Validation;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Tests for the server-validation plumbing.
/// </summary>
/// <remarks>
/// This exists because a measured consumer showed the gap plainly: its backend returns structured,
/// localizable per-field validation errors, the generated API client models them as real types —
/// and nothing rendered them. `ValidationMessage`/`DataAnnotationsValidator` appeared in 1 file out
/// of 208, `EditForm` in 1, `Validator` in 0. Meanwhile TextField and NumericField already had
/// Error/ErrorText. Only the wiring was missing.
/// </remarks>
public class ServerValidationStateTests
{
    [Fact]
    public void Reports_errors_for_a_field()
    {
        ServerValidationState state = new();
        state.SetErrors([new KeyValuePair<string, string>("Name", "Name is required")]);

        state.HasErrors.ShouldBeTrue();
        state.HasErrorFor("Name").ShouldBeTrue();
        state.FirstErrorFor("Name").ShouldBe("Name is required");
        state.ErrorsFor("Name").Count.ShouldBe(1);
    }

    [Fact]
    public void Field_lookup_is_case_insensitive()
    {
        ServerValidationState state = new();
        state.SetErrors([("name", "boom")]);

        state.HasErrorFor("Name").ShouldBeTrue();
        state.HasErrorFor("NAME").ShouldBeTrue();
    }

    [Theory]
    [InlineData("request.Name")]
    [InlineData("Model.Address.City")]
    public void Falls_back_to_the_last_dot_segment(string serverKey)
    {
        // Real APIs rarely agree on how deeply they qualify field names; the component should not
        // force the caller to normalise them by hand.
        string field = serverKey.Split('.')[^1];

        ServerValidationState state = new();
        state.SetErrors([(serverKey, "nope")]);

        state.HasErrorFor(field).ShouldBeTrue();
        state.FirstErrorFor(field).ShouldBe("nope");
    }

    [Fact]
    public void Keeps_every_message_for_a_field()
    {
        ServerValidationState state = new();
        state.SetErrors([("Name", "too short"), ("Name", "already taken")]);

        state.ErrorsFor("Name").ShouldBe(["too short", "already taken"]);
        state.Count.ShouldBe(2);
    }

    [Fact]
    public void Unknown_field_and_null_are_safe()
    {
        ServerValidationState state = new();
        state.SetErrors([("Name", "x")]);

        state.ErrorsFor("Nope").ShouldBeEmpty();
        state.ErrorsFor(null).ShouldBeEmpty();
        state.HasErrorFor("").ShouldBeFalse();
    }

    [Fact]
    public void SetErrors_replaces_rather_than_accumulates()
    {
        ServerValidationState state = new();
        state.SetErrors([("Name", "first")]);
        state.SetErrors([("Other", "second")]);

        state.HasErrorFor("Name").ShouldBeFalse();
        state.HasErrorFor("Other").ShouldBeTrue();
    }

    [Fact]
    public void Clear_and_ClearField_work_and_notify()
    {
        int notifications = 0;
        ServerValidationState state = new();
        state.OnChange += () => notifications++;

        state.SetErrors([("A", "a"), ("B", "b")]);
        state.ClearField("A");
        state.HasErrorFor("A").ShouldBeFalse();
        state.HasErrorFor("B").ShouldBeTrue();

        state.Clear();
        state.HasErrors.ShouldBeFalse();

        notifications.ShouldBe(3);   // SetErrors + ClearField + Clear
    }

    [Fact]
    public void Blank_field_or_message_is_ignored()
    {
        ServerValidationState state = new();
        state.SetErrors([("", "orphan"), ("Field", "")]);

        state.HasErrors.ShouldBeFalse();
    }
}

/// <summary>Rendering tests for ServerValidation, FieldError and the wired inputs.</summary>
public class ServerValidationRenderTests : BunitContext
{
    private IRenderedComponent<ServerValidation> RenderForm(ServerValidationState state, RenderFragment inner) =>
        Render<ServerValidation>(ps => ps
            .Add(p => p.State, state)
            .Add(p => p.ChildContent, inner));

    [Fact]
    public void TextField_picks_up_a_server_error_via_FieldName()
    {
        ServerValidationState state = new();
        state.SetErrors([("Name", "Name is required")]);

        IRenderedComponent<ServerValidation> cut = RenderForm(state, b =>
        {
            b.OpenComponent<TextField>(0);
            b.AddAttribute(1, nameof(TextField.Label), "Name");
            b.AddAttribute(2, nameof(TextField.FieldName), "Name");
            b.CloseComponent();
        });

        cut.Markup.ShouldContain("Name is required");
        cut.Markup.ShouldContain("input-error");
    }

    [Fact]
    public void TextField_without_ServerValidation_is_unchanged()
    {
        // The whole point: adding this feature must not alter a single existing usage.
        IRenderedComponent<TextField> cut = Render<TextField>(ps => ps
            .Add(p => p.Label, "Name")
            .Add(p => p.HelperText, "helper"));

        cut.Markup.ShouldNotContain("input-error");
        cut.Markup.ShouldNotContain("text-error");
        cut.Markup.ShouldContain("helper");
    }

    [Fact]
    public void Explicit_Error_still_wins_over_the_cascade()
    {
        ServerValidationState state = new();   // no errors at all

        IRenderedComponent<ServerValidation> cut = RenderForm(state, b =>
        {
            b.OpenComponent<TextField>(0);
            b.AddAttribute(1, nameof(TextField.FieldName), "Name");
            b.AddAttribute(2, nameof(TextField.Error), true);
            b.AddAttribute(3, nameof(TextField.ErrorText), "explicit");
            b.CloseComponent();
        });

        cut.Markup.ShouldContain("explicit");
        cut.Markup.ShouldContain("input-error");
    }

    [Fact]
    public void A_field_without_an_error_stays_clean()
    {
        ServerValidationState state = new();
        state.SetErrors([("Other", "boom")]);

        IRenderedComponent<ServerValidation> cut = RenderForm(state, b =>
        {
            b.OpenComponent<TextField>(0);
            b.AddAttribute(1, nameof(TextField.FieldName), "Name");
            b.AddAttribute(2, nameof(TextField.HelperText), "helper");
            b.CloseComponent();
        });

        cut.Markup.ShouldNotContain("input-error");
        cut.Markup.ShouldContain("helper");
    }

    [Fact]
    public void Select_now_supports_errors_and_the_cascade()
    {
        ServerValidationState state = new();
        state.SetErrors([("Kind", "pick one")]);

        IRenderedComponent<ServerValidation> cut = RenderForm(state, b =>
        {
            b.OpenComponent<Select<string>>(0);
            b.AddAttribute(1, "FieldName", "Kind");
            b.CloseComponent();
        });

        cut.Markup.ShouldContain("pick one");
        cut.Markup.ShouldContain("select-error");
    }

    [Fact]
    public void NumericField_picks_up_a_server_error()
    {
        ServerValidationState state = new();
        state.SetErrors([("Seats", "must be positive")]);

        IRenderedComponent<ServerValidation> cut = RenderForm(state, b =>
        {
            b.OpenComponent<NumericField<int>>(0);
            b.AddAttribute(1, "FieldName", "Seats");
            b.CloseComponent();
        });

        cut.Markup.ShouldContain("must be positive");
        cut.Markup.ShouldContain("input-error");
    }

    [Fact]
    public void FieldError_renders_every_message_and_works_for_any_control()
    {
        ServerValidationState state = new();
        state.SetErrors([("Name", "too short"), ("Name", "already taken")]);

        IRenderedComponent<ServerValidation> cut = RenderForm(state, b =>
        {
            b.OpenComponent<FieldError>(0);
            b.AddAttribute(1, nameof(FieldError.For), "Name");
            b.CloseComponent();
        });

        cut.Markup.ShouldContain("too short");
        cut.Markup.ShouldContain("already taken");
        cut.Markup.ShouldContain("text-error");
    }

    [Fact]
    public void FieldError_renders_nothing_when_the_field_is_clean()
    {
        ServerValidationState state = new();

        IRenderedComponent<ServerValidation> cut = RenderForm(state, b =>
        {
            b.OpenComponent<FieldError>(0);
            b.AddAttribute(1, nameof(FieldError.For), "Name");
            b.CloseComponent();
        });

        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void Errors_arriving_after_render_light_the_field_up()
    {
        // The cascaded value is a mutable object, so this only works because ServerValidation
        // subscribes to OnChange and re-renders. Without that, SetErrors() would be invisible.
        ServerValidationState state = new();

        IRenderedComponent<ServerValidation> cut = RenderForm(state, b =>
        {
            b.OpenComponent<TextField>(0);
            b.AddAttribute(1, nameof(TextField.FieldName), "Name");
            b.CloseComponent();
        });

        cut.Markup.ShouldNotContain("input-error");

        cut.InvokeAsync(() => state.SetErrors([("Name", "arrived late")]));

        cut.Markup.ShouldContain("arrived late");
        cut.Markup.ShouldContain("input-error");
    }
}
