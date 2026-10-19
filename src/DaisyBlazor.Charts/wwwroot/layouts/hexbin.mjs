import { finite, valueScale, cartesian, r2 } from './_distribution.mjs';

const SQRT3 = Math.sqrt(3);

// spec: { points: [{ x, y }], width, height, xTitle, yTitle, radius }
// Groups the points into pointy-top hexagons of the given radius (chart units) and counts each.
// Returns the bins (centre and count, densest last) and the hexagon outline around (0, 0).
export default function hexbin({ points, width, height, xTitle, yTitle, radius = 12 }) {
    const valid = (points ?? []).filter(p => finite(p.x) && finite(p.y));
    const r = finite(radius) && radius >= 2 ? radius : 12;
    const x = valueScale(valid.map(p => p.x));
    const y = valueScale(valid.map(p => p.y));
    const { plot, xTicks, yTicks } = cartesian({ width, height, x, y, xTitle, yTitle });
    const hexagon = hexagonPath(r);
    if (valid.length === 0) {
        return { plot, xTicks, yTicks, hexagon, maxCount: 0, bins: [] };
    }

    const bins = new Map();
    for (const p of valid) {
        const [i, j] = binOf(x(p.x) - plot.left, y(p.y) - plot.top, r);
        const key = `${i},${j}`;
        const bin = bins.get(key) ?? { i, j, count: 0 };
        bin.count++;
        bins.set(key, bin);
    }

    const dx = r * SQRT3, dy = r * 1.5;
    const out = [...bins.values()]
        .map(b => ({ cx: r2(plot.left + (b.i + (b.j & 1) / 2) * dx), cy: r2(plot.top + b.j * dy), count: b.count }))
        .sort((a, b) => a.count - b.count || a.cy - b.cy || a.cx - b.cx);
    return { plot, xTicks, yTicks, hexagon, maxCount: out[out.length - 1].count, bins: out };
}

// The hexagon (column i, row j) holding (px, py); odd rows shift half a column right. The nearest
// centre is always in one of the two rows around the point, so compare the best of each.
export function binOf(px, py, r) {
    const dx = r * SQRT3, dy = r * 1.5;
    const below = Math.floor(py / dy);
    let best = null;
    for (const j of [below, below + 1]) {
        const shift = (j & 1) / 2;
        const i = Math.round(px / dx - shift);
        const d = (px - (i + shift) * dx) ** 2 + (py - j * dy) ** 2;
        if (best === null || d < best[2]) {
            best = [i, j, d];
        }
    }
    return [best[0], best[1]];
}

function hexagonPath(r) {
    const corners = Array.from({ length: 6 }, (_, k) => {
        const a = Math.PI / 3 * k - Math.PI / 6;
        return `${r2(r * Math.cos(a))},${r2(r * Math.sin(a))}`;
    });
    return `M${corners.join('L')}Z`;
}
