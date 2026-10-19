import { scaleBand } from '../d3.mjs';
import { LABEL_ROW, finite, r2, textWidth, maxWidth, fit, compact, valueScale, ticks } from './_shared.mjs';

// Bar chart, flat or grouped. spec: { categories: [string], series: [[number|null]], horizontal,
// showGrid, width, height } (px); a flat chart is one series. Bars run from the zero line to their
// value in either direction; null values draw no bar. The value ticks and the category labels go
// on the x or the y axis, depending on the direction. Long category names are cut with an
// ellipsis (full keeps the whole name).
export default function bar({ categories = [], series = [], horizontal = false, showGrid = true, width, height }) {
    const values = series.flat().filter(finite);
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
        const lastTick = showGrid ? ticks(value, 5, compact).at(-1)?.label ?? '' : '';
        right = width - Math.max(6, textWidth(lastTick) / 2 + 2);
        value.range([left, right]);
    } else {
        bottom = height - (categories.length > 0 ? LABEL_ROW : 4);
        value = valueScale(values, { zero: true, count: 5 }).range([bottom, top]);
        const tickLabels = showGrid ? ticks(value, 5, compact) : [];
        left = tickLabels.length > 0 ? Math.max(...tickLabels.map(t => textWidth(t.label))) + 6 : 0;
        right = width - 6;
    }

    const band = scaleBand()
        .domain([...categories.keys()])
        .range(horizontal ? [top, bottom] : [left, right])
        .paddingInner(0.25)
        .paddingOuter(0.125);
    const inner = scaleBand().domain([...series.keys()]).range([0, band.bandwidth()]);
    const zero = value(0);

    const bars = [];
    series.forEach((data, s) => categories.forEach((_, c) => {
        const v = data[c];
        if (!finite(v)) {
            return;
        }
        const start = band(c) + inner(s);
        const lo = Math.min(zero, value(v));
        const length = Math.abs(value(v) - zero);
        bars.push(horizontal
            ? { c, s, x: r2(lo), y: r2(start), width: r2(length), height: r2(inner.bandwidth()) }
            : { c, s, x: r2(start), y: r2(lo), width: r2(inner.bandwidth()), height: r2(length) });
    }));

    const room = horizontal ? left - 6 : band.step() - 4;
    const valueTicks = showGrid ? ticks(value, 5, compact) : [];
    const categoryTicks = categories.map((label, c) => ({
        pos: r2(band(c) + band.bandwidth() / 2),
        label: fit(label, room),
        full: String(label ?? ''),
    }));
    return {
        plot: { left: r2(left), top, right: r2(right), bottom: r2(bottom) },
        xTicks: horizontal ? valueTicks : categoryTicks,
        yTicks: horizontal ? categoryTicks : valueTicks,
        bars,
    };
}
