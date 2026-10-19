namespace DaisyBlazor.Charts;

/// <summary>Which time-independent chart a <see cref="PriceActionChart"/> draws from its prices.</summary>
public enum PriceActionKind
{
    /// <summary>
    /// Bricks of <see cref="PriceActionChart.BoxSize"/> from the first close: a rising brick each time a close
    /// is a whole box above the top of the last brick, a falling one each time it is a whole box below its
    /// bottom, so a trend goes on after one box and reverses after two.
    /// </summary>
    Renko,

    /// <summary>
    /// Lines that go on while closes extend them and turn when a close comes back by the
    /// <see cref="PriceActionChart.Reversal"/> amount. A line turns thick (yang) where it rises above the previous
    /// shoulder and thin (yin) where it falls below the previous waist; the first line is thick when it rises.
    /// </summary>
    Kagi,

    /// <summary>
    /// A new line each time a close passes the last line's top (a rising line) or bottom (a falling one);
    /// a line against the trend must also pass the extreme of the last <see cref="PriceActionChart.Lines"/> lines.
    /// </summary>
    LineBreak,

    /// <summary>
    /// Columns of X (rising) and O (falling) boxes of <see cref="PriceActionChart.BoxSize"/>; a column reverses when a
    /// close is <see cref="PriceActionChart.Reversal"/> boxes (default 3) past its end, starting one box away from it.
    /// </summary>
    PointAndFigure,
}

/// <summary>Geometry of a <see cref="PriceActionChart"/>, computed by D3.</summary>
/// <param name="Kind">The kind drawn: "renko", "kagi", "linebreak" or "pointandfigure".</param>
/// <param name="BoxSize">The box used by Renko and point &amp; figure (the ATR when none was given); 0 for the other kinds.</param>
/// <param name="Reversal">Kagi's reversal amount (or percentage), point &amp; figure's reversal in boxes, the line break's line count; 0 for Renko.</param>
/// <param name="Items">The bricks, lines or columns, in order.</param>
/// <param name="XTicks">Ticks of the item number.</param>
/// <param name="YTicks">Ticks of the price.</param>
/// <param name="Plot">The plotting rectangle.</param>
public sealed record PriceActionLayout(
    string Kind, double BoxSize, double Reversal,
    IReadOnlyList<PriceActionItem> Items,
    IReadOnlyList<AxisTick> XTicks,
    IReadOnlyList<AxisTick> YTicks,
    PlotArea Plot)
{
    /// <summary>Kagi's vertical pieces and the horizontal joins at its turns, thick (yang) or thin (yin).</summary>
    public IReadOnlyList<PriceActionStroke> Strokes { get; init; } = [];
}

/// <summary>
/// A brick, line or column: its number from 1, whether it rises, the prices it runs between, the dates of the
/// prices that started (<paramref name="From"/>) and completed (<paramref name="To"/>) it, and its coordinates.
/// </summary>
public sealed record PriceActionItem(
    int Index, bool Up, double Open, double Close, string From, string To,
    double X, double Width, double YOpen, double YClose)
{
    /// <summary>The centres (y) of a point &amp; figure column's boxes, from the bottom up.</summary>
    public IReadOnlyList<double> Marks { get; init; } = [];
}

/// <summary>A segment of a Kagi chart, thick for yang and thin for yin.</summary>
public sealed record PriceActionStroke(double X1, double Y1, double X2, double Y2, bool Thick);
