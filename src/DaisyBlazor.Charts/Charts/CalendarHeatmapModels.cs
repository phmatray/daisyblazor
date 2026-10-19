namespace DaisyBlazor.Charts;

/// <summary>One day of a <see cref="CalendarHeatmap"/>: the calendar <paramref name="Date"/> and its <paramref name="Value"/>.</summary>
public sealed record CalendarDay(DateOnly Date, double Value);

/// <summary>Geometry of a <see cref="CalendarHeatmap"/>, computed by D3: a cell per day, month labels (placed at an x, above the grid) and weekday labels (at a y, left of the grid), and the level thresholds.</summary>
public sealed record CalendarHeatmapLayout(
    IReadOnlyList<CalendarHeatmapCell> Cells,
    IReadOnlyList<AxisTick> Months,
    IReadOnlyList<AxisTick> Weekdays,
    IReadOnlyList<double> Thresholds,
    double Max,
    double CellSize);

/// <summary>A placed day: its ISO date, display label, value, level (0 = none) and square.</summary>
public sealed record CalendarHeatmapCell(string Date, string Label, double Value, int Level, double X, double Y, double Size);
