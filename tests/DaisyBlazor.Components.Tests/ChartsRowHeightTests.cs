using DaisyBlazor.Charts;

namespace DaisyBlazor.Components.Tests;

/// <summary>
/// RowHeight on the charts that lay their categories out as rows: each counts its rows the way its
/// layout does and sizes itself RowHeight × (rows + padding) + chrome. The heights are the ones
/// tests/charts-row-height.test.mjs feeds the layouts to prove their rows land RowHeight apart, so
/// the two suites together tie each component's count and constants to its layout.
/// </summary>
public class ChartsRowHeightTests : BunitContext
{
    private static readonly ValueGroup[] _groups =
        [new("A", [1, 2, 3, 4]), new("B", [2, 3, 5]), new("C", [1, 4]), new("D", [])];

    private static readonly DateTime _day = new(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc);

    // Loose: the layout call goes unanswered, so the chart stays on its placeholder, which already
    // carries the view box's shape.
    private string Placeholder<TChart>(Action<ComponentParameterCollectionBuilder<TChart>> parameters)
        where TChart : Microsoft.AspNetCore.Components.IComponent
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        this.SetupChartModule();
        return Render(parameters).Find("[aria-busy=true]").GetAttribute("style")!;
    }

    private static string Shape(double height) => $"aspect-ratio:640 / {height.ToString(System.Globalization.CultureInfo.InvariantCulture)};";

    [Fact]
    public void Category_frame_charts_size_by_their_rows()
    {
        Placeholder<LollipopChart>(ps => ps
                .Add(p => p.Data, [new("A", 1), new("B", 2), new("C", 3), new("D", 4), new("X", double.NaN)])
                .Add(p => p.RowHeight, 24))
            .ShouldBe(Shape(126.8));
        Placeholder<DumbbellChart>(ps => ps
                .Add(p => p.Items, [new("A", 0, 3), new("B", 1, 4), new("C", 2, 5)])
                .Add(p => p.RowHeight, 24))
            .ShouldBe(Shape(102.8));
    }

    [Fact]
    public void Bullet_rows_keep_a_tick_line_each_or_share_one()
    {
        BulletItem[] rows = [new("A", 1, null, [1, 2, 3]), new("B", 2, null, [1, 2, 4]), new("C", 3, null, [1, 2, 5])];
        Placeholder<BulletChart>(ps => ps.Add(p => p.Items, rows).Add(p => p.SharedAxis, false).Add(p => p.RowHeight, 24)).ShouldBe(Shape(76));
        Placeholder<BulletChart>(ps => ps.Add(p => p.Items, rows).Add(p => p.SharedAxis, true).Add(p => p.RowHeight, 24)).ShouldBe(Shape(92));
    }

    [Fact]
    public void Heatmap_counts_its_named_rows_or_the_rows_of_its_cells()
    {
        HeatmapCell[] cells = [.. new[] { "Mon", "Tue", "Wed", "Thu" }.SelectMany((row, r) => new[] { "00", "06", "12" }.Select((column, c) => new HeatmapCell(row, column, r + c)))];
        Placeholder<HeatmapChart>(ps => ps.Add(p => p.Cells, cells).Add(p => p.RowHeight, 24)).ShouldBe(Shape(119.44));
        Placeholder<HeatmapChart>(ps => ps.Add(p => p.Cells, cells).Add(p => p.Rows, ["Mon", "Tue", "Wed", "Thu", "Fri"]).Add(p => p.RowHeight, 24))
            .ShouldBe(Shape(143.44));
    }

    [Fact]
    public void Gantt_counts_lanes_and_group_headers_and_sends_its_window()
    {
        GanttTask[] tasks =
        [
            new("Prep", _day.AddHours(6), _day.AddHours(7)),
            new("Stand 1", _day.AddHours(6.5), _day.AddHours(8), "Apron", Row: "S1"),
            new("Stand 1 bis", _day.AddHours(9), _day.AddHours(10), "Apron", Row: "S1"),
            new("Stand 2", _day.AddHours(7), _day.AddHours(9), "Apron", Row: "S2"),
            new("Gate 4", _day.AddHours(8), _day.AddHours(11), "Gates"),
        ];

        // Prep, S1, S2 and Gate 4, plus the Apron and Gates headers.
        Placeholder<GanttChart>(ps => ps.Add(p => p.Tasks, tasks).Add(p => p.RowHeight, 24)).ShouldBe(Shape(186));

        // Without headers the four lanes alone; a window from 08:30 leaves out Prep (ends 07:00).
        Placeholder<GanttChart>(ps => ps.Add(p => p.Tasks, tasks).Add(p => p.ShowGroupHeaders, false).Add(p => p.RowHeight, 24))
            .ShouldBe(Shape(138));
        Placeholder<GanttChart>(ps => ps.Add(p => p.Tasks, tasks).Add(p => p.From, _day.AddHours(8.5)).Add(p => p.RowHeight, 24))
            .ShouldBe(Shape(162));
    }

    [Fact]
    public void Distribution_charts_count_the_groups_with_values()
    {
        Placeholder<BeeswarmChart>(ps => ps.Add(p => p.Groups, _groups).Add(p => p.RowHeight, 24)).ShouldBe(Shape(107.92));
        Placeholder<BeeswarmChart>(ps => ps.Add(p => p.Groups, _groups).Add(p => p.ValueTitle, "Minutes").Add(p => p.RowHeight, 24)).ShouldBe(Shape(125.92));
        Placeholder<ViolinChart>(ps => ps.Add(p => p.Groups, _groups).Add(p => p.Horizontal, true).Add(p => p.RowHeight, 24)).ShouldBe(Shape(109.6));
        Placeholder<BoxPlotChart>(ps => ps.Add(p => p.Groups, _groups).Add(p => p.Horizontal, true).Add(p => p.RowHeight, 24)).ShouldBe(Shape(113.2));
        Placeholder<RidgelineChart>(ps => ps.Add(p => p.Groups, _groups).Add(p => p.Overlap, 1.6).Add(p => p.RowHeight, 24)).ShouldBe(Shape(120.4));
    }

    [Fact]
    public void Horizontal_waterfall_and_column3d_size_by_their_rows()
    {
        WaterfallStep[] steps =
        [
            new("Open", 100, WaterfallStepKind.Start), new("Sales", 40), new("Costs", -25), new("Skipped", double.NaN),
            new("Q1", double.NaN, WaterfallStepKind.Subtotal), new("Close", double.NaN, WaterfallStepKind.End),
        ];
        Placeholder<WaterfallChart>(ps => ps.Add(p => p.Steps, steps).Add(p => p.Horizontal, true).Add(p => p.RowHeight, 24)).ShouldBe(Shape(150));

        ChartDataPoint[] six = [new("A", 1), new("B", 2), new("C", 3), new("D", 4), new("E", 5), new("F", 6)];
        Placeholder<Column3DChart>(ps => ps.Add(p => p.Data, six).Add(p => p.Horizontal, true).Add(p => p.RowHeight, 24)).ShouldBe(Shape(173.6));
    }

    [Fact]
    public void A_chart_without_rows_in_its_orientation_keeps_its_aspect_ratio()
    {
        ChartDataPoint[] points = [new("A", 1), new("B", 2)];
        Placeholder<LollipopChart>(ps => ps.Add(p => p.Data, points).Add(p => p.Horizontal, false).Add(p => p.RowHeight, 24)).ShouldBe(Shape(360));
        Placeholder<WaterfallChart>(ps => ps.Add(p => p.Steps, [new WaterfallStep("A", 1)]).Add(p => p.RowHeight, 24)).ShouldBe(Shape(360));
        Placeholder<LollipopChart>(ps => ps.Add(p => p.Data, points)).ShouldBe(Shape(360));
    }

    [Fact]
    public void A_horizontal_bar_chart_sizes_its_box_by_its_categories()
    {
        JSInterop.SetupCharts();
        ChartDataPoint[] six = [new("A", 1), new("B", 2), new("C", 3), new("D", 4), new("E", 5), new("F", 6)];

        Render<BarChart>(ps => ps.Add(p => p.Data, six).Add(p => p.Horizontal, true).Add(p => p.RowHeight, 24))
            .Markup.ShouldContain("height:171.2px;");
        Render<BarChart>(ps => ps.Add(p => p.Data, six).Add(p => p.RowHeight, 24).Add(p => p.Height, "200px"))
            .Markup.ShouldContain("height:200px;");
    }
}
