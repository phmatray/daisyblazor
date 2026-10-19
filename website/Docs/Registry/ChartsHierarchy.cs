using DaisyBlazor.Charts;
using DaisyBlazor.Docs.Examples.Charts;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> ChartsHierarchy() =>
    [
        new("TreemapChart", Category.Charts,
            "A tree as nested rectangles, each leaf's area proportional to its value: storage by team and project, requests by region and service. D3 tiles the rectangles; the chart draws them in theme colours, one colour per top-level branch, with each parent's name in a header.",
            [typeof(TreemapChart)],
            [
                new("Basic", "Root is a HierarchyNode: leaves carry a Value, parents add up their Children. Tiles show their name and value when they fit; hover any tile for its full path.", typeof(TreemapChartBasicExample)),
                new("Tiling", "Tile chooses how each parent is cut (here SliceDice, which alternates rows and columns by level); ValueFormat sets the unit.", typeof(TreemapChartTileExample)),
                new("Drill-down", "Drilldown=\"true\" makes every parent a button (click, or Tab then Enter): the chart re-roots on it at full size, colours stay those of its top-level branch, and the breadcrumb above goes back up. @bind-Focus sets or follows the drilled node as a path like \"Platform/api\"; OnNodeClick reports any clicked node. Clicks need an interactive render mode; static rendering draws the Focus node.", typeof(TreemapChartDrilldownExample)),
            ])
        {
            Portrait = typeof(TreemapChartCoverExample),
        },

        new("SunburstChart", Category.Charts,
            "A tree as rings around a centre disc, each arc's angle proportional to its value: support tickets by product area and topic, customers by plan and region. D3 places the arcs; the chart draws them in theme colours, one colour per top-level branch.",
            [typeof(SunburstChart)],
            [
                new("Basic", "Root is a HierarchyNode; the centre disc shows its name and total, and each ring is one level deeper. Arcs too thin for a name keep it in their tooltip.", typeof(SunburstChartBasicExample)),
                new("Deeper trees", "Any depth works: branches may stop at different levels. GroupColors maps a top-level branch to any CSS colour, and ValueFormat sets the unit in tooltips.", typeof(SunburstChartDepthExample)),
                new("Drill-down", "Drilldown=\"true\": click (or Tab then Enter on) an arc with children to make it the centre, at full circle; click the centre or the breadcrumb to go back up. @bind-Focus follows the drilled node. Clicks need an interactive render mode.", typeof(SunburstChartDrilldownExample)),
            ])
        {
            Portrait = typeof(SunburstChartCoverExample),
        },

        new("CirclePackingChart", Category.Charts,
            "A tree as nested circles, each leaf's area proportional to its value: deployments by team and service, projects by plan and region. D3 packs the circles; the chart draws them in theme colours, one colour per top-level branch, and names the leaves that are large enough.",
            [typeof(CirclePackingChart)],
            [
                new("Basic", "Root is a HierarchyNode; each parent circle encloses its children, and the legend names the top-level branches.", typeof(CirclePackingChartBasicExample)),
                new("Padding", "Padding sets the gap between circles; ShowValues=\"false\" keeps only the names, and the values stay in the tooltips.", typeof(CirclePackingChartPaddingExample)),
            ])
        {
            Portrait = typeof(CirclePackingChartCoverExample),
        },

        new("IcicleChart", Category.Charts,
            "A tree as stacked bands, one band per level, each node as long as its value: storage by team and project, requests by region and service. D3 places the bands; the chart draws them in theme colours, one colour per top-level branch.",
            [typeof(IcicleChart)],
            [
                new("Basic", "Root is a HierarchyNode; the root fills the first column and each level splits it further. Bands show their name and value when they fit.", typeof(IcicleChartBasicExample)),
                new("Vertical", "Horizontal=\"false\" puts the root on top with one row per level.", typeof(IcicleChartVerticalExample)),
                new("Drill-down", "Drilldown=\"true\" with Focus=\"Europe\" set up front: the chart starts on that branch. Click a band with children to drill further; the first band and the breadcrumb go back up. Clicks need an interactive render mode.", typeof(IcicleChartDrilldownExample)),
            ])
        {
            Portrait = typeof(IcicleChartCoverExample),
        },

        new("ChordChart", Category.Charts,
            "Relations between named nodes around a circle: each node is an arc as long as its total, each relation a ribbon between two arcs, such as calls between services or customers moving between plans. D3 places the arcs and ribbons; the chart draws them in theme colours.",
            [typeof(ChordChart)],
            [
                new("Basic", "Each GraphLink joins Source and Target with a Value; node names come from the links, and a link counts once on each of its nodes.", typeof(ChordChartBasicExample)),
                new("Directed", "Directed=\"true\" draws each ribbon as an arrow from Source to Target; ShowValues prints each node's total and NodeColors maps a node to any CSS colour.", typeof(ChordChartDirectedExample)),
            ])
        {
            Portrait = typeof(ChordChartCoverExample),
        },

        new("NetworkChart", Category.Charts,
            "A force-directed graph: linked nodes pull together and all nodes push apart, which shows clusters and hubs, such as dependencies between services. D3 runs the simulation to rest before drawing, so the same data always gives the same picture; the chart draws it in theme colours.",
            [typeof(NetworkChart)],
            [
                new("Basic", "Each GraphLink joins two node names; nodes grow with their number of links, and their names sit to the right.", typeof(NetworkChartBasicExample)),
                new("Groups", "Nodes gives each GraphNode a Group, which sets its colour and a legend key; a link's Value sets its thickness.", typeof(NetworkChartGroupsExample)),
            ])
        {
            Portrait = typeof(NetworkChartCoverExample),
        },

        new("ArcDiagramChart", Category.Charts,
            "Nodes in a row on one baseline, each link an arc above it as thick as its value: shared tickets between teams, calls between services. D3 places the nodes and arcs; the chart draws them in theme colours with the names below.",
            [typeof(ArcDiagramChart)],
            [
                new("Basic", "Each GraphLink joins two node names with a Value; nodes appear in order of first mention and grow with their number of links.", typeof(ArcDiagramChartBasicExample)),
                new("Groups and order", "Nodes gives each node a Group (colour and legend); Sort=\"Group\" places each group side by side so links inside a group stay short.", typeof(ArcDiagramChartGroupsExample)),
            ])
        {
            Portrait = typeof(ArcDiagramChartCoverExample),
        },
    ];
}
