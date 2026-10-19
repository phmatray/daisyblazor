import { finite, r2, ticks, valueScale } from './_shared.mjs';
import { cartesian } from './_distribution.mjs';

// Shape chart: polygons and polylines given in data coordinates on shared linear axes.
// spec: { shapes: [{ rings: [[{ x, y }]], closed }], width, height, keepAspect, xTitle, yTitle }.
// Returns the plot, the ticks and one SVG path per shape (in input order): a closed shape's rings
// are each closed (the chart fills them even-odd, so an inner ring is a hole), an open shape's
// rings are separate lines. Points without a finite x and y are skipped; a ring left with no point
// adds nothing. keepAspect widens one axis so a unit is as long across as up, as on a floor plan.
export default function shape({ shapes = [], width, height, keepAspect = false, xTitle, yTitle }) {
    const rings = (shapes ?? []).map(s => (s?.rings ?? []).map(r => (r ?? []).filter(p => finite(p?.x) && finite(p?.y))).filter(r => r.length > 0));
    const all = rings.flat(2);
    const x = valueScale(all.map(p => p.x));
    const y = valueScale(all.map(p => p.y));
    let { plot, xTicks, yTicks } = cartesian({ width, height, x, y, xTitle, yTitle });

    if (keepAspect && all.length > 0) {
        const pw = plot.right - plot.left;
        const ph = plot.bottom - plot.top;
        const [x0, x1] = x.domain();
        const [y0, y1] = y.domain();
        const ux = (x1 - x0) / pw;
        const uy = (y1 - y0) / ph;
        if (ux > uy) {
            const mid = (y0 + y1) / 2;
            y.domain([mid - (ux * ph) / 2, mid + (ux * ph) / 2]);
        } else {
            const mid = (x0 + x1) / 2;
            x.domain([mid - (uy * pw) / 2, mid + (uy * pw) / 2]);
        }
        xTicks = ticks(x, pw / 80);
        yTicks = ticks(y, Math.max(2, height / 60));
    }

    const at = p => `${r2(x(p.x))},${r2(y(p.y))}`;
    const paths = rings.map((list, i) => list.map(r => 'M' + r.map(at).join('L') + (shapes[i]?.closed === false ? '' : 'Z')).join(''));
    return { plot, xTicks, yTicks, paths };
}
