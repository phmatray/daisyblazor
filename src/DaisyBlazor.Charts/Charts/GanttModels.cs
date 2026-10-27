namespace DaisyBlazor.Charts;

/// <summary>
/// One task of a <see cref="GanttChart"/>, from <paramref name="Start"/> to <paramref name="End"/>.
/// Without an end (or with an end not after the start) it is a milestone. Tasks with the same
/// <paramref name="Group"/> are listed together and share a colour; <paramref name="Progress"/> is the done fraction, 0 to 1.
/// Tasks with the same <paramref name="Row"/> share one row (a lane, such as a stand or a machine), labelled with that key.
/// <paramref name="Color"/> (any CSS colour) overrides the group's colour for this task.
/// </summary>
public sealed record GanttTask(string Name, DateTime Start, DateTime? End = null, string? Group = null, double? Progress = null, string? Row = null, string? Color = null);

/// <summary>Geometry of a <see cref="GanttChart"/>, computed by D3.</summary>
public sealed record GanttLayout(
    IReadOnlyList<GanttRow> Rows,
    IReadOnlyList<string> Groups,
    AxisTick? Today,
    IReadOnlyList<AxisTick> XTicks,
    PlotArea Plot);

/// <summary>
/// A placed row: a group header, or a task with its dates, progress and bar (a diamond when it is a milestone).
/// Tasks sharing a lane follow each other with the same Y. Index is the task's position in
/// <see cref="GanttChart.Tasks"/>; Indent is set for rows listed under a group header.
/// </summary>
public sealed record GanttRow(
    bool Header, string Name, string Label, int Group, bool Milestone, string? Start, string? End,
    double? Progress, double X0, double X1, double Y, double H, string? Color = null, bool Indent = false, int Index = -1);
