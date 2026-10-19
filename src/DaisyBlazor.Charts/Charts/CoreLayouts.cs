namespace DaisyBlazor.Charts;

// Geometry returned by the line, bar, pie and spark layouts in wwwroot/layouts/. Line, bar and
// spark run in measured mode, so their coordinates are CSS pixels of the chart's box.

/// <summary>A point in a chart's box.</summary>
public sealed record ChartPoint(double X, double Y);

/// <summary>Geometry of a <see cref="LineChart"/>, computed with D3; References, Bands, Trends, Markers and MarkerSpans are null when the chart has none.</summary>
public sealed record LineLayout(double Width, double Height, PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<LineLayoutSeries> Series,
    IReadOnlyList<ReferenceLineLayout>? References = null, IReadOnlyList<ReferenceBandLayout>? Bands = null, IReadOnlyList<ScatterTrend>? Trends = null,
    IReadOnlyList<ReferenceLineLayout>? Markers = null, IReadOnlyList<ReferenceBandLayout>? MarkerSpans = null);

/// <summary>One series of a <see cref="LineLayout"/>: line and area paths, and the points that have a value.</summary>
public sealed record LineLayoutSeries(string Line, string Area, IReadOnlyList<LineLayoutPoint> Points);

/// <summary>A value's point; <paramref name="Isolated"/> when no line reaches it, so it needs a marker.</summary>
public sealed record LineLayoutPoint(int Index, double X, double Y, bool Isolated);

/// <summary>Geometry of a <see cref="BarChart"/>, computed with D3: the value ticks and the category labels sit on the x or the y axis, depending on the direction. References, Bands, Trends, Markers and MarkerSpans are null when the chart has none.</summary>
public sealed record BarLayout(double Width, double Height, PlotArea Plot, IReadOnlyList<AxisTick> XTicks, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<BarLayoutBar> Bars,
    IReadOnlyList<ReferenceLineLayout>? References = null, IReadOnlyList<ReferenceBandLayout>? Bands = null, IReadOnlyList<ScatterTrend>? Trends = null,
    IReadOnlyList<ReferenceLineLayout>? Markers = null, IReadOnlyList<ReferenceBandLayout>? MarkerSpans = null);

/// <summary>A bar of category <paramref name="C"/> and series <paramref name="S"/>.</summary>
public sealed record BarLayoutBar(int C, int S, double X, double Y, double Width, double Height);

/// <summary>Geometry of a <see cref="PieChart"/> or <see cref="DonutChart"/>, computed with D3.</summary>
public sealed record PieLayout(double Cx, double Cy, double Radius, double InnerRadius, IReadOnlyList<PieLayoutSlice> Slices, double CenterFontSize);

/// <summary>
/// A slice of data point <paramref name="Index"/>; its path is centred on (0, 0). An exploded slice is
/// moved by (<paramref name="Dx"/>, <paramref name="Dy"/>) along its bisector; the others have 0.
/// With rings, <paramref name="Ring"/> is the slice's ring (0 the outermost) and <paramref name="Index"/>
/// its position in that ring's data.
/// </summary>
public sealed record PieLayoutSlice(int Index, string Path, double Percent, double StartAngle, double EndAngle, double Dx = 0, double Dy = 0, int Ring = 0);

/// <summary>Geometry of a <see cref="Sparkline"/>, computed with D3.</summary>
/// <remarks>
/// <paramref name="Bars"/> is set only for <see cref="SparklineKind.Column"/> and <see cref="SparklineKind.WinLoss"/>,
/// <paramref name="Band"/> (the normal range, its y and height) and <paramref name="Trend"/> (the least-squares line) only when asked for.
/// </remarks>
public sealed record SparkLayout(double Width, double Height, string Line, string Area, ChartPoint? End,
    IReadOnlyList<SparkLayoutBar>? Bars = null, SparkLayoutBand? Band = null, ChartLine? Trend = null);

/// <summary>The normal range of a <see cref="Sparkline"/>: a band from <paramref name="Y"/> down by <paramref name="Height"/>.</summary>
public sealed record SparkLayoutBand(double Y, double Height);

/// <summary>A bar of a column or win/loss <see cref="Sparkline"/>: value <paramref name="I"/> of the data, its rectangle and its value.</summary>
public sealed record SparkLayoutBar(int I, double X, double Y, double Width, double Height, double Value);

/// <summary>How a <see cref="Sparkline"/> draws its values.</summary>
public enum SparklineKind
{
    /// <summary>A line through the values (with an optional area under it).</summary>
    Line,

