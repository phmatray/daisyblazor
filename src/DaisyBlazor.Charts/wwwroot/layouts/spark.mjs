import { scaleLinear } from '../d3/d3-scale.mjs';
import { line as d3Line, area as d3Area } from '../d3/d3-shape.mjs';
import { finite, r2 } from './_shared.mjs';

// Sparkline: a tiny trend line with no axes. spec: { data: [number|null], pad, width, height } (px).
// The values span the box from their own low to high (flat data runs through the middle); null
// leaves a gap, and a value alone between gaps is a zero-length segment the round cap draws as a dot.
export default function spark({ data = [], pad = 2, width, height }) {
    const values = data.filter(finite);
    if (values.length === 0) {
        return { line: '', area: '', end: null };
    }

    const lo = Math.min(...values);
    const hi = Math.max(...values);
    const x = data.length === 1 ? () => width / 2 : scaleLinear().domain([0, data.length - 1]).range([pad, width - pad]);
    const y = hi === lo ? () => height / 2 : scaleLinear().domain([lo, hi]).range([height - pad, pad]);
    const defined = i => finite(data[i]);
    const indices = [...data.keys()];
    const last = indices.findLast(defined);
    return {
        line: d3Line().defined(defined).x(x).y(i => y(data[i])).digits(2)(indices),
        area: d3Area().defined(defined).x(x).y0(height - pad).y1(i => y(data[i])).digits(2)(indices),
        end: { x: r2(x(last)), y: r2(y(data[last])) },
    };
}
