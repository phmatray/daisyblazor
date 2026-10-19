namespace DaisyBlazor.Charts;

/// <summary>
/// A relation between two named nodes, drawn by <see cref="ChordChart"/>, <see cref="NetworkChart"/>
/// and <see cref="ArcDiagramChart"/>. Links to the same node, or without a positive value, are left out.
/// </summary>
/// <param name="Source">Name of the node the link starts from.</param>
/// <param name="Target">Name of the node the link goes to.</param>
/// <param name="Value">Strength of the link (band or stroke width).</param>
public sealed record GraphLink(string Source, string Target, double Value = 1);

/// <summary>An optional node description for <see cref="NetworkChart"/> and <see cref="ArcDiagramChart"/>: its group colours it.</summary>
/// <param name="Name">The node name used by <see cref="GraphLink.Source"/> and <see cref="GraphLink.Target"/>.</param>
/// <param name="Group">Optional group; nodes in the same group share a colour and a legend key.</param>
public sealed record GraphNode(string Name, string? Group = null);

/// <summary>How an <see cref="ArcDiagramChart"/> orders its nodes on the baseline.</summary>
public enum ArcDiagramSort
{
    /// <summary>The order of <c>Nodes</c>, then of first appearance in the links.</summary>
    Input,

    /// <summary>Nodes of the same group side by side, groups in order of first appearance.</summary>
    Group,

    /// <summary>Most connected nodes first.</summary>
    Degree,
}

/// <summary>Geometry of a <see cref="NetworkChart"/> or <see cref="ArcDiagramChart"/>.</summary>
/// <param name="Nodes">The placed nodes.</param>
/// <param name="Links">The placed links, indexing <paramref name="Nodes"/>.</param>
/// <param name="Groups">Distinct node groups; a node's GroupIndex indexes this list.</param>
/// <param name="LabelRoom">Space the layout left for node names, in chart units.</param>
public sealed record GraphLayout(IReadOnlyList<GraphLayoutNode> Nodes, IReadOnlyList<GraphLayoutLink> Links, IReadOnlyList<string> Groups, double LabelRoom);

/// <summary>A placed node: name, group (index -1 when none), number of links, centre and radius.</summary>
public sealed record GraphLayoutNode(string Name, string? Group, int GroupIndex, int Degree, double X, double Y, double R);

/// <summary>A placed link: node indexes, value, stroke width and SVG path.</summary>
public sealed record GraphLayoutLink(int Source, int Target, double Value, double Width, string Path);

/// <summary>Geometry of a <see cref="ChordChart"/>, computed by d3-chord. Paths are relative to (Cx, Cy).</summary>
public sealed record ChordLayout(IReadOnlyList<ChordLayoutGroup> Groups, IReadOnlyList<ChordLayoutRibbon> Ribbons, double Cx, double Cy);

/// <summary>An arc of the ring: the node name, its total, the arc path and where its label goes.</summary>
public sealed record ChordLayoutGroup(string Name, double Value, string Path, double LabelX, double LabelY, string Anchor);

/// <summary>A ribbon between two groups, with the value at each end.</summary>
public sealed record ChordLayoutRibbon(int Source, int Target, double SourceValue, double TargetValue, string Path);
