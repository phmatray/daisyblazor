// Helpers shared by the distribution layouts (scatter, contour, hexbin, box plot, violin, histogram,
// beeswarm, ridgeline). The leading underscore keeps it out of charts.mjs's kind pattern.
import { quantileSorted, deviation } from '../d3/d3-array.mjs';
import { scaleBand } from '../d3/d3-scale.mjs';
import { finite, maxWidth, r2, ticks as niceTicks, valueScale } from './_shared.mjs';

/** The finite values, ascending. */
export const sorted = values => (values ?? []).filter(finite).sort((a, b) => a - b);

/** About `count` labelled ticks of a continuous scale (count need not be a whole number). */
export const ticks = (scale, count) => niceTicks(scale, Math.max(2, Math.round(count)));

/**
 * The plot rectangle inside a width x height chart, leaving room for the y tick labels on the left,
 * the y title above them, the x tick labels and the x title below.
 */
export function frame({ width, height, yLabels, xTitle, yTitle, extraTop = 0 }) {
    const left = Math.ceil(maxWidth(yLabels)) + 12;
    const top = (yTitle ? 26 : 10) + extraTop;
    const bottom = height - (xTitle ? 42 : 24);
    return { left, top, right: width - 16, bottom };
}

/** A frame plus labelled x and y ticks for two continuous scales; the scales get their ranges. */
export function cartesian({ width, height, x, y, xTitle, yTitle, inset = 0, extraTop = 0 }) {
    const yCount = Math.max(2, height / 60);
    const plot = frame({ width, height, yLabels: ticks(y, yCount).map(t => t.label), xTitle, yTitle, extraTop });
    x.range([plot.left + inset, plot.right - inset]);
    y.range([plot.bottom - inset, plot.top + inset]);
    return { plot, xTicks: ticks(x, (plot.right - plot.left) / 80), yTicks: ticks(y, yCount) };
}

/**
 * Groups along a band axis and their values along a continuous axis. Vertical puts groups across the
 * bottom; horizontal lists them down the left. Returns the frame, ticks, value scale and band scale.
 */
export function grouped({ width, height, names, values, horizontal, valueTitle, padding = 0.3, extraTop = 0 }) {
    const value = valueScale(values);
    // Bands are keyed by position, so two groups may share a name.
    const band = scaleBand().domain(names.map((_, i) => i)).padding(padding);
    const count = Math.max(2, (horizontal ? width : height) / 70);
    const yLabels = horizontal ? names : ticks(value, count).map(t => t.label);
    const plot = frame({ width, height, yLabels, xTitle: horizontal && valueTitle, yTitle: !horizontal && valueTitle, extraTop });
    if (horizontal) {
        value.range([plot.left, plot.right]);
        band.range([plot.top, plot.bottom]);
    } else {
        value.range([plot.bottom, plot.top]);
        band.range([plot.left, plot.right]);
    }
    const bandTicks = names.map((n, i) => ({ pos: r2(band(i) + band.bandwidth() / 2), label: n }));
    const valueTicks = ticks(value, count);
    return {
        plot, value, band,
        xTicks: horizontal ? valueTicks : bandTicks,
        yTicks: horizontal ? bandTicks : valueTicks,
    };
}

/** Quartiles (d3.quantile, R-7), Tukey whiskers at 1.5 IQR and the outliers beyond them. */
export function boxStats(values) {
    const s = sorted(values);
    if (s.length === 0) {
        return null;
    }
    const q1 = quantileSorted(s, 0.25), median = quantileSorted(s, 0.5), q3 = quantileSorted(s, 0.75);
    const iqr = q3 - q1, lo = q1 - 1.5 * iqr, hi = q3 + 1.5 * iqr;
    const inside = s.filter(v => v >= lo && v <= hi);
    return {
        count: s.length, min: s[0], max: s[s.length - 1], q1, median, q3,
        low: inside.length ? inside[0] : q1,
        high: inside.length ? inside[inside.length - 1] : q3,
        outliers: s.filter(v => v < lo || v > hi),
    };
}

/** Silverman's rule-of-thumb bandwidth, falling back to a tenth of the range (or 1) for flat data. */
export function silverman(s) {
    const n = s.length;
    const sd = n > 1 ? deviation(s) : 0;
    const iqr = n > 1 ? quantileSorted(s, 0.75) - quantileSorted(s, 0.25) : 0;
    const spread = Math.min(sd, iqr / 1.34) || sd || (s[n - 1] - s[0]) / 10;
    return spread > 0 ? 0.9 * spread * Math.pow(n, -0.2) : 1;
}

/** Gaussian kernel density of the sorted values at each point of `at`. */
export function kde(s, bandwidth, at) {
    const h = finite(bandwidth) && bandwidth > 0 ? bandwidth : silverman(s);
    const k = 1 / (s.length * h * Math.sqrt(2 * Math.PI));
    return at.map(x => {
        let sum = 0;
        for (const v of s) {
            const u = (x - v) / h;
            sum += Math.exp(-0.5 * u * u);
        }
        return sum * k;
    });
}

/** `n` evenly spaced values from lo to hi inclusive. */
export const steps = (lo, hi, n) => Array.from({ length: n }, (_, i) => lo + (hi - lo) * i / (n - 1));
