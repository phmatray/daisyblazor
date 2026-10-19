namespace DaisyBlazor.Charts;

/// <summary>How a <see cref="PolarChart"/> draws its series.</summary>
public enum PolarChartMode
{
    /// <summary>A line through the points, in order (a spline when <c>Smooth</c>).</summary>
    Line,

    /// <summary>A closed shape through the points, filled with a tint of the series colour.</summary>
    Area,

    /// <summary>Markers only.</summary>
    Marker,
}

/// <summary>A point of a <see cref="PolarChart"/>.</summary>
/// <param name="Angle">Position around the circle, from 0 to the chart's <c>AngleMax</c> for a full turn (larger values wrap).</param>
/// <param name="Radius">Distance from the centre, on the chart's radius scale.</param>
/// <param name="Label">Optional name shown in the tooltip.</param>
public sealed record PolarPoint(double Angle, double Radius, string? Label = null);

/// <summary>A named set of points on a <see cref="PolarChart"/>.</summary>
/// <param name="Name">Name shown in the legend and the tooltips.</param>
/// <param name="Points">The points, in drawing order.</param>
/// <param name="Color">Optional CSS colour; falls back to the theme palette.</param>
public sealed record PolarSeries(string Name, IReadOnlyList<PolarPoint> Points, string? Color = null);

/// <summary>Geometry of a <see cref="PolarChart"/>: centre, radius, grid rings, angle spokes and the placed series.</summary>
public sealed record PolarLayout(double Cx, double Cy, double Radius, IReadOnlyList<PolarRing> Rings, IReadOnlyList<PolarSpoke> Spokes, IReadOnlyList<PolarLayoutSeries> Series);

/// <summary>A grid circle at radius tick <paramref name="Value"/>: its radius <paramref name="R"/> and where its label goes.</summary>
public sealed record PolarRing(double Value, string Label, double R, double X, double Y);

/// <summary>An angle gridline at <paramref name="Value"/>: its outer end and where its label is written.</summary>
public sealed record PolarSpoke(double Value, string Label, double X, double Y, double LabelX, double LabelY, string Anchor);

/// <summary>A series' SVG path (empty for markers only) and its placed points.</summary>
public sealed record PolarLayoutSeries(string Path, IReadOnlyList<PolarLayoutPoint> Points);

/// <summary>A placed point: <paramref name="I"/> is its index in the series' points.</summary>
public sealed record PolarLayoutPoint(int I, double X, double Y);
