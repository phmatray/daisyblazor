namespace DaisyBlazor.Charts;

/// <summary>Geometry of a <see cref="ParetoChart"/>. VitalFew is how many bars (from the left) it takes for the cumulative share to reach the threshold.</summary>
public sealed record ParetoLayout(
    IReadOnlyList<ParetoBar> Bars, string Line, IReadOnlyList<AxisTick> Ticks, IReadOnlyList<AxisTick> ShareTicks,
    PlotArea? Plot, double ThresholdY, int VitalFew);

/// <summary>A placed bar, its share of the total, the cumulative share up to it, and the line point (Cx, Cy).</summary>
public sealed record ParetoBar(
    string Label, double Value, double Share, double Cumulative,
    double X, double Y, double Width, double Height, double Cx, double Cy);
