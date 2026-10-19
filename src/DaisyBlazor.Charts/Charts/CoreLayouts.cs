namespace DaisyBlazor.Charts;

// Geometry returned by the line, bar, pie and spark layouts in wwwroot/layouts/. Line, bar and
// spark run in measured mode, so their coordinates are CSS pixels of the chart's box.

/// <summary>A point in a chart's box.</summary>
public sealed record ChartPoint(double X, double Y);

/// <summary>Geometry of a <see cref="LineChart"/>, computed with D3.</summary>
public sealed record LineLayout(double Width, double Height, PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<LineLayoutSeries> Series);

/// <summary>One series of a <see cref="LineLayout"/>: line and area paths, and the points that have a value.</summary>
public sealed record LineLayoutSeries(string Line, string Area, IReadOnlyList<LineLayoutPoint> Points);

/// <summary>A value's point; <paramref name="Isolated"/> when no line reaches it, so it needs a marker.</summary>
public sealed record LineLayoutPoint(int Index, double X, double Y, bool Isolated);

/// <summary>Geometry of a <see cref="BarChart"/>, computed with D3: the value ticks and the category labels sit on the x or the y axis, depending on the direction.</summary>
public sealed record BarLayout(double Width, double Height, PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<BarLayoutBar> Bars);

/// <summary>A bar of category <paramref name="C"/> and series <paramref name="S"/>.</summary>
public sealed record BarLayoutBar(int C, int S, double X, double Y, double Width, double Height);

/// <summary>Geometry of a <see cref="PieChart"/> or <see cref="DonutChart"/>, computed with D3.</summary>
public sealed record PieLayout(double Cx, double Cy, double Radius, double InnerRadius, IReadOnlyList<PieLayoutSlice> Slices, double CenterFontSize);

/// <summary>A slice of data point <paramref name="Index"/>; its path is centred on (0, 0).</summary>
public sealed record PieLayoutSlice(int Index, string Path, double Percent, double StartAngle, double EndAngle);

/// <summary>Geometry of a <see cref="Sparkline"/>, computed with D3.</summary>
public sealed record SparkLayout(double Width, double Height, string Line, string Area, ChartPoint? End);

/// <summary>Text helpers shared by the charts.</summary>
internal static class ChartText
{
    // Charts cannot measure text while they render, so widths use the layouts' average glyph of 0.6 em.
    private const double Em = 0.6;

    /// <summary>A data value for tooltips and screen-reader descriptions: 48200 → "48,200".</summary>
    public static string Value(double value) => value.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>A share in percent for tooltips: 33.333 → "33.3%".</summary>
    public static string Percent(double value) => value.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "%";

    /// <summary>A CSS size of a measured chart box; an empty one falls back so the box can be measured.</summary>
    public static string Size(string? size, string fallback) => string.IsNullOrWhiteSpace(size) ? fallback : size;

    /// <summary>Estimated width of a label at the given font size, as the layouts estimate it.</summary>
    public static double Width(string text, double fontSize = 11) => text.Length * fontSize * Em;

    /// <summary>The text shortened with an ellipsis so it fits <paramref name="room"/>, or null when not even three letters fit.</summary>
    public static string? Fit(string text, double room, double fontSize = 11)
    {
        if (Width(text, fontSize) <= room)
        {
            return text;
        }

        int keep = (int)(room / (fontSize * Em)) - 1;
        return keep >= 3 ? text[..keep] + "…" : null;
    }
}
