namespace DaisyBlazor.Charts;

/// <summary>An axis tick placed by a layout: its label and anchor point, and the tick or grid line.</summary>
public sealed record CompareTick(string Label, double X, double Y, string Anchor, double X1, double Y1, double X2, double Y2);

/// <summary>A text placed by a layout: content, anchor point, text-anchor, and rotation in degrees around the point.</summary>
public sealed record CompareText(string Text, double X, double Y, string Anchor, double Rotate);

/// <summary>A rectangle placed by a layout, in chart units.</summary>
public sealed record CompareRect(double X, double Y, double Width, double Height);

/// <summary>A line segment placed by a layout, in chart units.</summary>
public sealed record CompareLine(double X1, double Y1, double X2, double Y2);

/// <summary>Spec helpers shared by the compare charts.</summary>
internal static class CompareSpec
{
    // JSON has no NaN or Infinity, so values that are not finite travel as null and the layouts skip them.
    public static double? Finite(double value) => double.IsFinite(value) ? value : null;

    public static double? Finite(double? value) => value is { } v && double.IsFinite(v) ? v : null;
}
