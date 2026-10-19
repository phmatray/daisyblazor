using DaisyBlazor.Charts;
using DaisyBlazor.Docs.Examples.Charts;

namespace DaisyBlazor.Docs.Docs;

public static partial class DocRegistry
{
    private static partial IEnumerable<ComponentDoc> ChartsTime() =>
    [
        new("CalendarHeatmap", Category.Charts,
            "A square for every day, weeks as columns and weekdays as rows, shaded by how much happened that day: deployments, builds, sign-ins. D3 builds the calendar and splits the values into levels; dates are calendar dates, so the time zone never moves a day.",
            [typeof(CalendarHeatmap)],
            [
                new("Basic", "Each CalendarDay gives a DateOnly and a Value; the calendar runs from the first to the last day, with month and weekday names and a Less–More key.", typeof(CalendarHeatmapBasicExample)),
                new("Range, week start and levels", "From and To fix the days shown, FirstDayOfWeek picks the top row, Levels sets the number of shades and Color the theme colour they step up to.", typeof(CalendarHeatmapRangeExample)),
            ])
        {
            Portrait = typeof(CalendarHeatmapCoverExample),
        },
        new("HeatmapChart", Category.Charts,
            "A grid of rows and columns where each cell is shaded by its value: requests by weekday and hour, response time by region and endpoint. D3 lays out the grid and the sequential scale from the smallest to the largest value; the shades are mixed from a theme colour.",
            [typeof(HeatmapChart)],
            [
                new("Basic", "Each HeatmapCell puts a Value at a Row and Column; rows and columns follow the order the data first names them, and crowded column labels are thinned.", typeof(HeatmapChartBasicExample)),
                new("Values, order and colour", "ShowValues writes each value in its cell, Rows and Columns fix the order, and Color picks the theme colour of the largest value.", typeof(HeatmapChartValuesExample)),
            ])
        {
            Portrait = typeof(HeatmapChartCoverExample),
        },
        new("RangeAreaChart", Category.Charts,
            "A band from a low to a high value at each point, with an optional line through it: response time between its fast and slow percentiles around the median, sessions between the quietest and busiest moment. D3 computes the scales, ticks and smooth curves on a time or category axis.",
            [typeof(RangeAreaChart)],
            [
                new("Basic", "Points built with a date sit on a time axis; Low and High bound the band and Center draws the line. BandLabel and CenterLabel name them in the legend and tooltips.", typeof(RangeAreaChartBasicExample)),
                new("Category axis", "Points built with a label sit on a category axis; Smooth=\"false\" joins them with straight segments and Color picks the theme colour.", typeof(RangeAreaChartCategoryExample)),
                new("Step", "Step draws the band and the center line as steps that hold until the next point; it wins over Smooth, which is on by default.", typeof(RangeAreaChartStepExample)),
            ])
        {
            Portrait = typeof(RangeAreaChartCoverExample),
        },
        new("StreamChart", Category.Charts,
            "Series stacked as flowing layers over time, so both the total and each part's change read at a glance: weekly active users by product area, requests by client. D3 stacks the layers as a streamgraph, a stacked area or shares of 100%, and draws the smooth areas.",
            [typeof(StreamChart)],
            [
                new("Basic", "Each ChartSeries is a layer; Dates put its values on a time axis. The default Wiggle offset centres the stream, and each layer is named where it is thick enough.", typeof(StreamChartBasicExample)),
                new("Percent and stacked", "Offset=\"Percent\" shows each layer's share of the total; Offset=\"Stacked\" stacks the values on a zero baseline with a value axis. Labels put the values on a category axis.", typeof(StreamChartOffsetExample)),
            ])
        {
            Portrait = typeof(StreamChartCoverExample),
        },
        new("CandlestickChart", Category.Charts,
            "The first, highest, lowest and last value of each period on a time axis: CPU utilisation per hour, queue depth per ten minutes. D3 computes the time scale, ticks and the width each period gets; rising periods take the success colour and falling ones the error colour.",
            [typeof(CandlestickChart)],
            [
                new("Basic", "Each Candle gives a Time and its Open, High, Low and Close values; a candle's body runs from open to close and its wick from low to high.", typeof(CandlestickChartBasicExample)),
                new("OHLC and high–low bars", "CandleStyle=\"Ohlc\" draws ticks for the open and close; CandleStyle=\"HighLow\" only the range. UpLabel and DownLabel name the colours in the legend.", typeof(CandlestickChartStyleExample)),
                new("Indicators", "Indicators takes ChartIndicator records computed over the closes. Sma is the mean of the last Period closes; Ema uses α = 2 / (Period + 1), seeded with the first SMA; Bollinger draws the SMA ± StdDev population standard deviations. Rsi (Wilder's smoothing, lines at 30 and 70) and Macd (EMA 12 − EMA 26, signal EMA 9, histogram) get a panel each under the candles. Each indicator takes a palette colour other than the rising and falling ones, is named in the legend and shows its value in the candle's tooltip.", typeof(CandlestickChartIndicatorsExample)),
                new("Volume", "Give each Candle a Volume (an init property: new Candle(...) { Volume = 1200 }) and set ShowVolume=\"true\": the volumes are drawn as bars in a panel under the price, on the same time axis, in the period's rising or falling colour. A ChartIndicator's Label renames it in the legend.", typeof(CandlestickChartVolumeExample)),
                new("Close line and area", "CandleStyle=\"Line\" draws the closes as a line and CandleStyle=\"Area\" fills under it; the value axis fits the closes, PriceColor sets the colour and each period's tooltip still gives its four values. Indicators draw over them as usual.", typeof(CandlestickChartPriceLineExample)),
                new("Heikin-Ashi", "CandleStyle=\"HeikinAshi\" draws candles computed from the periods: close = (O + H + L + C) / 4, open = (previous HA open + previous HA close) / 2 (the first one (O + C) / 2), high and low the extremes of the period's high or low and the HA open and close. Indicators still use the real values.", typeof(CandlestickChartHeikinAshiExample)),
                new("EquiVolume and CandleVolume", "CandleStyle=\"EquiVolume\" draws a box from low to high and CandleStyle=\"CandleVolume\" a candle, each as wide as its Volume (the largest takes 90% of the space between periods), centred on its time so the time axis reads as usual. When any period has no positive Volume, every mark falls back to the same width.", typeof(CandlestickChartVolumeWidthExample)),
                new("Price channel", "IndicatorKind.PriceChannel draws the highest high and the lowest low of the last Period periods (default 20, the current one included) as a band, with their midpoint as the line.", typeof(CandlestickChartPriceChannelExample)),
            ])
        {
            Portrait = typeof(CandlestickChartCoverExample),
        },
        new("PriceActionChart", Category.Charts,
            "Charts that only move when the value does, whatever the time: Renko bricks, Kagi lines, line break lines and point and figure columns, built from a series of readings such as CPU utilisation or queue depth. D3 computes the bricks, lines or columns from the closes; the x axis numbers them, the y axis is the value, and each tooltip gives the dates of the move.",
            [typeof(PriceActionChart)],
            [
                new("Renko", "Prices takes Candle records (only the closes build the chart). ChartKind=\"Renko\" draws a brick of BoxSize each time a close is a whole box past the last brick, so a trend goes on after one box and reverses after two. Without a BoxSize, the brick is the average true range over AtrPeriod (14) periods.", typeof(PriceActionChartRenkoExample)),
                new("Kagi", "ChartKind=\"Kagi\" draws a line that goes on while closes extend it and turns when one comes back by Reversal (in value units, or a percentage of the line's end with ReversalPercent). The line turns thick (yang) where it rises above the previous shoulder and thin (yin) where it falls below the previous waist.", typeof(PriceActionChartKagiExample)),
                new("Line break", "ChartKind=\"LineBreak\" draws a new line each time a close passes the last line's top or bottom; a line against the trend must also pass the extreme of the last Lines lines (3 by default).", typeof(PriceActionChartLineBreakExample)),
                new("Point and figure", "ChartKind=\"PointAndFigure\" stacks X boxes of BoxSize while closes rise and O boxes while they fall; a column turns when a close is Reversal boxes (3 by default) past its end, starting one box away from it.", typeof(PriceActionChartPointAndFigureExample)),
            ])
        {
            Portrait = typeof(PriceActionChartCoverExample),
        },
        new("GanttChart", Category.Charts,
            "Tasks as bars from their start to their end date, with progress, milestones, groups and a marker for today: a release plan, a sprint. D3 computes the time scale, rows and ticks; each group takes a theme colour.",
            [typeof(GanttChart)],
            [
                new("Basic", "Each GanttTask has a Name, Start and End; tasks with the same Group are listed under its name, Progress fills the done part and a task without an End is a milestone.", typeof(GanttChartBasicExample)),
                new("Marker", "Today places the marker at any moment (it defaults to the current date) and TodayLabel names it; without groups the tasks are listed in order.", typeof(GanttChartMarkerExample)),
            ])
        {
            Portrait = typeof(GanttChartCoverExample),
        },
        new("BumpChart", Category.Charts,
            "The rank of each series in each period, joined by smooth lines, so overtakes stand out: plans ranked by sign-ups, features ranked by use. D3 ranks the values and draws the curves; each series is named at both ends.",
            [typeof(BumpChart)],
            [
                new("Basic", "Each ChartSeries holds one value per period in Labels; the highest value in a period ranks first, and tooltips show the rank and the value.", typeof(BumpChartBasicExample)),
                new("Ranks as data", "ValuesAreRanks=\"true\" takes the data as places (1 = top) instead of values to rank; ShowPoints=\"false\" leaves only the lines.", typeof(BumpChartRanksExample)),
            ])
        {
            Portrait = typeof(BumpChartCoverExample),
        },
        new("RangeSelector", Category.Charts,
            "A compact overview of a time series with a window you move and resize, to pick the stretch of time a chart shows in detail. Drag the window to move it and its handles to resize it; the handles are sliders, so arrows move them by one step, Shift+arrow by ten, Home and End to the limits. Outside the window is dimmed.",
            [typeof(RangeSelector)],
            [
                new("Basic", "Points holds a RangeSelectorPoint (a Time and a Value) per moment. Bind From and To with @bind-From and @bind-To; Step snaps the window to whole steps (a day here) and is the arrow keys' step.", typeof(RangeSelectorBasicExample)),
                new("With a chart", "Bind From and To to fields and give the chart only the data between them: here the candles of a CandlestickChart, filtered on Time. Moving the window redraws the chart as you drag. The same works for a LineChart, filtering its labels and values.", typeof(RangeSelectorChartExample)),
            ])
        {
            Portrait = typeof(RangeSelectorCoverExample),
        },
    ];
}
