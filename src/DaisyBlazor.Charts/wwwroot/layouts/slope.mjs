import { dodge, finite, maxWidth, r2, valueScale } from './_shared.mjs';

const EMPTY = { headers: [], x1: 0, x2: 0, y0: 0, y1: 0, items: [] };

// spec: { items: [{ label, start, end, startText, endText }], width, height, startLabel, endLabel, showValues }
// Two columns on a shared vertical scale, one line per item from its start to its end value. Each
// line is named at both ends ("Label 12" on the left, "18 Label" on the right); names are pushed
// apart so they never overlap. Items need both values finite.
export default function slope({ items, width, height, startLabel = '', endLabel = '', showValues = true }) {
    const rows = (items ?? []).filter(d => finite(d.start) && finite(d.end));
    if (rows.length === 0) {
        return EMPTY;
    }

    const leftText = d => showValues ? `${d.label ?? ''} ${d.startText ?? d.start}` : String(d.label ?? '');
    const rightText = d => showValues ? `${d.endText ?? d.end} ${d.label ?? ''}` : String(d.label ?? '');
    const xl = Math.max(maxWidth(rows.map(leftText), 12) + 12, maxWidth([startLabel], 12) / 2 + 4);
    const xr = width - Math.max(maxWidth(rows.map(rightText), 12) + 12, maxWidth([endLabel], 12) / 2 + 4);
    const y0 = startLabel || endLabel ? 36 : 10;
    const y1 = height - 10;
    const scale = valueScale(rows.flatMap(d => [d.start, d.end]), { count: 5 }).range([y1, y0]);

    const gap = 15;
    const left = dodge(rows.map(d => scale(d.start)), gap, y0, y1);
    const right = dodge(rows.map(d => scale(d.end)), gap, y0, y1);

    return {
        headers: [
            { text: String(startLabel ?? ''), x: r2(xl), y: 16, anchor: 'middle' },
            { text: String(endLabel ?? ''), x: r2(xr), y: 16, anchor: 'middle' },
        ].filter(h => h.text),
        x1: r2(xl), x2: r2(xr), y0: r2(y0), y1: r2(y1),
        items: rows.map((d, i) => ({
            label: String(d.label ?? ''), start: d.start, end: d.end,
            x1: r2(xl), y1: r2(scale(d.start)), x2: r2(xr), y2: r2(scale(d.end)),
            left: { text: leftText(d), x: r2(xl - 10), y: r2(left[i] + 4), anchor: 'end' },
            right: { text: rightText(d), x: r2(xr + 10), y: r2(right[i] + 4), anchor: 'start' },
        })),
    };
}
