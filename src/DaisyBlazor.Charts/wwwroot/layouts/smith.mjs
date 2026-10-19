import { FONT, r2 } from './_shared.mjs';

// Smith chart. spec: { series: [{ points: [{ r, x }] }], admittance, width, height }. A normalized
// impedance z = r + jx sits at its reflection coefficient Γ = (z - 1) / (z + 1) inside the unit
// circle (open circuit at the right edge, short circuit at the left, matched load in the centre).
// The grid draws circles of constant resistance and arcs of constant reactance; admittance mirrors
// it left to right (constant conductance and susceptance), the points stay where Γ puts them.

const R_VALUES = [0.2, 0.5, 1, 2, 5];
const X_VALUES = [0.2, 0.5, 1, 2, 5];

/** Γ of a normalized impedance r + jx; an infinite r or x is the open circuit (1, 0). */
export function gamma(r, x) {
    if (r === Infinity || Math.abs(x) === Infinity) {
        return { re: 1, im: 0 };
    }
    const d = (r + 1) * (r + 1) + x * x;
    return { re: (r * r + x * x - 1) / d, im: (2 * x) / d };
}

const label = v => String(+v.toFixed(2));

export default function smith({ series = [], admittance = false, width, height }) {
    // Room around the circle for the reactance labels.
    const margin = FONT * 2.6;
    const radius = Math.max(1, Math.min(width, height) / 2 - margin);
    const cx = width / 2, cy = height / 2;
    const flip = admittance ? -1 : 1;
    const at = (re, im) => ({ x: r2(cx + flip * re * radius), y: r2(cy - im * radius) });

    const circles = R_VALUES.map(r => {
        const c = at(r / (1 + r), 0);
        return { value: r, cx: c.x, cy: c.y, r: r2(radius / (1 + r)) };
    });

    // A reactance arc runs from the open circuit (1, 0) to where its circle meets the unit circle;
    // the two circles are orthogonal, so the part inside is always the minor arc.
    const arcs = [];
    const labels = R_VALUES.map(r => {
        const p = at((r - 1) / (r + 1), 0);
        return { text: label(r), x: r2(p.x + flip * 2), y: r2(p.y - 4), anchor: admittance ? 'end' : 'start' };
    });
    labels.push({ text: '0', x: at(-1, 0).x + flip * 2, y: r2(cy - 4), anchor: admittance ? 'end' : 'start' });
    for (const v of X_VALUES) {
        for (const x of [v, -v]) {
            const a = at(1, 0), b = at((x * x - 1) / (x * x + 1), (2 * x) / (x * x + 1)), c = at(1, 1 / x);
            const cross = (a.x - c.x) * (b.y - c.y) - (a.y - c.y) * (b.x - c.x);
            const rr = r2(radius / Math.abs(x));
            arcs.push({ value: x, path: `M${a.x},${a.y}A${rr},${rr},0,0,${cross > 0 ? 1 : 0},${b.x},${b.y}` });
            // The label sits just outside the rim, anchored away from the circle.
            const dx = b.x - cx, dy = b.y - cy, len = Math.hypot(dx, dy) || 1;
            const lx = r2(b.x + (dx / len) * 6), ly = r2(b.y + (dy / len) * 6 + (dy > 0 ? FONT * 0.7 : 0));
            const anchor = dx > radius * 0.15 ? 'start' : dx < -radius * 0.15 ? 'end' : 'middle';
            labels.push({ text: `${x < 0 ? '−' : '+'}j${label(Math.abs(x))}`, x: lx, y: ly, anchor });
        }
    }

    const out = series.map(s => {
        const points = [];
        (s?.points ?? []).forEach((p, i) => {
            const r = p?.r, x = p?.x;
            // A negative resistance lies outside the chart; null is a missing value.
            if (!(typeof r === 'number' && r >= 0) || typeof x !== 'number' || Number.isNaN(x)) {
                return;
            }
            const g = gamma(r, x);
            // Points are physical reflection coefficients: never mirrored.
            points.push({
                i,
                x: r2(cx + g.re * radius),
                y: r2(cy - g.im * radius),
                magnitude: +Math.hypot(g.re, g.im).toFixed(4),
                angle: +(Math.atan2(g.im, g.re) * 180 / Math.PI).toFixed(2),
            });
        });
        const path = points.length > 1 ? 'M' + points.map(p => `${p.x},${p.y}`).join('L') : '';
        return { points, path };
    });

    return { cx: r2(cx), cy: r2(cy), radius: r2(radius), circles, arcs, labels, series: out };
}
