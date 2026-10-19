namespace DaisyBlazor.Charts;

/// <summary>One line of a <see cref="SlopeChart"/>: a <paramref name="Label"/> going from <paramref name="Start"/> to <paramref name="End"/>.</summary>
/// <param name="Label">Name written at both ends of the line.</param>
/// <param name="Start">Value at the first time point.</param>
/// <param name="End">Value at the second time point.</param>
/// <param name="Color">Optional CSS colour; otherwise the line takes the next theme palette colour.</param>
public sealed record SlopeItem(string Label, double Start, double End, string? Color = null);

/// <summary>Geometry of a <see cref="SlopeChart"/>, computed by D3: column headers, column positions and lines.</summary>
public sealed record SlopeLayout(IReadOnlyList<CompareText> Headers, double X1, double X2, double Y0, double Y1, IReadOnlyList<SlopeLayoutItem> Items);

/// <summary>A placed line with its end labels, already moved apart from their neighbours.</summary>
public sealed record SlopeLayoutItem(string Label, double Start, double End, double X1, double Y1, double X2, double Y2, CompareText Left, CompareText Right);
