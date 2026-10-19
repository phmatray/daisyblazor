namespace DaisyBlazor.Charts;

/// <summary>The shape of a <see cref="RadarChart"/>'s grid rings.</summary>
public enum RadarGrid
{
    /// <summary>Straight-sided rings joining the spokes.</summary>
    Polygon,

    /// <summary>Round rings.</summary>
    Circle,
}

/// <summary>Geometry of a <see cref="RadarChart"/>, computed by D3: centre, radius, spokes, rings and series shapes.</summary>
public sealed record RadarLayout(double Cx, double Cy, double Radius, IReadOnlyList<RadarLayoutAxis> Axes, IReadOnlyList<RadarLayoutRing> Rings, IReadOnlyList<RadarLayoutSeries> Series);

/// <summary>A spoke: its name, outer end, and where its name is written.</summary>
public sealed record RadarLayoutAxis(string Name, double X, double Y, double LabelX, double LabelY, string Anchor);

/// <summary>A grid ring at a tick value, with its radius and SVG path.</summary>
public sealed record RadarLayoutRing(double Value, string Label, double R, string Path);

/// <summary>A series' closed SVG path and its points, one per spoke.</summary>
public sealed record RadarLayoutSeries(string Path, IReadOnlyList<RadarLayoutPoint> Points);

/// <summary>A series value on a spoke; Value is null when it was missing.</summary>
public sealed record RadarLayoutPoint(double X, double Y, double? Value);
