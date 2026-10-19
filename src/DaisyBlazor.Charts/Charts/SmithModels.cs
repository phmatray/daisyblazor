namespace DaisyBlazor.Charts;

/// <summary>Which grid a <see cref="SmithChart"/> draws.</summary>
public enum SmithChartMode
{
    /// <summary>Circles of constant resistance and arcs of constant reactance (the Z chart).</summary>
    Impedance,

    /// <summary>The same grid mirrored left to right: circles of constant conductance and arcs of constant susceptance (the Y chart).</summary>
    Admittance,
}

/// <summary>A normalized impedance z = r + jx on a <see cref="SmithChart"/> (divide by the reference impedance, often 50 Ω).</summary>
/// <param name="Resistance">The real part r, 0 or more; a negative or non-finite value is left out.</param>
/// <param name="Reactance">The imaginary part x: positive is inductive, negative capacitive.</param>
/// <param name="Label">Optional name shown in the tooltip, such as the frequency.</param>
public sealed record SmithPoint(double Resistance, double Reactance, string? Label = null);

/// <summary>A named set of points on a <see cref="SmithChart"/>, such as an antenna's impedance over a frequency sweep.</summary>
/// <param name="Name">Name shown in the legend and the tooltips.</param>
/// <param name="Points">The impedances, in order; the connecting line follows this order.</param>
/// <param name="Color">Optional CSS colour; falls back to the theme palette.</param>
public sealed record SmithSeries(string Name, IReadOnlyList<SmithPoint> Points, string? Color = null);

/// <summary>Geometry of a <see cref="SmithChart"/>: the unit circle, the grid, its labels and the placed points.</summary>
public sealed record SmithLayout(
    double Cx, double Cy, double Radius,
    IReadOnlyList<SmithCircle> Circles, IReadOnlyList<SmithArc> Arcs, IReadOnlyList<ChartLabel> Labels,
    IReadOnlyList<SmithLayoutSeries> Series);

/// <summary>A grid circle of constant resistance (or conductance) <paramref name="Value"/>.</summary>
public sealed record SmithCircle(double Value, double Cx, double Cy, double R);

/// <summary>A grid arc of constant reactance (or susceptance) <paramref name="Value"/>, as an SVG path.</summary>
public sealed record SmithArc(double Value, string Path);

/// <summary>The placed points of one series and the line joining them (empty for fewer than two points).</summary>
public sealed record SmithLayoutSeries(IReadOnlyList<SmithLayoutPoint> Points, string Path);

/// <summary>
/// A placed point: <paramref name="I"/> is its index in the series' points, (<paramref name="X"/>, <paramref name="Y"/>)
/// its position, <paramref name="Magnitude"/> and <paramref name="Angle"/> (degrees) its reflection coefficient Γ.
/// </summary>
public sealed record SmithLayoutPoint(int I, double X, double Y, double Magnitude, double Angle);
