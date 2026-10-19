namespace DaisyBlazor.Charts;

/// <summary>What a <see cref="WaterfallStep"/> does to the running total.</summary>
public enum WaterfallStepKind
{
    /// <summary>Adds <see cref="WaterfallStep.Value"/> (negative to subtract) and draws it as a floating bar.</summary>
    Delta,

    /// <summary>Sets the running total to <see cref="WaterfallStep.Value"/> and draws it as a full bar.</summary>
    Start,

    /// <summary>Draws the running total so far as a full bar; its value is ignored.</summary>
    Subtotal,

    /// <summary>Draws the final running total as a full bar; its value is ignored.</summary>
    End,
}

/// <summary>One bar of a <see cref="WaterfallChart"/>.</summary>
/// <param name="Label">Category label under (or beside) the bar.</param>
/// <param name="Value">The change for a Delta, the opening total for a Start; ignored for Subtotal and End.</param>
/// <param name="Kind">How the step affects the running total.</param>
public sealed record WaterfallStep(string Label, double Value, WaterfallStepKind Kind = WaterfallStepKind.Delta);

/// <summary>Geometry of a <see cref="WaterfallChart"/>, computed with D3 scales.</summary>
public sealed record WaterfallLayout(
    IReadOnlyList<WaterfallBar> Bars,
    IReadOnlyList<WaterfallConnector> Connectors,
    IReadOnlyList<AxisTick> Ticks,
    PlotArea? Plot,
    double Zero);

/// <summary>A placed bar. <paramref name="Kind"/> is "increase", "decrease" or "total"; <paramref name="Total"/> is the running total after it.</summary>
public sealed record WaterfallBar(
    string Label, string Kind, double Value, double Total,
    double X, double Y, double Width, double Height,
    double LabelX, double LabelY, string Anchor);

/// <summary>The rule carrying a running total from one bar to the next.</summary>
public sealed record WaterfallConnector(double X1, double Y1, double X2, double Y2);
