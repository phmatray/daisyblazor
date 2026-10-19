import { scalePoint } from '../d3/d3-scale.mjs';
import { line as d3Line, area as d3Area, stack, stackOffsetExpand, curveMonotoneX, curveLinear, curveStepAfter } from '../d3/d3-shape.mjs';
import { LABEL_ROW, finite, r2, textWidth, fit, stride, compact, valueScale, ticks } from './_shared.mjs';

// Line and area charts. spec: { series: [[number|null]], labels: [string], stacked, smooth,
// showGrid, width, height } (px). Points sit in the middle of equal slots, one per label or value.
// Non-finite values (null) leave a gap; Stacked draws each series on the running total, a missing
// value counting as 0. Returns pixel geometry: the plot box, the x and y ticks (long x labels are
// cut with an ellipsis, full keeps the text) and, per series,
// its line and area paths and its points (isolated = no neighbour to draw a line to). percent
// (implies stacked) draws each value as its share of the index's total, in %, negative values
// counting as 0. step draws step-after segments and wins over smooth.
export default function line({ series = [], labels = [], stacked = false, percent = false, smooth = false, step = false, showGrid = true, width, height }) {
    stacked ||= percent;
    const dataCount = Math.max(0, ...series.map(s => s.length));
    const count = Math.max(dataCount, labels.length);
    const indices = [...Array(dataCount).keys()];

    // [base, top] per series and index; unstacked bands start at zero. Percent ends are capped at
    // 100: d3 can sum the shares to 1.0000000000000002, which would stretch the nice axis to 120%.
    const bands = percent
        ? stack().keys([...series.keys()]).value((i, s) => (finite(series[s][i]) && series[s][i] > 0 ? series[s][i] : 0)).offset(stackOffsetExpand)(indices)
              .map(layer => layer.map(([base, top]) => [base, top].map(end => Math.min(100, end * 100))))
        : stacked
        ? stack().keys([...series.keys()]).value((i, s) => (finite(series[s][i]) ? series[s][i] : 0))(indices)
              .map(layer => layer.map(([base, top]) => [base, top]))
        : series.map(s => indices.map(i => [0, finite(s[i]) ? s[i] : null]));

    const values = bands.flatMap(b => b.map(([, top]) => top)).filter(finite);
    const top = 8;
    const bottom = height - (labels.length > 0 ? LABEL_ROW : 4);
    const y = valueScale(values, { zero: true, count: 5 }).range([bottom, top]);
    const yTicks = showGrid ? ticks(y, 5, percent ? v => `${compact(v)}%` : compact) : [];
    const left = yTicks.length > 0 ? Math.max(...yTicks.map(t => textWidth(t.label))) + 6 : 0;
    const right = width - 6;

    const x = scalePoint().domain([...Array(count).keys()]).range([left, right]).padding(0.5);
    const slot = count > 0 ? (right - left) / count : 0;
    // Thin out the labels on long or narrow axes so they never overlap.
    const every = stride(labels, slot, 8);
    const xTicks = [];
    for (let i = 0; i < labels.length; i += every) {
        xTicks.push({ pos: r2(x(i)), label: fit(labels[i], slot * every - 4), full: String(labels[i] ?? '') });
    }

    const curve = step ? curveStepAfter : smooth ? curveMonotoneX : curveLinear;
    return {
        plot: { left: r2(left), top, right: r2(right), bottom: r2(bottom) },
        xTicks,
        yTicks,
        series: bands.map((band, s) => {
            const defined = i => finite(band[i][1]);
            const line = d3Line().defined(defined).x(i => x(i)).y(i => y(band[i][1])).curve(curve).digits(2);
            const area = d3Area().defined(defined).x(i => x(i)).y0(i => y(band[i][0])).y1(i => y(band[i][1])).curve(curve).digits(2);
            const own = series[s];
            return {
                line: line(indices) ?? '',
                area: area(indices) ?? '',
                points: indices.filter(i => finite(own[i])).map(i => ({
                    index: i,
                    x: r2(x(i)),
                    y: r2(y(band[i][1])),
                    isolated: !(i > 0 && defined(i - 1)) && !(i < dataCount - 1 && defined(i + 1)),
                })),
            };
        }),
    };
}
