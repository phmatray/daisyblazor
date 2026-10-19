namespace DaisyBlazor.Charts;

/// <summary>How a <see cref="StreamChart"/> stacks its layers.</summary>
public enum StreamOffset
{
    /// <summary>A streamgraph: layers flow around a moving centre line that keeps changes smooth.</summary>
    Wiggle,

    /// <summary>A stacked area chart on a zero baseline, with a value axis.</summary>
    Stacked,

    /// <summary>Each x position fills the full height, so layers show their share (100%).</summary>
    Percent,
}

/// <summary>Geometry of a <see cref="StreamChart"/>, computed by d3.stack.</summary>
public sealed record StreamLayout(
    IReadOnlyList<StreamLayer> Layers,
    IReadOnlyList<StreamColumn> Columns,
    IReadOnlyList<AxisTick> XTicks,
    IReadOnlyList<AxisTick> YTicks,
    PlotArea Plot);

/// <summary>A placed layer: its series index, area path, and where its name fits (if it does).</summary>
public sealed record StreamLayer(int Index, string Name, string Path, double LabelX, double LabelY, bool ShowLabel);

/// <summary>The hover strip of one x position, with every series' value there.</summary>
public sealed record StreamColumn(string Label, double X0, double X1, IReadOnlyList<double> Values);
