import { categoryFrame, finite, frameOut, maxWidth, r2, valueScale, valueText } from './_compare.mjs';

const EMPTY = { plot: { x: 0, y: 0, width: 0, height: 0 }, ticks: [], categories: [], items: [] };

// spec: { items: [{ label, value, text }], width, height, vertical, sort: 'descending'|'ascending'|'none', showValues }
// Each item is a stem from zero to its value with a dot at the end; `text` is the formatted value,
// drawn past the dot when showValues is set. Items without a finite value are skipped.
export default function lollipop({ items, width, height, vertical = false, sort = 'descending', showValues = true }) {
    const rows = (items ?? []).map((d, index) => ({ ...d, index })).filter(d => finite(d.value));
    if (rows.length === 0) {
        return EMPTY;
    }
    if (sort === 'descending') {
        rows.sort((a, b) => b.value - a.value || a.index - b.index);
    } else if (sort === 'ascending') {
        rows.sort((a, b) => a.value - b.value || a.index - b.index);
    }

    const shown = showValues ? rows : [];
    const space = list => list.length === 0 ? 0 : vertical ? 16 : maxWidth(list.map(d => d.text), 11) + 10;
    const f = categoryFrame({
        width, height, vertical,
        labels: rows.map(d => String(d.label ?? '')),
        scale: valueScale(rows.map(d => d.value)),
        before: space(shown.filter(d => d.value < 0)),
        after: space(shown.filter(d => d.value >= 0)),
        padding: 0.2,
    });

    const zero = f.scale(0);
    return {
        ...frameOut(f),
        items: rows.map((d, i) => {
            const mid = f.band(i) + f.band.bandwidth() / 2;
            const end = f.scale(d.value);
            const [x1, y1, x2, y2] = vertical ? [mid, zero, mid, end] : [zero, mid, end, mid];
            return {
                label: String(d.label ?? ''), value: d.value, index: d.index,
                x1: r2(x1), y1: r2(y1), x2: r2(x2), y2: r2(y2),
                text: showValues ? valueText(String(d.text ?? d.value), x2, y2, vertical, d.value >= 0) : null,
            };
        }),
    };
}
