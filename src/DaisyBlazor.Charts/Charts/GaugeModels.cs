namespace DaisyBlazor.Charts;

/// <summary>A coloured range on a <see cref="GaugeChart"/> or <see cref="LinearGaugeChart"/> scale, such as "below target".</summary>
/// <param name="From">Where the range starts, in scale units.</param>
/// <param name="To">Where the range ends, in scale units.</param>
/// <param name="Color">Optional CSS colour (e.g. <c>var(--color-success)</c>); falls back to the theme palette.</param>
/// <param name="Label">Optional name, shown in the tooltip.</param>
public sealed record GaugeBand(double From, double To, string? Color = null, string? Label = null);

/// <summary>Geometry of a <see cref="GaugeChart"/>. Track, fill and band paths are relative to (Cx, Cy).</summary>
public sealed record GaugeLayout(
    double Cx, double Cy, double Radius, string Track, string Fill,
    IReadOnlyList<GaugeLayoutBand> Bands, GaugeLayoutMarker? Marker, IReadOnlyList<GaugeLayoutTick> Ticks,
    double Value, double Fraction, double FontSize, double ValueFontSize, double ValueY);

/// <summary>A placed band: the index of the source <see cref="GaugeBand"/>, its clamped range and its arc.</summary>
public sealed record GaugeLayoutBand(int Index, double From, double To, string Path);

/// <summary>The target marker: its clamped value and the line crossing the arc.</summary>
public sealed record GaugeLayoutMarker(double Value, double X1, double Y1, double X2, double Y2);

/// <summary>A tick label around the arc.</summary>
public sealed record GaugeLayoutTick(double Value, double X, double Y, string Anchor);

/// <summary>Geometry of a <see cref="LinearGaugeChart"/>.</summary>
public sealed record LinearGaugeLayout(
    ChartRect? Track, ChartRect? Fill, IReadOnlyList<LinearGaugeBand> Bands,
    LinearGaugePointer? Pointer, GaugeLayoutMarker? Marker, IReadOnlyList<LinearGaugeTick> Ticks,
    double Value, double FontSize);

/// <summary>A placed band: the index of the source <see cref="GaugeBand"/>, its clamped range and its rectangle.</summary>
public sealed record LinearGaugeBand(int Index, double From, double To, double X, double Y, double Width, double Height);

/// <summary>The value pointer (a triangle path) and where its label goes.</summary>
public sealed record LinearGaugePointer(string Path, double LabelX, double LabelY, string Anchor);

/// <summary>A tick: its mark and its label.</summary>
public sealed record LinearGaugeTick(double Value, double X1, double Y1, double X2, double Y2, double LabelX, double LabelY, string Anchor);
