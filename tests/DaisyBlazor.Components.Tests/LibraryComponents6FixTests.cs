using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Components.Tests;

/// <summary>Regression tests for the library-components-6 bug bucket.</summary>
public class LibraryComponents6FixTests : BunitContext
{
    private readonly DialogService _dialogs = new();

    public LibraryComponents6FixTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IDialogService>(_dialogs);
    }

    [Fact]
    public void IconButton_inherit_follows_current_color()
    {
        IRenderedComponent<IconButton> cut = Render<IconButton>(ps => ps.Add(p => p.Icon, "mail"));
        cut.Find("button").GetAttribute("style")!.ShouldContain("--btn-rest-fg:currentColor");

        IRenderedComponent<IconButton> coloured = Render<IconButton>(ps => ps
            .Add(p => p.Icon, "mail").Add(p => p.Color, Color.Primary));
        coloured.Find("button").HasAttribute("style").ShouldBeFalse();
    }

    [Fact]
    public void Disabled_IconButton_link_is_not_navigable()
    {
        IRenderedComponent<IconButton> cut = Render<IconButton>(ps => ps
            .Add(p => p.Icon, "open_in_new").Add(p => p.Href, "/docs").Add(p => p.Disabled, true));

        AngleSharp.Dom.IElement a = cut.Find("a");
        a.HasAttribute("href").ShouldBeFalse();
        a.GetAttribute("aria-disabled").ShouldBe("true");
        a.GetAttribute("tabindex").ShouldBe("-1");
        a.ClassList.ShouldContain("btn-disabled");
    }

    [Fact]
    public void FabAction_button_is_named_by_its_tooltip_and_takes_Class()
    {
        IRenderedComponent<FabAction> cut = Render<FabAction>(ps => ps
            .Add(p => p.Icon, "add").Add(p => p.Tooltip, "New project").Add(p => p.Class, "my-action"));

        AngleSharp.Dom.IElement button = cut.Find("button");
        button.GetAttribute("aria-label").ShouldBe("New project");
        button.ClassList.ShouldContain("my-action");
    }

    [Fact]
    public async Task Dialog_max_width_medium_is_wider_than_the_default_and_title_clears_close_button()
    {
        IRenderedComponent<DialogProvider> cut = Render<DialogProvider>();
        await cut.InvokeAsync(() => _dialogs.ShowAsync<Dialog>("A rather long dialog title",
            options: new DialogOptions { MaxWidth = MaxWidth.Medium, CloseButton = true }));

        cut.WaitForAssertion(() =>
        {
            AngleSharp.Dom.IElement box = cut.Find(".modal-box");
            box.ClassList.ShouldContain("max-w-4xl");
            box.ClassList.ShouldContain("[&_[id$=-title]]:pr-8");
        });
    }

    [Fact]
    public void Dropdown_ActivatorClass_styles_the_summary()
    {
        IRenderedComponent<Dropdown> cut = Render<Dropdown>(ps => ps.Add(p => p.ActivatorClass, "btn btn-primary"));
        cut.Find("summary").ClassList.ShouldContain("btn-primary");
    }

    [Fact]
    public void Disabled_breadcrumb_is_marked_disabled()
    {
        IRenderedComponent<Breadcrumbs> cut = Render<Breadcrumbs>(ps => ps.Add(p => p.Items,
        [
            new BreadcrumbItem("Archive", "/archive", Disabled: true),
            new BreadcrumbItem("Invoices", null),
        ]));

        AngleSharp.Dom.IElement span = cut.FindAll("span[aria-disabled]").ShouldHaveSingleItem();
        span.TextContent.ShouldContain("Archive");
        span.ClassList.ShouldNotContain("opacity-50");
    }

    [Fact]
    public void ProgressCircular_determinate_renders_a_filled_ring()
    {
        IRenderedComponent<ProgressCircular> cut = Render<ProgressCircular>(ps => ps
            .Add(p => p.Indeterminate, false).Add(p => p.Value, 40));

        AngleSharp.Dom.IElement ring = cut.Find(".radial-progress");
        ring.GetAttribute("role").ShouldBe("progressbar");
        ring.GetAttribute("aria-valuenow").ShouldBe("40");
        ring.GetAttribute("style")!.ShouldContain("--value:40");
        cut.FindAll(".loading-spinner").ShouldBeEmpty();
    }

    [Fact]
    public void ProgressCircular_maps_value_into_min_max()
    {
        IRenderedComponent<ProgressCircular> cut = Render<ProgressCircular>(ps => ps
            .Add(p => p.Indeterminate, false).Add(p => p.Min, 10).Add(p => p.Max, 20).Add(p => p.Value, 15));

        cut.Find(".radial-progress").GetAttribute("style")!.ShouldContain("--value:50");
    }

    [Fact]
    public void Validator_hint_is_a_sibling_of_the_validator_element()
    {
        IRenderedComponent<Validator> cut = Render<Validator>(ps => ps
            .Add(p => p.Hint, "Enter a valid email address.")
            .AddChildContent("<input type=\"email\" required />"));

        // daisyUI: .validator:has(:user-invalid) ~ .validator-hint
        cut.Find(".validator ~ .validator-hint").TextContent.ShouldBe("Enter a valid email address.");
        cut.Find(".validator input").ShouldNotBeNull();
    }
}
