using System.Collections.Generic;

namespace DaisyBlazor.Charts;

/// <summary>
/// A single categorical data point: a <paramref name="Label"/>, its numeric
/// <paramref name="Value"/>, and an optional explicit <paramref name="Color"/> that overrides
/// the cycling theme palette. Used by bar, pie and donut charts.
/// </summary>
/// <param name="Label">Category / slice label shown in axes, legends and tooltips.</param>
/// <param name="Value">Numeric value (must be finite; negatives are treated as their magnitude for circular charts).</param>
/// <param name="Color">Optional CSS color override (e.g. "var(--color-primary)", "#f00"). Falls back to the palette when null.</param>
public record ChartDataPoint(string Label, double Value, string? Color = null);

/// <summary>A named ring of a multi-ring <see cref="DonutChart"/> (or <see cref="PieChart"/>).</summary>
/// <param name="Name">Name of the ring, shown in its slices' tooltips.</param>
/// <param name="Data">The ring's slices; a label used in several rings keeps one colour.</param>
public sealed record DonutRing(string Name, IReadOnlyList<ChartDataPoint> Data);

/// <summary>One pie of a <see cref="MultiPieChart"/>.</summary>
/// <param name="Name">Name of the pie, shown above it and used as its accessible name.</param>
/// <param name="Data">The pie's slices; a label used in several pies keeps one colour.</param>
public sealed record PieGroup(string Name, IReadOnlyList<ChartDataPoint> Data);

/// <summary>
/// A named numeric series for multi-series line, area and bar charts. The ordinal index of each
/// value lines up with the chart's X labels.
/// </summary>
public class ChartSeries
{
    /// <summary>Series display name (legend / tooltip label).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Ordered values; index <c>i</c> aligns with X label <c>i</c>.</summary>
    public IReadOnlyList<double> Data { get; set; } = new List<double>();

    /// <summary>Optional CSS color override; falls back to the cycling palette when null.</summary>
    public string? Color { get; set; }

    /// <summary>
    /// Optional stack name for a stacked <see cref="BarChart"/>: series with the same name stack
    /// together and different stacks stand side by side in each category. Series without one
    /// share a single stack. Ignored by other charts and when the bars are not stacked.
    /// </summary>
    public string? Stack { get; set; }

    /// <summary>Creates an empty series.</summary>
    public ChartSeries()
    {
    }

    /// <summary>Creates a named series from a sequence of values.</summary>
    public ChartSeries(string name, IReadOnlyList<double> data, string? color = null)
    {
        Name = name;
        Data = data;
        Color = color;
    }
}

/// <summary>
/// Resolves chart colors as daisyUI v5 theme tokens. daisyUI exposes each theme color as a CSS
/// custom property (e.g. <c>--color-primary</c>), so returning <c>var(--color-primary)</c> makes
/// every chart follow the active <c>data-theme</c> automatically — no recompilation, no JS.
/// </summary>
public static class ChartPalette
{
    /// <summary>
    /// The default cycling order: primary, secondary, accent, info, success, warning, error.
    /// Charts index into this (modulo length) so series/slices get distinct, theme-driven colors.
    /// </summary>
    public static readonly IReadOnlyList<string> Default = new[]
    {
        "var(--color-primary)",
        "var(--color-secondary)",
        "var(--color-accent)",
        "var(--color-info)",
        "var(--color-success)",
        "var(--color-warning)",
        "var(--color-error)"
    };

    /// <summary>
    /// A neutral grey for marks: the theme's text colour mixed 60 % into the page background. Unlike
    /// <c>var(--color-neutral)</c>, which is near the card colour in dark themes, it stays visible
    /// on <c>base-100</c> in light and dark themes alike.
    /// </summary>
    public const string Neutral = "color-mix(in oklab, var(--color-base-content) 60%, var(--color-base-100))";

    /// <summary>
    /// CSS colour for a semantic <see cref="Color"/>, or null for Default/Inherit. <see cref="Color.Dark"/>
    /// is <see cref="Neutral"/> and <see cref="Color.Surface"/> a lighter grey (the text colour mixed
    /// 35 % into <c>base-100</c>), so both show on the chart background in light and dark themes;
    /// daisyUI's <c>neutral</c> token would nearly vanish on dark cards.
    /// </summary>
    public static string? FromColor(Color color) => color switch
    {
        Color.Primary => "var(--color-primary)",
        Color.Secondary => "var(--color-secondary)",
        Color.Tertiary => "var(--color-accent)",
        Color.Info => "var(--color-info)",
        Color.Success => "var(--color-success)",
        Color.Warning => "var(--color-warning)",
        Color.Error => "var(--color-error)",
        Color.Dark => Neutral,
        Color.Surface => "color-mix(in oklab, var(--color-base-content) 35%, var(--color-base-100))",
        _ => null
    };

    /// <summary>
    /// A readable text colour on top of <paramref name="color"/>: the matching <c>-content</c> token
    /// for a daisyUI theme colour (<c>var(--color-primary)</c> → <c>var(--color-primary-content)</c>),
    /// else <paramref name="fallback"/>, since the content colour of any other CSS colour is unknown.
    /// </summary>
    public static string ContentOf(string color, string fallback = "var(--color-base-content)") =>
        color.StartsWith("var(--color-", StringComparison.Ordinal) && color.EndsWith(')')
            && !color.StartsWith("var(--color-base", StringComparison.Ordinal) && !color.EndsWith("-content)", StringComparison.Ordinal)
            ? color[..^1] + "-content)"
            : fallback;

    /// <summary>
    /// Returns <paramref name="overrideColor"/> when provided; otherwise the palette entry at
    /// <paramref name="index"/> (wrapping with modulo). Safe for any non-negative index.
    /// </summary>
    public static string At(int index, string? overrideColor = null)
    {
        if (!string.IsNullOrWhiteSpace(overrideColor))
        {
            return overrideColor!;
        }

        if (Default.Count == 0)
        {
            return Neutral;
        }

        int i = index % Default.Count;
        if (i < 0)
        {
            i += Default.Count;
        }

        return Default[i];
    }
}
