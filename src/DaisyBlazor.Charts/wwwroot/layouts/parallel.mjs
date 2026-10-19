import { line, scalePoint } from '../d3.mjs';
import { finite, maxWidth, r2, textWidth, ticks, valueScale } from './_compare.mjs';

const EMPTY = { axes: [], lines: [] };

// spec: { dimensions: [name], items: [{ label, values: [number | null] }], width, height }
// One vertical axis per dimension, each with its own nice scale and ticks; one polyline per item
// through its values. A missing value breaks the line there. Needs two dimensions and one item.
export default function parallel({ dimensions, items, width, height }) {
    const names = (dimensions ?? []).map(d => String(d ?? ''));
    const rows = (items ?? []).map(d => ({ label: String(d.label ?? ''), values: names.map((_, i) => finite(d.values?.[i]) ? d.values[i] : null) }));
    if (names.length < 2 || rows.length === 0) {
        return EMPTY;
    }

    const scales = names.map((_, i) => valueScale(rows.map(r => r.values[i]), { zero: false, count: 5 }));
    const tickLists = scales.map(s => ticks(s, 5));
    const tickW = maxWidth(tickLists[0].map(t => t.label), 11);
    const x0 = Math.max(tickW + 10, textWidth(names[0], 12) / 2 + 4);
    const x1 = width - Math.max(textWidth(names[names.length - 1], 12) / 2 + 4, 6);
    const y0 = 30;
    const y1 = height - 8;
    const x = scalePoint(names.map((_, i) => i), [x0, x1]);
    scales.forEach(s => s.range([y1, y0]));

    const path = line().defined(p => p !== null);
    return {
        axes: names.map((name, i) => ({
            name, x: r2(x(i)), y1: r2(y0), y2: r2(y1),
            label: { text: name, x: r2(x(i)), y: 16, anchor: 'middle', rotate: 0 },
            ticks: tickLists[i].map(t => {
                const y = r2(scales[i](t.value));
                return { label: t.label, x: r2(x(i) - 6), y: r2(y + 4), anchor: 'end', x1: r2(x(i) - 3), y1: y, x2: r2(x(i)), y2: y };
            }),
        })),
        lines: rows.map((r, index) => ({
            label: r.label, index, values: r.values,
            path: path(r.values.map((v, i) => v === null ? null : [r2(x(i)), r2(scales[i](v))])) ?? '',
        })),
    };
}
