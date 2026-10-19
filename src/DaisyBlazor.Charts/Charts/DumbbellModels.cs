namespace DaisyBlazor.Charts;

/// <summary>One row of a <see cref="DumbbellChart"/>: a <paramref name="Label"/> with a <paramref name="Start"/> and an <paramref name="End"/> value.</summary>
public sealed record DumbbellItem(string Label, double Start, double End);

/// <summary>Geometry of a <see cref="DumbbellChart"/>, computed by D3.</summary>
public sealed record DumbbellLayout(CompareRect Plot, IReadOnlyList<CompareTick> Ticks, IReadOnlyList<CompareText> Categories, IReadOnlyList<DumbbellLayoutItem> Items);

/// <summary>A placed row: dot positions, row centre and the value texts (null when values are hidden).</summary>
public sealed record DumbbellLayoutItem(string Label, double Start, double End, double X1, double X2, double Y, CompareText? StartText, CompareText? EndText);
