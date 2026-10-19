namespace DaisyBlazor.Charts;

/// <summary>One cell of a <see cref="HeatmapChart"/>: the <paramref name="Value"/> at <paramref name="Row"/> and <paramref name="Column"/>.</summary>
public sealed record HeatmapCell(string Row, string Column, double Value);

/// <summary>Geometry of a <see cref="HeatmapChart"/>, computed by D3. Row and column ticks carry the name in Full; a column label left out to avoid crowding is empty.</summary>
public sealed record HeatmapLayout(
    IReadOnlyList<AxisTick> Rows,
    IReadOnlyList<AxisTick> Columns,
    IReadOnlyList<HeatmapLayoutCell> Cells,
    double Min,
    double Max,
    PlotArea Plot);

/// <summary>A placed cell: names, value, position on the colour scale (<paramref name="T"/>, 0 to 1) and rectangle.</summary>
public sealed record HeatmapLayoutCell(string Row, string Column, double Value, double T, double X, double Y, double W, double H);
