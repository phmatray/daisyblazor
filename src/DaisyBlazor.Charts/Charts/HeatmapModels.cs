namespace DaisyBlazor.Charts;

/// <summary>One cell of a <see cref="HeatmapChart"/>: the <paramref name="Value"/> at <paramref name="Row"/> and <paramref name="Column"/>.</summary>
public sealed record HeatmapCell(string Row, string Column, double Value);

/// <summary>Geometry of a <see cref="HeatmapChart"/>, computed by D3.</summary>
public sealed record HeatmapLayout(
    IReadOnlyList<HeatmapLabel> Rows,
    IReadOnlyList<HeatmapLabel> Columns,
    IReadOnlyList<HeatmapLayoutCell> Cells,
    double Min,
    double Max,
    TimePlotArea Plot);

/// <summary>A row or column name, its (possibly shortened) label, its centre and whether the label is drawn.</summary>
public sealed record HeatmapLabel(string Name, string Label, double Position, bool Show);

/// <summary>A placed cell: names, value, position on the colour scale (<paramref name="T"/>, 0 to 1) and rectangle.</summary>
public sealed record HeatmapLayoutCell(string Row, string Column, double Value, double T, double X, double Y, double W, double H);
