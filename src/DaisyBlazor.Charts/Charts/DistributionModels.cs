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
public sealed record ScatterPoint(double X, double Y, double? Size = null, string? Series = null, string? Label = null)
{
    /// <summary>Optional symmetric error on X (ScatterChart only): a horizontal error bar from X − XError to X + XError. Ignored unless above zero.</summary>
    public double? XError { get; init; }

    /// <summary>Optional symmetric error on Y (ScatterChart only): a vertical error bar from Y − YError to Y + YError. Ignored unless above zero.</summary>
    public double? YError { get; init; }

    /// <summary>Optional measured value at (X, Y), interpolated over the plot by a <see cref="ContourChart"/> in <see cref="ContourMode.Value"/> mode.</summary>
    public double? Value { get; init; }
}

/// <summary>What a <see cref="ContourChart"/> draws.</summary>
public enum ContourMode
{
    /// <summary>Where the points crowd together: bands of estimated point density.</summary>
    Density,

    /// <summary>A surface of the points' <see cref="ScatterPoint.Value"/>, interpolated between them: bands of equal value.</summary>
    Value,
}

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

/// <summary>A value and its position along the value axis.</summary>
public sealed record PositionedValue(double Value, double Pos);

/// <summary>Positions along the value axis of a group's summary statistics.</summary>
public sealed record BoxPositions(double Low, double Q1, double Median, double Q3, double High);

/// <summary>
/// Geometry of a <see cref="ScatterChart"/>. Lines and Trends are only present when the chart asks for them.
/// In high-density mode Points is empty and Density holds the counted grid cells, CellSize units square.
/// References and Bands are the placed horizontal reference lines and bands (at Y values), XReferences
/// and XBands the vertical ones (at X values); each pair is present only when the chart has some.
/// </summary>
public sealed record ScatterLayout(
    PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<string> Series, IReadOnlyList<ScatterLayoutPoint> Points,
    IReadOnlyList<ScatterLine>? Lines = null, IReadOnlyList<ScatterTrend>? Trends = null,
    IReadOnlyList<ScatterDensity>? Density = null, double CellSize = 0,
    IReadOnlyList<ReferenceLineLayout>? References = null, IReadOnlyList<ReferenceBandLayout>? Bands = null,
    IReadOnlyList<ReferenceLineLayout>? XReferences = null, IReadOnlyList<ReferenceBandLayout>? XBands = null);

/// <summary>
/// A series' occupied grid cells in high-density mode, as a flat list of (column, row, count) triples;
/// column 0 is at the plot's left and row 0 at its top.
/// </summary>
public sealed record ScatterDensity(int Series, IReadOnlyList<int> Cells);

/// <summary>An error bar along one axis: the error, and the positions of its two ends (clamped to the plot).</summary>
public sealed record ScatterWhisker(double Error, double From, double To);

/// <summary>A series' points joined in X order (an SVG path).</summary>
public sealed record ScatterLine(int Series, string Path);

/// <summary>
/// A series' fitted trend: its kind, coefficients A and B (see <see cref="ChartTrendline"/>), the
/// coefficient of determination R2, and the curve as an SVG path.
/// </summary>
public sealed record ScatterTrend(int Series, string Kind, double A, double B, double R2, string Path);

/// <summary>
/// A least-squares trend line fitted to each series of a <see cref="ScatterChart"/>, in data space.
/// R² is that of the straight-line fit after the transform, as spreadsheets report it.
/// </summary>
public enum ChartTrendline
{
    /// <summary>No trend line.</summary>
    None,

    /// <summary>y = a + b·x.</summary>
    Linear,

    /// <summary>y = a·e^(b·x), fitted to ln y; points with y ≤ 0 are left out of the fit.</summary>
    Exponential,

    /// <summary>y = a + b·ln x; points with x ≤ 0 are left out of the fit.</summary>
    Logarithmic,

    /// <summary>y = a·x^b, fitted to ln y against ln x; points with x or y ≤ 0 are left out of the fit.</summary>
    Power,
}

/// <summary>A placed point: its values, series index, circle and Voronoi cell (an SVG path); when stacked, the stacked Total it is drawn at; with errors, its whiskers.</summary>
public sealed record ScatterLayoutPoint(double X, double Y, double? Size, string? Label, int Series, double Cx, double Cy, double R, string Cell,
    double? Total = null, ScatterWhisker? XWhisker = null, ScatterWhisker? YWhisker = null);

/// <summary>
/// Geometry of a <see cref="ContourChart"/>; contour paths and labels are relative to the plot's top-left corner.
/// In value mode Max is the largest value, and Labels (with contour lines) places each level's value on its line.
/// </summary>
public sealed record ContourLayout(PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<ContourBand> Contours, IReadOnlyList<ContourLayoutPoint> Points,
    double? Max = null, IReadOnlyList<ContourLabel>? Labels = null);

/// <summary>Where to print a contour line's value.</summary>
public sealed record ContourLabel(double Value, double X, double Y);

/// <summary>One level: the area where the estimated density (or, in value mode, the interpolated value) is at least <paramref name="Value"/>.</summary>
public sealed record ContourBand(double Value, string Path);

/// <summary>A point drawn over the contours, with its value in value mode.</summary>
public sealed record ContourLayoutPoint(double X, double Y, double Cx, double Cy, double? Value = null);

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
