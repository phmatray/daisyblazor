namespace DaisyBlazor.Charts;

/// <summary>A corner of a <see cref="ChartShape"/>, in data coordinates.</summary>
/// <param name="X">Position along the horizontal axis.</param>
/// <param name="Y">Position along the vertical axis (up is larger).</param>
public sealed record ShapePoint(double X, double Y);

/// <summary>A polygon or polyline drawn by a <see cref="ShapeChart"/>.</summary>
/// <param name="Name">Name shown in the tooltip (and the legend when the shape has no <paramref name="Group"/>).</param>
/// <param name="Rings">
/// The shape's outlines. A closed shape fills them together (a ring inside another is a hole, by the
/// even-odd rule); an open one draws each as a separate line. Points without a finite X and Y are skipped.
/// </param>
/// <param name="Closed">True (default) for a filled polygon, false for an open polyline.</param>
/// <param name="Color">Optional CSS colour; else the group's (or the shape's) palette colour.</param>
/// <param name="Group">Optional legend entry: shapes of the same group share its palette colour and legend item.</param>
public sealed record ChartShape(string Name, IReadOnlyList<IReadOnlyList<ShapePoint>> Rings, bool Closed = true, string? Color = null, string? Group = null);

/// <summary>Geometry of a <see cref="ShapeChart"/>: plot area, axis ticks and one SVG path per shape, in input order.</summary>
public sealed record ShapeLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<string> Paths);
