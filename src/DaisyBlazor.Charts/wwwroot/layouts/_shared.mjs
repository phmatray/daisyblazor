// Helpers every layout shares: numbers, label sizes, nice scales and ticks. The leading underscore
// keeps this file out of reach of layout(kind), which only accepts names starting with a letter.
import { scaleLinear, scaleLog, extent } from '../d3.mjs';

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
