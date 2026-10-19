import { line, curveCatmullRom, curveCatmullRomClosed, curveLinear, curveLinearClosed } from '../d3/d3-shape.mjs';
import { FONT, finite, r2, ticks, valueScale, maxWidth } from './_shared.mjs';

const EMPTY = { cx: 0, cy: 0, radius: 0, rings: [], spokes: [], series: [] };

// Polar chart: points at a numeric angle and radius. spec: { series: [{ points: [{ a, r }] }],
// width, height, angleMax, startAngle, clockwise, angleTicks: [{ value, label }], mode, smooth,
// closed, levels }. An angle runs from 0 to angleMax for a full turn (360 degrees, 24 hours…),
// starting at startAngle degrees clockwise from 12 o'clock and turning clockwise unless told not to.
// The radius is a nice linear scale from zero (or the lowest value, when negative) with about
// `levels` rings; the angle ticks are labelled outside the circle, and the radius is the largest
// that keeps those labels in the box. mode 'line' joins each series' points in order (closed joins
// the last back to the first), 'area' always closes and is filled by the chart, 'marker' has no
// path; smooth draws a Catmull-Rom spline. Points without a finite angle and radius are skipped.
// stacked: each point sits on the running total of the earlier series at the same angle (its base);
// percent also divides each angle's total to 100. A stacked area is the band between the bases and
// the tops (the inner ring drawn the other way round, so it cuts out).
export default function polar({ series = [], width, height, angleMax = 360, startAngle = 0, clockwise = true, angleTicks = [], mode = 'line', smooth = false, closed = false, levels = 4, stacked = false, percent = false }) {
    if (!(width > 0 && height > 0)) {
        return EMPTY;
    }
    const turn = finite(angleMax) && angleMax > 0 ? angleMax : 360;
    const start = ((finite(startAngle) ? startAngle : 0) * Math.PI) / 180;
    const dir = clockwise === false ? -1 : 1;
    const theta = a => start + dir * (a / turn) * 2 * Math.PI;

    const cx = width / 2;
    const cy = height / 2;
    const gap = 8;
    const labels = (angleTicks ?? []).filter(t => finite(t?.value));
    const radius = Math.max(10, Math.min(cx - maxWidth(labels.map(t => t.label)) - gap - 2, cy - FONT * 1.2 - gap - 2));

    const rows = (series ?? []).map(s => (s?.points ?? []).map((p, i) => ({ i, a: p?.a, r: p?.r })).filter(p => finite(p.a) && finite(p.r)));
    const stack = stacked || percent;
    if (stack) {
        stackRows(rows, percent);
    }
    const scale = valueScale(stack ? rows.flat().flatMap(p => [p.base, p.top]) : rows.flat().map(p => p.r), { zero: true, count: levels }).range([0, radius]);
    const at = (a, r) => {
        const t = theta(a);
        const d = scale(r);
        return [cx + d * Math.sin(t), cy - d * Math.cos(t)];
    };

    const shut = mode === 'area' || closed;
    const curve = smooth ? (shut ? curveCatmullRomClosed : curveCatmullRom) : (shut ? curveLinearClosed : curveLinear);
    const path = line().curve(curve).digits(2);
    const t0 = theta(0);

    return {
        cx: r2(cx), cy: r2(cy), radius: r2(radius),
        // A ring's value sits just outside it on the start spoke, so a ring within a line of the rim
        // would run into that spoke's label: it stays unlabelled.
        rings: ticks(scale, levels).filter(t => t.pos > 0).map(t => ({
            value: t.value, label: t.pos > radius - FONT * 1.5 ? '' : t.label, r: t.pos,
            x: r2(cx + t.pos * Math.sin(t0) + 3), y: r2(cy - t.pos * Math.cos(t0) - 3),
        })),
        spokes: labels.map(({ value, label }) => {
            const t = theta(value);
            const sin = Math.sin(t);
            const cos = Math.cos(t);
            return {
                value, label: String(label ?? ''),
                x: r2(cx + radius * sin), y: r2(cy - radius * cos),
                labelX: r2(cx + (radius + gap) * sin),
                labelY: r2(cy - (radius + gap) * cos + (cos < -0.2 ? FONT * 0.8 : cos > 0.2 ? 0 : FONT * 0.35)),
                anchor: sin > 0.2 ? 'start' : sin < -0.2 ? 'end' : 'middle',
            };
        }),
        series: rows.map(row => {
            const points = row.map(p => {
                const [x, y] = at(p.a, stack ? p.top : p.r);
                return { i: p.i, x: r2(x), y: r2(y) };
            });
            if (mode === 'marker' || points.length < 2) {
                return { path: '', points };
            }
            const outer = path(points.map(p => [p.x, p.y]));
            const hole = stack && mode === 'area' && row.some(p => p.base !== 0)
                ? path(row.map(p => at(p.a, p.base).map(r2)).reverse())
                : '';
            return { path: outer + hole, points };
        }),
    };
}

// Adds base and top to each point, stacking the series in order per angle.
function stackRows(rows, percent) {
    const totals = new Map();
    for (const p of rows.flat()) {
        totals.set(p.a, (totals.get(p.a) ?? 0) + p.r);
    }
    const running = new Map();
    for (const row of rows) {
        for (const p of row) {
            const total = totals.get(p.a);
            const k = percent ? (total !== 0 ? 100 / total : 0) : 1;
            p.base = (running.get(p.a) ?? 0) * k;
            running.set(p.a, (running.get(p.a) ?? 0) + p.r);
            p.top = running.get(p.a) * k;
        }
    }
}
