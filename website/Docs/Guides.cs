namespace DaisyBlazor.Docs.Docs;

/// <summary>A guide page in the "Start here" side of the record.</summary>
public sealed record GuideDoc(string Title, string Url, string Summary, params string[] Sections);

public static class Guides
{
    public static IReadOnlyList<GuideDoc> All { get; } =
    [
        new("Getting started", "getting-started",
            "Install the packages, wire the CSS preset, and render your first component.",
            "Install", "Wire the CSS", "Register services", "Add the providers", "Your first component", "Start from the template"),
        new("Theming", "theming",
            "ThemeProvider, the 37 themes, dark mode, persistence and building your own picker.",
            "ThemeProvider", "Choosing themes", "Dark mode", "Build a picker", "Your own theme"),
        new("CSS preset", "css-preset",
            "One import wires daisyUI, the themes and the class safelist; NuGet runs Tailwind for you.",
            "What the preset does", "NuGet-only build", "preset vs preset-core", "Using your own Tailwind"),
        new("Charts", "charts",
            "Line, area, bar, pie, donut and sparkline charts rendered as plain SVG.",
            "Install", "Series and data", "Theme colours", "Accessibility"),
        new("Migrating from MudBlazor", "migration",
            "Move a MudBlazor app over one MudX → X rename at a time.",
            "The approach", "Compatible components", "Enums and icons", "Dialogs and snackbars", "What differs"),
        new("Themes", "themes",
            "Every release in the crate: all 37 themes rendered live, side by side."),
    ];
}
