using AngleSharp.Dom;
using DaisyBlazor.Feedback;
using DaisyBlazor.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Regression tests for Autocomplete, Skeleton, DetailLayout, ConfirmDialog and NumericField fixes.
/// </summary>
public class LibraryComponents10FixTests : BunitContext
{
    private static readonly string[] Customers = ["Acme Cloud", "Acme Labs", "Globex"];

    public LibraryComponents10FixTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    private IRenderedComponent<Autocomplete<string>> RenderAutocomplete(string? value = null) =>
        Render<Autocomplete<string>>(ps => ps
            .Add(p => p.Label, "Customer")
            .Add(p => p.Value, value)
            .Add(p => p.DebounceInterval, 0)
            .Add(p => p.MinCharacters, 1)
            .Add(p => p.SearchFunc, (text, _) =>
                Task.FromResult(Customers.Where(c => c.Contains(text, StringComparison.OrdinalIgnoreCase)))));

    private static async Task TypeAsync(IRenderedComponent<Autocomplete<string>> cut, string text)
    {
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = text });
        cut.WaitForAssertion(() => cut.FindAll("[role=option]").Count.ShouldBeGreaterThan(0));
    }

    [Fact]
    public void Autocomplete_focus_without_search_renders_no_empty_panel()
    {
        IRenderedComponent<Autocomplete<string>> cut = RenderAutocomplete();
        cut.Find("input").Focus();

        cut.FindAll("ul").Count.ShouldBe(0);
        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("false");
    }

    [Fact]
    public async Task Autocomplete_closes_on_escape_and_on_blur()
    {
        IRenderedComponent<Autocomplete<string>> cut = RenderAutocomplete();
        await TypeAsync(cut, "acme");
        cut.Find("input").GetAttribute("aria-expanded").ShouldBe("true");

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });
        cut.FindAll("ul").Count.ShouldBe(0);

        await TypeAsync(cut, "acm");
        await cut.Find("input").BlurAsync(new FocusEventArgs());
        cut.FindAll("ul").Count.ShouldBe(0);
    }

    [Fact]
    public async Task Autocomplete_has_combobox_semantics_and_arrow_enter_selection()
    {
        string? selected = null;
        IRenderedComponent<Autocomplete<string>> cut = Render<Autocomplete<string>>(ps => ps
            .Add(p => p.DebounceInterval, 0)
            .Add(p => p.MinCharacters, 1)
            .Add(p => p.ValueChanged, v => selected = v)
            .Add(p => p.SearchFunc, (text, _) =>
                Task.FromResult(Customers.Where(c => c.Contains(text, StringComparison.OrdinalIgnoreCase)))));
        await TypeAsync(cut, "acme");

        IElement input = cut.Find("input");
        input.GetAttribute("role").ShouldBe("combobox");
        cut.Find("[role=listbox]").Id.ShouldBe(input.GetAttribute("aria-controls"));

        await input.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        string active = cut.Find("input").GetAttribute("aria-activedescendant")!;
        cut.Find($"#{active}").TextContent.Trim().ShouldBe("Acme Labs");
        cut.Find($"#{active}").GetAttribute("aria-selected").ShouldBe("true");

        await cut.Find("input").KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
        selected.ShouldBe("Acme Labs");
        cut.FindAll("ul").Count.ShouldBe(0);
    }

    [Fact]
    public void Autocomplete_uses_fieldset_markup_like_other_fields()
    {
        IRenderedComponent<Autocomplete<string>> cut = RenderAutocomplete();

        cut.FindAll(".fieldset").Count.ShouldBe(1);
        cut.FindAll(".form-control").Count.ShouldBe(0);
    }

    [Fact]
    public void Autocomplete_text_follows_outside_value_changes()
    {
        IRenderedComponent<Autocomplete<string>> cut = RenderAutocomplete("Acme Cloud");
        cut.Find("input").GetAttribute("value").ShouldBe("Acme Cloud");

        cut.Render(ps => ps.Add(p => p.Value, "Globex"));
        cut.Find("input").GetAttribute("value").ShouldBe("Globex");

        cut.Render(ps => ps.Add(p => p.Value, (string?)null));
        cut.Find("input").GetAttribute("value").ShouldBe(string.Empty);
    }

    [Fact]
    public void Skeleton_text_is_a_pill_with_a_default_height_and_hidden_from_at()
    {
        IElement text = Render<Skeleton>().Find(".skeleton");
        text.ClassList.ShouldContain("rounded-full");
        text.GetAttribute("style")!.ShouldContain("min-height:1em");
        text.GetAttribute("aria-hidden").ShouldBe("true");

        IElement rect = Render<Skeleton>(ps => ps.Add(p => p.SkeletonType, SkeletonType.Rectangle)).Find(".skeleton");
        rect.ClassList.ShouldNotContain("rounded-full");
        rect.GetAttribute("style").ShouldBeNullOrEmpty();
    }

    [Fact]
    public void DetailLayout_back_control_is_a_link_and_panels_fill_their_cells()
    {
        IRenderedComponent<DetailLayout> cut = Render<DetailLayout>(ps => ps
            .Add(p => p.Title, "Invoice INV-1042")
            .Add(p => p.BackRoute, "/invoices")
            .Add(p => p.BackLabel, "All invoices")
            .Add(p => p.MainContent, (RenderFragment)(b => b.AddContent(0, "Pro plan")))
            .Add(p => p.Sidebar, (RenderFragment)(b => b.AddContent(0, "Total"))));

        IElement back = cut.Find("a");
        back.GetAttribute("href").ShouldBe("/invoices");
        back.TextContent.ShouldContain("All invoices");
        cut.FindAll(".h-full").Count.ShouldBe(2);
    }

    [Fact]
    public async Task ConfirmDialog_confirm_button_has_a_default_label()
    {
        DialogService dialogs = new();
        Services.AddSingleton<IDialogService>(dialogs);
        IRenderedComponent<DialogProvider> cut = Render<DialogProvider>();
        await cut.InvokeAsync(() => dialogs.ShowAsync<ConfirmDialog>("Delete project?"));

        cut.WaitForAssertion(() => cut.FindAll("button").Select(b => b.TextContent.Trim()).ShouldContain("Confirm"));
    }

    [Fact]
    public void NumericField_omits_unset_bounds_but_keeps_explicit_zero()
    {
        IElement unset = Render<NumericField<int>>(ps => ps.Add(p => p.Value, 12)).Find("input");
        unset.HasAttribute("min").ShouldBeFalse();
        unset.HasAttribute("max").ShouldBeFalse();
        unset.HasAttribute("step").ShouldBeFalse();

        IElement zeroMin = Render<NumericField<int>>(ps => ps.Add(p => p.Min, 0).Add(p => p.Max, 50)).Find("input");
        zeroMin.GetAttribute("min").ShouldBe("0");
        zeroMin.GetAttribute("max").ShouldBe("50");

        Render<NumericField<decimal>>().Find("input").GetAttribute("step").ShouldBe("any");
        Render<NumericField<int>>(ps => ps.Add(p => p.Step, 0)).Find("input").HasAttribute("step").ShouldBeFalse();
    }
}
