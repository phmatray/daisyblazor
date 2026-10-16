using DaisyBlazor.Theming;

namespace DaisyBlazor.Docs.Docs;

/// <summary>The themes the site offers: the two DaisyBlazor themes plus the 35 built-in daisyUI themes.</summary>
public static class ThemeDeck
{
    public const string Light = "daisyblazor";
    public const string Dark = "daisyblazor-dark";

    public static IReadOnlyList<string> Themes { get; } = [Light, Dark, .. ThemeProvider.DaisyUiThemes];

    public static IReadOnlyCollection<string> DarkThemes { get; } = [Dark, .. ThemeProvider.DaisyUiDarkThemes];

    /// <summary>1-based position of a theme in the list.</summary>
    public static int Number(string theme) => Math.Max(0, IndexOf(theme)) + 1;

    public static string Next(string theme) => Themes[(IndexOf(theme) + 1) % Themes.Count];

    private static int IndexOf(string theme)
    {
        for (int i = 0; i < Themes.Count; i++)
        {
            if (Themes[i] == theme) return i;
        }
        return -1;
    }

    /// <summary>Switches theme with the circular reveal from the pointer position.</summary>
    public static async Task PutOnAsync(ThemeProvider provider, DocsJs js, string theme, double x, double y)
    {
        if (provider.Theme == theme) return;
        await js.BeginThemeTransition(x, y);
        await provider.SetThemeAsync(theme);
    }
}
