namespace DaisyBlazor.Charts;

/// <summary>Geometry of a <see cref="ParetoChart"/>. VitalFew is how many bars (from the left) it takes for the cumulative share to reach the threshold.</summary>
public sealed record ParetoLayout(
    IReadOnlyList<ParetoBar> Bars, string Line, IReadOnlyList<ParetoTick> Ticks, IReadOnlyList<ParetoTick> ShareTicks,
    ParetoPlot? Plot, double ThresholdY, int VitalFew);

/// <summary>A placed bar, its share of the total, the cumulative share up to it, and the line point (Cx, Cy).</summary>
public sealed record ParetoBar(
    string Label, double Value, double Share, double Cumulative,
    double X, double Y, double Width, double Height, double Cx, double Cy);

/// <summary>An axis tick: its value and its vertical position.</summary>
public sealed record ParetoTick(double Value, double Pos);

/// <summary>The plotting area inside the axes.</summary>
public sealed record ParetoPlot(double Left, double Top, double Right, double Bottom);
