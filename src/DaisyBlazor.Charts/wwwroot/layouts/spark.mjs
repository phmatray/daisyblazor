import { scaleLinear } from '../d3/d3-scale.mjs';
import { line as d3Line, area as d3Area } from '../d3/d3-shape.mjs';
import { finite, r2 } from './_shared.mjs';

// Sparkline: a tiny trend line with no axes. spec: { data: [number|null], pad, width, height, kind,
// trend, normal } (px). The values span the box from their own low to high (flat data runs through
// the middle); null leaves a gap, and a value alone between gaps is a zero-length segment the round
// cap draws as a dot. kind 'column' or 'winloss' returns `bars` instead (see bars below).
// Line and column only: normal [lo, hi] adds `band` ({ y, height }), the normal range, and takes it
// into the value range; trend adds `trend` ({ x1, y1, x2, y2 }), the least-squares line across the
// values. Neither key is returned when not asked for.
export default function spark({ data = [], pad = 2, width, height, kind = 'line', trend = false, normal = null }) {
    const winloss = kind === 'winloss';
    const column = kind === 'column';
    const values = data.filter(finite);
    const band = !winloss && Array.isArray(normal) && finite(normal[0]) && finite(normal[1]) ? [Math.min(...normal), Math.max(...normal)] : null;
    const extra = {};

    let x, y;
    if (column || winloss) {
        const step = width / Math.max(1, data.length);
        x = i => i * step + step / 2;
        y = scaleLinear().domain([Math.min(0, ...values, ...(band ?? [])), Math.max(0, ...values, ...(band ?? []))]).range([height - pad, pad]);
        if (y.domain()[0] === y.domain()[1]) {
            y.domain([0, 1]);
        }
    } else if (values.length > 0) {
        const lo = Math.min(...values, ...(band ?? []));
        const hi = Math.max(...values, ...(band ?? []));
        x = data.length === 1 ? () => width / 2 : scaleLinear().domain([0, data.length - 1]).range([pad, width - pad]);
        y = hi === lo ? () => height / 2 : scaleLinear().domain([lo, hi]).range([height - pad, pad]);
    }

    if (band && values.length > 0) {
        const top = y(band[1]);
        extra.band = { y: r2(top), height: r2(y(band[0]) - top) };
    }
    if (trend && !winloss) {
        const fit = leastSquares(data);
        extra.trend = fit && { x1: r2(x(0)), y1: r2(y(fit.a)), x2: r2(x(data.length - 1)), y2: r2(y(fit.a + fit.b * (data.length - 1))) };    }

    if (column || winloss) {
        return { line: '', area: '', end: null, bars: bars(data, pad, width, height, winloss, y), ...extra };
    }
    if (values.length === 0) {
        return { line: '', area: '', end: null, ...extra };
    }

    const defined = i => finite(data[i]);
    const indices = [...data.keys()];
    const last = indices.findLast(defined);
    return {
        line: d3Line().defined(defined).x(x).y(i => y(data[i])).digits(2)(indices),
        area: d3Area().defined(defined).x(x).y0(height - pad).y1(i => y(data[i])).digits(2)(indices),
        end: { x: r2(x(last)), y: r2(y(data[last])) },
        ...extra,
    };
}

/** Least-squares line value = a + b·index over the finite values; null with fewer than two. */
export function leastSquares(data) {
    const pts = [...data.keys()].filter(i => finite(data[i]));
    const n = pts.length;
    if (n < 2) {
        return null;
    }
    const mx = pts.reduce((s, i) => s + i, 0) / n;
    const my = pts.reduce((s, i) => s + data[i], 0) / n;
    const sxx = pts.reduce((s, i) => s + (i - mx) ** 2, 0);
    const b = pts.reduce((s, i) => s + (i - mx) * (data[i] - my), 0) / sxx;
    return { a: my - b * mx, b };
}

// One bar per value, [{ i, x, y, width, height, value }]. column: bars grow from zero (negatives
// down) over a range that always holds zero; a non-zero value is at least 1 px tall. winloss:
// every bar is half the box tall, above the middle for a positive value, below for a negative one.
// Null and (for winloss) zero leave a gap.
function bars(data, pad, width, height, winloss, y) {
    const n = data.length;
    if (n === 0) {
        return [];
    }
    const step = width / n;
    const w = r2(Math.max(1, step * 0.75));
    const mid = height / 2;
    const out = [];
    data.forEach((v, i) => {
        if (!finite(v) || (winloss && v === 0)) {
            return;
        }
        const x = r2(i * step + (step - w) / 2);
        let top, bottom;
        if (winloss) {
            [top, bottom] = v > 0 ? [pad, mid - 0.5] : [mid + 0.5, height - pad];
        } else {
            [top, bottom] = [Math.min(y(v), y(0)), Math.max(y(v), y(0))];
            if (v !== 0 && bottom - top < 1) {
                [top, bottom] = v > 0 ? [bottom - 1, bottom] : [top, top + 1];
            }
        }
        out.push({ i, x, y: r2(top), width: w, height: r2(bottom - top), value: v });
    });
    return out;
}