    /// <summary>One bar per value, growing from zero; negative values hang down.</summary>
    Column,

    /// <summary>Equal-height bars by sign: up for a positive value, down for a negative one, a gap for zero.</summary>
    WinLoss,
}

/// <summary>Text helpers shared by the charts.</summary>
internal static class ChartText
{
    // Charts cannot measure text while they render, so widths use the layouts' estimate (_shared.mjs):
    // 0.6 em a glyph, 0.72 em a capital and 0.95 em the wide M, W, m, w, %, @ and em dash.
    private const double Em = 0.6;

    private static double Glyph(char c) => "MWmw%@—".Contains(c) ? 0.95 : c is >= 'A' and <= 'Z' ? 0.72 : Em;

    /// <summary>A data value for tooltips and screen-reader descriptions: 48200 → "48,200".</summary>
    public static string Value(double value) => value.ToString("#,0.##", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>A formatted number with a leading hyphen as the minus sign (U+2212) the D3 axis ticks print: "-8" → "−8".</summary>
    public static string Minus(string text) => text.StartsWith('-') ? "\u2212" + text[1..] : text;

    /// <summary>A share in percent for tooltips: 33.333 → "33.3%".</summary>
    public static string Percent(double value) => value.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "%";

    /// <summary>A CSS size of a measured chart box; an empty one falls back so the box can be measured.</summary>
    public static string Size(string? size, string fallback) => string.IsNullOrWhiteSpace(size) ? fallback : size;

    /// <summary>Estimated width of a label at the given font size, as the layouts estimate it.</summary>
    public static double Width(string text, double fontSize = 11) => text.Sum(Glyph) * fontSize;

    /// <summary>The text shortened with an ellipsis so it fits <paramref name="room"/>, or null when not even four letters fit (a shorter stub such as "Bil…" names nothing).</summary>
    public static string? Fit(string text, double room, double fontSize = 11)
    {
        if (Width(text, fontSize) <= room)
        {
            return text;
        }

        int keep = 0;
        while (keep < text.Length && Width(text[..(keep + 1)] + "…", fontSize) <= room)
        {
            keep++;
        }

        return keep >= 4 ? text[..keep] + "…" : null;
    }

    /// <summary>A trend kind in words: <see cref="ChartTrendline.MovingAverage"/> → "moving average".</summary>
    public static string TrendName(string kind) => kind == "movingaverage" ? "moving average" : kind;

    /// <summary>The kind a layout spec names: <see cref="ChartTrendline.MovingAverage"/> → "movingaverage".</summary>
    public static string TrendKind(ChartTrendline trendline) => trendline.ToString().ToLowerInvariant();

    /// <summary>
    /// A trend line's tooltip: "Pro linear trend: y = 1.5x + 0.5, R² = 1", or the period of a moving
    /// average; capitalised when there is no series name.
    /// </summary>
    public static string Trend(ScatterTrend t, string series)
    {
        string r2 = t.R2.ToString("0.###", System.Globalization.CultureInfo.CurrentCulture);
        string body = t.Period is { } period
            ? $"{TrendName(t.Kind)} trend: {period}-point moving average"
            : $"{TrendName(t.Kind)} trend: {_equation(t)}, R² = {r2}";
        return series.Length > 0 ? $"{series} {body}" : char.ToUpperInvariant(body[0]) + body[1..];
    }

    private static string _coef(double value) => value.ToString("G4", System.Globalization.CultureInfo.CurrentCulture);

    private static string _plus(double value) => (value < 0 ? " − " : " + ") + _coef(Math.Abs(value));

    private static string _equation(ScatterTrend t) => t.Kind switch
    {
        "exponential" => $"y = {_coef(t.A)}·e^({_coef(t.B)}x)",
        "logarithmic" => $"y = {_coef(t.B)}·ln(x){_plus(t.A)}",
        "power" => $"y = {_coef(t.A)}·x^{_coef(t.B)}",
        "polynomial" when t.Coefficients is { Count: > 0 } c => _polynomial(c),
        _ => $"y = {_coef(t.B)}x{_plus(t.A)}",
    };

    // Highest power first: y = 0.5x^2 − x + 2.
    private static string _polynomial(IReadOnlyList<double> c)
    {
        static string Power(int k) => k switch { 0 => string.Empty, 1 => "x", _ => $"x^{k}" };
        int top = c.Count - 1;
        string text = $"y = {_coef(c[top])}{Power(top)}";
        for (int k = top - 1; k >= 0; k--)
        {
            text += _plus(c[k]) + Power(k);
        }

        return text;
    }
}
