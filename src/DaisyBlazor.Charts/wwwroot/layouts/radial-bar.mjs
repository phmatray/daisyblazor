import { arc as d3Arc } from '../d3.mjs';

const SWEEP = 1.5 * Math.PI; // rings run three quarters of a turn, so the top-left quarter stays free for labels

// spec: { items: [{ label, value }], max, width, height }
// One ring per item, the first one outside. Each ring fills value / max of its track (clamped to [0, max]);
// without a positive max the largest value is the full track. Paths are relative to (cx, cy).
export default function radialBar({ items, max, width, height }) {
    const valid = (items ?? []).map((d, index) => ({ ...d, index })).filter(d => Number.isFinite(d.value));
    const full = Number.isFinite(max) && max > 0 ? max : Math.max(0, ...valid.map(d => d.value));
    if (valid.length === 0 || !(full > 0)) {
        return { cx: 0, cy: 0, rings: [] };
    }

    const cx = width / 2, cy = height / 2;
    const outer = Math.min(width, height) / 2 - 8, hole = outer * 0.25;
    const band = (outer - hole) / valid.length, thickness = band * 0.72;
    const fontSize = Math.min(Math.max(12, Math.min(width, height) / 36), thickness * 0.75);
    const arc = d3Arc().cornerRadius(thickness / 2);

    return {
        cx,
        cy,
        rings: valid.map((d, i) => {
            const r1 = outer - i * band, r0 = r1 - thickness;
            const fraction = Math.min(1, Math.max(0, d.value / full));
            return {
                index: d.index,
                label: d.label,
                value: d.value,
                fraction,
                track: arc({ innerRadius: r0, outerRadius: r1, startAngle: 0, endAngle: SWEEP }),
                fill: fraction > 0 ? arc({ innerRadius: r0, outerRadius: r1, startAngle: 0, endAngle: SWEEP * fraction }) : '',
                // Right-aligned just left of 12 o'clock, level with the ring's start.
                labelX: cx - 8,
                labelY: cy - (r0 + r1) / 2 + fontSize * 0.35,
                fontSize,
            };
        }),
    };
}
