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

/// <summary>
/// A reference line across a chart's plot at <paramref name="Value"/> on the value axis, such as a
/// target, a limit or a break-even point.
/// </summary>
/// <param name="Value">Where the line sits on the value axis; the axis grows to show it.</param>
/// <param name="Label">Optional text written at the end of the line, outside the plot.</param>
/// <param name="Color">Optional CSS colour of the line and its label; defaults to the base content colour.</param>
/// <param name="Dashed">Draw the line dashed (the default) or solid.</param>
public sealed record ChartReference(double Value, string? Label = null, string? Color = null, bool Dashed = true);

/// <summary>
/// A shaded range of the value axis behind a chart's data (a plot band), from <paramref name="From"/>
/// to <paramref name="To"/> in either order. Parts outside the axis are cut off.
/// </summary>
/// <param name="From">One end of the range.</param>
/// <param name="To">The other end of the range.</param>
/// <param name="Label">Optional text written beside the band, outside the plot.</param>
/// <param name="Color">Optional CSS colour of the shading; defaults to the base content colour.</param>
public sealed record ChartBand(double From, double To, string? Label = null, string? Color = null);

/// <summary>
/// A marker on a chart's category axis: a line at <paramref name="Category"/> (the month a pricing
/// change shipped, a release), or with <paramref name="To"/> a shaded span over the categories from
/// <paramref name="Category"/> to <paramref name="To"/> in either order. A marker naming a category
/// the chart does not have is ignored.
/// </summary>
/// <param name="Category">The category (label) the line sits at, or where the span starts.</param>
/// <param name="Label">Optional text written at the end of the line or beside the span, outside the plot.</param>
/// <param name="Color">Optional CSS colour of the mark and its label; defaults to the base content colour.</param>
/// <param name="To">The category where a span ends; null draws a line.</param>
public sealed record ChartMarker(string Category, string? Label = null, string? Color = null, string? To = null);

/// <summary>A placed <see cref="ChartReference"/> (<paramref name="Index"/> is its position in the input): the line, its label's anchor point and its label as it fits beside the plot.</summary>
public sealed record ReferenceLineLayout(int Index, double X1, double Y1, double X2, double Y2, double LabelX, double LabelY, string Anchor, string? Label = null);

/// <summary>A placed <see cref="ChartBand"/> (<paramref name="Index"/> is its position in the input): its rectangle within the plot, its label's anchor point and its label as it fits beside the plot.</summary>
public sealed record ReferenceBandLayout(int Index, double X, double Y, double Width, double Height, double LabelX, double LabelY, string Anchor, string? Label = null);
