import { scaleBand } from '../d3.mjs';
import { ticks, valueScale } from './_shared.mjs';

const EMPTY = { bars: [], connectors: [], ticks: [], plot: null, zero: 0 };

// spec: { steps: [{ label, value, kind }], width, height, horizontal }
// kind: 'start' sets the running total to value, 'delta' adds value to it, 'subtotal' and 'end'
// draw the running total so far (their value is ignored). Steps without a finite value are skipped.
export default function waterfall({ steps, width, height, horizontal = false }) {
    const valid = (steps ?? []).filter(s => s && (s.kind === 'subtotal' || s.kind === 'end' || Number.isFinite(s.value)));
    if (valid.length === 0) {
        return EMPTY;
    }

    let total = 0;
    const raw = valid.map(s => {
        const before = total;
        if (s.kind === 'delta') {
            total += s.value;
            return { label: s.label, kind: s.value >= 0 ? 'increase' : 'decrease', value: s.value, from: before, to: total };
        }
        if (s.kind === 'start') {
            total = s.value;
        }
        return { label: s.label, kind: 'total', value: total, from: 0, to: total };
    });

    // Value labels sit past the end of each bar, category labels in the gutter.
    const plot = horizontal
        ? { left: 120, top: 6, right: width - 56, bottom: height - 24 }
        : { left: 48, top: 22, right: width - 8, bottom: height - 26 };
    const band = scaleBand().domain(raw.map((_, i) => i)).paddingInner(0.3).paddingOuter(0.15)
        .range(horizontal ? [plot.top, plot.bottom] : [plot.left, plot.right]);
    const value = valueScale(raw.flatMap(b => [b.from, b.to]), { zero: true })
        .range(horizontal ? [plot.left, plot.right] : [plot.bottom, plot.top]);

    const bars = raw.map((b, i) => {
        const a = value(b.from), z = value(b.to), p = band(i), bw = band.bandwidth();
        if (horizontal) {
            const x = Math.min(a, z), w = Math.max(1, Math.abs(z - a));
            return { ...b, total: b.to, x, y: p, width: w, height: bw, labelX: x + w + 5, labelY: p + bw / 2, anchor: 'start' };
        }
        const y = Math.min(a, z), h = Math.max(1, Math.abs(z - a));
        return { ...b, total: b.to, x: p, y, width: bw, height: h, labelX: p + bw / 2, labelY: y - 6, anchor: 'middle' };
    });

    // A thin rule carries each running total across to the next bar.
    const connectors = bars.slice(0, -1).map((b, i) => {
        const at = value(b.to), next = bars[i + 1];
        return horizontal
            ? { x1: at, y1: b.y + b.height, x2: at, y2: next.y }
            : { x1: b.x + b.width, y1: at, x2: next.x, y2: at };
    });

    return {
        bars,
        connectors,
        ticks: ticks(value, 5),
        plot,
        zero: value(0),
    };
}
