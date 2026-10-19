// Helpers shared by the axis charts (line, bar). The leading underscore keeps this file out of
// reach of layout(kind), which only accepts names starting with a letter.
import { scaleLinear } from '../d3.mjs';

/** Axis text size in px; layouts run at the box's real pixel size, so this is what is drawn. */
export const FONT = 11;

/** Height of the label row under the plot: a gap, then one line of text. */
export const LABEL_ROW = FONT * 1.2 + 6;

/** Baseline of a label row whose top is at y. */
export const labelBaseline = y => y + 6 + FONT * 0.85;

export const isFiniteNumber = v => typeof v === 'number' && Number.isFinite(v);

export const round = v => Math.round(v * 100) / 100;

// No DOM to measure text in a layout: about 0.6 em per character errs on the wide side.
export const textWidth = text => String(text).length * FONT * 0.6;

/** The label, cut with an ellipsis so it fits maxWidth px. */
export function fit(label, maxWidth) {
    const text = String(label ?? '');
    if (textWidth(text) <= maxWidth) {
        return text;
    }
    const chars = Math.max(1, Math.floor(maxWidth / (FONT * 0.6)) - 1);
    return text.slice(0, chars).trimEnd() + '…';
}

/** Compact tick text: 1500 → 1.5k, 2400000 → 2.4M, 2.5 → 2.5 (at most two decimals). */
export function compact(value) {
    const abs = Math.abs(value);
    const [scaled, suffix] = abs >= 1e6 ? [value / 1e6, 'M'] : abs >= 1e3 ? [value / 1e3, 'k'] : [value, ''];
    return String(+scaled.toFixed(2) || 0) + suffix;
}

/**
 * A linear value scale over the values and zero (bars and areas grow from it), rounded out to
 * nice bounds with about five ticks, so axes read 0/10/20 rather than 0/11/22.
 */
export function valueScale(values, range) {
    let lo = 0;
    let hi = 0;
    for (const v of values) {
        lo = Math.min(lo, v);
        hi = Math.max(hi, v);
    }
    if (hi === lo) {
        hi = lo + 1;
    }
    return scaleLinear().domain([lo, hi]).nice(5).range(range);
}

/** The scale's ticks with their compact text. */
export const ticks = scale => scale.ticks(5).map(v => ({ value: v, label: compact(v) }));
