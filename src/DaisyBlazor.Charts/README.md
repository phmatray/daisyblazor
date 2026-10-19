# DaisyBlazor.Charts

Theme-aware SVG charts for Blazor, with no NuGet or npm dependency.

Every chart is rendered as inline `<svg>` by its Razor component. The core charts (line, area, bar,
pie, donut, sparkline) compute their geometry in C# and need no JavaScript, so they also render
under static server rendering. The advanced charts (Sankey, and more to come) compute their layout
with a D3 module bundled in the package (`_content/DaisyBlazor.Charts/charts.mjs`, loaded on first
use); until it answers, and under static rendering, they show a placeholder of the same size. Colors are
emitted as daisyUI v5 CSS custom properties (`var(--color-primary)`, `var(--color-secondary)`,
`var(--color-accent)`, …), so charts **automatically follow the active `data-theme`** with no
recompilation. The only project reference is `DaisyBlazor.Components` (for the public `Color`
enum and the `CssClass` class-merge helper).

## Components

| Component | Renders | Key parameters |
|-----------|---------|----------------|
| `LineChart` | One or more line series over a gridded axis | `Series`, `Labels`, `Area`, `Smooth`, `ShowGrid`, `ShowPoints`, `ShowLegend` |
| `AreaChart` | Filled line area (LineChart with `Area=true`, for discoverability) | `Series`, `Labels`, `Smooth`, `ShowGrid`, `ShowPoints`, `ShowLegend` |
| `BarChart` | Vertical or horizontal bars (flat or grouped) | `Data`, `Series`, `Labels`, `Horizontal`, `ShowGrid`, `ShowLegend` |
| `PieChart` | Pie slices with legend + percentages | `Data`, `ShowLegend`, `InnerRadiusRatio`, `CenterLabel` |
| `DonutChart` | Donut (pie with a hole) + optional center label | `Data`, `ShowLegend`, `InnerRadiusRatio`, `CenterLabel` |
| `Sparkline` | Tiny axis-less inline trend line | `Data`, `Area`, `ShowEndDot`, `StrokeWidth`, `Color` |
| `SankeyChart` (D3) | Flows between named stages, bands as wide as their value | `Links`, `NodeColors`, `NodeWidth`, `NodePadding`, `Align`, `ShowValues`, `ValueFormat` |

D3-backed charts take `AspectRatio` (width / height, default 16:9) instead of `Height`.
All chart components also accept `Width` (default `"100%"`), `Height` (default `"16rem"`;
Sparkline `"1.5rem"`), `Title`, and the standard `Class` / `Style` / unmatched-attribute splat.

## Models

```csharp
// A single categorical point (bar / pie / donut). Color is an optional per-point override.
public record ChartDataPoint(string Label, double Value, string? Color = null);

// A named numeric series (line / area / grouped bar).
public class ChartSeries
{
    public string Name { get; set; }
    public IReadOnlyList<double> Data { get; set; }
    public string? Color { get; set; }
}
```

`ChartPalette` resolves colors as theme tokens and cycles
`primary → secondary → accent → info → success → warning → error`. Per-point/series `Color`
overrides take precedence; pass any CSS color (`"var(--color-info)"`, `"#0af"`, `"oklch(...)"`).

## Usage

```razor
@using DaisyBlazor.Charts

<LineChart Title="Traffic"
           Labels="@(new[] { "Mon", "Tue", "Wed", "Thu", "Fri" })"
           Series="@_series"
           Smooth Area />

<BarChart Data="@(new[]
          {
              new ChartDataPoint("A", 12),
              new ChartDataPoint("B", 28),
              new ChartDataPoint("C", 19)
          })" />

<DonutChart Data="@_breakdown" CenterLabel="100%" />

<Sparkline Data="@(new double[] { 3, 5, 4, 8, 6, 9 })" Area />

@code {
    private readonly ChartSeries[] _series =
    {
        new("Visitors", new double[] { 30, 40, 35, 50, 49 }),
        new("Signups",  new double[] {  5,  8,  6, 12, 10 })
    };

    private readonly ChartDataPoint[] _breakdown =
    {
        new("Direct", 40),
        new("Search", 35),
        new("Social", 25)
    };
}
```

## Notes & geometry

- Charts use a fixed user-space viewBox scaled responsively (`preserveAspectRatio`), so they
  stay crisp at any size.
- Value axes include a zero baseline; flat or empty data is handled without divide-by-zero.
- Pie/donut arcs are built from cumulative angles (0 = 12 o'clock, clockwise) using SVG `A`
  arc commands; a single full-circle slice is nudged to avoid a degenerate zero-length arc.
- `Smooth` line mode uses Catmull-Rom-to-Bézier conversion.
- Every chart sets `role="img"` and an accessible `<title>` (from `Title` when provided).
