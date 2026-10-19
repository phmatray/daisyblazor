import { extent } from '../d3/d3-array.mjs';
import { Delaunay } from '../d3/d3-delaunay.mjs';
import { scaleSqrt } from '../d3/d3-scale.mjs';
import { line, curveMonotoneX } from '../d3/d3-shape.mjs';
import { finite, valueScale, r2, fitTrend, trendOf, referenceMarks, referenceValues } from './_shared.mjs';
import { cartesian } from './_distribution.mjs';

// spec: { points: [{ x, y, size, series, label, xError, yError }], width, height, xLog, yLog, xTitle, yTitle,
//         radius, minRadius, maxRadius, showLine, trendline, trendlineOrder, trendlinePeriod, smooth, highDensity, cellSize, stacked,
//         references, bands, xReferences, xBands }
// Points with a size become bubbles (area proportional to size). Each point also gets its Voronoi
// cell, so the chart can highlight the nearest point wherever the pointer is. showLine adds `lines`
// (each series joined in x order, as a monotone spline with smooth); trendline ('linear',
// 'exponential', 'logarithmic', 'power', 'polynomial' of trendlineOrder, 'movingaverage' of
// trendlinePeriod points; see fitTrend) adds `trends`, one fitted curve per series. Neither key is
// returned when not asked for. trendline is ignored when stacked (a fit of the own values would not
// follow the stacked points, a fit of the totals would not describe the series).
// Optional extras, each returned only when used: xError / yError (symmetric ±) give a point
// `xWhisker` / `yWhisker` ({ error, from, to } in chart units, clamped to the plot); stacked draws
// each series on the running total of the previous ones at the same x (the point keeps its own y and
// gains `total`). references and bands ([{ value, label }], [{ from, to, label }]) add horizontal
// reference lines and bands at y values, xReferences and xBands vertical ones at x values (see
// referenceMarks; reference values are taken into their axis, a value a log axis cannot show is
// dropped); the result then carries `references` and `bands`, or `xReferences` and `xBands`. highDensity
// returns no points: it counts them on a grid of cellSize-unit squares instead, `density` holding
// per series a flat list [column, row, count, …] of the occupied cells (row 0 at the plot's top).
export default function scatter({ points, width, height, xLog = false, yLog = false, xTitle, yTitle, radius = 4, minRadius = 3, maxRadius = 18, showLine = false, trendline = 'none', trendlineOrder = 2, trendlinePeriod = 3, smooth = false, highDensity = false, cellSize = 3, stacked = false, references, bands, xReferences, xBands }) {
    const ok = p => finite(p.x) && finite(p.y) && (!xLog || p.x > 0) && (!yLog || p.y > 0);
    const raw = (points ?? []).filter(ok);
    const series = [...new Set(raw.map(p => p.series ?? ''))];
    const index = new Map(series.map((s, i) => [s, i]));
    const valid = stacked ? stack(raw, series) : raw;
    const sizes = highDensity ? [] : valid.map(p => p.size).filter(s => finite(s) && s > 0);
    const bubble = sizes.length > 0;
    const inset = highDensity ? 0 : bubble ? maxRadius : radius;
    const err = e => finite(e) && e > 0;
    const xs = valid.map(p => p.x), ys = valid.map(p => p.y);
    for (const p of valid) {
        if (err(p.xError)) {
            xs.push(p.x - p.xError, p.x + p.xError);
        }
        if (err(p.yError)) {
            ys.push(p.y - p.yError, p.y + p.yError);
        }
    }
    // On a log axis, a reference or band at or below zero has no place: it is blanked, keeping the indices.
    const onAxis = (list, log, ok) => (list ?? []).map(r => (r && (!log || ok(r)) ? r : null));
    const yRefs = onAxis(references, yLog, r => r.value > 0), xRefs = onAxis(xReferences, xLog, r => r.value > 0);
    const yRanges = onAxis(bands, yLog, b => b.from > 0 && b.to > 0), xRanges = onAxis(xBands, xLog, b => b.from > 0 && b.to > 0);
    xs.push(...referenceValues(xRefs));
    ys.push(...referenceValues(yRefs));
    const x = valueScale(xs, { log: xLog });
    const y = valueScale(ys, { log: yLog, zero: stacked });
    const { plot, xTicks, yTicks } = cartesian({ width, height, x, y, xTitle, yTitle, inset });
    const extra = {};
    if (showLine) {
        extra.lines = [];
    }
    // A trend is fitted on each series' own values, so it would not follow stacked points: none when stacked.
    if (trendline && trendline !== 'none' && !stacked) {
        extra.trends = [];
    }
    if (highDensity) {
        extra.cellSize = finite(cellSize) && cellSize >= 1 ? cellSize : 3;
        extra.density = [];
    }
    if (yRefs.length || yRanges.length) {
        Object.assign(extra, referenceMarks(y, false, plot, yRefs, yRanges));
    }
    if (xRefs.length || xRanges.length) {
        const marks = referenceMarks(x, true, plot, xRefs, xRanges);
        extra.xReferences = marks.references;
        extra.xBands = marks.bands;
    }
    if (valid.length === 0) {
        return { plot, xTicks, yTicks, series: [], points: [], ...extra };
    }

    const px = valid.map(p => [x(p.x), y(p.y)]);
    const seriesOf = valid.map(p => index.get(p.series ?? ''));
    let out = [];
    if (highDensity) {
        extra.density = density(px, seriesOf, series.length, plot, extra.cellSize);
    } else {
        const [s0, s1] = extent(sizes);
        const r = scaleSqrt().domain([s0 === s1 ? 0 : s0, s1 || 1]).range([s0 === s1 ? maxRadius : minRadius, maxRadius]);
        const voronoi = Delaunay.from(px).voronoi([plot.left, plot.top, plot.right, plot.bottom]);
        const clamp = (v, lo, hi) => r2(Math.min(hi, Math.max(lo, v)));
        // On a log axis a low end at or below zero runs to the axis' start.
        const whisker = (scale, v, e, log, lo, hi) => ({
            error: e, from: clamp(log && v - e <= 0 ? scale.range()[0] : scale(v - e), lo, hi), to: clamp(scale(v + e), lo, hi),
        });

        // Larger bubbles first, so small ones stay visible on top.
        out = valid.map((p, i) => {
            const o = {
                x: p.x, y: stacked ? p.own : p.y, size: finite(p.size) ? p.size : null, label: p.label ?? null,
                series: seriesOf[i],
                cx: r2(px[i][0]), cy: r2(px[i][1]),
                r: bubble ? r2(finite(p.size) && p.size > 0 ? r(p.size) : minRadius) : radius,
                cell: voronoi.renderCell(i) || '',
            };
            if (stacked) {
                o.total = p.y;
            }
            if (err(p.xError)) {
                o.xWhisker = whisker(x, p.x, p.xError, xLog, plot.left, plot.right);
            }
            if (err(p.yError)) {
                // y runs downwards: from is the low value's position (the larger number).
                o.yWhisker = whisker(y, p.y, p.yError, yLog, plot.top, plot.bottom);
            }
            return o;
        });
        if (bubble) {
            out.sort((a, b) => b.r - a.r);
        }
    }

    if (extra.lines || extra.trends) {
        const bySeries = series.map(() => []);
        valid.forEach((p, i) => bySeries[seriesOf[i]].push({ p, i }));
        if (extra.lines) {
            const path = smooth ? line().curve(curveMonotoneX) : line();
            extra.lines = bySeries.map((list, s) => ({
                series: s,
                path: path(list.slice().sort((a, b) => a.p.x - b.p.x || a.i - b.i).map(({ i }) => [r2(px[i][0]), r2(px[i][1])])) ?? '',
            }));
        }
        if (extra.trends) {
            extra.trends = bySeries.map((list, s) => {
                const fit = fitTrend(trendline, list.map(({ p }) => p.x), list.map(({ p }) => p.y), { order: trendlineOrder, period: trendlinePeriod });
                return fit && trendOf(fit, trendline, s, trendPath(fit, x, y, plot, list.map(({ p }) => p.x), yLog));
            }).filter(Boolean);
        }
    }
    return { plot, xTicks, yTicks, series, points: out, ...extra };
}

