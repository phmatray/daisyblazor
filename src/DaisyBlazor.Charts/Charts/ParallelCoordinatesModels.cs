namespace DaisyBlazor.Charts;

/// <summary>One line of a <see cref="ParallelCoordinatesChart"/>: a <paramref name="Label"/> with one value per dimension.</summary>
/// <param name="Label">Name of the item, shown in the legend and tooltip.</param>
/// <param name="Values">Values in the order of the chart's dimensions; a NaN breaks the line on that axis.</param>
/// <param name="Color">Optional CSS colour; otherwise the line takes the next theme palette colour.</param>
public sealed record ParallelItem(string Label, IReadOnlyList<double> Values, string? Color = null);

/// <summary>Geometry of a <see cref="ParallelCoordinatesChart"/>, computed by D3.</summary>
public sealed record ParallelLayout(IReadOnlyList<ParallelLayoutAxis> Axes, IReadOnlyList<ParallelLayoutLine> Lines);

/// <summary>A vertical axis: its position, name label and its own ticks.</summary>
public sealed record ParallelLayoutAxis(string Name, double X, double Y1, double Y2, CompareText Label, IReadOnlyList<CompareTick> Ticks);

/// <summary>An item's polyline across the axes; Index is its position in the data.</summary>
public sealed record ParallelLayoutLine(string Label, int Index, IReadOnlyList<double?> Values, string Path);
