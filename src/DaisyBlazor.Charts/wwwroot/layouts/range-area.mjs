import { area, line, curveMonotoneX, curveLinear } from '../d3/d3-shape.mjs';
import { finite, r2, ticks, valueScale } from './_shared.mjs';
import { xScale, yAxisWidth, byDate } from './_time.mjs';

const EMPTY = { band: '', center: '', points: [], xTicks: [], yTicks: [], plot: { left: 0, top: 0, right: 0, bottom: 0 } };

// spec: { points: [{ date, label, low, high, center }], width, height, smooth }
// The x axis is a time axis when every point has a date, a category axis otherwise. Points
// without a finite low and high are left out; a missing center only breaks the center line.
export default function rangeArea({ points, width, height, smooth = true }) {
    const valid = byDate((points ?? []).filter(p => p)).filter(p => finite(p.low) && finite(p.high))
        .map(p => ({ ...p, low: Math.min(p.low, p.high), high: Math.max(p.low, p.high) }));
    if (valid.length === 0) {
        return EMPTY;
    }

    const values = valid.flatMap(p => [p.low, p.high, ...(finite(p.center) ? [p.center] : [])]);
    const y = valueScale(values, { zero: true });
    const top = 10, bottom = height - 24;
    const count = Math.max(2, Math.floor((bottom - top) / 40));
    const plot = { left: yAxisWidth(y, count), top, right: width - 24, bottom };
    y.range([plot.bottom, plot.top]);

    const x = xScale({
        dates: valid.map(p => p.date),
        labels: valid.map(p => p.label),
        count: valid.length, x0: plot.left, x1: plot.right, pad: 4,
    });
    const curve = smooth ? curveMonotoneX : curveLinear;
    const xs = valid.map((_, i) => x.at(i));

    return {
        band: area().x((_, i) => xs[i]).y0(p => y(p.low)).y1(p => y(p.high)).curve(curve)(valid) ?? '',
        center: line().defined(p => finite(p.center)).x((_, i) => xs[i]).y(p => y(p.center)).curve(curve)(valid) ?? '',
        // x0/x1 bound each point's hover strip: halfway to its neighbours.
        points: valid.map((p, i) => ({
            label: x.label(i), low: p.low, high: p.high, center: finite(p.center) ? p.center : null,
            x: r2(xs[i]), x0: r2(i === 0 ? plot.left : (xs[i - 1] + xs[i]) / 2), x1: r2(i === valid.length - 1 ? plot.right : (xs[i] + xs[i + 1]) / 2),
        })),
        xTicks: x.ticks,
        yTicks: ticks(y, count),
        plot,
    };
}
