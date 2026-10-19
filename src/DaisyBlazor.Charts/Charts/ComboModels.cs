namespace DaisyBlazor.Charts;

/// <summary>
/// Geometry of a <see cref="ComboChart"/>: the plot (null when there are no labels), category ticks,
/// the left value ticks, the right value ticks (empty without a secondary axis), the bars and the lines.
/// </summary>
public sealed record ComboLayout(
    PlotArea? Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<AxisTick> Y2Ticks,
    IReadOnlyList<ComboBar> Bars, IReadOnlyList<ComboLine> Lines);

/// <summary>A placed bar (or stacked segment) of bar series <paramref name="Series"/> at category <paramref name="Index"/>.</summary>
public sealed record ComboBar(int Series, int Index, double Value, double X, double Y, double Width, double Height);

/// <summary>A line series: its path and the points it passes through (missing values are left out).</summary>
public sealed record ComboLine(int Series, string Path, IReadOnlyList<ComboPoint> Points);

/// <summary>A point of a line at category <paramref name="Index"/>.</summary>
public sealed record ComboPoint(int Index, double Value, double Cx, double Cy);
