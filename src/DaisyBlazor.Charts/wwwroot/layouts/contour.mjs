import { contourDensity, geoPath } from '../d3.mjs';
import { finite, valueScale, cartesian, r2 } from './_distribution.mjs';

// spec: { points: [{ x, y }], width, height, xTitle, yTitle, bandwidth, thresholds, showPoints }
// Estimates the density of the points (d3-contour) and returns one filled band per level, lowest
// first, as SVG paths relative to the plot's top-left corner.
export default function contour({ points, width, height, xTitle, yTitle, bandwidth = 20, thresholds = 10, showPoints = false }) {
    const valid = (points ?? []).filter(p => finite(p.x) && finite(p.y));
    const x = valueScale(valid.map(p => p.x));
    const y = valueScale(valid.map(p => p.y));
    const { plot, xTicks, yTicks } = cartesian({ width, height, x, y, xTitle, yTitle });
    if (valid.length === 0) {
        return { plot, xTicks, yTicks, contours: [], points: [] };
    }

    // The density grid covers the plot; the component translates the paths into place.
    const contours = contourDensity()
        .x(p => x(p.x) - plot.left)
        .y(p => y(p.y) - plot.top)
        .size([Math.max(1, Math.round(plot.right - plot.left)), Math.max(1, Math.round(plot.bottom - plot.top))])
        .bandwidth(finite(bandwidth) && bandwidth > 0 ? bandwidth : 20)
        .thresholds(finite(thresholds) && thresholds >= 1 ? Math.round(thresholds) : 10)(valid);
    const path = geoPath();
    return {
        plot, xTicks, yTicks,
        contours: contours.map(c => ({ value: c.value, path: path(c) ?? '' })).filter(c => c.path !== ''),
        points: showPoints ? valid.map(p => ({ x: p.x, y: p.y, cx: r2(x(p.x)), cy: r2(y(p.y)) })) : [],
    };
}
