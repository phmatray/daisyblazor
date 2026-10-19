import { extent } from '../d3/d3-array.mjs';
import { Delaunay } from '../d3/d3-delaunay.mjs';
import { scaleSqrt } from '../d3/d3-scale.mjs';
import { finite, valueScale, r2 } from './_shared.mjs';
import { cartesian } from './_distribution.mjs';

// spec: { points: [{ x, y, size, series, label }], width, height, xLog, yLog, xTitle, yTitle,
//         radius, minRadius, maxRadius }
// Points with a size become bubbles (area proportional to size). Each point also gets its Voronoi
// cell, so the chart can highlight the nearest point wherever the pointer is.
export default function scatter({ points, width, height, xLog = false, yLog = false, xTitle, yTitle, radius = 4, minRadius = 3, maxRadius = 18 }) {
    const valid = (points ?? []).filter(p => finite(p.x) && finite(p.y) && (!xLog || p.x > 0) && (!yLog || p.y > 0));
    const sizes = valid.map(p => p.size).filter(s => finite(s) && s > 0);
    const bubble = sizes.length > 0;
    const inset = bubble ? maxRadius : radius;
    const x = valueScale(valid.map(p => p.x), { log: xLog });
    const y = valueScale(valid.map(p => p.y), { log: yLog });
    const { plot, xTicks, yTicks } = cartesian({ width, height, x, y, xTitle, yTitle, inset });
    const series = [...new Set(valid.map(p => p.series ?? ''))];
    if (valid.length === 0) {
        return { plot, xTicks, yTicks, series: [], points: [] };
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
    return { plot, xTicks, yTicks, series, points: out };
}
