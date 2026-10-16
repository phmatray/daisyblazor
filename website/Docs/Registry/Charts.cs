using DaisyBlazor.Charts;
using DaisyBlazor.Docs.Examples.Charts;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> Charts() =>
    [
        new("LineChart", Category.Charts,
            "One or more ChartSeries drawn as lines over a gridded, labelled axis, rendered as inline SVG with no JavaScript. Series colours cycle through the theme palette, so the chart re-skins with every theme.",
            [typeof(LineChart)],
            [
                new("Basic", "Labels name the x positions; each series' Data lines up with them by index. Points are shown by default.", typeof(LineChartBasicExample)),
                new("Several series, smoothed", "Each series takes the next palette colour and appears in the legend; Smooth draws curves.", typeof(LineChartSeriesExample)),
                new("Minimal", "Turn off ShowGrid and ShowLegend, set a Height, and give a series its own Color.", typeof(LineChartMinimalExample)),
            ])
        {
            Portrait = typeof(LineChartCoverExample),
        },

        new("AreaChart", Category.Charts,
            "A line chart with the area under each series filled, for showing volume over time. It wraps LineChart with Area on and takes the same Series and Labels; unlike LineChart, ShowPoints is off by default.",
            [typeof(AreaChart), typeof(LineChart)],
            [
                new("Basic", "One smoothed series with the area filled; points stay off by default for a clean silhouette.", typeof(AreaChartBasicExample)),
                new("Several series", "Fills are translucent so overlapping series stay readable; turn ShowPoints on to mark each value.", typeof(AreaChartSeriesExample)),
            ])
        {
            Portrait = typeof(AreaChartCoverExample),
        },

        new("BarChart", Category.Charts,
            "Vertical or horizontal bars, either flat from Data or grouped from Series (Series wins when both are set). Bars take theme colours unless a point or series sets its own.",
            [typeof(BarChart)],
            [
                new("Flat data", "A list of ChartDataPoint, one bar per label.", typeof(BarChartBasicExample)),
                new("Grouped series", "Labels name the groups; each ChartSeries adds one bar per group and a legend entry.", typeof(BarChartGroupedExample)),
                new("Horizontal", "Horizontal flips the axes so categories read down the side; keep their names short, as the label gutter has a fixed width.", typeof(BarChartHorizontalExample)),
                new("Per-bar colours", "Give a point any CSS colour; ChartPalette.FromColor maps a Color value to its theme token.", typeof(BarChartColorsExample)),
            ])
        {
            Portrait = typeof(BarChartCoverExample),
        },

        new("PieChart", Category.Charts,
            "Slices proportional to each point's value, with a legend that shows percentages. Only positive values count towards the total.",
            [typeof(PieChart)],
            [
                new("Basic", "Slices start at 12 o'clock and run clockwise, coloured from the theme palette.", typeof(PieChartBasicExample)),
                new("Colours and legend", "Set a Color per point and hide the legend with ShowLegend=\"false\".", typeof(PieChartColorsExample)),
            ])
        {
            Portrait = typeof(PieChartCoverExample),
        },

        new("DonutChart", Category.Charts,
            "A pie with a hole, and room for a CenterLabel in the middle. Good for a total and its breakdown at a glance.",
            [typeof(DonutChart)],
            [
                new("With a center label", "CenterLabel prints a headline figure inside the ring.", typeof(DonutChartBasicExample)),
                new("Ring thickness", "InnerRadiusRatio (default 0.6) sets the hole size: higher is a thinner ring, which makes a neat progress gauge.", typeof(DonutChartRingExample)),
            ])
        {
            Portrait = typeof(DonutChartCoverExample),
        },

        new("Sparkline", Category.Charts,
            "A tiny trend line with no axes, sized to sit inline in text, table cells or KPI tiles. Takes a plain list of numbers.",
            [typeof(Sparkline)],
            [
                new("Basic", "Area fills under the line, ShowEndDot marks the latest value, StrokeWidth sets the weight.", typeof(SparklineBasicExample)),
                new("In a table", "Width and Color (any CSS colour) let each row show its own trend.", typeof(SparklineTableExample)),
            ])
        {
            Portrait = typeof(SparklineCoverExample),
        },
    ];
}
