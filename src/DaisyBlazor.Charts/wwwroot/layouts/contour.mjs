import { extent, ticks } from '../d3/d3-array.mjs';
import { contourDensity, contours as d3Contours } from '../d3/d3-contour.mjs';
import { Delaunay } from '../d3/d3-delaunay.mjs';
import { geoPath, geoTransform } from '../d3/d3-geo.mjs';
import { finite, valueScale, r2 } from './_shared.mjs';
import { cartesian } from './_distribution.mjs';

// spec: { points: [{ x, y, value }], width, height, xTitle, yTitle, bandwidth, thresholds, showPoints, mode, showLines }
// Density mode (the default) estimates the density of the points (d3-contour) and returns one
// filled band per level, lowest first, as SVG paths relative to the plot's top-left corner.
// mode 'value' interpolates the points' values over the plot instead (linear on the Delaunay triangles of
// the points, on a grid) and returns one band per value level, the lowest covering the whole plot, plus `max`
// (the largest value); showLines adds `labels`, a place on each level's line to print its value.
export default function contour({ points, width, height, xTitle, yTitle, bandwidth = 20, thresholds = 10, showPoints = false, mode = 'density', showLines = false }) {
    const byValue = mode === 'value';
    const valid = (points ?? []).filter(p => finite(p.x) && finite(p.y) && (!byValue || finite(p.value)));
    const x = valueScale(valid.map(p => p.x));
    const y = valueScale(valid.map(p => p.y));
    const { plot, xTicks, yTicks } = cartesian({ width, height, x, y, xTitle, yTitle });
    if (valid.length === 0) {
        return byValue ? { plot, xTicks, yTicks, contours: [], points: [], max: null, ...(showLines ? { labels: [] } : {}) } : { plot, xTicks, yTicks, contours: [], points: [] };
    }

    const w = Math.max(1, Math.round(plot.right - plot.left)), h = Math.max(1, Math.round(plot.bottom - plot.top));
    const count = finite(thresholds) && thresholds >= 1 ? Math.round(thresholds) : 10;
    const dot = p => {
        const o = { x: p.x, y: p.y, cx: r2(x(p.x)), cy: r2(y(p.y)) };
        if (byValue) {
            o.value = p.value;
        }
        return o;
    };
    if (!byValue) {
        // The density grid covers the plot; the component translates the paths into place.
        const contours = contourDensity()
            .x(p => x(p.x) - plot.left)
            .y(p => y(p.y) - plot.top)
            .size([w, h])
            .bandwidth(finite(bandwidth) && bandwidth > 0 ? bandwidth : 20)
            .thresholds(count)(valid);
        const path = geoPath();
        return {
            plot, xTicks, yTicks,
            contours: contours.map(c => ({ value: c.value, path: path(c) ?? '' })).filter(c => c.path !== ''),
            points: showPoints ? valid.map(dot) : [],
        };
    }

    // Values on a grid of about STEP-unit cells, taken at each cell's centre and interpolated in
    // chart units (so both axes weigh alike whatever their data units).
    const nx = Math.max(2, Math.round(w / STEP)), ny = Math.max(2, Math.round(h / STEP));
    const sx = w / nx, sy = h / ny;
    const known = valid.map(p => ({ cx: x(p.x) - plot.left, cy: y(p.y) - plot.top, value: p.value }));
    const grid = surface(known, nx, ny, sx, sy);
    const [lo, hi] = extent(valid, p => p.value);
    const levels = [lo, ...ticks(lo, hi, count).filter(t => t > lo && t < hi)];
    // The grid's corners (0, 0) to (nx, ny) map onto the plot; every band is a closed area.
    const scale = geoTransform({ point(px, py) { this.stream.point(px * sx, py * sy); } });
    const path = geoPath(scale);
    const bands = d3Contours().size([nx, ny]).thresholds(levels)(Array.from(grid, v => Math.max(lo, v)));
    const out = {
        plot, xTicks, yTicks,
        contours: bands.map(c => ({ value: c.value, path: path(c) ?? '' })).filter(c => c.path !== ''),
        points: showPoints ? valid.map(dot) : [],
        max: hi,
    };
    if (showLines) {
        out.labels = [];
        for (const c of bands.slice(1)) {
            const spot = labelSpot(c, sx, sy, w, h, out.labels);
            if (spot) {
                out.labels.push(spot);
            }
        }
    }
    return out;
}

/** Grid spacing, in chart units, of the value surface. */
const STEP = 6;

/**
 * The value at the centre of each cell of an nx × ny grid of sx × sy cells (row by row), from the
 * known points [{ cx, cy, value }]. Inside the points' convex hull it is linear on their Delaunay
 * triangles; outside, it is the linear value at the nearest point of the hull's outline. Either way
 * it is exact at a known point and stays between the smallest and largest known value. With fewer
 * than three points not on one line there are no triangles, and inverse-distance weighting is used.
 */
