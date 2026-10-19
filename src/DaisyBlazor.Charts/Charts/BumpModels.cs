namespace DaisyBlazor.Charts;

/// <summary>Geometry of a <see cref="BumpChart"/>, computed by D3: one line per series and the period ticks.</summary>
public sealed record BumpLayout(IReadOnlyList<BumpLine> Lines, IReadOnlyList<TimeAxisTick> XTicks, int Ranks, TimePlotArea Plot);

/// <summary>A placed series: its index, name, line path, ranked points, and where its name goes at each end (null when unranked there).</summary>
public sealed record BumpLine(int Index, string Name, string Label, string Path, IReadOnlyList<BumpPoint> Points, BumpAnchor? Start, BumpAnchor? End);

/// <summary>A series' rank in one period, its value there, and its position.</summary>
public sealed record BumpPoint(string Period, int Rank, double? Value, double X, double Y);

/// <summary>A point in chart units.</summary>
public sealed record BumpAnchor(double X, double Y);
