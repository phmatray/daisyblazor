namespace DaisyBlazor.Charts;

/// <summary>One row of a <see cref="BulletChart"/>: a measured <paramref name="Value"/> against an optional <paramref name="Target"/>.</summary>
/// <param name="Label">Name of the measure.</param>
/// <param name="Value">The measured value, drawn as the thin bar.</param>
/// <param name="Target">The goal, drawn as a short line across the bar.</param>
/// <param name="Ranges">Ascending thresholds of the qualitative bands behind the bar (for example poor, fair, good).</param>
public sealed record BulletItem(string Label, double Value, double? Target = null, IReadOnlyList<double>? Ranges = null);

/// <summary>Geometry of a <see cref="BulletChart"/>, computed by D3.</summary>
public sealed record BulletLayout(IReadOnlyList<BulletLayoutRow> Rows);

/// <summary>A placed bullet: its plot, bands, value bar, target line, and its own axes (value ticks along the bullet, its name across it).</summary>
public sealed record BulletLayoutRow(string Label, double Value, double? Target, PlotArea Plot, IReadOnlyList<BulletLayoutRange> Ranges, ChartRect Bar, ChartLine? Marker, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks);

/// <summary>A qualitative band; Level 0 is the lowest range.</summary>
public sealed record BulletLayoutRange(double X, double Y, double Width, double Height, int Level);
