using Bunit;
using DaisyBlazor.Theming;
using Microsoft.JSInterop;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// Issue #71: a stored theme name that is not in <see cref="ThemeProvider.Themes"/> (stale or unknown)
/// resolves like "system" (OS light/dark default), matching theme-init.js's data-themes allow-list.
/// </summary>
public class ThemeAllowListTests : BunitContext
{
    private readonly BunitJSModuleInterop _module;

    public ThemeAllowListTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _module = JSInterop.SetupModule("./_content/DaisyBlazor.Components/theme.js");
    }

    [Theory]
    [InlineData(false, "brand-light")]
    [InlineData(true, "brand-dark")]
    public void Unknown_stored_theme_falls_back_to_the_system_default(bool systemDark, string expected)
    {
        _module.Setup<string?>("get", _ => true).SetResult("retired-theme");
        _module.Setup<bool>("prefersDark", _ => true).SetResult(systemDark);

        IRenderedComponent<ThemeProvider> cut = Render<ThemeProvider>(ps => ps
            .Add(p => p.Themes, ["brand-light", "brand-dark", "cupcake"])
            .Add(p => p.SystemLightTheme, "brand-light")
            .Add(p => p.SystemDarkTheme, "brand-dark"));

        cut.Instance.Theme.ShouldBe(expected);
    }
}
