using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>Form registers its fields and Validate() / IsValid actually reflect them (issue #77).</summary>
public class FormValidationTests : BunitContext
{
    private readonly List<bool> _isValidChanges = [];

    private IRenderedComponent<Form> RenderForm(RenderFragment fields) =>
        Render<Form>(ps => ps
            .Add(p => p.IsValidChanged, (bool v) => _isValidChanges.Add(v))
            .Add(p => p.ChildContent, fields));

    private static RenderFragment Name(string? value, object? validation = null) => b =>
    {
        b.OpenComponent<TextField>(0);
        b.AddAttribute(1, nameof(TextField.Label), "Workspace name");
        b.AddAttribute(2, nameof(TextField.Required), true);
        b.AddAttribute(3, nameof(TextField.RequiredError), "Workspace name is required");
        b.AddAttribute(4, nameof(TextField.Value), value);
        b.AddAttribute(5, nameof(TextField.Validation), validation);
        b.CloseComponent();
    };

    [Fact]
    public async Task Validate_marks_an_empty_required_field_invalid()
    {
        IRenderedComponent<Form> cut = RenderForm(Name(null));
        cut.Markup.ShouldNotContain("Workspace name is required");

        await cut.InvokeAsync(() => cut.Instance.Validate());

        cut.Instance.IsValid.ShouldBeFalse();
        cut.Markup.ShouldContain("Workspace name is required");
        cut.Markup.ShouldContain("input-error");
        _isValidChanges.ShouldBe([false]);
    }

    [Fact]
    public async Task Filling_the_field_makes_the_form_valid_again()
    {
        IRenderedComponent<Form> cut = RenderForm(Name(null));
        await cut.InvokeAsync(() => cut.Instance.Validate());

        cut.Find("input").Input("Acme");

        cut.Instance.IsValid.ShouldBeTrue();
        cut.Markup.ShouldNotContain("Workspace name is required");
        cut.Markup.ShouldNotContain("input-error");
        _isValidChanges.ShouldBe([false, true]);
    }

    [Fact]
    public async Task A_Validation_func_error_is_shown()
    {
        Func<string?, string?> slug = v => v is { Length: < 3 } ? "Use at least 3 characters" : null;
        IRenderedComponent<Form> cut = RenderForm(Name("ab", slug));

        await cut.InvokeAsync(() => cut.Instance.Validate());

        cut.Instance.IsValid.ShouldBeFalse();
        cut.Markup.ShouldContain("Use at least 3 characters");
    }

    [Fact]
    public async Task A_valid_form_stays_valid_and_does_not_raise_IsValidChanged()
    {
        IRenderedComponent<Form> cut = RenderForm(Name("Acme"));

        await cut.InvokeAsync(() => cut.Instance.Validate());

        cut.Instance.IsValid.ShouldBeTrue();
        _isValidChanges.ShouldBeEmpty();
    }

    [Fact]
    public async Task Required_Select_and_NumericField_are_validated_too()
    {
        IRenderedComponent<Form> cut = RenderForm(b =>
        {
            b.OpenComponent<Select<string>>(0);
            b.AddAttribute(1, "Required", true);
            b.AddAttribute(2, "RequiredError", "Pick a region");
            b.CloseComponent();
            b.OpenComponent<NumericField<int?>>(3);
            b.AddAttribute(4, "Required", true);
            b.AddAttribute(5, "RequiredError", "Seats are required");
            b.CloseComponent();
        });

        await cut.InvokeAsync(() => cut.Instance.Validate());

        cut.Instance.IsValid.ShouldBeFalse();
        cut.Markup.ShouldContain("Pick a region");
        cut.Markup.ShouldContain("Seats are required");
    }
}
