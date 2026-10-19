namespace DaisyBlazor.Docs.Docs;

/// <summary>Every documented component. Each category lives in its own file under <c>Registry/</c>.</summary>
public static partial class DocRegistry
{
    public static IReadOnlyList<ComponentDoc> All { get; } =
    [
        .. Actions(), .. DataDisplay(), .. Navigation(), .. Feedback(), .. DataInput(),
        .. Layout(), .. Mockup(), .. Data(), .. Charts(), .. ChartsHierarchy(), .. ChartsDistribution(), .. ChartsTime(), .. ChartsCompare(), .. ChartsProgress(), .. ChartsSpecial(),
    ];

    public static ComponentDoc? Find(string slug) =>
        All.FirstOrDefault(d => string.Equals(d.Slug, slug, StringComparison.OrdinalIgnoreCase));

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
