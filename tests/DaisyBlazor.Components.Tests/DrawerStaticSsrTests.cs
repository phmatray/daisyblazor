using Microsoft.Extensions.DependencyInjection;
using DaisyBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging.Abstractions;

namespace DaisyBlazor.Components.Tests;

/// <summary>Drawer on static SSR pages and its localisable overlay label (#69).</summary>
public class DrawerStaticSsrTests : BunitContext
{
    public DrawerStaticSsrTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public async Task Static_render_marks_the_drawer_for_the_js_fallback()
    {
        // HtmlRenderer is the static SSR path: no OnAfterRender, no circuit.
        await using HtmlRenderer renderer = new(Services, NullLoggerFactory.Instance);
        string html = await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<Drawer>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(Drawer.Id)] = "nav",
            }))).ToHtmlString());

        html.ShouldContain("data-drawer-static");
    }

    [Fact]
    public void Interactive_render_leaves_the_drawer_to_csharp()
    {
        IRenderedComponent<Drawer> cut = Render<Drawer>(ps => ps.Add(p => p.ToggleContent, "Menu"));

        cut.Find(".drawer").HasAttribute("data-drawer-static").ShouldBeFalse();
    }

    [Fact]
    public void Overlay_label_defaults_to_close_sidebar_and_can_be_localised()
    {
        Render<Drawer>().Find(".drawer-overlay").GetAttribute("aria-label").ShouldBe("close sidebar");
        Render<Drawer>(ps => ps.Add(p => p.CloseLabel, "Fermer le menu"))
            .Find(".drawer-overlay").GetAttribute("aria-label").ShouldBe("Fermer le menu");
    }

    [Fact]
    public void CloseOnNavigation_closes_an_open_drawer_on_a_location_change()
    {
        bool? reported = null;
        IRenderedComponent<Drawer> cut = Render<Drawer>(ps => ps
            .Add(p => p.Open, true)
            .Add(p => p.Variant, DrawerVariant.Temporary)
            .Add(p => p.CloseOnNavigation, true)
            .Add(p => p.OpenChanged, v => reported = v));
        cut.Find(".drawer").HasAttribute("data-drawer-close-on-navigate").ShouldBeTrue();

        Services.GetRequiredService<NavigationManager>().NavigateTo("/songs");

        cut.WaitForAssertion(() => reported.ShouldBe(false));
        cut.Find(".drawer-side").HasAttribute("inert").ShouldBeTrue();
    }
}
