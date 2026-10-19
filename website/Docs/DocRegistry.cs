namespace DaisyBlazor.Docs.Docs;

/// <summary>Every documented component. Each category lives in its own file under <c>Registry/</c>.</summary>
public static partial class DocRegistry
{
    /// <summary>The chart components by family, in the order the docs show them. Declared before <see cref="All"/>, which reads it.</summary>
    public static IReadOnlyList<ChartFamily> ChartFamilies { get; } =
    [
        new("Essentials", "Line, area, bar, pie, donut, sparkline and Sankey: where most dashboards start.", [.. Charts().Select(WithPlayground)]),
        new("Hierarchy and networks", "Trees, nested parts and the links between named things.", [.. ChartsHierarchy().Select(WithPlayground)]),
        new("Distribution", "How values spread: scatter, density, box plots, histograms.", [.. ChartsDistribution().Select(WithPlayground)]),
        new("Over time", "Calendars, ranges, streams, prices, schedules and rankings over time.", [.. ChartsTime().Select(WithPlayground)]),
        new("Compare", "Series side by side: radar, bullet, dumbbell, slope, lollipop and combos.", [.. ChartsCompare().Select(WithPlayground)]),
        new("Composition and progress", "Totals, shares and targets: waterfalls, funnels, gauges, waffles.", [.. ChartsProgress().Select(WithPlayground)]),
        new("Specialised", "Polar, Smith, 3D columns, shapes and choropleth maps.", [.. ChartsSpecial().Select(WithPlayground)]),
    ];

    public static IReadOnlyList<ComponentDoc> All { get; } =
    [
        .. Actions(), .. DataDisplay(), .. Navigation(), .. Feedback(), .. DataInput(),
        .. Layout(), .. Mockup(), .. Data(), .. ChartFamilies.SelectMany(f => f.Docs),
    ];

    /// <summary>The charts, counted apart from the UI components on the landing pages.</summary>
    public static int ChartCount => All.Count(d => d.Category == Category.Charts);

    public static int ComponentCount => All.Count - ChartCount;

    /// <summary>Finds a page by slug; hyphens are ignored, so "bar-chart" finds "barchart".</summary>
    public static ComponentDoc? Find(string slug) =>
        All.FirstOrDefault(d => string.Equals(d.Slug, slug.Replace("-", ""), StringComparison.OrdinalIgnoreCase));

    public static ChartFamily? FamilyOf(ComponentDoc doc) => ChartFamilies.FirstOrDefault(f => f.Docs.Contains(doc));

    public static IEnumerable<IGrouping<Category, ComponentDoc>> ByCategory() =>
        All.GroupBy(d => d.Category).OrderBy(g => g.Key);

    private static partial IEnumerable<ComponentDoc> Actions();
    private static partial IEnumerable<ComponentDoc> DataDisplay();
    private static partial IEnumerable<ComponentDoc> Navigation();
    private static partial IEnumerable<ComponentDoc> Feedback();
    private static partial IEnumerable<ComponentDoc> DataInput();
    private static partial IEnumerable<ComponentDoc> Layout();
    private static partial IEnumerable<ComponentDoc> Mockup();
    private static partial IEnumerable<ComponentDoc> Data();
    private static partial IEnumerable<ComponentDoc> Charts();
    private static partial IEnumerable<ComponentDoc> ChartsHierarchy();
    private static partial IEnumerable<ComponentDoc> ChartsDistribution();
    private static partial IEnumerable<ComponentDoc> ChartsTime();
    private static partial IEnumerable<ComponentDoc> ChartsCompare();
    private static partial IEnumerable<ComponentDoc> ChartsProgress();
    private static partial IEnumerable<ComponentDoc> ChartsSpecial();
}

/// <summary>A family of chart components (one registry file), shown as a group in the docs.</summary>
public sealed record ChartFamily(string Title, string Blurb, IReadOnlyList<ComponentDoc> Docs)
{
    public string Slug => Slugify.Of(Title);
}
