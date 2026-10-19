namespace DaisyBlazor.Charts;

/// <summary>
/// One point of a <see cref="RangeAreaChart"/>: a band from <paramref name="Low"/> to
/// <paramref name="High"/>, with an optional <paramref name="Center"/> value drawn as a line.
/// Built with a <paramref name="Label"/> it sits on a category axis; built with a date (the second
/// constructor) it sits on a time axis. The chart uses a time axis when every point has a date.
/// </summary>
public sealed record RangePoint(string Label, double Low, double High, double? Center = null)
{
    /// <summary>Creates a point on a time axis.</summary>
    public RangePoint(DateTime date, double low, double high, double? center = null)
        : this(string.Empty, low, high, center) => Date = date;

    /// <summary>The point's date, for a time axis; null on a category axis.</summary>
    public DateTime? Date { get; init; }
}

/// <summary>Geometry of a <see cref="RangeAreaChart"/>, computed by D3.</summary>
public sealed record RangeAreaLayout(
    string Band,
    string Center,
    IReadOnlyList<RangeAreaLayoutPoint> Points,
    IReadOnlyList<TimeAxisTick> XTicks,
    IReadOnlyList<TimeAxisTick> YTicks,
    TimePlotArea Plot);

/// <summary>A placed point: its label, values, x position and the hover strip from X0 to X1.</summary>
public sealed record RangeAreaLayoutPoint(string Label, double Low, double High, double? Center, double X, double X0, double X1);
