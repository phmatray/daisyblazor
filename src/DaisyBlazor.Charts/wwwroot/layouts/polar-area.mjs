import { scaleRadial } from '../d3/d3-scale.mjs';
import { arc as d3Arc } from '../d3/d3-shape.mjs';

// spec: { items: [{ label, value }], width, height }
// Rose (Nightingale) chart: every item gets an equal angle, starting at 12 o'clock and running
// clockwise; a wedge's area, not its radius, is proportional to its value. Paths are relative to (cx, cy).
// Stacked (a wind rose): spec { labels: [name], series: [[value | null]], width, height } instead of
// items. Every label gets an equal wedge, and the series stack outward in it, each a `segment`
// from the running total before it to its own top (area proportional to the total); missing or
// negative values count as 0. The wedge carries the label and the total.
export default function polarArea({ items, labels, series, width, height }) {
    if (series) {
        return windRose(labels ?? [], series, width, height);
    }
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

function windRose(labels, series, width, height) {
    const n = labels.length;
    const value = (s, i) => (Number.isFinite(series[s]?.[i]) && series[s][i] > 0 ? series[s][i] : 0);
    const totals = labels.map((_, i) => series.reduce((sum, _s, s) => sum + value(s, i), 0));
    const max = Math.max(0, ...totals);
    if (n === 0 || max === 0) {
        return { cx: 0, cy: 0, radius: 0, wedges: [], rings: [], fontSize: 12, segments: [] };
    }

    const fontSize = Math.max(12, Math.min(width, height) / 36);
    const radius = Math.max(10, Math.min(height / 2 - fontSize * 1.6, width / 2 - fontSize * 7));
    const cx = width / 2, cy = height / 2;
    const r = scaleRadial().domain([0, max]).range([0, radius]).nice();
    const step = (2 * Math.PI) / n;
    const arc = d3Arc().padAngle(n > 1 ? 0.012 : 0);
    const rim = radius + fontSize * 0.8;
    const segments = [];
    const wedges = labels.map((label, i) => {
        const a0 = i * step, a1 = a0 + step, mid = a0 + step / 2;
        let base = 0;
        series.forEach((_, s) => {
            const v = value(s, i);
            if (v > 0) {
                segments.push({ index: i, series: s, value: v, path: arc({ innerRadius: r(base), outerRadius: r(base + v), startAngle: a0, endAngle: a1 }) });
                base += v;
            }
        });
        const sin = Math.sin(mid), cos = Math.cos(mid);
        return {
            index: i,
            label: String(label ?? ''),
            value: totals[i],
            path: arc({ innerRadius: 0, outerRadius: r(totals[i]), startAngle: a0, endAngle: a1 }) ?? '',
            labelX: cx + rim * sin,
            labelY: cy - rim * cos + fontSize * 0.35 * (1 - cos),
            anchor: sin > 0.2 ? 'start' : sin < -0.2 ? 'end' : 'middle',
        };
    });
    return { cx, cy, radius, fontSize, rings: r.ticks(4).filter(v => v > 0).map(v => ({ value: v, radius: r(v) })), wedges, segments };
}
