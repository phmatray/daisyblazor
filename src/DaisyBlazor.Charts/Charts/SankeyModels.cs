namespace DaisyBlazor.Charts;

/// <summary>One flow of a <see cref="SankeyChart"/>: <paramref name="Value"/> units from <paramref name="Source"/> to <paramref name="Target"/>.</summary>
public sealed record SankeyLink(string Source, string Target, double Value);

/// <summary>How a <see cref="SankeyChart"/> places nodes in columns.</summary>
public enum SankeyAlign
{
    /// <summary>Nodes without outgoing links go to the last column.</summary>
    Justify,

    /// <summary>Each node sits as far left as its inputs allow.</summary>
    Left,

    /// <summary>Each node sits as far right as its outputs allow.</summary>
    Right,

    /// <summary>Like Justify, but sources without inputs move next to their first target.</summary>
    Center,
}

/// <summary>Geometry of a <see cref="SankeyChart"/>, computed by d3-sankey.</summary>
public sealed record SankeyLayout(IReadOnlyList<SankeyLayoutNode> Nodes, IReadOnlyList<SankeyLayoutLink> Links);

/// <summary>A placed node: its name, column, total and rectangle.</summary>
public sealed record SankeyLayoutNode(string Name, int Depth, double Value, double X0, double Y0, double X1, double Y1);

/// <summary>A placed band: node indexes, value, stroke width and SVG path.</summary>
public sealed record SankeyLayoutLink(int Source, int Target, double Value, double Width, string Path);
