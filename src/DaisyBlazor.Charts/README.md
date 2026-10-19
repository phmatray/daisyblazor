# DaisyBlazor.Charts

Theme-aware SVG charts for Blazor, with no NuGet or npm dependency.

Every chart is rendered as inline `<svg>` by its Razor component, and every chart computes its
geometry (scales, nice ticks, paths, arcs, stacking, bands) with a D3 module bundled in the package
(`_content/DaisyBlazor.Charts/charts.mjs`, loaded on first use). The charts therefore need
JavaScript: until the module answers, and under static server rendering, each chart shows a
placeholder of the same size. Colors are
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

The other 39 types, by family (each has a docs page with live examples and its parameters):

- **Hierarchy and flow:** `TreemapChart`, `SunburstChart`, `CirclePackingChart`, `IcicleChart`, `SankeyChart`, `ChordChart`, `NetworkChart`, `ArcDiagramChart`
- **Distribution:** `ScatterChart` (bubbles, connected points, trend lines), `ContourChart`, `HexbinChart`, `BoxPlotChart`, `ViolinChart`, `HistogramChart`, `BeeswarmChart`, `RidgelineChart`
- **Over time:** `CalendarHeatmap`, `HeatmapChart`, `RangeAreaChart`, `StreamChart`, `CandlestickChart` (candle, OHLC, high-low, close line or area, Heikin-Ashi, EquiVolume, CandleVolume; SMA, EMA, Bollinger, price channel, RSI, MACD; volume), `PriceActionChart` (Renko, Kagi, line break, point and figure), `GanttChart`, `BumpChart`, `RangeSelector` (pick a window of time for a chart)
- **Compare:** `RadarChart`, `BulletChart`, `DumbbellChart`, `SlopeChart`, `LollipopChart`, `ParallelCoordinatesChart`, `RangeBarChart`, `ComboChart` (bars and lines, optional right axis)
- **Composition and progress:** `WaterfallChart`, `FunnelChart` (and pyramid), `GaugeChart`, `LinearGaugeChart`, `RadialBarChart`, `PolarAreaChart`, `ParetoChart`, `MarimekkoChart`, `WaffleChart`

The core charts (line, area, bar, pie, donut, sparkline) take a CSS `Width` (default `"100%"`;
Sparkline `"5rem"`) and `Height` (default `"16rem"`; Sparkline `"1.5rem"`). Line, area, bar and
sparkline are laid out at the real pixel size of that box and again when it is resized, so their
text and strokes are drawn at true size; pie and donut scale a square to their `Height`. The other
charts (Sankey and more) take `AspectRatio` (width / height, default 16:9) instead of `Height`. All
charts also accept `Title` and the standard `Class` / `Style` / unmatched-attribute splat.

## Server rendering

Add `DaisyBlazor.Charts.Server` and call `builder.Services.AddDaisyBlazorChartsServerRendering()` to
lay out every chart on the server (with Jint, no Node): prerendered and statically rendered pages then
contain the finished SVG instead of a placeholder.

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

- The layouts live in `wwwroot/layouts/` (`line`, `bar`, `pie`, `spark`, `sankey`, …) and return
  plain data; the Razor components draw it with theme colors.
- Value axes include a zero baseline and round out to nice ticks with compact labels (`50k`);
  flat or empty data is handled without divide-by-zero, and NaN/infinite values leave a gap.
- Pie/donut slices come from `d3.pie`/`d3.arc` in data order, from 12 o'clock, clockwise.
- `Smooth` line mode uses monotone cubic curves (`d3.curveMonotoneX`), which never overshoot.
- Every chart sets `role="img"` and an accessible `<title>` (from `Title` when provided).
