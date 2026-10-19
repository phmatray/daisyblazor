namespace DaisyBlazor.Charts;

/// <summary>A tick on a chart axis, computed by a D3 layout: its position in chart units and its label.</summary>
public sealed record TimeAxisTick(double Position, string Label);

/// <summary>The plot rectangle of a D3-backed chart, in chart units; axes and labels sit around it.</summary>
public sealed record TimePlotArea(double X0, double Y0, double X1, double Y1);

/// <summary>Helpers for building layout specs.</summary>
internal static class SpecValue
{
    /// <summary>JSON has no NaN or infinity: non-finite values travel as null, which the layouts treat as missing.</summary>
    public static double? Finite(double value) => double.IsFinite(value) ? value : null;
}
