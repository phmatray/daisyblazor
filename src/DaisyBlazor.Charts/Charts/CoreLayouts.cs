namespace DaisyBlazor.Charts;

// Geometry returned by the line, bar, pie and spark layouts in wwwroot/layouts/. Line, bar and
// spark run in measured mode, so their coordinates are CSS pixels of the chart's box.

/// <summary>The plot area inside a chart's box, in pixels.</summary>
public sealed record PlotBox(double Left, double Top, double Right, double Bottom);

/// <summary>An axis tick or category label at <paramref name="Pos"/>; <paramref name="Full"/> is the uncut text when <paramref name="Label"/> was shortened.</summary>
public sealed record AxisLabel(double Pos, string Label, string? Full = null);

/// <summary>A point in a chart's box.</summary>
public sealed record ChartPoint(double X, double Y);

/// <summary>Geometry of a <see cref="LineChart"/>, computed with D3.</summary>
public sealed record LineLayout(double Width, double Height, PlotBox Plot, IReadOnlyList<AxisLabel> YTicks, IReadOnlyList<AxisLabel> XLabels, double LabelY, IReadOnlyList<LineLayoutSeries> Series);

/// <summary>One series of a <see cref="LineLayout"/>: line and area paths, and the points that have a value.</summary>
public sealed record LineLayoutSeries(string Line, string Area, IReadOnlyList<LineLayoutPoint> Points);

/// <summary>A value's point; <paramref name="Isolated"/> when no line reaches it, so it needs a marker.</summary>
public sealed record LineLayoutPoint(int Index, double X, double Y, bool Isolated);

/// <summary>Geometry of a <see cref="BarChart"/>, computed with D3.</summary>
public sealed record BarLayout(double Width, double Height, PlotBox Plot, IReadOnlyList<AxisLabel> ValueTicks, IReadOnlyList<AxisLabel> CategoryLabels, double LabelAt, double ValueLabelAt, IReadOnlyList<BarLayoutBar> Bars);

/// <summary>A bar of category <paramref name="C"/> and series <paramref name="S"/>.</summary>
public sealed record BarLayoutBar(int C, int S, double X, double Y, double Width, double Height);

/// <summary>Geometry of a <see cref="PieChart"/> or <see cref="DonutChart"/>, computed with D3.</summary>
public sealed record PieLayout(double Cx, double Cy, double Radius, double InnerRadius, IReadOnlyList<PieLayoutSlice> Slices, double CenterFontSize);

/// <summary>A slice of data point <paramref name="Index"/>; its path is centred on (0, 0).</summary>
public sealed record PieLayoutSlice(int Index, string Path, double Percent, double StartAngle, double EndAngle);

/// <summary>Geometry of a <see cref="Sparkline"/>, computed with D3.</summary>
public sealed record SparkLayout(double Width, double Height, string Line, string Area, ChartPoint? End);

/// <summary>Text and spec values shared by the charts.</summary>
internal static class ChartText
{
    /// <summary>A data value for tooltips and screen-reader descriptions: 48200 → "48,200".</summary>
    public static string Value(double value) => value.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>The values for a layout spec: NaN and infinities become null (a gap), which JSON can carry.</summary>
    public static double?[] Spec(IReadOnlyList<double>? values) =>
        values is null ? [] : [.. values.Select(v => double.IsFinite(v) ? v : (double?)null)];

    /// <summary>A CSS size of a measured chart box; an empty one falls back so the box can be measured.</summary>
    public static string Size(string? size, string fallback) => string.IsNullOrWhiteSpace(size) ? fallback : size;
}
