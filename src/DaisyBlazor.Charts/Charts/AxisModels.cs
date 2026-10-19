namespace DaisyBlazor.Charts;

// Geometry shared by the D3 layouts in wwwroot/layouts/: the plot rectangle, axis ticks and the
// other placed marks several charts return. Coordinates are chart units (CSS pixels for the
// measured charts).

/// <summary>The plotting rectangle of a chart, inside the margins kept for tick labels and axis titles.</summary>
public sealed record PlotArea(double Left, double Top, double Right, double Bottom);

/// <summary>
/// A labelled tick or category on an axis: its position along the axis (an x for the x axis, a y
/// for the y axis) and its text. <paramref name="Value"/> is the number at a value tick, so a chart
/// can format it its own way; <paramref name="Full"/> is the whole name when <paramref name="Label"/>
/// was shortened to fit. An empty label is not drawn.
/// </summary>
public sealed record AxisTick(double Pos, string Label, double? Value = null, string? Full = null)
{
    /// <summary>The tick's text; a missing label (a data point or group without a name) reads as empty.</summary>
    public string Label { get; init; } = Label ?? string.Empty;
}

/// <summary>A text placed by a layout: content, anchor point and text-anchor.</summary>
public sealed record ChartLabel(string Text, double X, double Y, string Anchor);

/// <summary>A rectangle placed by a layout.</summary>
public sealed record ChartRect(double X, double Y, double Width, double Height);

/// <summary>A line segment placed by a layout.</summary>
public sealed record ChartLine(double X1, double Y1, double X2, double Y2);
