using DaisyBlazor.Charts;
using DaisyBlazor.Docs.Examples.Charts;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> ChartsCompare() =>
    [
        new("RadarChart", Category.Charts,
            "Several series over the same named axes, each a closed shape around the centre: plans scored on seats, storage and support, teams on a health check. D3 sizes the radius so every axis name fits and places the rings on round values; the chart draws them in theme colours. It needs JavaScript, so under static server rendering it shows a placeholder.",
            [typeof(RadarChart)],
            [
                new("Basic", "Labels names the axes clockwise from the top; value i of each ChartSeries lines up with axis i, and each series gets the next palette colour.", typeof(RadarChartBasicExample)),
                new("Round grid, outlines", "Grid=\"RadarGrid.Circle\" draws round rings, Filled=\"false\" keeps only the outlines, and Max fixes the outer ring instead of rounding up the largest value.", typeof(RadarChartGridExample)),
            ])
        {
            Portrait = typeof(RadarChartCoverExample),
        },

        new("BulletChart", Category.Charts,
            "A measure against its target over shaded bands for behind, on track and ahead: active seats against the quarterly goal, deployments against plan. Each row has its own scale, so measures in different units sit together. D3 computes the scales and ticks; the chart draws them in theme colours. It needs JavaScript, so under static server rendering it shows a placeholder.",
            [typeof(BulletChart)],
            [
                new("Basic", "Each BulletItem has a Value, an optional Target (the short line) and ascending Ranges for the bands behind the bar.", typeof(BulletChartBasicExample)),
                new("Vertical", "Vertical=\"true\" stands the bullets up as columns; BarColor takes any CSS colour and ValueFormat shapes the tooltips.", typeof(BulletChartVerticalExample)),
            ])
        {
            Portrait = typeof(BulletChartCoverExample),
        },

        new("DumbbellChart", Category.Charts,
            "Two values per category joined by a line, so the size and direction of each change reads at a glance: response time before and after a release, per region. D3 computes the shared scale and ticks; the chart draws the dots in theme colours. It needs JavaScript, so under static server rendering it shows a placeholder.",
            [typeof(DumbbellChart)],
            [
                new("Basic", "Each DumbbellItem has a Start and an End; StartLabel and EndLabel name them in the legend, and the values sit outside each pair.", typeof(DumbbellChartBasicExample)),
                new("Colours", "StartColor and EndColor take any CSS colour; ShowValues=\"false\" leaves the numbers to the tooltips.", typeof(DumbbellChartColorsExample)),
            ])
        {
            Portrait = typeof(DumbbellChartCoverExample),
        },

        new("SlopeChart", Category.Charts,
            "Each item as a line between two time points on one scale, named at both ends: NPS by team from the first quarter to the last. D3 computes the scale and moves end labels apart so close values never overlap; the chart draws the lines in theme colours. It needs JavaScript, so under static server rendering it shows a placeholder.",
            [typeof(SlopeChart)],
            [
                new("Basic", "Each SlopeItem goes from Start to End; StartLabel and EndLabel head the two columns.", typeof(SlopeChartBasicExample)),
                new("Colour by change", "ColorByChange=\"true\" draws rising lines in the success colour and falling ones in the error colour.", typeof(SlopeChartChangeExample)),
            ])
        {
            Portrait = typeof(SlopeChartCoverExample),
        },

        new("LollipopChart", Category.Charts,
            "Categories ranked by value, each a thin stem from zero with a dot at the end: a lighter bar chart for NPS by team or deployments by day. D3 computes the scale, ticks and bands; the chart draws them in theme colours. It needs JavaScript, so under static server rendering it shows a placeholder.",
            [typeof(LollipopChart)],
            [
                new("Basic", "Data takes ChartDataPoint values; they are sorted largest first, and negative values hang on the other side of zero.", typeof(LollipopChartBasicExample)),
                new("Vertical, in order", "Vertical=\"true\" stands the stems up, Sort=\"LollipopSort.None\" keeps the data's order, and a point's Color overrides the chart's.", typeof(LollipopChartVerticalExample)),
            ])
        {
            Portrait = typeof(LollipopChartCoverExample),
        },

        new("ParallelCoordinatesChart", Category.Charts,
            "Items compared on several measures at once: one vertical axis per measure, each with its own scale and ticks, and one line per item through its values, such as plans across seats, storage, API calls and support hours. D3 computes the scales and lines; the chart draws them in theme colours. It needs JavaScript, so under static server rendering it shows a placeholder.",
            [typeof(ParallelCoordinatesChart)],
            [
                new("Basic", "Dimensions names the axes left to right; value i of each ParallelItem lines up with dimension i.", typeof(ParallelCoordinatesChartBasicExample)),
                new("Highlight", "Highlight brings one item forward and fades the others; a NaN value breaks that item's line on its axis.", typeof(ParallelCoordinatesChartHighlightExample)),
            ])
        {
            Portrait = typeof(ParallelCoordinatesChartCoverExample),
        },

        new("RangeBarChart", Category.Charts,
            "A bar from a low to a high value per category: seats allowed per plan, response time from median to worst case, optionally with groups side by side. D3 computes the scale, ticks and bands; the chart draws the bars in theme colours. It needs JavaScript, so under static server rendering it shows a placeholder.",
            [typeof(RangeBarChart)],
            [
                new("Basic", "Each RangeBarItem spans from Low to High; ShowValues writes both ends beside the bar.", typeof(RangeBarChartBasicExample)),
                new("Grouped", "Items with the same Label share a band and each Group gets its own slot, colour and legend entry; GroupColors maps a group to any CSS colour.", typeof(RangeBarChartGroupedExample)),
            ])
        {
            Portrait = typeof(RangeBarChartCoverExample),
        },
    ];
}
