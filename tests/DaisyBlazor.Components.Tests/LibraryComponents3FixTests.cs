using DaisyBlazor.Layout;
using Microsoft.AspNetCore.Components;

namespace DaisyBlazor.Components.Tests;

/// <summary>Regression tests for the library-components-3 visual/a11y bug batch.</summary>
public class LibraryComponents3FixTests : BunitContext
{
    [Fact]
    public void ReadOnly_checkbox_ignores_changes_and_flags_aria_readonly()
    {
        bool raised = false;
        IRenderedComponent<Checkbox> cut = Render<Checkbox>(ps => ps
            .Add(p => p.Value, true)
            .Add(p => p.ReadOnly, true)
            .Add(p => p.ValueChanged, (bool _) => raised = true));

        cut.Find("input").Change(false);

        raised.ShouldBeFalse();
        cut.Find("input").GetAttribute("aria-readonly").ShouldBe("true");
    }

    [Fact]
    public void Dark_checkbox_maps_to_neutral()
    {
        IRenderedComponent<Checkbox> cut = Render<Checkbox>(ps => ps.Add(p => p.Color, Color.Dark));

        cut.Find("input").ClassList.ShouldContain("checkbox-neutral");
    }

    [Fact]
    public void Indeterminate_checkbox_sets_the_dom_property()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Render<Checkbox>(ps => ps.Add(p => p.Indeterminate, true));

        JSInterop.VerifyInvoke("Reflect.set");
    }

    [Fact]
    public void Plain_checkbox_makes_no_js_call()
    {
        // Strict mode would throw on any unplanned interop call.
        Render<Checkbox>(ps => ps.Add(p => p.Value, true));

        JSInterop.Invocations.ShouldBeEmpty();
    }

    [Fact]
    public void Clearable_textfield_renders_a_clear_button_that_empties_the_value()
    {
        string? value = "acme";
        IRenderedComponent<TextField> cut = Render<TextField>(ps => ps
            .Add(p => p.Value, value)
            .Add(p => p.Clearable, true)
            .Add(p => p.ValueChanged, (string? v) => value = v));

        cut.Find("button[aria-label='Clear']").Click();

        value.ShouldBeNull();
        cut.FindAll("button[aria-label='Clear']").ShouldBeEmpty();
    }

    [Fact]
    public void Text_variant_renders_ghost_fields()
    {
        Render<TextField>(ps => ps.Add(p => p.Variant, Variant.Text))
            .Find("input").ClassList.ShouldContain("input-ghost");
        Render<TextField>(ps => ps.Add(p => p.Variant, Variant.Text).Add(p => p.Lines, 3))
            .Find("textarea").ClassList.ShouldContain("textarea-ghost");
        Render<NumericField<int>>(ps => ps.Add(p => p.Variant, Variant.Text))
            .Find("input").ClassList.ShouldContain("input-ghost");
        Render<Select<string>>(ps => ps.Add(p => p.Variant, Variant.Text))
            .Find("select").ClassList.ShouldContain("select-ghost");
    }

    [Fact]
    public void Error_textfield_links_input_to_its_message()
    {
        IRenderedComponent<TextField> cut = Render<TextField>(ps => ps
            .Add(p => p.Error, true)
            .Add(p => p.ErrorText, "Use four digits"));

        AngleSharp.Dom.IElement input = cut.Find("input");
        input.GetAttribute("aria-invalid").ShouldBe("true");
        cut.Find($"#{input.GetAttribute("aria-describedby")}").TextContent.ShouldBe("Use four digits");
    }

    [Fact]
    public void Helper_text_is_described_without_aria_invalid_on_numeric_and_select()
    {
        AngleSharp.Dom.IElement num = Render<NumericField<int>>(ps => ps.Add(p => p.HelperText, "Seats"))
            .Find("input");
        num.HasAttribute("aria-invalid").ShouldBeFalse();
        num.GetAttribute("aria-describedby").ShouldNotBeNullOrEmpty();

        IRenderedComponent<Select<string>> sel = Render<Select<string>>(ps => ps
            .Add(p => p.Error, true).Add(p => p.ErrorText, "Pick a plan"));
        AngleSharp.Dom.IElement select = sel.Find("select");
        select.GetAttribute("aria-invalid").ShouldBe("true");
        sel.Find($"#{select.GetAttribute("aria-describedby")}").TextContent.ShouldBe("Pick a plan");
    }

    [Fact]
    public void PageHeader_wraps_and_uses_muted_subtitle()
    {
        IRenderedComponent<PageHeader> cut = Render<PageHeader>(ps => ps
            .Add(p => p.Title, "Team")
            .Add(p => p.Subtitle, "Members of Acme Cloud")
            .Add(p => p.Actions, (RenderFragment)(b => b.AddMarkupContent(0, "<button>Invite</button>"))));

        cut.Find("div").ClassList.ShouldContain("flex-wrap");
        cut.Markup.ShouldContain("text-base-content/70");
        cut.Markup.ShouldNotContain("text-secondary");
    }

    [Fact]
    public void DetailRow_value_wraps_and_label_is_muted()
    {
        IRenderedComponent<DetailRow> cut = Render<DetailRow>(ps => ps
            .Add(p => p.Label, "Email")
            .Add(p => p.Value, "ana.silva@northwind.example"));

        cut.FindAll("td")[1].ClassList.ShouldContain("wrap-anywhere");
        cut.Markup.ShouldNotContain("text-secondary");
    }

    [Fact]
    public void Stat_merges_class()
    {
        Render<Stat>(ps => ps.Add(p => p.Class, "bg-base-200"))
            .Find(".stat").ClassList.ShouldContain("bg-base-200");
    }

    [Fact]
    public void ThemeController_HideInput_hides_the_toggle()
    {
        AngleSharp.Dom.IElement input = Render<ThemeController>(ps => ps
            .Add(p => p.Value, "dark")
            .Add(p => p.HideInput, true)).Find("input");

        input.ClassList.ShouldContain("theme-controller");
        input.ClassList.ShouldContain("hidden");
        input.ClassList.ShouldNotContain("toggle");
    }
}
