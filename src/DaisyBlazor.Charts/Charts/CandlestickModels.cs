namespace DaisyBlazor.Charts;

/// <summary>One period of a <see cref="CandlestickChart"/> starting at <paramref name="Time"/>: its first (<paramref name="Open"/>), highest, lowest and last (<paramref name="Close"/>) values.</summary>
public sealed record Candle(DateTime Time, double Open, double High, double Low, double Close)
{
    /// <summary>How much happened in the period (requests, jobs, trades), drawn in the volume panel when <see cref="CandlestickChart.ShowVolume"/> is on.</summary>
    public double? Volume { get; init; }
}

/// <summary>How a <see cref="CandlestickChart"/> draws each period.</summary>
public enum CandlestickStyle
{
    /// <summary>A body from open to close with a wick from low to high.</summary>
    Candle,

    /// <summary>A bar from low to high, with a tick to the left at the open and to the right at the close.</summary>
    Ohlc,

    /// <summary>A bar from low to high only.</summary>
    HighLow,

    /// <summary>A line through the closes; the price axis fits the closes. Each period's tooltip still gives its four values.</summary>
    Line,

    /// <summary>The closes as a line over a filled area down to the bottom of the plot; the price axis fits the closes.</summary>
    Area,

    /// <summary>
    /// Heikin-Ashi candles computed from the periods: close = (O + H + L + C) / 4; open = (previous HA open +
    /// previous HA close) / 2, the first one (O + C) / 2; high and low = the highest and lowest of the period's
    /// high or low and the HA open and close. Indicators still use the real values.
    /// </summary>
    HeikinAshi,

    /// <summary>
    /// A box from low to high whose width is proportional to the period's <see cref="Candle.Volume"/>
    /// (the largest takes 90% of the space between periods), centred on its time so the time axis reads as
    /// usual. When any period has no positive volume, every box gets the same width.
    /// </summary>
    EquiVolume,

    /// <summary>A candle whose width is proportional to the period's <see cref="Candle.Volume"/>, as for <see cref="EquiVolume"/>; equal widths when any period has no positive volume.</summary>
    CandleVolume,
}

/// <summary>A technical indicator a <see cref="CandlestickChart"/> computes over the closes.</summary>
public enum IndicatorKind
{
    /// <summary>Simple moving average: the mean of the last Period closes (default 20), drawn over the candles.</summary>
    Sma,

    /// <summary>Exponential moving average with α = 2 / (Period + 1), seeded with the SMA of the first Period closes (default 20), drawn over the candles.</summary>
    Ema,

    /// <summary>Bollinger bands: the SMA of Period closes (default 20) ± StdDev population standard deviations of those closes, drawn over the candles.</summary>
    Bollinger,

    /// <summary>Relative strength index over Period changes (default 14) with Wilder's smoothing, from 0 to 100 in its own panel with lines at 30 and 70.</summary>
    Rsi,

    /// <summary>MACD: EMA(Period, default 12) − EMA(SlowPeriod, 26), its signal line EMA(SignalPeriod, 9) of the MACD and their difference as a histogram, in its own panel.</summary>
    Macd,

    /// <summary>Price channel: the highest high and lowest low of the last Period periods (default 20, the current one included) as a band, with their midpoint as the line, drawn over the candles.</summary>
    PriceChannel,
}

/// <summary>
/// A technical indicator for <see cref="CandlestickChart.Indicators"/>, e.g.
/// <c>new ChartIndicator(IndicatorKind.Sma, Period: 20)</c> or <c>new ChartIndicator(IndicatorKind.Bollinger, 20, StdDev: 2)</c>.
/// </summary>
/// <param name="Kind">Which indicator.</param>
/// <param name="Period">Number of periods; null takes the usual default (20, or 14 for RSI and 12 for the MACD's fast average).</param>
/// <param name="StdDev">Width of Bollinger bands in population standard deviations.</param>
/// <param name="SlowPeriod">Periods of the MACD's slow average.</param>
/// <param name="SignalPeriod">Periods of the MACD's signal line.</param>
/// <param name="Color">CSS colour; null takes the next palette colour other than the rising and falling ones.</param>
/// <param name="Label">Legend name; null names the indicator and its periods, e.g. "SMA 20".</param>
public sealed record ChartIndicator(
    IndicatorKind Kind, int? Period = null, double StdDev = 2, int SlowPeriod = 26, int SignalPeriod = 9,
    string? Color = null, string? Label = null);

/// <summary>Geometry of a <see cref="CandlestickChart"/>, computed by D3.</summary>
public sealed record CandlestickLayout(
    IReadOnlyList<CandlestickLayoutCandle> Candles,
    IReadOnlyList<AxisTick> XTicks,
    IReadOnlyList<AxisTick> YTicks,
    PlotArea Plot)
{
    /// <summary>SVG path of the closes, for <see cref="CandlestickStyle.Line"/> and <see cref="CandlestickStyle.Area"/>; null otherwise.</summary>
    public string? PriceLine { get; init; }

    /// <summary>SVG path of the area under the closes, for <see cref="CandlestickStyle.Area"/>; null or empty otherwise.</summary>
    public string? PriceArea { get; init; }

    /// <summary>The indicators that could be computed, in the order they were asked for.</summary>
    public IReadOnlyList<CandlestickLayoutIndicator> Indicators { get; init; } = [];

    /// <summary>The panels under the price panel (RSI, MACD, then volume), sharing its time axis.</summary>
    public IReadOnlyList<CandlestickLayoutPanel> Panels { get; init; } = [];
}

/// <summary>A placed period: its time label, values, whether it closed at or above its open, and its coordinates.</summary>
public sealed record CandlestickLayoutCandle(
    string Label, double Open, double High, double Low, double Close, bool Up,
    double X, double Width, double YOpen, double YClose, double YHigh, double YLow)
{
    /// <summary>The period's volume, when the chart shows a volume panel.</summary>
    public double? Volume { get; init; }
}

/// <summary>
/// A computed indicator: <paramref name="Index"/> in the requested list, the panel it is drawn in
/// (-1 for the price panel), its main line, the MACD's signal line, the Bollinger band edges and
/// band (empty strings when unused), the MACD histogram, and its main value at each candle (null until its window is full).
/// </summary>
public sealed record CandlestickLayoutIndicator(
    int Index, string Kind, int Panel, string Line, string Signal, string Upper, string Lower, string Band,
    IReadOnlyList<CandlestickLayoutBar> Bars, IReadOnlyList<double?> Values);

/// <summary>A panel under the price panel: its kind ("rsi", "macd" or "volume"), vertical extent, value ticks, guide lines (y positions) and bars.</summary>
public sealed record CandlestickLayoutPanel(
    string Kind, double Top, double Bottom, IReadOnlyList<AxisTick> YTicks, IReadOnlyList<double> Guides, IReadOnlyList<CandlestickLayoutBar> Bars);

/// <summary>A bar of a volume panel or a MACD histogram, rising (<paramref name="Up"/>) or falling.</summary>
public sealed record CandlestickLayoutBar(double X, double Y, double Width, double Height, bool Up, double Value);