// Each series sits on the running total of the series before it at the same x; a point at an x no
// earlier series has starts from 0. The copy's y is the stacked top, `own` its own value.
function stack(list, series) {
    const out = new Array(list.length);
    let totals = new Map();
    for (const name of series) {
        const next = new Map(totals);
        list.forEach((p, i) => {
            if ((p.series ?? '') === name) {
                const top = (totals.get(p.x) ?? 0) + p.y;
                next.set(p.x, top);
                out[i] = { ...p, y: top, own: p.y };
            }
        });
        totals = next;
    }
    return out;
}

/**
 * Counts the points (pixel positions px, series index seriesOf) on a grid of `size`-unit squares over
 * the plot. Returns per series { series, cells: [column, row, count, …] }, so the result is bounded by
 * the grid (columns × rows per series), not by the number of points.
 */
export function density(px, seriesOf, seriesCount, plot, size) {
    const cols = Math.max(1, Math.ceil((plot.right - plot.left) / size));
    const rows = Math.max(1, Math.ceil((plot.bottom - plot.top) / size));
    const grids = Array.from({ length: seriesCount }, () => new Map());
    for (let i = 0; i < px.length; i++) {
        const c = Math.min(cols - 1, Math.max(0, Math.floor((px[i][0] - plot.left) / size)));
        const r = Math.min(rows - 1, Math.max(0, Math.floor((px[i][1] - plot.top) / size)));
        const grid = grids[seriesOf[i]], key = r * cols + c;
        grid.set(key, (grid.get(key) ?? 0) + 1);
    }
    return grids.map((grid, s) => {
        const cells = [];
        for (const [key, count] of grid) {
            cells.push(key % cols, Math.floor(key / cols), count);
        }
        return { series: s, cells };
    });
}

// The fitted curve across the series' x extent, sampled evenly along the drawn x axis (so it bends
// correctly on log scales) and broken wherever it leaves the plot; a moving average joins its points.
function trendPath(fit, x, y, plot, xs, yLog) {
    const inPlot = ([, py]) => finite(py) && py >= plot.top - 1e-6 && py <= plot.bottom + 1e-6;
    if (fit.points) {
        return line().defined(inPlot)(fit.points.map(([v, w]) => [r2(x(v)), r2(!yLog || w > 0 ? y(w) : NaN)])) ?? '';
    }
    const [lo, hi] = extent(xs.filter(v => finite(x(v))));
    const p0 = x(lo), p1 = x(hi), steps = 48;
    const pts = Array.from({ length: steps + 1 }, (_, k) => {
        const px = p0 + (p1 - p0) * k / steps;
        const v = fit.f(x.invert(px));
        const py = finite(v) && (!yLog || v > 0) ? y(v) : NaN;
        return [r2(px), r2(py)];
    });
    return line().defined(inPlot)(pts) ?? '';
}
