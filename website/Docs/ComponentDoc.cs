using System.Text;

namespace DaisyBlazor.Docs.Docs;

/// <summary>One documented component page (<c>/components/{Slug}</c>).</summary>
/// <param name="Name">Display name, usually the primary type name (e.g. "Button").</param>
/// <param name="Category">The daisyUI category it belongs to.</param>
/// <param name="Summary">One or two sentences: what it is and when to reach for it.</param>
/// <param name="Types">Public types documented in the API section: components (the primary first), then any service interfaces or options classes.</param>
/// <param name="Examples">Live examples, in reading order; each type lives under <c>Examples/</c>.</param>
public sealed record ComponentDoc(
    string Name,
    Category Category,
    string Summary,
    Type[] Types,
    ExampleDoc[] Examples)
{
    /// <summary>URL segment, defaults to the lower-cased name.</summary>
    public string Slug { get; init; } = Name.ToLowerInvariant();

    /// <summary>The underlying daisyUI class (e.g. "btn"), shown on the sleeve.</summary>
    public string? DaisyClass { get; init; }

    /// <summary>The MudBlazor component this replaces (e.g. "MudButton"), when the API is compatible.</summary>
    public string? MudEquivalent { get; init; }

    /// <summary>Optional migration note shown under the MudBlazor track.</summary>
    public string? MigrationNote { get; init; }

    /// <summary>The cover portrait shown in the sleeve; defaults to the first example.</summary>
    public Type? Portrait { get; init; }

    /// <summary>Interactive playground, when the component works standalone.</summary>
    public PlaygroundSpec? Playground { get; init; }

    public string Url => $"components/{Slug}";
}

/// <summary>A live example: a Razor component under <c>Examples/</c> whose source is embedded.</summary>
public sealed record ExampleDoc(string Title, string Description, Type Component)
{
    /// <summary>Anchor id derived from the title.</summary>
    public string Id => Slugify.Of(Title);
}

/// <summary>What the playground exposes for one component.</summary>
/// <param name="Component">The (closed, if generic) component type rendered live.</param>
/// <param name="Parameters">Parameter names to expose as controls; empty exposes every simple parameter.</param>
public sealed record PlaygroundSpec(Type Component, params string[] Parameters)
{
    /// <summary>Starting values that differ from the component's own defaults.</summary>
    public IReadOnlyDictionary<string, object?> Initial { get; init; } = new Dictionary<string, object?>();

    /// <summary>Text rendered as ChildContent (and editable), or null when the component takes none.</summary>
    public string? ChildText { get; init; }
}

public enum Category
{
    Actions,
    DataDisplay,
    Navigation,
    Feedback,
    DataInput,
    Layout,
    Mockup,
    Data,
    Charts,
}

public static class CategoryInfo
{
    public static string Title(this Category c) => c switch
    {
        Category.DataDisplay => "Data display",
        Category.DataInput => "Data input",
        Category.Data => "Data components",
        _ => c.ToString(),
    };

    public static string Blurb(this Category c) => c switch
    {
        Category.Actions => "Buttons, menus and controls that trigger something.",
        Category.DataDisplay => "Cards, badges, tables, lists and other ways to show content.",
        Category.Navigation => "Moving around an app: menus, tabs, breadcrumbs, steps.",
        Category.Feedback => "Alerts, progress, dialogs, snackbars and loading states.",
        Category.DataInput => "Typed form fields with validation that bind to your model.",
        Category.Layout => "Grids, stacks, drawers, heroes and page structure.",
        Category.Mockup => "Browser, phone, window and code frames for showing things off.",
        Category.Data => "Composed building blocks for data-heavy screens.",
        Category.Charts => "Dependency-free SVG charts that follow the active theme.",
        _ => "",
    };

    public static string Slug(this Category c) => Slugify.Of(c.Title());
}

public static class Slugify
{
    public static string Of(string text)
    {
        StringBuilder sb = new(text.Length);
        bool dash = false;
        foreach (char ch in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch)) { sb.Append(ch); dash = false; }
            else if (!dash && sb.Length > 0) { sb.Append('-'); dash = true; }
        }
        return sb.ToString().TrimEnd('-');
    }
}
