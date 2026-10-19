# Charts

`DaisyBlazor.Charts` ships **54 chart types**. Every chart is rendered as **inline `<svg>`** by its
Razor component, and every color is emitted as a daisyUI theme token (`var(--color-primary)`, …), so
charts **follow the active `data-theme` automatically**. The geometry (scales, ticks, layouts, paths)
is computed by D3 modules bundled in the package (`_content/DaisyBlazor.Charts/d3/`, trimmed to the D3 code the charts use, plus one small
layout file per chart), loaded on first use: there is nothing to install from npm, and the only NuGet
dependency is `DaisyBlazor.Components`.

In the browser the layouts need JavaScript; until they answer, a chart shows a placeholder of the same
size. To render the finished SVG during prerendering and static server rendering, add the optional
server package, which runs the same layouts in-process with Jint (no Node):

```bash
dotnet add package DaisyBlazor.Charts.Server
```

```csharp
builder.Services.AddDaisyBlazorChartsServerRendering();
```

```bash
dotnet add package DaisyBlazor.Charts
```

```razor
@using DaisyBlazor.Charts
```

## The six components

| Component | Renders | Key parameters |
|-----------|---------|----------------|
| `LineChart` | One or more line series over a gridded, labelled axis | `Series`, `Labels`, `Area`, `Smooth`, `ShowGrid`, `ShowPoints`, `ShowLegend` |
| `AreaChart` | A filled line chart (a `LineChart` with `Area=true`, for discoverability) | `Series`, `Labels`, `Smooth`, `ShowGrid`, `ShowPoints`, `ShowLegend` |
| `BarChart` | Vertical or horizontal bars — flat (`Data`) or grouped (`Series`) | `Data`, `Series`, `Labels`, `Horizontal`, `ShowGrid`, `ShowLegend` |
| `PieChart` | Pie slices with a legend + percentages | `Data`, `ShowLegend`, `InnerRadiusRatio`, `CenterLabel` |
| `DonutChart` | A donut (pie with a hole) + optional center label | `Data`, `ShowLegend`, `InnerRadiusRatio` (default `0.6`), `CenterLabel` |
| `Sparkline` | A tiny, axis-less inline trend line, columns or win/loss bars (table cells, KPI tiles) | `Data`, `Type`, `Area`, `ShowEndDot`, `StrokeWidth`, `Color` |

Every chart also accepts `Title` (heading above the chart; also the accessible label), `Width`
(default `"100%"`), `Height` (default `"16rem"`; `Sparkline` `"1.5rem"`, `Width` `"5rem"`), and the
standard `Class` / `Style` / unmatched-attribute splat.

## Data models

```csharp
// A single categorical point — used by BarChart (flat), PieChart, DonutChart.
// Color is an optional per-point CSS override.
public record ChartDataPoint(string Label, double Value, string? Color = null);

// A named numeric series — used by LineChart, AreaChart, grouped BarChart.
// Data index i lines up with the chart's X label i.
public class ChartSeries
{
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<double> Data { get; set; } = new List<double>();
    public string? Color { get; set; }              // optional CSS override

    public ChartSeries() { }
    public ChartSeries(string name, IReadOnlyList<double> data, string? color = null) { /* … */ }
}
```

`ChartSeries.Data` aligns by **ordinal index** with `Labels`. `ChartDataPoint.Value` must be finite;
for the circular charts, non-positive values are ignored.

## Colors follow the daisyUI theme

Colors come from `ChartPalette`, which returns daisyUI tokens as CSS variables. The default cycling
order is:

```
primary → secondary → accent → info → success → warning → error
```

Charts index into this palette (wrapping with modulo) so each series/slice gets a distinct,
theme-driven color. Because the values are `var(--color-*)`, switching the daisyUI theme re-skins every
chart instantly, with no rebuild and no new layout.

Override the color of an individual series or point by setting its `Color` to **any CSS color**:

```csharp
new ChartSeries("Errors", data, color: "var(--color-error)");   // a theme token
new ChartDataPoint("Other", 12, "#94a3b8");                     // a hex literal
```

`ChartPalette.FromColor(Color)` also maps the DaisyBlazor `Color` enum to its token, so you can drive
chart colors from the same semantic enum used by the components.

## Usage

```razor
@using DaisyBlazor.Charts

<LineChart Title="Weekly visitors"
           Labels="@(new[] { "Mon", "Tue", "Wed", "Thu", "Fri" })"
           Series="@_traffic"
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
    private readonly ChartSeries[] _traffic =
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

- Charts use a fixed user-space `viewBox` scaled responsively (`preserveAspectRatio`), so they stay
  crisp at any rendered size.
- Value axes include a **zero baseline**; flat or empty data is handled without divide-by-zero.
- `BarChart` takes `Series` (grouped) over `Data` (flat) when both are supplied; `Horizontal` flips the
  axis.
- Pie/donut arcs are built from cumulative angles (0 = 12 o'clock, clockwise) with SVG `A` arc
  commands; a single full-circle slice is nudged to avoid a degenerate zero-length arc. Only positive
  values contribute to the total.
- `Smooth` line mode uses a Catmull-Rom-to-Bézier conversion.
- Every chart sets `role="img"` and an accessible `<title>` (from `Title` when provided).
