import { scaleBand } from '../d3/d3-scale.mjs';
import { LABEL_ROW, finite, r2, textWidth, maxWidth, fit, compact, valueScale, ticks, referenceMarks, referenceValues } from './_shared.mjs';

// Bar chart, flat or grouped. spec: { categories: [string], series: [[number|null]], horizontal,
// showGrid, width, height } (px); a flat chart is one series. Bars run from the zero line to their
// value in either direction; null values draw no bar. The value ticks and the category labels go
// on the x or the y axis, depending on the direction. Long category names are cut with an
// ellipsis (full keeps the whole name). stacked puts the series in one bar per category, positive
// values stacking up from zero and negative ones down, so mixed signs diverge; stacks (one number
// per series, default 0) splits them into stacks standing side by side in the category. percent
// (implies stacked) turns each value into its share of its stack's total of absolute values, in %.
// references ([{ value, label }], taken into the value axis) and bands ([{ from, to, label }]) add
// reference lines and value bands (see referenceMarks); the result then carries `references` and `bands`.
export default function bar({ categories = [], series = [], horizontal = false, showGrid = true, stacked = false, percent = false, stacks = [], references, bands, width, height }) {
    stacked ||= percent;
    const stackOf = s => (stacked ? stacks[s] ?? 0 : s);
    // [from, to] per series and category, null for no bar; unstacked bars start at zero.
    const spans = series.map(data => categories.map((_, c) => (finite(data[c]) ? [0, data[c]] : null)));
    if (stacked) {
        for (const g of new Set(series.map((_, s) => stackOf(s)))) {
            const members = spans.filter((_, s) => stackOf(s) === g);
            categories.forEach((_, c) => {
                const total = percent ? members.reduce((sum, span) => sum + (span[c] ? Math.abs(span[c][1]) : 0), 0) : 1;
                let up = 0;
                let down = 0;
                for (const span of members.filter(m => m[c])) {
                    const v = total ? span[c][1] / total * (percent ? 100 : 1) : 0;
                    span[c] = v < 0 ? [down, down += v] : [up, up += v];
                    if (percent) {
                        // Shares can sum to 100.00000000000001, which would stretch the nice axis to 120%.
                        span[c] = span[c].map(end => Math.max(-100, Math.min(100, end)));
                    }
                }
            });
        }
    }
    const values = [...(stacked ? spans.flat().filter(Boolean).flat() : series.flat().filter(finite)), ...referenceValues(references)];
    const tickText = percent ? v => `${compact(v)}%` : compact;
    const top = 8;
    let left;
    let right;
    let bottom;
    let value;
    if (horizontal) {
        // Category names down the side (the gutter takes at most 40% of the width), values below.
        const gutter = Math.min(maxWidth(categories), width * 0.4);
        left = categories.length > 0 ? gutter + 6 : 0;
        bottom = height - (showGrid ? LABEL_ROW : 4);
        value = valueScale(values, { zero: true, count: 5 });
        const lastTick = showGrid ? ticks(value, 5, tickText).at(-1)?.label ?? '' : '';
        right = width - Math.max(6, textWidth(lastTick) / 2 + 2);
        value.range([left, right]);
    } else {
        bottom = height - (categories.length > 0 ? LABEL_ROW : 4);
        value = valueScale(values, { zero: true, count: 5 }).range([bottom, top]);
        const tickLabels = showGrid ? ticks(value, 5, tickText) : [];
        left = tickLabels.length > 0 ? Math.max(...tickLabels.map(t => textWidth(t.label))) + 6 : 0;
        right = width - 6;
    }

    const band = scaleBand()
        .domain([...categories.keys()])
        .range(horizontal ? [top, bottom] : [left, right])
        .paddingInner(0.25)
        .paddingOuter(0.125);
    // Side-by-side stacks get a small gap; grouped bars touch, as before.
    const inner = scaleBand().domain([...new Set(series.map((_, s) => stackOf(s)))]).range([0, band.bandwidth()]).paddingInner(stacked ? 0.08 : 0);

    const bars = [];
    spans.forEach((span, s) => categories.forEach((_, c) => {
        if (!span[c]) {
            return;
        }
        const [from, to] = span[c].map(v => value(v));
        const start = band(c) + inner(stackOf(s));
        const lo = Math.min(from, to);
        const length = Math.abs(to - from);
        bars.push(horizontal
            ? { c, s, x: r2(lo), y: r2(start), width: r2(length), height: r2(inner.bandwidth()) }
            : { c, s, x: r2(start), y: r2(lo), width: r2(inner.bandwidth()), height: r2(length) });
    }));

    const room = horizontal ? left - 6 : band.step() - 4;
    const valueTicks = showGrid ? ticks(value, 5, tickText) : [];
    const categoryTicks = categories.map((label, c) => ({
        pos: r2(band(c) + band.bandwidth() / 2),
        label: fit(label, room),
        full: String(label ?? ''),
    }));
    const plot = { left: r2(left), top, right: r2(right), bottom: r2(bottom) };
    const out = {
        plot,
        xTicks: horizontal ? valueTicks : categoryTicks,
        yTicks: horizontal ? categoryTicks : valueTicks,
        bars,
    };
    return references?.length || bands?.length ? { ...out, ...referenceMarks(value, horizontal, plot, references, bands) } : out;
}
