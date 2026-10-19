import { format } from '../d3/d3-format.mjs';
import { scaleLinear } from '../d3/d3-scale.mjs';
import { stack, area, curveBasis, curveMonotoneX, stackOffsetWiggle, stackOffsetNone, stackOffsetExpand, stackOrderInsideOut, stackOrderNone } from '../d3/d3-shape.mjs';
import { finite, r2, textWidth, ticks } from './_shared.mjs';
import { xScale, yAxisWidth } from './_time.mjs';

const OFFSET = { wiggle: stackOffsetWiggle, stacked: stackOffsetNone, percent: stackOffsetExpand };
const EMPTY = { layers: [], columns: [], xTicks: [], yTicks: [], plot: { left: 0, top: 0, right: 0, bottom: 0 } };

// spec: { series: [{ name, values }], dates, labels, offset: 'wiggle' | 'stacked' | 'percent', width, height }
// values[i] lines up with dates[i] (a time axis, in order) or labels[i] (a category axis). Missing,
// negative or non-finite values count as 0. The wiggle offset (a streamgraph) has no meaningful
// y scale, so it gets no y axis; its layers are ordered inside-out to keep the stream balanced.
export default function stream({ series, dates, labels, offset = 'wiggle', width, height }) {
    const named = (series ?? []).filter(s => s && Array.isArray(s.values));
    const count = Math.max(0, ...named.map(s => s.values.length));
    if (named.length === 0 || count === 0) {
        return EMPTY;
    }

    const value = (s, i) => (finite(s.values[i]) && s.values[i] > 0 ? s.values[i] : 0);
    const rows = Array.from({ length: count }, (_, i) => named.map(s => value(s, i)));
    const layers = stack()
        .keys(named.map((_, k) => k))
        .order(offset === 'wiggle' ? stackOrderInsideOut : stackOrderNone)
        .offset(OFFSET[offset] ?? stackOffsetWiggle)(rows);

    const lo = Math.min(...layers.flatMap(l => l.map(d => d[0])));
    const hi = Math.max(...layers.flatMap(l => l.map(d => d[1])));
    const y = scaleLinear().domain([lo, hi > lo ? hi : lo + 1]);
    const top = 10, bottom = height - 24;
    const showY = offset !== 'wiggle';
    if (showY) {
        y.nice();
    }
    const yCount = Math.max(2, Math.floor((bottom - top) / 40));
    const yFormat = offset === 'percent' ? format('.0%') : undefined;
    const plot = { left: showY ? yAxisWidth(y, yCount, yFormat) : 12, top, right: width - 24, bottom };
    y.range([plot.bottom, plot.top]);

    const x = xScale({ dates, labels, count, x0: plot.left, x1: plot.right });
    const xs = rows.map((_, i) => x.at(i));
    const shape = area().x((_, i) => xs[i]).y0(d => y(d[0])).y1(d => y(d[1]))
        .curve(offset === 'wiggle' ? curveBasis : curveMonotoneX);

    return {
        // One layer per series, in series order. The label goes at the position where the layer
        // leaves the most room around the text, checked at every data point the text spans, and
        // only when the text fits inside the layer there.
        layers: layers.map(layer => {
            const name = named[layer.key].name ?? '';
            const half = textWidth(name) / 2 + 4;
            // The layer's bottom and top edge at any x, interpolated between data points.
            const edges = px => {
                const j = Math.max(0, Math.min(count - 2, xs.findIndex((x, k) => k === count - 1 || xs[k + 1] >= px)));
                const f = count < 2 ? 0 : Math.min(1, Math.max(0, (px - xs[j]) / (xs[j + 1] - xs[j] || 1)));
                const at = k => layer[Math.min(k, count - 1)];
                return [y(at(j)[0]) * (1 - f) + y(at(j + 1)[0]) * f, y(at(j)[1]) * (1 - f) + y(at(j + 1)[1]) * f];
            };
            let best = null;
            for (let i = 0; i < count; i++) {
                const cx = Math.min(Math.max(xs[i], plot.left + half), plot.right - half);
                const [b, t] = edges(cx);
                const cy = (b + t) / 2;
                const samples = [cx - half, cx - half / 2, cx, cx + half / 2, cx + half, ...xs.filter(x => Math.abs(x - cx) <= half)];
                const room = Math.min(...samples.map(px => {
                    const [bottom, top] = edges(px);
                    return Math.min(bottom - cy, cy - top);
                })) * 2;
                if (!best || room > best.room) {
                    best = { cx, cy, room };
                }
            }
            return {
                index: layer.key, name, path: shape(layer) ?? '',
                labelX: r2(best.cx), labelY: r2(best.cy), showLabel: best.room >= 14,
            };
        }).sort((a, b) => a.index - b.index),
        // One hover strip per x position, holding every series' raw value.
        columns: xs.map((cx, i) => ({
            label: x.label(i),
            x0: r2(i === 0 ? plot.left : (xs[i - 1] + cx) / 2),
            x1: r2(i === count - 1 ? plot.right : (cx + xs[i + 1]) / 2),
            values: named.map(s => value(s, i)),
        })),
        xTicks: x.ticks,
        yTicks: showY ? ticks(y, yCount, yFormat) : [],
        plot,
    };
}
