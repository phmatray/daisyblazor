using DaisyBlazor.Charts;
using DaisyBlazor.Docs.Examples.Charts;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> Charts() =>
    [
        new("LineChart", Category.Charts,
            "One or more ChartSeries drawn as lines over a gridded, labelled axis, as inline SVG laid out by the bundled D3 module at the chart's real size. Series colours cycle through the theme palette, so the chart re-skins with every theme.",
            [typeof(LineChart)],
            [
                new("Basic", "Labels name the x positions; each series' Data lines up with them by index. Points are shown by default.", typeof(LineChartBasicExample)),
                new("Several series, smoothed", "Each series takes the next palette colour and appears in the legend; Smooth draws curves.", typeof(LineChartSeriesExample)),
                new("Minimal", "Turn off ShowGrid and ShowLegend, set a Height, and give a series its own Color.", typeof(LineChartMinimalExample)),
                new("Step", "Step draws each value as a step that holds until the next one (step-after), for counts that change in jumps; it wins over Smooth.", typeof(LineChartStepExample)),
                new("Rotated", "Horizontal puts the labels down the side and the values along the bottom, each line reading top to bottom; it works with Smooth, Step, Stacked, Percent and Area.", typeof(LineChartRotatedExample)),
                new("Reference lines and bands", "References draws a ChartReference line across the plot at a value (dashed unless Dashed is false) and Bands shades a ChartBand range behind the data; labels sit inside the plot, and the axis grows to show every line.", typeof(LineChartReferencesExample)),
                new("Trend lines", "Trendline fits a dashed trend to each series over the label index (Linear, Exponential, Logarithmic, Power, Polynomial of TrendlineOrder 2 to 6, or a MovingAverage of TrendlinePeriod points) and draws it through the label positions; here a three-day moving average. Hover it for the equation and R², or the period. Ignored when Stacked or Percent.", typeof(LineChartTrendlineExample)),
            ])
        {
            Portrait = typeof(LineChartCoverExample),
        },

        new("AreaChart", Category.Charts,
            "A line chart with the area under each series filled, for showing volume over time. It wraps LineChart with Area on and takes the same Series and Labels; unlike LineChart, ShowPoints is off by default. Like LineChart it is laid out by the bundled D3 module.",
            [typeof(AreaChart), typeof(LineChart)],
            [
                new("Basic", "One smoothed series with the area filled; points stay off by default for a clean silhouette.", typeof(AreaChartBasicExample)),
                new("Several series", "Fills are translucent so overlapping series stay readable; turn ShowPoints on to mark each value.", typeof(AreaChartSeriesExample)),
                new("Stacked", "Stacked draws each series on top of the running total, so the bands add up to the whole and the fills never mix; tooltips still show each series' own value.", typeof(AreaChartStackedExample)),
                new("100% stacked", "Percent stacks the series to 100% at every label, so the fills show each one's share; tooltips show the value and the share.", typeof(AreaChartPercentExample)),
                new("Rotated", "Horizontal turns the chart so the labels run down the side and the areas fill from the left; here also Stacked and Step. References and Bands work as on LineChart.", typeof(AreaChartRotatedExample)),
            ])
        {
            Portrait = typeof(AreaChartCoverExample),
        },

        new("BarChart", Category.Charts,
            "Vertical or horizontal bars, either flat from Data or grouped from Series (Series wins when both are set). Bars take theme colours unless a point or series sets its own. D3 lays out the bands and the value axis.",
            [typeof(BarChart)],
            [
                new("Flat data", "A list of ChartDataPoint, one bar per label.", typeof(BarChartBasicExample)),
                new("Grouped series", "Labels name the groups; each ChartSeries adds one bar per group and a legend entry.", typeof(BarChartGroupedExample)),
                new("Horizontal", "Horizontal flips the axes so categories read down the side; the label gutter grows with the names up to 40% of the width, and longer names are cut with an ellipsis.", typeof(BarChartHorizontalExample)),
                new("Stacked", "Stacked puts the series in one bar per label; positive values stack up from zero and negative ones down, so mixed signs diverge. Tooltips keep each series' own value.", typeof(BarChartStackedExample)),
                new("100% stacked", "Percent stacks each bar to 100% of its total, here horizontal; tooltips show the value and the share.", typeof(BarChartPercentExample)),
                new("Side-by-side stacks", "Give each ChartSeries a Stack name: series with the same name stack together and the stacks stand side by side in each label.", typeof(BarChartStackGroupsExample)),
                new("Per-bar colours", "Give a point any CSS colour; ChartPalette.FromColor maps a Color value to its theme token.", typeof(BarChartColorsExample)),
                new("Reference lines and bands", "References draws a line across the bars at a value (a target, a limit) and Bands shades a value range behind them; on a Horizontal chart both run down the plot.", typeof(BarChartReferencesExample)),
                new("Trend lines", "Trendline fits a dashed trend to each series over the categories and draws it through their centres, also on a Horizontal chart; the same kinds as LineChart, here Linear. Hover it for the equation and R². Ignored when Stacked or Percent.", typeof(BarChartTrendlineExample)),
            ])
        {
            Portrait = typeof(BarChartCoverExample),
        },

        new("PieChart", Category.Charts,
            "Slices proportional to each point's value, with a legend that shows percentages. Only positive values count towards the total. D3 computes the arcs.",
            [typeof(PieChart)],
            [
                new("Basic", "Slices start at 12 o'clock and run clockwise, coloured from the theme palette.", typeof(PieChartBasicExample)),
                new("Colours and legend", "Set a Color per point and hide the legend with ShowLegend=\"false\".", typeof(PieChartColorsExample)),
                new("Others slice", "OthersThreshold (a fraction: 0.05 is 5 %) merges the slices below it into one slice named OthersLabel, drawn last; its tooltip lists what it holds. A single small slice is kept as it is.", typeof(PieChartOthersExample)),
                new("Exploded slice", "Exploded lists the labels of the slices to pull out along their bisector, by ExplodeOffset times the radius (default 0.08); the pie shrinks so they stay in the box.", typeof(PieChartExplodedExample)),
            ])
        {
            Portrait = typeof(PieChartCoverExample),
        },

        new("DonutChart", Category.Charts,
            "A pie with a hole, and room for a CenterLabel in the middle. Good for a total and its breakdown at a glance. It wraps PieChart, so it is laid out by the same D3 module.",
            [typeof(DonutChart)],
            [
                new("With a center label", "CenterLabel prints a headline figure inside the ring.", typeof(DonutChartBasicExample)),
                new("Ring thickness", "InnerRadiusRatio (default 0.6) sets the hole size: higher is a thinner ring, which makes a neat progress gauge.", typeof(DonutChartRingExample)),
                new("Several rings", "Rings takes a list of DonutRing (a name and its data), drawn as concentric rings sharing the hole, outermost first. A label keeps one colour in every ring, the legend lists each label once, and tooltips name the ring.", typeof(DonutChartRingsExample)),
            ])
        {
            Portrait = typeof(DonutChartCoverExample),
        },

        new("Sparkline", Category.Charts,
            "A tiny trend line, column or win/loss chart with no axes, sized to sit inline in text, table cells or KPI tiles. Takes a plain list of numbers. D3 draws it at the box's real size.",
            [typeof(Sparkline)],
            [
                new("Basic", "Area fills under the line, ShowEndDot marks the latest value, StrokeWidth sets the weight.", typeof(SparklineBasicExample)),
                new("In a table", "Width and Color (any CSS colour) let each row show its own trend.", typeof(SparklineTableExample)),
                new("Column and win/loss", "Type=\"SparklineKind.Column\" draws one bar per value from zero, negative ones down in NegativeColor (the error colour by default); SparklineKind.WinLoss draws equal bars up or down by sign, with a gap for zero.", typeof(SparklineKindsExample)),
                new("Trend line and normal range", "ShowTrendline adds the dashed least-squares line across the values; NormalMin and NormalMax shade the expected range behind them. Both work on lines and columns.", typeof(SparklineTrendExample)),
            ])
        {
            Portrait = typeof(SparklineCoverExample),
        },

        new("SankeyChart", Category.Charts,
            "Flows between named stages, each band as wide as its value: signups from source to plan to status, budgets from team to project. D3 places the nodes and bands; the chart draws them in theme colours.",
            [typeof(SankeyChart)],
            [
                new("Basic", "Each SankeyLink moves a Value from Source to Target; node names come from the links, and each node shows its total.", typeof(SankeyChartBasicExample)),
                new("Node colours", "NodeColors maps a node name to any CSS colour; ShowValues=\"false\" keeps only the names, and AspectRatio sets the shape.", typeof(SankeyChartColorsExample)),
            ])
        {
            Portrait = typeof(SankeyChartCoverExample),
        },
    ];
}
