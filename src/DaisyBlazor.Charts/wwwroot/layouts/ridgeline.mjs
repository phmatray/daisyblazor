import { max } from '../d3/d3-array.mjs';
import { area, curveBasis } from '../d3/d3-shape.mjs';
import { valueScale, r2 } from './_shared.mjs';
import { frame, kde, sorted, steps, ticks } from './_distribution.mjs';

const SAMPLES = 64;

// spec: { groups: [{ name, values }], width, height, valueTitle, bandwidth, overlap }
// One density curve per group (Gaussian kernel; bandwidth in value units, 0 = Silverman's rule),
// stacked top to bottom on a shared value axis. The tallest peak rises `overlap` rows above its
// baseline, so curves may overlap the rows above them. Returns each curve's filled area and its outline.
export default function ridgeline({ groups, width, height, valueTitle, bandwidth = 0, overlap = 1.5 }) {
    const valid = (groups ?? [])
        .map((g, index) => ({ name: String(g.name ?? ''), index, values: sorted(g.values) }))
        .filter(g => g.values.length > 0);
    const rise = Number.isFinite(overlap) && overlap > 0 ? overlap : 1.5;
    const x = valueScale(valid.flatMap(g => [g.values[0], g.values[g.values.length - 1]]));
    const plot = frame({ width, height, yLabels: valid.map(g => g.name), xTitle: valueTitle, yTitle: false });
    x.range([plot.left, plot.right]);
    const xTicks = ticks(x, (plot.right - plot.left) / 80);
    if (valid.length === 0) {
        return { plot, xTicks: [], yTicks: [], groups: [] };
    }

    // Rows are evenly spaced; the first row's baseline leaves room for its peak.
    const step = (plot.bottom - plot.top) / (valid.length - 1 + rise);
    const at = steps(...x.domain(), SAMPLES);
    const curves = valid.map(g => kde(g.values, bandwidth, at));
    const peak = max(curves, c => max(c)) || 1;

    const out = valid.map((g, i) => {
        const base = plot.top + (i + rise) * step;
        const shape = area().x(v => x(v)).y0(base).y1((_, k) => base - (curves[i][k] / peak) * rise * step).curve(curveBasis).digits(2);
        return { name: g.name, index: g.index, count: g.values.length, baseline: r2(base), path: shape(at), line: shape.lineY1()(at) };
    });
    return { plot, xTicks, yTicks: out.map(g => ({ pos: g.baseline, label: g.name })), groups: out };
}
