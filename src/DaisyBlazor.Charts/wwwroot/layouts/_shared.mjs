// Helpers every layout shares: numbers, label sizes, nice scales and ticks. The leading underscore
// keeps this file out of reach of layout(kind), which only accepts names starting with a letter.
import { extent } from '../d3/d3-array.mjs';
import { scaleLinear, scaleLog } from '../d3/d3-scale.mjs';

/** Axis text size in chart units; the components draw tick and category labels at this size. */
export const FONT = 11;

/** Height of a row of axis labels under a plot: a gap, then one line of text. */
export const LABEL_ROW = FONT * 1.2 + 6;

/** Average glyph width in em. Layouts cannot measure text, so they estimate it on the wide side. */
const EM = 0.6;

export const finite = v => typeof v === 'number' && Number.isFinite(v);

export const r2 = v => Math.round(v * 100) / 100;

/** Estimated width of a label at the given font size. */
// ponytail: estimate, not measurement; measure with getComputedTextLength if a font ever overflows.
export const textWidth = (text, size = FONT) => String(text ?? '').length * size * EM;

/** Estimated width of the widest label of the list. */
export const maxWidth = (list, size = FONT) => list.reduce((m, s) => Math.max(m, textWidth(s, size)), 0);

/** The label, cut with an ellipsis so it fits `room` chart units. */
export function fit(label, room, size = FONT) {
    const text = String(label ?? '');
    if (textWidth(text, size) <= room) {
        return text;
    }
    const chars = Math.max(1, Math.floor(room / (size * EM)) - 1);
    return text.slice(0, chars).trimEnd() + '…';
}

/** Show every n-th label along an axis whose labels are `step` apart, so neighbours keep `gap` between them. */
export const stride = (labels, step, gap, size = FONT) =>
    Math.max(1, Math.ceil((maxWidth(labels, size) + gap) / Math.max(1, step)));

/** Compact tick text: 1500 → 1.5k, 2400000 → 2.4M, 2.5 → 2.5 (at most two decimals). */
export function compact(value) {
    const abs = Math.abs(value);
    const [scaled, suffix] = abs >= 1e6 ? [value / 1e6, 'M'] : abs >= 1e3 ? [value / 1e3, 'k'] : [value, ''];
    return String(+scaled.toFixed(2) || 0) + suffix;
}

/**
 * A linear (or base-10 log) scale over the finite values, rounded out to nice bounds for about
 * `count` ticks. `zero` takes zero into the domain (bars and areas grow from it). A flat domain is
 * widened: [0, 1] when everything is zero, ±10 % (or ±1) around a single value, a decade for a log scale.
 */
export function valueScale(values, { zero = false, log = false, count = 10 } = {}) {
    const vs = (values ?? []).filter(v => finite(v) && (!log || v > 0));
    if (zero && !log) {
        vs.push(0);
    }
    let [lo, hi] = vs.length ? extent(vs) : (log ? [1, 10] : [0, 1]);
    if (lo === hi) {
        if (log) {
            lo /= 10;
            hi *= 10;
        } else if (zero) {
            hi = lo + 1;
        } else {
            const pad = Math.abs(lo) / 10 || 1;
            lo -= pad;
            hi += pad;
        }
    }
    return (log ? scaleLog() : scaleLinear()).domain([lo, hi]).nice(count);
}

/**
 * About `count` ticks of a continuous scale: [{ value, pos, label }], pos taken from the scale's
 * current range. Labels come from `format`, else the scale's own tick format; log scales label a
 * readable subset only, so ticks with an empty label are dropped.
 */
export function ticks(scale, count = 5, format) {
    const log = typeof scale.base === 'function';
    const text = format ?? scale.tickFormat(count, log ? ',' : undefined);
    return scale.ticks(count).map(value => ({ value, pos: r2(scale(value)), label: text(value) })).filter(t => t.label !== '');
}

/**
 * Moves label positions apart so neighbours are at least `gap` apart, keeping them inside
 * [lo, hi] when there is room. Returns the new positions in the input's order.
 */
export function dodge(ys, gap, lo, hi) {
    const order = ys.map((_, i) => i).sort((a, b) => ys[a] - ys[b] || a - b);
    const out = ys.slice();
    let prev = -Infinity;
    for (const i of order) {
        out[i] = Math.max(out[i], prev + gap, lo);
        prev = out[i];
    }
    let next = Infinity;
    for (const i of order.reverse()) {
        out[i] = Math.min(out[i], next - gap, hi);
        next = out[i];
    }
    return out;
}

