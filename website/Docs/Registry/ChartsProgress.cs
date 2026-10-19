using DaisyBlazor.Charts;
using DaisyBlazor.Docs.Examples.Charts;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> ChartsProgress() =>
    [
        new("WaterfallChart", Category.Charts,
            "How a total gets from one value to another: a starting count, the changes that raise or lower it, optional subtotals and the closing count, such as customers gained and lost over a quarter. Increases are green, decreases red, totals in the primary colour. D3 computes the running totals and the scales.",
            [typeof(WaterfallChart)],
            [
                new("Basic", "A Start step sets the opening total, each Delta step adds its Value (negative to subtract), and an End step draws where it lands. Dashed rules carry each total across to the next bar.", typeof(WaterfallChartBasicExample)),
                new("Subtotals, sideways", "Subtotal steps draw the running total part way through; Horizontal lays the bars out top to bottom with room for longer names.", typeof(WaterfallChartSubtotalsExample)),
            ])
        {
            Portrait = typeof(WaterfallChartCoverExample),
        },

        new("FunnelChart", Category.Charts,
            "Stages that each keep part of the one before, such as trial to paid: every stage is as wide as its value and shows its conversion from the previous stage. D3 computes the outlines.",
            [typeof(FunnelChart)],
            [
                new("Basic", "Stages are ChartDataPoint values, first to last; the right side shows each value and the share kept from the stage before.", typeof(FunnelChartBasicExample)),
                new("Pyramid", "Shape=\"FunnelShape.Pyramid\" cuts a triangle into slices whose heights follow the values, first stage at the base. Color=\"@null\" gives each stage its own palette colour.", typeof(FunnelChartPyramidExample)),
            ])
        {
            Portrait = typeof(FunnelChartCoverExample),
        },

        new("GaugeChart", Category.Charts,
            "One reading on an arc between Min and Max, such as uptime against its SLA, with optional coloured bands and a target marker. D3 computes the arcs and fits them in the box; the accessible name includes the value.",
            [typeof(GaugeChart)],
            [
                new("Bands and marker", "Bands colour ranges of the scale in a ring outside the arc; Marker draws the target across it. A value outside Min to Max fills the arc to its end but prints as given.", typeof(GaugeChartBasicExample)),
                new("Half circle", "StartAngle and EndAngle (degrees from 12 o'clock) shape the arc: -90 to 90 is a half circle. Thickness sets the arc width and Color its theme colour.", typeof(GaugeChartHalfExample)),
            ])
        {
            Portrait = typeof(GaugeChartCoverExample),
        },

        new("LinearGaugeChart", Category.Charts,
            "One reading on a straight scale with ticks, a pointer, coloured bands and an optional target, such as storage used against its quota. D3 scales place everything along the track; the accessible name includes the value.",
            [typeof(LinearGaugeChart)],
            [
                new("Basic", "Bands are painted on the track, Marker draws the target, and ValueFormat adds the unit.", typeof(LinearGaugeChartBasicExample)),
                new("Vertical", "Vertical runs the scale bottom to top. Give it an AspectRatio below 1 and a narrow Width: a vertical gauge sizes its text from its width.", typeof(LinearGaugeChartVerticalExample)),
            ])
        {
            Portrait = typeof(LinearGaugeChartCoverExample),
        },

        new("RadialBarChart", Category.Charts,
            "Several progress values as concentric rings, one per item, such as the share of each quota used. Rings run three quarters of a turn so their labels sit in the free quarter and never cross a ring. D3 computes the arcs.",
            [typeof(RadialBarChart)],
            [
                new("Basic", "Each ChartDataPoint is a ring, the first outside; a full ring is Max (100 by default).", typeof(RadialBarChartBasicExample)),
                new("Max", "Max sets what a full ring means, here the 8 onboarding steps; a point's Color overrides the palette.", typeof(RadialBarChartMaxExample)),
            ])
        {
            Portrait = typeof(RadialBarChartCoverExample),
        },

        new("PolarAreaChart", Category.Charts,
            "A rose (Nightingale) chart: every category gets an equal slice of the circle and reaches out according to its value, so the area of each wedge is proportional to it. Good for counts per category or per weekday. D3 computes the wedges.",
            [typeof(PolarAreaChart)],
            [
                new("Basic", "Wedges run clockwise from 12 o'clock in the order given; grid circles mark the scale.", typeof(PolarAreaChartBasicExample)),
                new("Colours, names only", "Give a point its own Color to make it stand out; ShowValues=\"false\" keeps only the names (values stay in the tooltips).", typeof(PolarAreaChartColorsExample)),
            ])
        {
            Portrait = typeof(PolarAreaChartCoverExample),
        },

        new("ParetoChart", Category.Charts,
            "Which few causes make up most of the total: bars sorted largest first, the cumulative share as a line on a 0–100 % axis, and a reference line at 80 %. The bars needed to reach it stay solid. D3 computes the sort, the scales and the line.",
            [typeof(ParetoChart)],
            [
                new("Basic", "Pass the categories in any order; the chart sorts them and fades the bars past the threshold.", typeof(ParetoChartBasicExample)),
                new("Threshold and colours", "Threshold moves the reference line (0.9 here); BarColor, LineColor and ThresholdColor take any CSS colour.", typeof(ParetoChartThresholdExample)),
            ])
        {
            Portrait = typeof(ParetoChartCoverExample),
        },

        new("MarimekkoChart", Category.Charts,
            "Two breakdowns at once: each column is as wide as its share of the total and split 100 % by segment, so every cell's area is its share of everything, such as customers by region and plan. D3 scales place the columns and cells.",
            [typeof(MarimekkoChart)],
            [
                new("Basic", "Each MarimekkoCell gives the Value of a Segment in a Column. Cells show their share of the column where they fit; columns show their share of the total.", typeof(MarimekkoChartBasicExample)),
                new("Segment colours", "SegmentColors maps a segment name to any CSS colour; ShowValues=\"false\" leaves the cells plain.", typeof(MarimekkoChartColorsExample)),
            ])
        {
            Portrait = typeof(MarimekkoChartCoverExample),
        },

        new("WaffleChart", Category.Charts,
            "Parts of a whole as a 10 × 10 grid where each cell is 1 %, such as seats by status. Shares are rounded so the cells always add up. D3 computes the layout.",
            [typeof(WaffleChart)],
            [
                new("Basic", "The items fill the grid row by row in the order given; the legend shows each share.", typeof(WaffleChartBasicExample)),
                new("Out of a total", "Total larger than the sum leaves the remaining cells empty; Rows and Columns change the grid (5 × 20 here).", typeof(WaffleChartTotalExample)),
            ])
        {
            Portrait = typeof(WaffleChartCoverExample),
        },
    ];
}
