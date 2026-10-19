namespace DaisyBlazor.Charts;

/// <summary>One bar of a <see cref="RangeBarChart"/>: a span from <paramref name="Low"/> to <paramref name="High"/>.</summary>
/// <param name="Label">The category; items with the same label share its band.</param>
/// <param name="Low">Start of the span.</param>
/// <param name="High">End of the span.</param>
/// <param name="Group">Optional group; each group gets its own slot in every band and its own colour.</param>
public sealed record RangeBarItem(string Label, double Low, double High, string? Group = null);

/// <summary>Geometry of a <see cref="RangeBarChart"/>, computed by D3.</summary>
public sealed record RangeBarLayout(CompareRect Plot, IReadOnlyList<CompareTick> Ticks, IReadOnlyList<CompareText> Categories, IReadOnlyList<string> Groups, IReadOnlyList<RangeBarLayoutBar> Bars);

/// <summary>A placed bar with its group, rectangle and end value texts (null when values are hidden).</summary>
public sealed record RangeBarLayoutBar(string Label, string Group, int GroupIndex, double Low, double High, double X, double Y, double Width, double Height, CompareText? LowText, CompareText? HighText);
