using Bunit;
using DaisyBlazor.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Guards ThemeProvider: JS goes through the theme.js module (no eval), the cascade is fixed (theme
/// changes do not re-render the wrapped tree), OS light/dark changes are followed in "system" mode,
/// and a missing JS runtime (prerender / disconnected circuit) is tolerated.
/// </summary>
public class ThemeProviderTests : BunitContext
{
    private const string ModulePath = "./_content/DaisyBlazor.Components/theme.js";

    private readonly BunitJSModuleInterop _module;

    public ThemeProviderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _module = JSInterop.SetupModule(ModulePath);
    }

    private IRenderedComponent<ThemeProvider> RenderProvider(string? stored = null, bool systemDark = false)
    {
        _module.Setup<string?>("get", _ => true).SetResult(stored);
        _module.Setup<bool>("prefersDark", _ => true).SetResult(systemDark);
        return Render<ThemeProvider>(ps => ps.AddChildContent<Probe>());
    }

    [Fact]
    public void Loads_stored_preference_without_eval()
    {
        IRenderedComponent<ThemeProvider> cut = RenderProvider(stored: "dracula");

        cut.Instance.Preference.ShouldBe("dracula");
        cut.Instance.IsDarkMode.ShouldBeTrue();
        JSInterop.Invocations.ShouldNotContain(i => i.Identifier == "eval");
        _module.Invocations.ShouldContain(i => i.Identifier == "subscribe");
    }

    [Fact]
    public async Task Theme_change_does_not_rerender_cascade_consumers_but_raises_ThemeChanged()
    {
        IRenderedComponent<ThemeProvider> cut = RenderProvider();
        IRenderedComponent<Probe> probe = cut.FindComponent<Probe>();
        int renders = probe.RenderCount;
        int changes = 0;
        cut.Instance.ThemeChanged += () => changes++;

        await cut.InvokeAsync(() => cut.Instance.SetThemeAsync("synthwave"));

        cut.Instance.Theme.ShouldBe("synthwave");
        changes.ShouldBe(1);
        probe.RenderCount.ShouldBe(renders);
        _module.Invocations.ShouldContain(i => i.Identifier == "set" && Equals(i.Arguments[4], "synthwave"));
    }

    [Fact]
    public async Task Follows_os_changes_in_system_mode()
    {
        IRenderedComponent<ThemeProvider> cut = RenderProvider();
        cut.Instance.Theme.ShouldBe("light");
        string? raised = null;
        cut.Instance.ThemeChanged += () => raised = cut.Instance.Theme;

        await cut.InvokeAsync(() => cut.Instance.OnSystemThemeChanged(true));

        cut.Instance.Theme.ShouldBe("dark");
        raised.ShouldBe("dark");
    }

    [Fact]
    public async Task Ignores_os_changes_for_an_explicit_choice_but_remembers_them_for_system()
    {
        IRenderedComponent<ThemeProvider> cut = RenderProvider(stored: "cupcake");

        await cut.InvokeAsync(() => cut.Instance.OnSystemThemeChanged(true));
        cut.Instance.Theme.ShouldBe("cupcake");

        await cut.InvokeAsync(() => cut.Instance.SetThemeAsync(ThemeProvider.SystemPreference));
        cut.Instance.Theme.ShouldBe("dark");
    }

    [Fact]
    public async Task Tolerates_a_disconnected_js_runtime()
    {
        IRenderedComponent<ThemeProvider> cut = RenderProvider();
        _module.SetupVoid("set", _ => true).SetException(new JSDisconnectedException("gone"));

        await cut.InvokeAsync(() => cut.Instance.SetThemeAsync("dark"));

        cut.Instance.Theme.ShouldBe("dark");
    }

    private sealed class Probe : ComponentBase
    {
        [CascadingParameter] public ThemeProvider? Theme { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder) =>
            builder.AddContent(0, Theme?.Theme);
    }
}
