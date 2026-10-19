namespace DaisyBlazor.Charts;

/// <summary>Geometry of a <see cref="PolarAreaChart"/>. Wedge paths and ring radii are relative to (Cx, Cy).</summary>
public sealed record PolarAreaLayout(
    double Cx, double Cy, double Radius, double FontSize,
    IReadOnlyList<PolarAreaRing> Rings, IReadOnlyList<PolarAreaWedge> Wedges);

/// <summary>A grid circle at a scale tick.</summary>
public sealed record PolarAreaRing(double Value, double Radius);

/// <summary>A placed wedge and its label outside the rim.</summary>
public sealed record PolarAreaWedge(int Index, string Label, double Value, string Path, double LabelX, double LabelY, string Anchor);
