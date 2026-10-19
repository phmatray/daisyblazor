namespace DaisyBlazor.Charts;

/// <summary>
/// One node of the tree drawn by <see cref="TreemapChart"/>, <see cref="SunburstChart"/>,
/// <see cref="CirclePackingChart"/> and <see cref="IcicleChart"/>. A leaf is sized by its
/// <paramref name="Value"/>; a parent is the sum of its <paramref name="Children"/> (its own Value is ignored).
/// </summary>
/// <param name="Name">Label shown on the node and in tooltips.</param>
/// <param name="Value">Size of a leaf. Zero, negative and non-finite values are left out.</param>
/// <param name="Children">Child nodes, or null for a leaf.</param>
public sealed record HierarchyNode(string Name, double Value = 0, IReadOnlyList<HierarchyNode>? Children = null);

/// <summary>How a <see cref="TreemapChart"/> cuts each parent into tiles.</summary>
public enum TreemapTile
{
    /// <summary>Tiles as close to square as possible (easiest to compare and label).</summary>
    Squarify,

    /// <summary>Splits each parent in two, alternating horizontal and vertical cuts.</summary>
    Binary,

    /// <summary>Stacks the children of every parent top to bottom.</summary>
    Slice,

    /// <summary>Lays the children of every parent out left to right.</summary>
    Dice,

    /// <summary>Alternates Slice and Dice by depth.</summary>
    SliceDice,
}

/// <summary>Which way an <see cref="IcicleChart"/> grows from its root.</summary>
public enum IcicleOrientation
{
    /// <summary>Root on the left, one column per level.</summary>
    Horizontal,

    /// <summary>Root on top, one row per level.</summary>
    Vertical,
}

/// <summary>Geometry of a hierarchy chart, computed by d3-hierarchy.</summary>
/// <param name="Nodes">The placed nodes, parents before their children.</param>
/// <param name="Groups">Names of the root's children; a node's <see cref="HierarchyLayoutNode.Group"/> indexes this list.</param>
/// <param name="Cx">Horizontal centre for radial layouts (their paths are relative to it).</param>
/// <param name="Cy">Vertical centre for radial layouts.</param>
public sealed record HierarchyLayout(IReadOnlyList<HierarchyLayoutNode> Nodes, IReadOnlyList<string> Groups, double Cx, double Cy);

/// <summary>
/// A placed hierarchy node. X0..Y1 is its rectangle (treemap, icicle) or the bounding box of its
/// circle (circle packing); a sunburst node has an arc in <see cref="D"/>. The label sits at
/// LabelX/LabelY, rotated by LabelAngle degrees, and fits when it is at most <see cref="Room"/>
/// long and <see cref="Thickness"/> tall.
/// </summary>
public sealed record HierarchyLayoutNode(
    string Name, string Path, int Depth, int Group, bool Leaf, double Value,
    double X0, double Y0, double X1, double Y1, string? D,
    double LabelX, double LabelY, double LabelAngle, double Room, double Thickness);

/// <summary>Colours and label fitting shared by the hierarchy and graph charts.</summary>
internal static class ChartInk
{
    /// <summary>Rough width of a label at the given font size (average glyph ≈ 0.6 em).</summary>
    public static double TextWidth(string text, double fontSize) => text.Length * fontSize * 0.6;

    /// <summary>The text shortened with an ellipsis so it fits <paramref name="room"/>, or null when not even three letters fit.</summary>
    public static string? Fit(string text, double room, double fontSize)
    {
        if (TextWidth(text, fontSize) <= room)
        {
            return text;
        }

        int keep = (int)(room / (fontSize * 0.6)) - 1;
        return keep >= 3 ? text[..keep] + "…" : null;
    }

    /// <summary>A readable text colour on top of <paramref name="fill"/>: the matching <c>-content</c> token for a theme colour, base content otherwise.</summary>
    public static string ContentOn(string fill) =>
        fill.StartsWith("var(--color-", StringComparison.Ordinal) && !fill.StartsWith("var(--color-base", StringComparison.Ordinal)
            ? fill[..^1] + "-content)"
            : "var(--color-base-content)";

    /// <summary>Text description of a tree for screen readers: each top-level branch and its total.</summary>
    public static string? Describe(HierarchyNode? root, Func<double, string> format) =>
        root?.Children is { Count: > 0 } children
            ? string.Join("; ", children.Select(c => $"{c.Name}: {format(Sum(c))}"))
            : null;

    /// <summary>Total of a tree, counting only positive finite leaf values (as the layouts do).</summary>
    public static double Sum(HierarchyNode node) => node.Children is { Count: > 0 } children
        ? children.Sum(Sum)
        : double.IsFinite(node.Value) && node.Value > 0 ? node.Value : 0;

    /// <summary>Legend keys for a list of group names, coloured as <see cref="GroupColor"/> colours them.</summary>
    public static IReadOnlyList<(string Name, string Color)> Legend(IReadOnlyList<string> groups, IReadOnlyDictionary<string, string>? overrides) =>
        groups.Select((g, i) => (g, GroupColor(i, groups, overrides))).ToList();

    /// <summary>Theme colour for a group index (overridden by name), or neutral for -1.</summary>
    public static string GroupColor(int group, IReadOnlyList<string> groups, IReadOnlyDictionary<string, string>? overrides) =>
        group < 0 ? ChartPalette.Neutral
        : overrides is not null && group < groups.Count && overrides.TryGetValue(groups[group], out string? c) ? c
        : ChartPalette.At(group);
}
