// Helpers every layout shares: numbers, label sizes, nice scales and ticks. The leading underscore
// keeps this file out of reach of layout(kind), which only accepts names starting with a letter.
import { extent } from '../d3/d3-array.mjs';
import { scaleLinear, scaleLog } from '../d3/d3-scale.mjs';
import { line } from '../d3/d3-shape.mjs';

/** Axis text size in chart units; the components draw tick and category labels at this size. */
export const FONT = 11;

/** Height of a row of axis labels under a plot: a gap, then one line of text. */
export const LABEL_ROW = FONT * 1.2 + 6;

/** Average glyph width in em. Layouts cannot measure text, so they estimate it on the wide side. */
const EM = 0.6;

/** Glyph width in em: wide letters (M, W, m, w, %, @) and other capitals run past the average. */
const glyph = c => /[MWmw%@—]/.test(c) ? 0.95 : c >= 'A' && c <= 'Z' ? 0.72 : EM;

export const finite = v => typeof v === 'number' && Number.isFinite(v);

export const r2 = v => Math.round(v * 100) / 100;

/** Estimated width of a label at the given font size. */
// ponytail: estimate, not measurement; measure with getComputedTextLength if a font ever overflows.
export const textWidth = (text, size = FONT) => {
    let em = 0;
    for (const c of String(text ?? '')) {
        em += glyph(c);
    }
    return em * size;
};

/** Estimated width of the widest label of the list. */
export const maxWidth = (list, size = FONT) => list.reduce((m, s) => Math.max(m, textWidth(s, size)), 0);

