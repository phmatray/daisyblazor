namespace DaisyBlazor.Charts;

/// <summary>One value of a <see cref="MarimekkoChart"/>: how much of <paramref name="Segment"/> there is in <paramref name="Column"/>.</summary>
public sealed record MarimekkoCell(string Column, string Segment, double Value);

/// <summary>Geometry of a <see cref="MarimekkoChart"/>.</summary>
public sealed record MarimekkoLayout(
    IReadOnlyList<MarimekkoColumn> Columns, IReadOnlyList<MarimekkoLayoutCell> Cells,
    IReadOnlyList<string> Segments, IReadOnlyList<AxisTick> Ticks, PlotArea? Plot);

/// <summary>A placed column: its total, its share of the grand total and its horizontal extent.</summary>
public sealed record MarimekkoColumn(string Label, double Total, double Share, double X0, double X1, double LabelX);

/// <summary>A placed cell: column and segment indexes, value, share of its column and rectangle.</summary>
public sealed record MarimekkoLayoutCell(int Column, int Segment, double Value, double Share, double X, double Y, double Width, double Height);
