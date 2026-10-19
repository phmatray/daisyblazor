namespace DaisyBlazor.Charts;

/// <summary>
/// Geometry of a <see cref="Column3DChart"/>: the plot rectangle (the front plane), axis ticks, the
/// floor and back wall polygons, the gridlines along them and the columns in drawing order.
/// </summary>
public sealed record Column3DLayout(
    PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks,
    string Floor, string Wall, IReadOnlyList<Column3DGridLine> Grid, IReadOnlyList<Column3DColumn> Columns);

/// <summary>A gridline at <paramref name="Value"/>, drawn along the floor or side and across the back wall.</summary>
public sealed record Column3DGridLine(double Value, string Path);

/// <summary>
/// A column of category <paramref name="C"/> and series <paramref name="S"/>: its front rectangle and
/// the SVG <c>points</c> of its front, top and side faces.
/// </summary>
public sealed record Column3DColumn(
    int C, int S, double Value, double X, double Y, double Width, double Height,
    string Front, string Top, string Side);
