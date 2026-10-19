namespace DaisyBlazor.Charts;

/// <summary>Geometry of a <see cref="PolarAreaChart"/>. Wedge paths and ring radii are relative to (Cx, Cy). Segments holds the stacked series of a wind rose, else null.</summary>
public sealed record PolarAreaLayout(
    double Cx, double Cy, double Radius, double FontSize,
    IReadOnlyList<PolarAreaRing> Rings, IReadOnlyList<PolarAreaWedge> Wedges, IReadOnlyList<PolarAreaSegment>? Segments = null);

/// <summary>Series <paramref name="Series"/>'s part of wedge <paramref name="Index"/> in a wind rose: its value and its path, relative to the centre.</summary>
public sealed record PolarAreaSegment(int Index, int Series, double Value, string Path);

/// <summary>A grid circle at a scale tick.</summary>
public sealed record PolarAreaRing(double Value, double Radius);

/// <summary>A placed wedge and its label outside the rim.</summary>
public sealed record PolarAreaWedge(int Index, string Label, double Value, string Path, double LabelX, double LabelY, string Anchor);
