import { scaleUtc } from '../d3/d3-scale.mjs';
import { area, line, curveMonotoneX, curveLinear } from '../d3/d3-shape.mjs';
import { finite, valueScale } from './_shared.mjs';
import { parseDate, timeTicks } from './_time.mjs';

// spec: { points: [{ time, value }], width, height, smooth }
// A compact overview for picking a window of time: the series as an area (and its top line) over
// a time axis that runs from the first to the last point, with no value axis. Points without a
// date or a finite value are left out. The component maps times to x itself, linearly across the
// plot, so the domain is exactly the data's extent (never rounded out).
export default function rangeSelector({ points, width, height, smooth = true }) {
    const plot = { left: 8, top: 4, right: Math.max(9, width - 8), bottom: Math.max(5, height - 22) };
    const valid = (points ?? [])
        .map(p => ({ at: parseDate(p?.time), value: p?.value }))
        .filter(p => p.at && finite(p.value))
        .sort((a, b) => a.at - b.at);
    if (valid.length === 0) {
        return { width, height, plot, xTicks: [], area: '', line: '' };
    }

    const first = valid[0].at, last = valid[valid.length - 1].at;
    const x = scaleUtc().domain([first, +first === +last ? new Date(+first + 86400000) : last]).range([plot.left, plot.right]);
    const values = valid.map(p => p.value);
    const y = valueScale(values, { zero: values.every(v => v >= 0) }).range([plot.bottom, plot.top + 2]);
    const curve = smooth ? curveMonotoneX : curveLinear;

    return {
        width, height, plot,
        xTicks: timeTicks(x, valid.map(p => p.at)),
        area: area().x(p => x(p.at)).y0(plot.bottom).y1(p => y(p.value)).curve(curve)(valid) ?? '',
        line: line().x(p => x(p.at)).y(p => y(p.value)).curve(curve)(valid) ?? '',
    };
}
