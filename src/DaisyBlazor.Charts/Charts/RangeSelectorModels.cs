namespace DaisyBlazor.Charts;

/// <summary>A value at a moment: one point of the overview a <see cref="RangeSelector"/> draws.</summary>
public sealed record RangeSelectorPoint(DateTime Time, double Value);

/// <summary>
/// Geometry of a <see cref="RangeSelector"/>, computed with D3 at the box's pixel size: the plot,
/// time ticks, and the overview as an area and a line. Across the plot, x runs linearly from the
/// first to the last point's time.
/// </summary>
public sealed record RangeSelectorLayout(double Width, double Height, PlotArea Plot, IReadOnlyList<AxisTick> XTicks, string Area, string Line);
