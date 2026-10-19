using DaisyBlazor.Charts;
using DaisyBlazor.Docs.Examples.Charts;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> ChartsDistribution() =>
    [
        new("ScatterChart", Category.Charts,
            "One dot per observation on two value axes, to show how two measures move together: seats against API calls, payload against response time. Points can carry a Size (bubbles) and a Series (colour and legend), and either axis can be logarithmic. D3 builds the scales and a Voronoi diagram, so hovering anywhere highlights the nearest point.",
            [typeof(ScatterChart), typeof(ScatterPoint), typeof(AxisScale), typeof(ChartTrendline)],
            [
                new("Basic", "Each ScatterPoint has an X and a Y; Series groups points by colour and adds a legend. XTitle and YTitle name the axes and the tooltips.", typeof(ScatterChartBasicExample)),
                new("Bubbles on log axes", "Give points a Size to draw bubbles whose area follows it, and a Label for the tooltip. XScale and YScale set to AxisScale.Log spread values over several orders of magnitude.", typeof(ScatterChartBubbleExample)),
                new("Connected points", "ShowLine=\"true\" joins each series' points in X order, for measurements taken at increasing steps such as a load test.", typeof(ScatterChartLineExample)),
                new("Trend lines", "Trendline fits a least-squares curve to each series (Linear, Exponential, Logarithmic or Power) in data space, so it also works on log axes. Hover the dashed line for its equation and R².", typeof(ScatterChartTrendlineExample)),
            ])
        {
            Portrait = typeof(ScatterChartCoverExample),
        },

        new("ContourChart", Category.Charts,
            "Where a cloud of points is densest, drawn as nested bands like a contour map: the darker the band, the more points there. Useful when a scatter plot has too many dots to read. D3 (d3-contour) estimates the density; the chart fills the bands in one theme colour.",
            [typeof(ContourChart), typeof(ScatterPoint)],
            [
                new("Basic", "Pass ScatterPoint values; X and Y are used and the rest ignored. Two clusters show up as two peaks.", typeof(ContourChartBasicExample)),
                new("Bandwidth and points", "Bandwidth sets how far each point's influence spreads (in chart units), Levels how many bands to draw, and ShowPoints overlays the observations.", typeof(ContourChartPointsExample)),
            ])
        {
            Portrait = typeof(ContourChartCoverExample),
        },

        new("HexbinChart", Category.Charts,
            "Thousands of points counted into hexagons and shaded by count, so dense data stays readable where dots would pile up. The binning runs in the chart's layout on D3 scales; hexagons take one theme colour, more opaque where fuller.",
            [typeof(HexbinChart), typeof(ScatterPoint)],
            [
                new("Basic", "Each hexagon's tooltip gives its count, formatted with CountFormat; the legend shows the lightest and darkest counts.", typeof(HexbinChartBasicExample)),
                new("Hexagon size", "Radius sets the hexagon size in chart units: bigger hexagons give a coarser, smoother picture. Color takes any CSS colour.", typeof(HexbinChartRadiusExample)),
            ])
        {
            Portrait = typeof(HexbinChartCoverExample),
        },

        new("BoxPlotChart", Category.Charts,
            "Compares how values spread across groups: each box runs from the first to the third quartile with the median across it, whiskers reach the furthest values within 1.5 IQR, and dots mark the outliers. Pass the raw values; D3 computes the statistics and the scales.",
            [typeof(BoxPlotChart), typeof(ValueGroup)],
            [
                new("Basic", "Each ValueGroup holds a name and its raw values. Hover a box for its median, quartiles, whiskers and count.", typeof(BoxPlotChartBasicExample)),
                new("Horizontal", "Horizontal puts the groups down the side, which suits long names. A group's Color overrides the palette.", typeof(BoxPlotChartHorizontalExample)),
            ])
        {
            Portrait = typeof(BoxPlotChartCoverExample),
        },

        new("ViolinChart", Category.Charts,
            "The shape of each group's values as a mirrored density curve: wide where values are common, narrow where rare, so two humps or a long tail stand out where a box plot would hide them. The layout estimates the densities and D3 draws the outlines.",
            [typeof(ViolinChart), typeof(ValueGroup)],
            [
                new("Basic", "Pass raw values per group. The bar inside each violin spans the quartiles and the dot marks the median.", typeof(ViolinChartBasicExample)),
                new("Horizontal, fixed bandwidth", "Bandwidth (in value units) sets the smoothing for every group instead of picking one each; ShowQuartiles=\"false\" leaves only the outline.", typeof(ViolinChartHorizontalExample)),
            ])
        {
            Portrait = typeof(ViolinChartCoverExample),
        },

        new("HistogramChart", Category.Charts,
            "Raw values counted into equal-width bins, one bar per bin, to show the shape of a single measure such as response times or deployment durations. D3 (d3.bin) picks round bin edges.",
            [typeof(HistogramChart)],
            [
                new("Basic", "Values takes the raw numbers; without Bins the count of bins follows Sturges' rule. Hover a bar for its range and count.", typeof(HistogramChartBasicExample)),
                new("Bins and median", "Bins asks for about that many bins (D3 rounds to tidy edges) and ShowMedian marks the median with a dashed line.", typeof(HistogramChartMedianExample)),
            ])
        {
            Portrait = typeof(HistogramChartCoverExample),
        },

        new("BeeswarmChart", Category.Charts,
            "Every value as its own dot along one axis, nudged aside just enough not to overlap, one row per group. It keeps individual customers or runs visible while still showing the overall spread. The layout places the dots on a D3 scale, the same way every time.",
            [typeof(BeeswarmChart), typeof(ValueGroup)],
            [
                new("Basic", "Each ValueGroup becomes a row; hover a dot for its value.", typeof(BeeswarmChartBasicExample)),
                new("One row", "A single group with an empty name draws one unlabelled row. Radius sets the dot size; a crowded row squeezes its dots rather than spill over.", typeof(BeeswarmChartSingleExample)),
            ])
        {
            Portrait = typeof(BeeswarmChartCoverExample),
        },

        new("RidgelineChart", Category.Charts,
            "One density curve per group, stacked in rows on a shared axis, so the shapes are easy to compare: sign-ins by hour for each weekday, response times by region. The layout estimates the densities and D3 draws the curves.",
            [typeof(RidgelineChart), typeof(ValueGroup)],
            [
                new("Basic", "Groups are drawn top to bottom in order. By default the tallest peak rises 1.6 rows, so curves overlap the row above.", typeof(RidgelineChartBasicExample)),
                new("Overlap and bandwidth", "Overlap=\"1\" keeps each curve within its own row; Bandwidth (in value units) applies the same smoothing to every group.", typeof(RidgelineChartOverlapExample)),
            ])
        {
            Portrait = typeof(RidgelineChartCoverExample),
        },
    ];
}
