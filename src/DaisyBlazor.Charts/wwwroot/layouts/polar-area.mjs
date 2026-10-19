import { arc as d3Arc, scaleRadial } from '../d3.mjs';

// spec: { items: [{ label, value }], width, height }
// Rose (Nightingale) chart: every item gets an equal angle, starting at 12 o'clock and running
// clockwise; a wedge's area, not its radius, is proportional to its value. Paths are relative to (cx, cy).
export default function polarArea({ items, width, height }) {
    const valid = (items ?? []).map((d, index) => ({ ...d, index })).filter(d => Number.isFinite(d.value) && d.value >= 0);
    const max = Math.max(0, ...valid.map(d => d.value));
    if (valid.length === 0 || max === 0) {
        return { cx: 0, cy: 0, radius: 0, wedges: [], rings: [], fontSize: 12 };
    }

    const fontSize = Math.max(12, Math.min(width, height) / 36);
    // Labels sit outside the rim, so leave a line above and below and about ten characters each side.
    const radius = Math.max(10, Math.min(height / 2 - fontSize * 1.6, width / 2 - fontSize * 7));
    const cx = width / 2, cy = height / 2;
    const r = scaleRadial().domain([0, max]).range([0, radius]).nice();
    const step = (2 * Math.PI) / valid.length;
    const arc = d3Arc().padAngle(valid.length > 1 ? 0.012 : 0);
    const rim = radius + fontSize * 0.8;

    return {
        cx,
        cy,
        radius,
        fontSize,
        rings: r.ticks(4).filter(v => v > 0).map(v => ({ value: v, radius: r(v) })),
        wedges: valid.map((d, i) => {
            const a0 = i * step, a1 = a0 + step, mid = a0 + step / 2;
            const s = Math.sin(mid), c = Math.cos(mid);
            return {
                index: d.index,
                label: d.label,
                value: d.value,
                path: arc({ innerRadius: 0, outerRadius: r(d.value), startAngle: a0, endAngle: a1 }),
                labelX: cx + rim * s,
                labelY: cy - rim * c + fontSize * 0.35 * (1 - c),
                anchor: s > 0.2 ? 'start' : s < -0.2 ? 'end' : 'middle',
            };
        }),
    };
}
