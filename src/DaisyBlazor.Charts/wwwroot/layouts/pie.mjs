import { pie as d3Pie, arc as d3Arc } from '../d3/d3-shape.mjs';
import { r2 } from './_shared.mjs';

// Pie and donut. spec: { values: [number|null], size, innerRatio, centerLabel }. Only positive
// finite values count; slices run clockwise from 12 o'clock in input order, their paths centred on
// (0, 0) so the chart translates them to (cx, cy). index is the slice's position in values.
// explode: [index] pulls those slices out along their bisector by explodeOffset × radius (their
// dx, dy); the radius shrinks so they stay inside the square. Without it no slice has dx, dy.
// rings: [[number|null]] draws concentric rings instead of values, outermost first, sharing the
// hole and splitting the rest of the radius evenly; each slice then carries its ring index and its
// percent is of its own ring.
export default function pie({ values = [], size = 200, innerRatio = 0, centerLabel = '', explode = [], explodeOffset = 0.08, rings = null }) {
    const pulled = new Set(explode ?? []);
    const offset = pulled.size > 0 && Number.isFinite(explodeOffset) ? Math.min(Math.max(explodeOffset, 0), 0.5) : 0;
    const radius = (size / 2 - 6) / (1 + offset);
    const innerRadius = radius * Math.min(Math.max(innerRatio || 0, 0), 0.95);

    let slices;
    if (Array.isArray(rings) && rings.length > 0) {
        const band = (radius - innerRadius) / rings.length;
        slices = rings.flatMap((ring, k) => {
            const arc = d3Arc().outerRadius(radius - k * band).innerRadius(radius - (k + 1) * band).digits(2);
            return slicesOf(ring ?? [], arc, new Set(), 0, 0).map(s => ({ ...s, ring: k }));
        });
    } else {
        const arc = d3Arc().outerRadius(radius).innerRadius(innerRadius).digits(2);
        slices = slicesOf(values, arc, pulled, offset, radius);
    }

    // Centre label: at most 22 units, shrunk to fit the hole (about 0.6 em per character at weight 600).
    const chars = Math.max(1, String(centerLabel ?? '').length);
    const centerFontSize = Math.max(4, Math.min(22, innerRadius * 0.6, (2 * innerRadius * 0.85) / (0.6 * chars)));
    return { cx: size / 2, cy: size / 2, radius, innerRadius, slices, centerFontSize };
}

function slicesOf(values, arc, pulled, offset, radius) {
    const valid = [...values.keys()].filter(i => Number.isFinite(values[i]) && values[i] > 0);
    const total = valid.reduce((sum, i) => sum + values[i], 0);
    return d3Pie().sort(null).value(i => values[i])(valid).map(a => {
        const slice = {
            index: a.data,
            path: arc(a),
            percent: (values[a.data] / total) * 100,
            startAngle: a.startAngle,
            endAngle: a.endAngle,
        };
        if (pulled.has(a.data)) {
            const mid = (a.startAngle + a.endAngle) / 2;
            slice.dx = r2(Math.sin(mid) * offset * radius);
            slice.dy = r2(-Math.cos(mid) * offset * radius);
        }
        return slice;
    });
}
