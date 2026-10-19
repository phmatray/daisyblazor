import { scalePoint, line as d3Line, area as d3Area, stack, curveMonotoneX, curveLinear } from '../d3.mjs';
import { LABEL_ROW, labelBaseline, isFiniteNumber, round, textWidth, fit, valueScale, ticks } from './_axis.mjs';

// Line and area charts. spec: { series: [[number|null]], labels: [string], stacked, smooth,
// showGrid, width, height } (px). Points sit in the middle of equal slots, one per label or value.
// Non-finite values (null) leave a gap; Stacked draws each series on the running total, a missing
// value counting as 0. Returns pixel geometry: the plot box, y ticks and x labels (pos is their y
// or x; long labels are cut with an ellipsis, full keeps the text) and, per series,
// its line and area paths and its points (isolated = no neighbour to draw a line to).
export default function line({ series = [], labels = [], stacked = false, smooth = false, showGrid = true, width, height }) {
    const dataCount = Math.max(0, ...series.map(s => s.length));
    const count = Math.max(dataCount, labels.length);
    const indices = [...Array(dataCount).keys()];

    // [base, top] per series and index; unstacked bands start at zero.
    const bands = stacked
        ? stack().keys([...series.keys()]).value((i, s) => (isFiniteNumber(series[s][i]) ? series[s][i] : 0))(indices)
              .map(layer => layer.map(([base, top]) => [base, top]))
        : series.map(s => indices.map(i => [0, isFiniteNumber(s[i]) ? s[i] : null]));

    const values = bands.flatMap(b => b.map(([, top]) => top)).filter(isFiniteNumber);
    const top = 8;
    const bottom = height - (labels.length > 0 ? LABEL_ROW : 4);
    const y = valueScale(values, [bottom, top]);
    const yTicks = showGrid ? ticks(y) : [];
    const left = yTicks.length > 0 ? Math.max(...yTicks.map(t => textWidth(t.label))) + 6 : 0;
    const right = width - 6;

    const x = scalePoint().domain([...Array(count).keys()]).range([left, right]).padding(0.5);
    const slot = count > 0 ? (right - left) / count : 0;
    // Thin out the labels on long or narrow axes so they never overlap.
    const every = Math.max(1, Math.ceil((Math.max(0, ...labels.map(textWidth)) + 8) / Math.max(slot, 1)));
    const xLabels = [];
    for (let i = 0; i < labels.length; i += every) {
        xLabels.push({ pos: round(x(i)), label: fit(labels[i], slot * every - 4), full: String(labels[i] ?? '') });
    }

    const curve = smooth ? curveMonotoneX : curveLinear;
    return {
        plot: { left: round(left), top, right: round(right), bottom: round(bottom) },
        yTicks: yTicks.map(t => ({ pos: round(y(t.value)), label: t.label })),
        xLabels,
        labelY: round(labelBaseline(bottom)),
        series: bands.map((band, s) => {
            const defined = i => isFiniteNumber(band[i][1]);
            const line = d3Line().defined(defined).x(i => x(i)).y(i => y(band[i][1])).curve(curve).digits(2);
            const area = d3Area().defined(defined).x(i => x(i)).y0(i => y(band[i][0])).y1(i => y(band[i][1])).curve(curve).digits(2);
            const own = series[s];
            return {
                line: line(indices) ?? '',
                area: area(indices) ?? '',
                points: indices.filter(i => isFiniteNumber(own[i])).map(i => ({
                    index: i,
                    x: round(x(i)),
                    y: round(y(band[i][1])),
                    isolated: !(i > 0 && defined(i - 1)) && !(i < dataCount - 1 && defined(i + 1)),
                })),
            };
        }),
    };
}
