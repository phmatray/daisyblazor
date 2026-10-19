namespace DaisyBlazor.Docs.Docs;

/// <summary>A guide page.</summary>
public sealed record GuideDoc(string Title, string Url, string Summary, params string[] Sections)
{
    /// <summary>The sections as jump links; a guide page renders one h2 per section, so search and page stay in sync.</summary>
    public IReadOnlyList<Track> Tracks { get; } = Sections.Select(Track.Of).ToArray();

    /// <summary>Breadcrumb for the guide page.</summary>
    public IReadOnlyList<(string Label, string? Href)> Trail => [("Guides", "getting-started"), (Title, null)];
}

public static class Guides
{
    public static GuideDoc Get(string url) => All.First(g => g.Url == url);

    public static IReadOnlyList<GuideDoc> All { get; } =
    [
        new("Getting started", "getting-started",
            "Install the packages, turn on the CSS build, register services and render your first component.",
            "Start from the template", "Add the package", "Turn on the CSS build", "Author your stylesheet",
            "Register services", "Wrap your layout", "Use components"),
        new("Theming", "theming",
            "ThemeProvider, the built-in themes, dark mode, persistence, brand themes and a theme picker.",
            "ThemeProvider", "No flash on first paint", "Parameters", "Reading and switching",
            "Built-in themes", "A custom brand theme", "A theme picker"),
        new("CSS preset", "css-preset",
            "What preset.css ships, how the NuGet package builds your CSS without Node, and the class safelist.",
            "What the preset does", "Wiring it into your app", "MSBuild properties", "Only the DaisyBlazor themes",
            "Only your own themes", "Source globs", "The comment-scanning gotcha", "The class safelist",
            "The Material Symbols webfont", "The heading font"),
        new("Charts", "charts",
            "54 theme-aware SVG chart types, laid out by a bundled D3 module, with optional server rendering.",
            "All 54 charts", "Install", "Data models", "Colours follow the theme", "Usage", "Notes and geometry", "Server rendering"),
        new("Migrating from MudBlazor", "migration",
            "Move a MudBlazor app over incrementally, mostly by renaming MudX to X.",
            "The approach", "The shared enums", "Icons", "Dialogs", "Snackbar", "Setup deltas",
            "Component name map", "Beyond MudBlazor", "Migration recipe"),
        new("Themes", "themes",
            $"All {ThemeDeck.Themes.Count} themes rendered live, side by side; switch the whole site with one click."),
    ];
}