export function surface(known, nx, ny, sx, sy) {
    const grid = new Float64Array(nx * ny);
    const delaunay = Delaunay.from(known, k => k.cx, k => k.cy);
    const { triangles, hull } = delaunay;
    if (triangles.length === 0) {
        for (let j = 0; j < ny; j++) {
            for (let i = 0; i < nx; i++) {
                grid[j * nx + i] = idw(known, (i + 0.5) * sx, (j + 0.5) * sy);
            }
        }
        return grid;
    }

    const done = new Uint8Array(nx * ny);
    for (let t = 0; t < triangles.length; t += 3) {
        const a = known[triangles[t]], b = known[triangles[t + 1]], c = known[triangles[t + 2]];
        const det = (b.cy - c.cy) * (a.cx - c.cx) + (c.cx - b.cx) * (a.cy - c.cy);
        if (det === 0) {
            continue;
        }
        const i0 = Math.max(0, Math.ceil(Math.min(a.cx, b.cx, c.cx) / sx - 0.5)), i1 = Math.min(nx - 1, Math.floor(Math.max(a.cx, b.cx, c.cx) / sx - 0.5));
        const j0 = Math.max(0, Math.ceil(Math.min(a.cy, b.cy, c.cy) / sy - 0.5)), j1 = Math.min(ny - 1, Math.floor(Math.max(a.cy, b.cy, c.cy) / sy - 0.5));
        for (let j = j0; j <= j1; j++) {
            for (let i = i0; i <= i1; i++) {
                const px = (i + 0.5) * sx, py = (j + 0.5) * sy;
                const u = ((b.cy - c.cy) * (px - c.cx) + (c.cx - b.cx) * (py - c.cy)) / det;
                const v = ((c.cy - a.cy) * (px - c.cx) + (a.cx - c.cx) * (py - c.cy)) / det;
                const wc = 1 - u - v;
                if (u >= -1e-9 && v >= -1e-9 && wc >= -1e-9 && !done[j * nx + i]) {
                    grid[j * nx + i] = u * a.value + v * b.value + wc * c.value;
                    done[j * nx + i] = 1;
                }
            }
        }
    }

    for (let j = 0; j < ny; j++) {
        for (let i = 0; i < nx; i++) {
            if (!done[j * nx + i]) {
                grid[j * nx + i] = onHull(known, hull, (i + 0.5) * sx, (j + 0.5) * sy);
            }
        }
    }
    return grid;
}

// The linear value at the point of the hull's outline nearest to (px, py).
function onHull(known, hull, px, py) {
    let best = Infinity, value = known[hull[0]].value;
    for (let h = 0; h < hull.length; h++) {
        const a = known[hull[h]], b = known[hull[(h + 1) % hull.length]];
        const dx = b.cx - a.cx, dy = b.cy - a.cy, len = dx * dx + dy * dy;
        const t = len === 0 ? 0 : Math.min(1, Math.max(0, ((px - a.cx) * dx + (py - a.cy) * dy) / len));
        const d = (a.cx + t * dx - px) ** 2 + (a.cy + t * dy - py) ** 2;
        if (d < best) {
            best = d;
            value = a.value + t * (b.value - a.value);
        }
    }
    return value;
}

/**
 * Inverse-distance-weighted value at (px, py) of the known points [{ cx, cy, value }]: exact at a
 * known point, and always between the smallest and largest known value.
 */
export function idw(known, px, py, power = 2) {
    let num = 0, den = 0;
    for (const k of known) {
        const d2 = (k.cx - px) ** 2 + (k.cy - py) ** 2;
        if (d2 < 1e-12) {
            return k.value;
        }
        const wt = 1 / d2 ** (power / 2);
        num += wt * k.value;
        den += wt;
    }
    return num / den;
}

// Where to print a level's value: the vertex of its outline farthest from the plot's edges (the
// rest of the outline may run along an edge) and from the labels already placed, relative to the
// plot. Null when the outline only follows edges.
function labelSpot(contour, sx, sy, w, h, placed) {
    let best = null, room = 8;
    for (const polygon of contour.coordinates) {
        for (const ring of polygon) {
            for (const [gx, gy] of ring) {
                const px = gx * sx, py = gy * sy;
                const m = placed.reduce((d, l) => Math.min(d, Math.hypot(l.x - px, l.y - py)), Math.min(px, py, w - px, h - py));
                if (m > room) {
                    room = m;
                    best = { value: contour.value, x: r2(px), y: r2(py) };
                }
            }
        }
    }
    return best;
}