/**
 * Reference lines and shaded value bands across a plot, on its value scale. `across` is true when
 * the values run along x (lines and bands are then vertical). references: [{ value, label }],
 * bands: [{ from, to, label }]; bands are clipped to the plot and dropped when nothing is left.
 * Labels sit inside the plot, clear of the tick labels outside it: a horizontal line's at its right
 * end above it (below it near the top), a vertical line's at the top beside it, a band's in its corner.
 * Returns { references: [{ index, x1, y1, x2, y2, labelX, labelY, anchor }],
 * bands: [{ index, x, y, width, height, labelX, labelY, anchor }] }, index being the input position.
 */
export function referenceMarks(scale, across, plot, references = [], bands = []) {
    const lo = across ? plot.left : plot.top;
    const hi = across ? plot.right : plot.bottom;
    const at = v => Math.max(lo, Math.min(hi, scale(v)));
    const lines = (references ?? []).map((r, index) => ({ r, index })).filter(({ r }) => finite(r?.value)).map(({ r, index }) => {
        const p = r2(at(r.value));
        if (across) {
            const flip = p + 4 + textWidth(r.label) > plot.right;
            return { index, x1: p, y1: plot.top, x2: p, y2: plot.bottom, labelX: r2(flip ? p - 4 : p + 4), labelY: r2(plot.top + FONT), anchor: flip ? 'end' : 'start' };
        }
        const below = p - 4 - FONT < plot.top;
        return { index, x1: plot.left, y1: p, x2: plot.right, y2: p, labelX: r2(plot.right - 4), labelY: r2(below ? p + FONT + 2 : p - 4), anchor: 'end' };
    });
    const shaded = (bands ?? []).map((b, index) => ({ b, index })).filter(({ b }) => finite(b?.from) && finite(b?.to)).map(({ b, index }) => {
        const [a, z] = [at(b.from), at(b.to)].sort((m, n) => m - n);
        return across
            ? { index, x: r2(a), y: plot.top, width: r2(z - a), height: r2(plot.bottom - plot.top), labelX: r2(a + 4), labelY: r2(plot.bottom - 6), anchor: 'start' }
            : { index, x: plot.left, y: r2(a), width: r2(plot.right - plot.left), height: r2(z - a), labelX: r2(plot.left + 4), labelY: r2(a + FONT + 2), anchor: 'start' };
    }).filter(b => b.width > 0 && b.height > 0);
    return { references: lines, bands: shaded };
}

/** The finite reference values, to take into a value axis's domain so every reference line shows. */
export const referenceValues = references => (references ?? []).map(r => r?.value).filter(finite);

// How each trend maps (x, y) to a straight-line fit Y = c0 + c1·X, and back: linear y = a + b·x,
// exponential y = a·e^(b·x), logarithmic y = a + b·ln x, power y = a·x^b.
const TRENDS = {
    linear: { x: v => v, y: v => v, a: c => c, f: (a, b, v) => a + b * v },
    exponential: { x: v => v, y: Math.log, a: Math.exp, f: (a, b, v) => a * Math.exp(b * v) },
    logarithmic: { x: Math.log, y: v => v, a: c => c, f: (a, b, v) => a + b * Math.log(v) },
    power: { x: Math.log, y: Math.log, a: Math.exp, f: (a, b, v) => a * Math.pow(v, b) },
};

/**
 * Least-squares fit of `kind` to the points, done in data space. Points the transform cannot take
 * (y ≤ 0 for exponential and power, x ≤ 0 for logarithmic and power) are left out. R² is that of the
 * straight-line fit in the transformed space, as spreadsheets report it. Null with fewer than two
 * usable points or all x equal.
 */
export function fitTrend(kind, xs, ys) {
    const t = TRENDS[kind];
    if (!t) {
        return null;
    }
    const X = [], Y = [];
    xs.forEach((xv, i) => {
        const u = t.x(xv), v = t.y(ys[i]);
        if (finite(u) && finite(v)) {
            X.push(u);
            Y.push(v);
        }
    });
    const n = X.length;
    if (n < 2) {
        return null;
    }
    const mx = X.reduce((s, v) => s + v, 0) / n, my = Y.reduce((s, v) => s + v, 0) / n;
    let sxx = 0, sxy = 0, syy = 0;
    for (let i = 0; i < n; i++) {
        sxx += (X[i] - mx) ** 2;
        sxy += (X[i] - mx) * (Y[i] - my);
        syy += (Y[i] - my) ** 2;
    }
    if (sxx === 0) {
        return null;
    }
    const b = sxy / sxx, c0 = my - b * mx;
    const r2 = syy === 0 ? 1 : (sxy * sxy) / (sxx * syy);
    const a = t.a(c0);
    return { a, b, r2, f: v => t.f(a, b, v) };
}
