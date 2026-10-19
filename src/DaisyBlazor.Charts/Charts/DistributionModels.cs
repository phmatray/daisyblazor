namespace DaisyBlazor.Charts;

/// <summary>
/// One observation of a <see cref="ScatterChart"/>, <see cref="ContourChart"/> or <see cref="HexbinChart"/>.
/// Points whose X or Y is not a finite number are skipped.
/// </summary>
/// <param name="X">Horizontal value.</param>
/// <param name="Y">Vertical value.</param>
/// <param name="Size">Optional bubble size (ScatterChart only); the bubble's area is proportional to it.</param>
/// <param name="Series">Optional series name (ScatterChart only); each series gets its own colour and legend entry.</param>
/// <param name="Label">Optional name shown first in the point's tooltip (ScatterChart only).</param>
public sealed record ScatterPoint(double X, double Y, double? Size = null, string? Series = null, string? Label = null);

/// <summary>
/// A named set of raw values, summarised by <see cref="BoxPlotChart"/>, <see cref="ViolinChart"/>,
/// <see cref="BeeswarmChart"/> and <see cref="RidgelineChart"/>. Values that are not finite numbers are ignored;
/// a group with none left is not drawn.
/// </summary>
/// <param name="Name">Group label, shown on the axis and in tooltips.</param>
/// <param name="Values">The raw observations.</param>
/// <param name="Color">Optional CSS colour; falls back to the theme palette by position.</param>
public sealed record ValueGroup(string Name, IReadOnlyList<double> Values, string? Color = null);

/// <summary>How a continuous axis maps values to positions.</summary>
public enum AxisScale
{
    /// <summary>Equal steps for equal differences.</summary>
    Linear,

    /// <summary>Equal steps for equal ratios (base 10). Values of zero or below cannot be placed and are skipped.</summary>
    Log,
}

/// <summary>The plotting rectangle of a chart, inside the margins kept for tick labels and axis titles.</summary>
public sealed record PlotArea(double Left, double Top, double Right, double Bottom);

/// <summary>A labelled tick: its position along the axis (chart units) and its text.</summary>
public sealed record AxisTick(double Pos, string Label);

/// <summary>A value and its position along the value axis.</summary>
public sealed record PositionedValue(double Value, double Pos);

/// <summary>Positions along the value axis of a group's summary statistics.</summary>
public sealed record BoxPositions(double Low, double Q1, double Median, double Q3, double High);

/// <summary>Geometry of a <see cref="ScatterChart"/>.</summary>
public sealed record ScatterLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<string> Series, IReadOnlyList<ScatterLayoutPoint> Points);

/// <summary>A placed point: its values, series index, circle and Voronoi cell (an SVG path).</summary>
public sealed record ScatterLayoutPoint(double X, double Y, double? Size, string? Label, int Series, double Cx, double Cy, double R, string Cell);

/// <summary>Geometry of a <see cref="ContourChart"/>; contour paths are relative to the plot's top-left corner.</summary>
public sealed record ContourLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<ContourBand> Contours, IReadOnlyList<ContourLayoutPoint> Points);

/// <summary>One density level: the area where the estimated density is at least <paramref name="Value"/>.</summary>
public sealed record ContourBand(double Value, string Path);

/// <summary>A point drawn over the contours.</summary>
public sealed record ContourLayoutPoint(double X, double Y, double Cx, double Cy);

/// <summary>Geometry of a <see cref="HexbinChart"/>: the hexagon outline around (0, 0) and the occupied bins.</summary>
public sealed record HexbinLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, string Hexagon, int MaxCount, IReadOnlyList<HexbinBin> Bins);

/// <summary>An occupied hexagon: its centre and how many points fell in it.</summary>
public sealed record HexbinBin(double Cx, double Cy, int Count);

/// <summary>Geometry of a <see cref="BoxPlotChart"/>.</summary>
public sealed record BoxPlotLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, bool Horizontal, double Half, IReadOnlyList<BoxPlotLayoutGroup> Groups);

/// <summary>A group's five-number summary, its outliers, and where its box sits across the band axis.</summary>
public sealed record BoxPlotLayoutGroup(string Name, int Index, int Count, double Low, double Q1, double Median, double Q3, double High, double Center, BoxPositions Pos, IReadOnlyList<PositionedValue> Outliers);

/// <summary>Geometry of a <see cref="ViolinChart"/>.</summary>
public sealed record ViolinLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, bool Horizontal, IReadOnlyList<ViolinLayoutGroup> Groups);

/// <summary>A group's violin outline (an SVG path) and the positions of its quartiles.</summary>
public sealed record ViolinLayoutGroup(string Name, int Index, int Count, double Q1, double Median, double Q3, double Center, string Path, BoxPositions Pos);

/// <summary>Geometry of a <see cref="HistogramChart"/>.</summary>
public sealed record HistogramLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<HistogramBar> Bars, PositionedValue? Median);

/// <summary>One bin: values from <paramref name="X0"/> (inclusive) to <paramref name="X1"/>, their count, and its bar.</summary>
public sealed record HistogramBar(double X0, double X1, int Count, double Left, double Right, double Top, double Bottom);

/// <summary>Geometry of a <see cref="BeeswarmChart"/>.</summary>
public sealed record BeeswarmLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, double Radius, IReadOnlyList<BeeswarmLayoutGroup> Groups);

/// <summary>A row of dots and its centre line.</summary>
public sealed record BeeswarmLayoutGroup(string Name, int Index, double Center, IReadOnlyList<BeeswarmDot> Dots);

/// <summary>A placed dot and the value it stands for.</summary>
public sealed record BeeswarmDot(double Value, double Cx, double Cy);

/// <summary>Geometry of a <see cref="RidgelineChart"/>.</summary>
public sealed record RidgelineLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<RidgelineLayoutGroup> Groups);

/// <summary>A group's density curve: the filled area and its top outline (SVG paths) above its baseline.</summary>
public sealed record RidgelineLayoutGroup(string Name, int Index, int Count, double Baseline, string Path, string Line);
