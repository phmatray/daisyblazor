import { line, curveLinearClosed, scaleLinear } from '../d3.mjs';
import { finite, r2, textWidth } from './_shared.mjs';

const EMPTY = { cx: 0, cy: 0, radius: 0, axes: [], rings: [], series: [] };

// spec: { axes: [name], series: [[value | null]], width, height, max, levels, grid: 'polygon'|'circle' }
// One spoke per axis, clockwise from the top; each series is a closed shape through its values.
// The radius is the largest that keeps every axis name inside the box. Missing or negative values
// sit at the centre. Needs at least three axes.
export default function radar({ axes, series, width, height, max = null, levels = 4, grid = 'polygon' }) {
    const names = (axes ?? []).map(a => String(a ?? ''));
    const n = names.length;
    const rows = (series ?? []).map(s => names.map((_, i) => finite(s?.[i]) ? Math.max(0, s[i]) : null));
    if (n < 3 || rows.length === 0 || !(width > 0 && height > 0)) {
        return EMPTY;
    }

    const cx = width / 2;
    const cy = height / 2;
    const angle = i => -Math.PI / 2 + (2 * Math.PI * i) / n;
    const gap = 10;

    // Largest radius at which every name, placed `gap` beyond its spoke, stays inside the box.
    let radius = Math.min(cx, cy) - 4;
    names.forEach((name, i) => {
        const cos = Math.cos(angle(i));
        const sin = Math.sin(angle(i));
        const w = textWidth(name, 12);
        const across = Math.abs(cos) > 0.2 ? w : w / 2;
        if (Math.abs(cos) > 1e-6) {
            radius = Math.min(radius, (cx - 4 - across) / Math.abs(cos) - gap);
        }
        if (Math.abs(sin) > 1e-6) {
            radius = Math.min(radius, (cy - 4 - (Math.abs(sin) > 0.2 ? 14 : 7)) / Math.abs(sin) - gap);
        }
    });
    radius = Math.max(radius, 10);

    const values = rows.flat().filter(finite);
    const fixed = finite(max) && max > 0;
    const scale = scaleLinear().domain([0, fixed ? max : Math.max(...values, 0) || 1]).range([0, radius]).clamp(true);
    if (!fixed) {
        scale.nice(levels);
    }
    const top = scale.domain()[1];
    const format = scale.tickFormat(levels);
    const ringValues = [...scale.ticks(levels).filter(v => v > 0 && v < top), top];

    const point = (i, v) => [cx + scale(v) * Math.cos(angle(i)), cy + scale(v) * Math.sin(angle(i))];
    const closed = line().curve(curveLinearClosed);
    const ring = r => grid === 'circle'
        ? `M${r2(cx - r)},${r2(cy)}A${r2(r)},${r2(r)},0,1,1,${r2(cx + r)},${r2(cy)}A${r2(r)},${r2(r)},0,1,1,${r2(cx - r)},${r2(cy)}Z`
        : closed(names.map((_, i) => point(i, scale.invert(r)).map(r2)));

    return {
        cx: r2(cx), cy: r2(cy), radius: r2(radius),
        axes: names.map((name, i) => {
            const cos = Math.cos(angle(i));
            const sin = Math.sin(angle(i));
            const [x, y] = point(i, top);
            return {
                name, x: r2(x), y: r2(y),
                labelX: r2(cx + (radius + gap) * cos),
                labelY: r2(cy + (radius + gap) * sin + (Math.abs(sin) <= 0.2 ? 4 : sin > 0 ? 11 : 0)),
                anchor: cos > 0.2 ? 'start' : cos < -0.2 ? 'end' : 'middle',
            };
        }),
        rings: ringValues.map(v => ({ value: v, label: format(v), r: r2(scale(v)), path: ring(scale(v)) })),
        series: rows.map(row => ({
            path: closed(row.map((v, i) => point(i, v ?? 0).map(r2))),
            points: row.map((v, i) => {
                const [x, y] = point(i, v ?? 0);
                return { x: r2(x), y: r2(y), value: v };
            }),
        })),
    };
}