/** The label, cut with an ellipsis so it fits `room` chart units. */
export function fit(label, room, size = FONT) {
    const text = String(label ?? '');
    if (textWidth(text, size) <= room) {
        return text;
    }
    let chars = 1;
    while (chars < text.length && textWidth(text.slice(0, chars + 1) + '…', size) <= room) {
        chars++;
    }
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
 * Room the reference labels need outside a plot, for a layout to reserve before it places the plot:
 * `right`, a margin for the labels of lines and bands across y (at most 30 % of the width; longer
 * labels are cut), or with `across`, `top`, a row above the plot for those along x. Zero when
 * nothing is labelled.
 */
export function referenceRoom(width, across, references = [], bands = []) {
    const labels = [...(references ?? []), ...(bands ?? [])].map(r => r?.label).filter(Boolean);
    if (labels.length === 0) {
        return { right: 0, top: 0 };
    }
    return across ? { right: 0, top: FONT + 4 } : { right: Math.round(Math.min(maxWidth(labels) + 8, width * 0.3)), top: 0 };
}

/**
 * Reference lines and shaded value bands across a plot, on its value scale. `across` is true when
 * the values run along x (lines and bands are then vertical). references: [{ value, label }],
 * bands: [{ from, to, label }]; bands are clipped to the plot and dropped when nothing is left.
 * Labels sit outside the plot so they never cover the data, in the room referenceRoom reserved:
 * across y, in the right margin level with the line or the band's middle (moved apart when they
 * would overlap, cut to the margin's width `room`); along x, in the row above the plot, centred
 * on the line or the band and kept inside the chart.
 * Returns { references: [{ index, x1, y1, x2, y2, labelX, labelY, anchor, label }],
 * bands: [{ index, x, y, width, height, labelX, labelY, anchor, label }] }, index being the input position.
 */
// ponytail: labels in the row above are not moved apart; dodge them along x if close vertical marks ever collide.
export function referenceMarks(scale, across, plot, references = [], bands = [], room = Infinity) {
    const lo = across ? plot.left : plot.top;
    const hi = across ? plot.right : plot.bottom;
    const at = v => Math.max(lo, Math.min(hi, scale(v)));
    const above = (p, s) => {
        const half = textWidth(s) / 2;
        return { labelX: r2(Math.max(plot.left + half, Math.min(plot.right + 6 - half, p))), labelY: r2(plot.top - 4), anchor: 'middle', label: s };
    };
    const beside = (p, s) => ({ labelX: r2(plot.right + 4), labelY: r2(p + FONT * 0.35), anchor: 'start', label: s ? fit(s, room - 4) : s });
    const lines = (references ?? []).map((r, index) => ({ r, index })).filter(({ r }) => finite(r?.value)).map(({ r, index }) => {
        const p = r2(at(r.value));
        return across
            ? { index, x1: p, y1: plot.top, x2: p, y2: plot.bottom, ...above(p, r.label) }
            : { index, x1: plot.left, y1: p, x2: plot.right, y2: p, ...beside(p, r.label) };
    });
    const shaded = (bands ?? []).map((b, index) => ({ b, index })).filter(({ b }) => finite(b?.from) && finite(b?.to)).map(({ b, index }) => {
        const [a, z] = [at(b.from), at(b.to)].sort((m, n) => m - n);
        return across
            ? { index, x: r2(a), y: plot.top, width: r2(z - a), height: r2(plot.bottom - plot.top), ...above((a + z) / 2, b.label) }
            : { index, x: plot.left, y: r2(a), width: r2(plot.right - plot.left), height: r2(z - a), ...beside((a + z) / 2, b.label) };
    }).filter(b => b.width > 0 && b.height > 0);
    if (!across) {
        // The margin labels share one column: move overlapping ones apart.
        const labelled = [...lines, ...shaded].filter(m => m.label);
        dodge(labelled.map(m => m.labelY), FONT + 2, plot.top, plot.bottom + FONT * 0.35).forEach((y, i) => (labelled[i].labelY = r2(y)));
    }
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
 * Least-squares fit of `kind` to the points, done in data space. 'polynomial' and 'movingaverage'
 * take `order` and `period` (see fitPolynomial and movingAverage). Points the transform cannot take
 * (y ≤ 0 for exponential and power, x ≤ 0 for logarithmic and power) are left out. R² is that of the
 * straight-line fit in the transformed space, as spreadsheets report it. Null with fewer than two
 * usable points or all x equal.
 */
export function fitTrend(kind, xs, ys, { order = 2, period = 3 } = {}) {
    if (kind === 'polynomial') {
        return fitPolynomial(xs, ys, order);
    }
    if (kind === 'movingaverage') {
        return movingAverage(xs, ys, period);
    }
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

/**
 * Least-squares polynomial y = c0 + c1·x + … + cn·xⁿ, n = order clamped to 2–6 (and below the number
 * of distinct x). Solved on x centred and scaled to [-1, 1] (normal equations with partial pivoting,
 * well conditioned there), then expanded back to powers of x. Returns { a: c0, b: c1, coefficients,
 * r2, f }; R² = 1 − SSres/SStot. Null with fewer than two distinct x.
 */
export function fitPolynomial(xs, ys, order = 2) {
    const X = [], Y = [];
    xs.forEach((xv, i) => {
        if (finite(xv) && finite(ys[i])) {
            X.push(xv);
            Y.push(ys[i]);
        }
    });
    const n = Math.min(Math.max(2, Math.round(finite(order) ? order : 2)), 6, new Set(X).size - 1);
    if (n < 1) {
        return null;
    }
    const lo = Math.min(...X), hi = Math.max(...X), m = (lo + hi) / 2, h = (hi - lo) / 2;
    // Normal equations A·d = r on t = (x − m) / h.
    const A = Array.from({ length: n + 1 }, () => new Array(n + 2).fill(0));
    X.forEach((xv, k) => {
        const t = (xv - m) / h;
        const pw = Array.from({ length: n + 1 }, (_, j) => t ** j);
        for (let i = 0; i <= n; i++) {
            for (let j = 0; j <= n; j++) {
                A[i][j] += pw[i] * pw[j];
            }
            A[i][n + 1] += pw[i] * Y[k];
        }
    });
    for (let c = 0; c <= n; c++) {
        let p = c;
        for (let r = c + 1; r <= n; r++) {
            if (Math.abs(A[r][c]) > Math.abs(A[p][c])) {
                p = r;
            }
        }
        [A[c], A[p]] = [A[p], A[c]];
        for (let r = 0; r <= n; r++) {
            if (r !== c) {
                const f = A[r][c] / A[c][c];
                for (let k = c; k <= n + 1; k++) {
                    A[r][k] -= f * A[c][k];
                }
            }
        }
    }
    const d = A.map((row, i) => row[n + 1] / row[i]);
    // Σ d_k·((x − m)/h)^k expanded binomially into powers of x.
    const coefficients = new Array(n + 1).fill(0);
    d.forEach((dk, k) => {
        let binom = 1;
        for (let j = 0; j <= k; j++) {
            coefficients[j] += dk * binom * (-m) ** (k - j) / h ** k;
            binom = binom * (k - j) / (j + 1);
        }
    });
    const scaled = v => d.reduce((s, dk, k) => s + dk * ((v - m) / h) ** k, 0);
    const my = Y.reduce((s, v) => s + v, 0) / Y.length;
    const ssTot = Y.reduce((s, v) => s + (v - my) ** 2, 0);
    const ssRes = Y.reduce((s, v, k) => s + (v - scaled(X[k])) ** 2, 0);
    const r2 = ssTot === 0 ? 1 : Math.max(0, 1 - ssRes / ssTot);
    return { a: coefficients[0], b: coefficients[1], coefficients, r2, f: scaled };
}

/**
 * Simple trailing moving average over the points in x order (ties keep their order): point i ≥
 * period − 1 gets the mean of itself and the period − 1 points before it. period is at least 2.
 * Returns { period, points: [[x, mean]] } (from the period-th point), or null with fewer points than period.
 */
export function movingAverage(xs, ys, period = 3) {
    const k = Math.max(2, Math.round(finite(period) ? period : 3));
    const pts = xs.map((x, i) => [x, ys[i], i]).filter(([x, y]) => finite(x) && finite(y)).sort((p, q) => p[0] - q[0] || p[2] - q[2]);
    if (pts.length < k) {
        return null;
    }
    const points = [];
    let sum = 0;
    pts.forEach(([x, y], i) => {
        sum += y - (i >= k ? pts[i - k][1] : 0);
        if (i >= k - 1) {
            points.push([x, sum / k]);
        }
    });
    return { period: k, points };
}

/** What a layout returns for a fitted trend: its kind, coefficients (or period) and path. */
export function trendOf(fit, kind, series, path) {
    return fit.points
        ? { series, kind, a: 0, b: 0, r2: 0, period: fit.period, path }
        : { series, kind, a: fit.a, b: fit.b, r2: fit.r2, ...(fit.coefficients ? { coefficients: fit.coefficients } : {}), path };
}

/**
 * A trend per series of a category chart, fitted over the category index (0..n−1) on the series'
 * own values (missing values left out) and drawn through the category positions: place(i) is
 * index i's position along the category axis, value the value scale. Curves are sampled between
 * the first and last fitted index and broken where they leave the plot. spec: { kind, order, period }.
 */
export function categoryTrends(series, place, value, horizontal, plot, { kind, order, period }) {
    const point = (i, v, s) => {
        const lo = Math.floor(i), hi = Math.min(lo + 1, Math.ceil(i));
        const p = lo === hi ? place(lo, s) : place(lo, s) + (place(hi, s) - place(lo, s)) * (i - lo);
        const q = value(v);
        return horizontal ? [r2(q), r2(p)] : [r2(p), r2(q)];
    };
    const [qLo, qHi] = horizontal ? [plot.left, plot.right] : [plot.top, plot.bottom];
    const inPlot = pt => finite(pt[0]) && finite(pt[1]) && pt[horizontal ? 0 : 1] >= qLo - 1e-6 && pt[horizontal ? 0 : 1] <= qHi + 1e-6;
    return series.map((data, s) => {
        const xs = [], ys = [];
        data.forEach((v, i) => {
            if (finite(v)) {
                xs.push(i);
                ys.push(v);
            }
        });
        const fit = fitTrend(kind, xs, ys, { order, period });
        if (!fit) {
            return null;
        }
        const steps = 48, first = xs[0], last = xs.at(-1);
        const pts = fit.points
            ? fit.points.map(([i, v]) => point(i, v, s))
            : Array.from({ length: steps + 1 }, (_, k) => point(first + (last - first) * k / steps, fit.f(first + (last - first) * k / steps), s));
        return trendOf(fit, kind, s, line().defined(inPlot)(pts) ?? '');
    }).filter(Boolean);
}
