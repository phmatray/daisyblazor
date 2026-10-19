import { scaleRadial } from '../d3/d3-scale.mjs';
import { arc as d3Arc } from '../d3/d3-shape.mjs';
import { textWidth } from './_shared.mjs';

// spec: { items: [{ label, value, text }], width, height }
// Rose (Nightingale) chart: every item gets an equal angle, starting at 12 o'clock and running
// clockwise; a wedge's area, not its radius, is proportional to its value. Paths are relative to (cx, cy).
// Stacked (a wind rose): spec { labels: [name], series: [[value | null]], width, height } instead of
// items. Every label gets an equal wedge, and the series stack outward in it, each a `segment`
// from the running total before it to its own top (area proportional to the total); missing or
// negative values count as 0. The wedge carries the label and the total. `text` (or `texts[i]` when
// stacked) is the label as the chart prints it, value included; its width is kept free beside the rim.
export default function polarArea({ items, labels, texts, series, width, height }) {
    if (series) {
        const value = (s, i) => (Number.isFinite(series[s]?.[i]) && series[s][i] > 0 ? series[s][i] : 0);
        const wedges = (labels ?? []).map((label, i) => ({ index: i, label: String(label ?? ''), text: texts?.[i], value: series.reduce((sum, _s, s) => sum + value(s, i), 0) }));
        return rose(wedges, width, height, i => series.map((_, s) => value(s, i))) ?? { ...EMPTY, segments: [] };
    }
    const valid = (items ?? []).map((d, index) => ({ index, label: d.label, text: d.text, value: d.value })).filter(d => Number.isFinite(d.value) && d.value >= 0);
    return rose(valid, width, height) ?? EMPTY;
}

const EMPTY = { cx: 0, cy: 0, radius: 0, wedges: [], rings: [], fontSize: 12 };

// The geometry both forms share: one equal wedge per entry ({ index, label, text, value }), its area
// following the value. layers(i), when given, lists the stacked values of wedge i, and the result
// then carries their `segments`. Null when there is nothing to draw.
function rose(entries, width, height, layers) {
    const max = Math.max(0, ...entries.map(d => d.value));
    if (entries.length === 0 || max === 0) {
        return null;
    }

    const fontSize = Math.max(12, Math.min(width, height) / 36);
    const step = (2 * Math.PI) / entries.length;
    // Labels sit 0.8 em outside the rim: leave a line above and below, and beside the circle the
    // widest a side label reaches (a label at angle a starts sin(a)·(radius + 0.8 em) from the centre).
    const room = entries.reduce((m, d, i) => {
        const sin = Math.abs(Math.sin(i * step + step / 2));
        return sin > 0.2 ? Math.min(m, (width / 2 - 2 - textWidth(d.text ?? d.label, fontSize)) / sin - fontSize * 0.8) : m;
    }, Infinity);
    const radius = Math.max(10, Math.min(height / 2 - fontSize * 1.6, room));
    const cx = width / 2, cy = height / 2;
    const r = scaleRadial().domain([0, max]).range([0, radius]).nice();
    const arc = d3Arc().padAngle(entries.length > 1 ? 0.012 : 0);
    const rim = radius + fontSize * 0.8;
    const segments = [];

    const wedges = entries.map((d, i) => {
        const a0 = i * step, a1 = a0 + step, mid = a0 + step / 2;
        let base = 0;
        (layers?.(i) ?? []).forEach((v, s) => {
            if (v > 0) {
                segments.push({ index: i, series: s, value: v, path: arc({ innerRadius: r(base), outerRadius: r(base + v), startAngle: a0, endAngle: a1 }) });
                base += v;
            }
        });
        const sin = Math.sin(mid), cos = Math.cos(mid);
        return {
            index: d.index,
            label: d.label,
            value: d.value,
            path: arc({ innerRadius: 0, outerRadius: r(d.value), startAngle: a0, endAngle: a1 }),
            labelX: cx + rim * sin,
            labelY: cy - rim * cos + fontSize * 0.35 * (1 - cos),
            anchor: sin > 0.2 ? 'start' : sin < -0.2 ? 'end' : 'middle',
        };
    });
    const out = { cx, cy, radius, fontSize, rings: r.ticks(4).filter(v => v > 0).map(v => ({ value: v, radius: r(v) })), wedges };
    return layers ? { ...out, segments } : out;
}
