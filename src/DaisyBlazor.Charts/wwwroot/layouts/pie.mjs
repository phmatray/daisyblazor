import { pie as d3Pie, arc as d3Arc } from '../d3/d3-shape.mjs';

// Pie and donut. spec: { values: [number|null], size, innerRatio, centerLabel }. Only positive
// finite values count; slices run clockwise from 12 o'clock in input order, their paths centred on
// (0, 0) so the chart translates them to (cx, cy). index is the slice's position in values.
export default function pie({ values = [], size = 200, innerRatio = 0, centerLabel = '' }) {
    const radius = size / 2 - 6;
    const innerRadius = radius * Math.min(Math.max(innerRatio || 0, 0), 0.95);
    const valid = [...values.keys()].filter(i => Number.isFinite(values[i]) && values[i] > 0);
    const total = valid.reduce((sum, i) => sum + values[i], 0);
    const arc = d3Arc().outerRadius(radius).innerRadius(innerRadius).digits(2);
    const slices = d3Pie().sort(null).value(i => values[i])(valid).map(a => ({
        index: a.data,
        path: arc(a),
        percent: (values[a.data] / total) * 100,
        startAngle: a.startAngle,
        endAngle: a.endAngle,
    }));

    // Centre label: at most 22 units, shrunk to fit the hole (about 0.6 em per character at weight 600).
    const chars = Math.max(1, String(centerLabel ?? '').length);
    const centerFontSize = Math.max(4, Math.min(22, innerRadius * 0.6, (2 * innerRadius * 0.85) / (0.6 * chars)));
    return { cx: size / 2, cy: size / 2, radius, innerRadius, slices, centerFontSize };
}
