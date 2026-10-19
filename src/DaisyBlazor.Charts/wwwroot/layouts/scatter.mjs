import { extent } from '../d3/d3-array.mjs';
import { Delaunay } from '../d3/d3-delaunay.mjs';
import { scaleSqrt } from '../d3/d3-scale.mjs';
import { line } from '../d3/d3-shape.mjs';
import { finite, valueScale, r2 } from './_shared.mjs';
import { cartesian } from './_distribution.mjs';

// spec: { points: [{ x, y, size, series, label }], width, height, xLog, yLog, xTitle, yTitle,
//         radius, minRadius, maxRadius, showLine, trendline }
// Points with a size become bubbles (area proportional to size). Each point also gets its Voronoi
// cell, so the chart can highlight the nearest point wherever the pointer is. showLine adds `lines`
// (each series joined in x order); trendline ('linear', 'exponential', 'logarithmic', 'power') adds
// `trends`, one fitted curve per series. Neither key is returned when not asked for.
export default function scatter({ points, width, height, xLog = false, yLog = false, xTitle, yTitle, radius = 4, minRadius = 3, maxRadius = 18, showLine = false, trendline = 'none' }) {
    const valid = (points ?? []).filter(p => finite(p.x) && finite(p.y) && (!xLog || p.x > 0) && (!yLog || p.y > 0));
    const sizes = valid.map(p => p.size).filter(s => finite(s) && s > 0);
    const bubble = sizes.length > 0;
    const inset = bubble ? maxRadius : radius;
    const x = valueScale(valid.map(p => p.x), { log: xLog });
    const y = valueScale(valid.map(p => p.y), { log: yLog });
    const { plot, xTicks, yTicks } = cartesian({ width, height, x, y, xTitle, yTitle, inset });
    const series = [...new Set(valid.map(p => p.series ?? ''))];
    const extra = {};
    if (showLine) {
        extra.lines = [];
    }
    if (trendline && trendline !== 'none') {
        extra.trends = [];
    }
    if (valid.length === 0) {
        return { plot, xTicks, yTicks, series: [], points: [], ...extra };
    }

    const [s0, s1] = extent(sizes);
    const r = scaleSqrt().domain([s0 === s1 ? 0 : s0, s1 || 1]).range([s0 === s1 ? maxRadius : minRadius, maxRadius]);
    const px = valid.map(p => [x(p.x), y(p.y)]);
    const voronoi = Delaunay.from(px).voronoi([plot.left, plot.top, plot.right, plot.bottom]);

    // Larger bubbles first, so small ones stay visible on top.
    const out = valid.map((p, i) => ({
        x: p.x, y: p.y, size: finite(p.size) ? p.size : null, label: p.label ?? null,
        series: series.indexOf(p.series ?? ''),
        cx: r2(px[i][0]), cy: r2(px[i][1]),
        r: bubble ? r2(finite(p.size) && p.size > 0 ? r(p.size) : minRadius) : radius,
        cell: voronoi.renderCell(i) || '',
    }));
    if (bubble) {
        out.sort((a, b) => b.r - a.r);
    }

    const bySeries = series.map((_, s) => valid.map((p, i) => ({ p, i })).filter(({ p }) => series.indexOf(p.series ?? '') === s));
    if (extra.lines) {
        extra.lines = bySeries.map((list, s) => ({
            series: s,
            path: line()(list.slice().sort((a, b) => a.p.x - b.p.x || a.i - b.i).map(({ i }) => [r2(px[i][0]), r2(px[i][1])])) ?? '',
        }));
    }
    if (extra.trends) {
        extra.trends = bySeries.map((list, s) => {
            const fit = fitTrend(trendline, list.map(({ p }) => p.x), list.map(({ p }) => p.y));
            return fit && { series: s, kind: trendline, a: fit.a, b: fit.b, r2: fit.r2, path: trendPath(fit, x, y, plot, list.map(({ p }) => p.x), yLog) };
        }).filter(Boolean);
    }
    return { plot, xTicks, yTicks, series, points: out, ...extra };
}

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

// The fitted curve across the series' x extent, sampled evenly along the drawn x axis (so it bends
// correctly on log scales) and broken wherever it leaves the plot.
function trendPath(fit, x, y, plot, xs, yLog) {
    const [lo, hi] = extent(xs.filter(v => finite(x(v))));
    const p0 = x(lo), p1 = x(hi), steps = 48;
    const pts = Array.from({ length: steps + 1 }, (_, k) => {
        const px = p0 + (p1 - p0) * k / steps;
        const v = fit.f(x.invert(px));
        const py = finite(v) && (!yLog || v > 0) ? y(v) : NaN;
        return [r2(px), r2(py)];
    });
    return line().defined(([, py]) => finite(py) && py >= plot.top - 1e-6 && py <= plot.bottom + 1e-6)(pts) ?? '';
}
