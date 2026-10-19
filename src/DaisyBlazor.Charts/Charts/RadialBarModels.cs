namespace DaisyBlazor.Charts;

/// <summary>Geometry of a <see cref="RadialBarChart"/>. Ring paths are relative to (Cx, Cy).</summary>
public sealed record RadialBarLayout(double Cx, double Cy, IReadOnlyList<RadialBarRing> Rings);

/// <summary>A placed ring: its track, its filled part (<paramref name="Fraction"/> of the track) and its label position.</summary>
public sealed record RadialBarRing(
    int Index, string Label, double Value, double Fraction, string Track, string Fill,
    double LabelX, double LabelY, double FontSize);
