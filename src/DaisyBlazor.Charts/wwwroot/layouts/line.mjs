import { scalePoint } from '../d3/d3-scale.mjs';
import { line as d3Line, area as d3Area, stack, stackOffsetExpand, curveMonotoneX, curveMonotoneY, curveLinear, curveStepAfter, curveStepBefore } from '../d3/d3-shape.mjs';
import { LABEL_ROW, finite, r2, textWidth, maxWidth, fit, stride, compact, valueScale, ticks, referenceRoom, referenceMarks, referenceValues, categoryTrends, categoryMarks, markerRoom } from './_shared.mjs';

// Line and area charts. spec: { series: [[number|null]], labels: [string], stacked, smooth,
// showGrid, width, height } (px). Points sit in the middle of equal slots, one per label or value.
// Non-finite values (null) leave a gap; Stacked draws each series on the running total, a missing
// value counting as 0. Returns pixel geometry: the plot box, the x and y ticks (long x labels are
// cut with an ellipsis, full keeps the text) and, per series,
// its line and area paths and its points (isolated = no neighbour to draw a line to). percent
// (implies stacked) draws each value as its share of the index's total, in %, negative values
// counting as 0. step draws step-after segments and wins over smooth.
// horizontal rotates the chart: the labels run down the y axis (category gutter on the left, at
// most 40% of the width) and the values along x. references ([{ value, label }], taken into the
// value axis) and bands ([{ from, to, label }]) add reference lines and value bands (see
// referenceMarks); the result then carries `references` and `bands`. trendline ('linear',
// 'exponential', 'logarithmic', 'power', 'polynomial' of trendlineOrder, 'movingaverage' of
// trendlinePeriod points) adds `trends`, one per series fitted over the label index on its own
// values (see categoryTrends); it is ignored when stacked or percent. markers ([{ at, to, label }],
// label indices) add lines at a label or shaded spans over labels (see categoryMarks); the result then
// carries `markers` and `markerSpans`.
export default function line({ markers = [],  trendline = 'none', trendlineOrder = 2, trendlinePeriod = 3, series = [], labels = [], stacked = false, percent = false, smooth = false, step = false, showGrid = true, horizontal = false, references, bands: ranges, width, height }) {
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

    const values = [...bands.flatMap(b => b.map(([, top]) => top)).filter(finite), ...referenceValues(references)];
    const tickText = percent ? v => `${compact(v)}%` : compact;
    const marginal = referenceRoom(width, horizontal, references, ranges);
    const marked = markerRoom(width, !horizontal, markers);
    const top = 8 + Math.max(marginal.top, marked.top);
    let plot;
    let xTicks;
    let yTicks;
    let value;
    let place; // index → position along the category axis
    if (horizontal) {
        // Category names down the side, values below, as in a horizontal bar chart.
        const left = labels.length > 0 ? Math.min(maxWidth(labels), width * 0.4) + 6 : 4;
        const bottom = height - (showGrid ? LABEL_ROW : 4);
        value = valueScale(values, { zero: true, count: 5 });
        const lastTick = showGrid ? ticks(value, 5, tickText).at(-1)?.label ?? '' : '';
        const right = width - Math.max(6, textWidth(lastTick) / 2 + 2, marked.right);
        value.range([left, right]);
        place = scalePoint().domain([...Array(count).keys()]).range([top, bottom]).padding(0.5);
        const slot = count > 0 ? (bottom - top) / count : 0;
        const every = Math.max(1, Math.ceil(LABEL_ROW / Math.max(1, slot)));
        yTicks = [];
        for (let i = 0; i < labels.length; i += every) {
            yTicks.push({ pos: r2(place(i)), label: fit(labels[i], left - 6), full: String(labels[i] ?? '') });
        }
        xTicks = showGrid ? ticks(value, 5, tickText) : [];
        plot = { left: r2(left), top, right: r2(right), bottom: r2(bottom) };
    } else {
        const bottom = height - (labels.length > 0 ? LABEL_ROW : 4);
        value = valueScale(values, { zero: true, count: 5 }).range([bottom, top]);
        yTicks = showGrid ? ticks(value, 5, tickText) : [];
        const left = yTicks.length > 0 ? Math.max(...yTicks.map(t => textWidth(t.label))) + 6 : 0;
        const right = width - Math.max(6, marginal.right, marked.right);

        place = scalePoint().domain([...Array(count).keys()]).range([left, right]).padding(0.5);
        const slot = count > 0 ? (right - left) / count : 0;
        // Thin out the labels on long or narrow axes so they never overlap.
        const every = stride(labels, slot, 8);
        xTicks = [];
        for (let i = 0; i < labels.length; i += every) {
            xTicks.push({ pos: r2(place(i)), label: fit(labels[i], slot * every - 4), full: String(labels[i] ?? '') });
        }
        plot = { left: r2(left), top, right: r2(right), bottom: r2(bottom) };
    }

    // Rotated, a step holds its value down to the next label (step-before in drawing order) and
    // smoothing follows y.
    const curve = step ? (horizontal ? curveStepBefore : curveStepAfter) : smooth ? (horizontal ? curveMonotoneY : curveMonotoneX) : curveLinear;
    const out = {
        plot,
        xTicks,
        yTicks,
        series: bands.map((band, s) => {
            const defined = i => finite(band[i][1]);
            const line = horizontal
                ? d3Line().defined(defined).x(i => value(band[i][1])).y(i => place(i)).curve(curve).digits(2)
                : d3Line().defined(defined).x(i => place(i)).y(i => value(band[i][1])).curve(curve).digits(2);
            const area = horizontal
                ? d3Area().defined(defined).y(i => place(i)).x0(i => value(band[i][0])).x1(i => value(band[i][1])).curve(curve).digits(2)
                : d3Area().defined(defined).x(i => place(i)).y0(i => value(band[i][0])).y1(i => value(band[i][1])).curve(curve).digits(2);
            const own = series[s];
            return {
                line: line(indices) ?? '',
                area: area(indices) ?? '',
                points: indices.filter(i => finite(own[i])).map(i => ({
                    index: i,
                    x: r2(horizontal ? value(band[i][1]) : place(i)),
                    y: r2(horizontal ? place(i) : value(band[i][1])),
                    isolated: !(i > 0 && defined(i - 1)) && !(i < dataCount - 1 && defined(i + 1)),
                })),
            };
        }),
    };
    if (trendline && trendline !== 'none' && !stacked) {
        out.trends = categoryTrends(series, place, value, horizontal, plot, { kind: trendline, order: trendlineOrder, period: trendlinePeriod });
    }
    if (markers.length) {
        Object.assign(out, categoryMarks(markers, place, place.step(), !horizontal, plot, width - plot.right));
    }
    return references?.length || ranges?.length ? { ...out, ...referenceMarks(value, horizontal, plot, references, ranges, marginal.right) } : out;
}
