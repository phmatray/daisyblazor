import { line, scaleBand, scaleLinear } from '../d3.mjs';

// spec: { items: [{ label, value }], threshold, width, height }
// Bars sorted by value (largest first) on the left axis, the cumulative share as a line on a
// 0–100 % right axis, and a reference line at threshold (0.8 by default). Only positive values count.
export default function pareto({ items, threshold = 0.8, width, height }) {
    const valid = (items ?? [])
        .map((d, i) => ({ label: d?.label, value: d?.value, i }))
        .filter(d => Number.isFinite(d.value) && d.value > 0)
        .sort((a, b) => b.value - a.value || a.i - b.i);
    if (valid.length === 0) {
        return { bars: [], line: '', ticks: [], shareTicks: [], plot: null, thresholdY: 0, vitalFew: 0 };
    }

    const t = Number.isFinite(threshold) ? Math.min(1, Math.max(0, threshold)) : 0.8;
    const plot = { left: 48, top: 12, right: width - 46, bottom: height - 26 };
    const total = valid.reduce((s, d) => s + d.value, 0);
    const x = scaleBand().domain(valid.map((_, i) => i)).range([plot.left, plot.right]).paddingInner(0.25).paddingOuter(0.1);
    const y = scaleLinear().domain([0, valid[0].value]).nice().range([plot.bottom, plot.top]);
    const share = scaleLinear().domain([0, 1]).range([plot.bottom, plot.top]);

    let running = 0;
    const bars = valid.map((d, i) => {
        running += d.value;
        const cumulative = running / total;
        return {
            label: d.label,
            value: d.value,
            share: d.value / total,
            cumulative,
            x: x(i),
            y: y(d.value),
            width: x.bandwidth(),
            height: plot.bottom - y(d.value),
            cx: x(i) + x.bandwidth() / 2,
            cy: share(cumulative),
        };
    });

    // The "vital few": every bar up to and including the one where the cumulative share reaches the threshold.
    const vitalFew = bars.findIndex(b => b.cumulative >= t - 1e-9) + 1;

    return {
        bars,
        line: line().x(b => b.cx).y(b => b.cy)(bars),
        ticks: y.ticks(5).map(v => ({ value: v, pos: y(v) })),
        shareTicks: share.ticks(5).map(v => ({ value: v, pos: share(v) })),
        plot,
        thresholdY: share(t),
        vitalFew,
    };
}
