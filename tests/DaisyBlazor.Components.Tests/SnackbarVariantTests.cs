using Microsoft.Extensions.DependencyInjection;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// SnackbarProvider alert style hooks (#68): provider-wide Variant/AlertClass and per-message overrides.
/// </summary>
public class SnackbarVariantTests : BunitContext
{
    private readonly SnackbarService _snackbar = new();

    public SnackbarVariantTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<ISnackbar>(_snackbar);
    }

    [Fact]
    public void Default_toast_is_a_filled_alert()
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>();
        cut.InvokeAsync(() => _snackbar.Add("Saved", Severity.Success));

        cut.WaitForAssertion(() => cut.Find(".alert").ClassName.ShouldBe("alert alert-success shadow-lg"));
    }

    [Fact]
    public void Provider_variant_and_alert_class_apply_to_every_toast()
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>(ps => ps
            .Add(p => p.Variant, Variant.Text)
            .Add(p => p.AlertClass, "w-96"));
        cut.InvokeAsync(() => _snackbar.Add("Storage almost full", Severity.Warning));

        cut.WaitForAssertion(() =>
        {
            string cls = cut.Find(".alert").ClassName!;
            cls.ShouldContain("alert-warning");
            cls.ShouldContain("alert-soft");
            cls.ShouldContain("text-base-content");
            cls.ShouldContain("w-96");
            cut.Find(".alert").InnerHtml.ShouldContain("text-warning"); // icon tint
        });
    }

    [Fact]
    public void Message_options_override_the_provider_variant_and_add_classes()
    {
        IRenderedComponent<SnackbarProvider> cut = Render<SnackbarProvider>(ps => ps.Add(p => p.Variant, Variant.Text));
        cut.InvokeAsync(() => _snackbar.Add("Payment declined", Severity.Error, o =>
        {
            o.Variant = Variant.Outlined;
            o.Class = "toast-payment";
        }));
        cut.InvokeAsync(() => _snackbar.Add("Deploy done", Severity.Info, o => o.Variant = Variant.Filled));

        cut.WaitForAssertion(() =>
        {
            IReadOnlyList<AngleSharp.Dom.IElement> alerts = cut.FindAll(".alert");
            alerts.Count.ShouldBe(2);
            alerts[0].ClassName.ShouldBe("alert alert-error alert-outline text-base-content shadow-lg toast-payment");
            alerts[1].ClassName.ShouldBe("alert alert-info shadow-lg");
        });
    }
}
