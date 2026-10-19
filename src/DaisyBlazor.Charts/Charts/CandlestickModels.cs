namespace DaisyBlazor.Charts;

/// <summary>One period of a <see cref="CandlestickChart"/> starting at <paramref name="Time"/>: its first (<paramref name="Open"/>), highest, lowest and last (<paramref name="Close"/>) values.</summary>
public sealed record Candle(DateTime Time, double Open, double High, double Low, double Close);

/// <summary>How a <see cref="CandlestickChart"/> draws each period.</summary>
public enum CandlestickStyle
{
    /// <summary>A body from open to close with a wick from low to high.</summary>
    Candle,

    /// <summary>A bar from low to high, with a tick to the left at the open and to the right at the close.</summary>
    Ohlc,

    /// <summary>A bar from low to high only.</summary>
    HighLow,
}

/// <summary>Geometry of a <see cref="CandlestickChart"/>, computed by D3.</summary>
public sealed record CandlestickLayout(
    IReadOnlyList<CandlestickLayoutCandle> Candles,
    IReadOnlyList<AxisTick> XTicks,
    IReadOnlyList<AxisTick> YTicks,
    PlotArea Plot);

/// <summary>A placed period: its time label, values, whether it closed at or above its open, and its coordinates.</summary>
public sealed record CandlestickLayoutCandle(
    string Label, double Open, double High, double Low, double Close, bool Up,
    double X, double Width, double YOpen, double YClose, double YHigh, double YLow);
