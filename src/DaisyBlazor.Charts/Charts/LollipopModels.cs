namespace DaisyBlazor.Charts;

/// <summary>The order of a <see cref="LollipopChart"/>'s categories.</summary>
public enum LollipopSort
{
    /// <summary>Largest value first.</summary>
    Descending,

    /// <summary>Smallest value first.</summary>
    Ascending,

    /// <summary>The order of the data.</summary>
    None,
}

/// <summary>Geometry of a <see cref="LollipopChart"/>, computed by D3.</summary>
public sealed record LollipopLayout(CompareRect Plot, IReadOnlyList<CompareTick> Ticks, IReadOnlyList<CompareText> Categories, IReadOnlyList<LollipopLayoutItem> Items);

/// <summary>A placed lollipop: the stem from zero (X1, Y1) to the dot (X2, Y2), the item's index in the data, and its value text.</summary>
public sealed record LollipopLayoutItem(string Label, double Value, int Index, double X1, double Y1, double X2, double Y2, CompareText? Text);
