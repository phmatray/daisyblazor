namespace DaisyBlazor.Charts;

/// <summary>Geometry of a <see cref="WaffleChart"/>.</summary>
public sealed record WaffleLayout(double Size, IReadOnlyList<WaffleItem> Items, IReadOnlyList<WaffleCell> Cells);

/// <summary>An item's share of the whole and the number of cells it fills.</summary>
public sealed record WaffleItem(string Label, double Value, double Share, int Cells);

/// <summary>A placed cell and the index of the item that fills it (-1 for empty).</summary>
public sealed record WaffleCell(double X, double Y, int Item);
