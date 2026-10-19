import { scaleBand } from '../d3.mjs';
import { LABEL_ROW, labelBaseline, isFiniteNumber, round, textWidth, fit, valueScale, ticks } from './_axis.mjs';

// Bar chart, flat or grouped. spec: { categories: [string], series: [[number|null]], horizontal,
// showGrid, width, height } (px); a flat chart is one series. Bars run from the zero line to their
// value in either direction; null values draw no bar. pos is the y (vertical bars) or x
// (horizontal bars) of a value tick or a category label; labelAt is the other coordinate of the
// category labels. Long category names are cut with an ellipsis (full keeps the whole name).
export default function bar({ categories = [], series = [], horizontal = false, showGrid = true, width, height }) {
    const values = series.flat().filter(isFiniteNumber);
    const top = 8;
    let left;
    let right;
    let bottom;
    let value;
    if (horizontal) {
        // Category names down the side (the gutter takes at most 40% of the width), values below.
        const gutter = Math.min(Math.max(0, ...categories.map(textWidth)), width * 0.4);
        left = categories.length > 0 ? gutter + 6 : 0;
        bottom = height - (showGrid ? LABEL_ROW : 4);
        value = valueScale(values, [left, 0]);
        const lastTick = showGrid ? ticks(value).at(-1)?.label ?? '' : '';
        right = width - Math.max(6, textWidth(lastTick) / 2 + 2);
        value.range([left, right]);
    } else {
        bottom = height - (categories.length > 0 ? LABEL_ROW : 4);
        value = valueScale(values, [bottom, top]);
        const tickLabels = showGrid ? ticks(value) : [];
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
        if (!isFiniteNumber(v)) {
            return;
        }
        const start = band(c) + inner(s);
        const lo = Math.min(zero, value(v));
        const length = Math.abs(value(v) - zero);
        bars.push(horizontal
            ? { c, s, x: round(lo), y: round(start), width: round(length), height: round(inner.bandwidth()) }
            : { c, s, x: round(start), y: round(lo), width: round(inner.bandwidth()), height: round(length) });
    }));

    const room = horizontal ? left - 6 : band.step() - 4;
    return {
        plot: { left: round(left), top, right: round(right), bottom: round(bottom) },
        valueTicks: showGrid ? ticks(value).map(t => ({ pos: round(value(t.value)), label: t.label })) : [],
        categoryLabels: categories.map((label, c) => ({
            pos: round(band(c) + band.bandwidth() / 2),
            label: fit(label, room),
            full: String(label ?? ''),
        })),
        labelAt: round(horizontal ? left - 6 : labelBaseline(bottom)),
        valueLabelAt: round(horizontal ? labelBaseline(bottom) : left - 6),
        bars,
    };
}
