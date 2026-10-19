import { scaleBand } from '../d3/d3-scale.mjs';
import { line, curveMonotoneX } from '../d3/d3-shape.mjs';
import { finite, maxWidth, r2, stride, textWidth, ticks, valueScale } from './_shared.mjs';

const EMPTY = { plot: null, xTicks: [], yTicks: [], y2Ticks: [], bars: [], lines: [] };

// spec: { labels, bars: [{ name, data }], lines: [{ name, data }], stackBars, smooth, secondaryAxis, yLabelPad, y2LabelPad, width, height }
// One category per label. Bar series are grouped side by side in each category, or stacked (positive
// values up from zero, negative ones down) with stackBars. Line series join the category centres;
// with secondaryAxis they get their own scale, labelled right of the plot, else they share the bars'.
// Value i of a series belongs to label i; values that are not finite numbers are skipped.
// yLabelPad and y2LabelPad (default 0) are the characters the component's tick formats add to D3's
// labels on the left and right axes; the margins grow by that much.
export default function combo({ labels, bars = [], lines = [], stackBars = false, smooth = false, secondaryAxis = false, yLabelPad = 0, y2LabelPad = 0, width, height }) {
    const n = (labels ?? []).length;
    if (n === 0) {
        return EMPTY;
    }
    const at = (s, i) => (finite(s?.data?.[i]) ? s.data[i] : null);
    const barSeries = bars ?? [];
    const lineSeries = lines ?? [];
    const index = Array.from({ length: n }, (_, i) => i);

    // Stacks: where each segment starts and ends, positive and negative values on their own sides.
    const segments = [];
    index.forEach(i => {
        let up = 0, down = 0;
        barSeries.forEach((s, k) => {
            const v = at(s, i);
            if (v === null) {
                return;
            }
            const from = !stackBars ? 0 : v >= 0 ? up : down;
            if (stackBars) {
                v >= 0 ? (up += v) : (down += v);
            }
            segments.push({ series: k, index: i, value: v, from, to: from + v });
        });
    });

    const lineValues = lineSeries.flatMap(s => index.map(i => at(s, i))).filter(v => v !== null);
    const y = valueScale([...segments.map(s => s.to), ...(secondaryAxis ? [] : lineValues)], { zero: true, count: 5 });
    // No right axis without line values to put on it.
    const y2 = secondaryAxis && lineValues.length > 0 ? valueScale(lineValues, { zero: true, count: 5 }) : null;

    // Margins sized to the tick labels on each side. The component labels the ticks again with its
    // own formats, which add yLabelPad / y2LabelPad characters to D3's labels at most.
    const labelWidth = (scale, pad) => Math.ceil(maxWidth(ticks(scale, 5).map(t => t.label)) + textWidth('0'.repeat(Math.max(0, pad | 0))));
    const left = labelWidth(y, yLabelPad) + 16;
    const right = width - (y2 ? labelWidth(y2, y2LabelPad) + 20 : 8);
    const plot = { left, top: 10, right, bottom: height - 24 };
    y.range([plot.bottom, plot.top]);
    y2?.range([plot.bottom, plot.top]);

    const band = scaleBand().domain(index).range([plot.left, plot.right]).paddingInner(0.25).paddingOuter(0.1);
    const slot = scaleBand().domain(barSeries.map((_, k) => k)).range([0, band.bandwidth()]).padding(0.08);
    const centre = i => band(i) + band.bandwidth() / 2;

    const placed = segments.map(s => {
        const x = stackBars ? band(s.index) : band(s.index) + slot(s.series);
        const w = stackBars ? band.bandwidth() : slot.bandwidth();
        const a = y(s.from), b = y(s.to);
        return { series: s.series, index: s.index, value: s.value, x: r2(x), y: r2(Math.min(a, b)), width: r2(w), height: r2(Math.abs(b - a)) };
    });

    const scale = y2 ?? y;
    const path = line().defined(p => p.value !== null).x(p => p.cx).y(p => p.cy);
    if (smooth) {
        path.curve(curveMonotoneX);
    }
    const placedLines = lineSeries.map((s, k) => {
        const points = index.map(i => {
            const v = at(s, i);
            return { index: i, value: v, cx: r2(centre(i)), cy: v === null ? 0 : r2(scale(v)) };
        });
        return { series: k, path: path(points) ?? '', points: points.filter(p => p.value !== null) };
    });

    // Category labels thin out when they would collide; the full name stays in the tooltip.
    const every = stride(labels, band.step(), 8);
    const xTicks = labels.map((label, i) => ({ pos: r2(centre(i)), label: i % every === 0 ? String(label ?? '') : '', full: String(label ?? '') }));

    return {
        plot,
        xTicks,
        yTicks: ticks(y, 5),
        y2Ticks: y2 ? ticks(y2, 5) : [],
        bars: placed,
        lines: placedLines,
    };
}
