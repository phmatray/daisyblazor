import { finite, maxWidth, r2, valueScale } from './_shared.mjs';
import { categoryFrame, frameOut, valueText } from './_compare.mjs';

const EMPTY = { plot: { left: 0, top: 0, right: 0, bottom: 0 }, xTicks: [], yTicks: [], xAxisAt: 0, yAxisAt: 0, labelAngle: 0, items: [] };

// Room between the low end of the value axis and the category axis, so a dot (radius 6) at zero
// never sits on its category label.
const DOT = 8;

// spec: { items: [{ label, value, text }], width, height, vertical, sort: 'descending'|'ascending'|'none', showValues, min, max }
// Each item is a stem from zero to its value with a dot at the end; `text` is the formatted value,
// drawn past the dot when showValues is set. Items without a finite value are skipped. min and max
// fix either end of the value axis (else it takes in zero and the values, rounded); values beyond
// it are clamped.
export default function lollipop({ items, width, height, vertical = false, sort = 'descending', showValues = true, min, max }) {
    const rows = (items ?? []).map((d, index) => ({ ...d, index })).filter(d => finite(d.value));
    if (rows.length === 0) {
        return EMPTY;
    }
    if (sort === 'descending') {
        rows.sort((a, b) => b.value - a.value || a.index - b.index);
    } else if (sort === 'ascending') {
        rows.sort((a, b) => a.value - b.value || a.index - b.index);
    }

    const scale = valueScale(rows.map(d => d.value), { zero: true, count: 5 });
    if (finite(min) || finite(max)) {
        const [lo, hi] = scale.domain();
        scale.domain([finite(min) ? min : lo, finite(max) ? max : hi]).clamp(true);
    }
    const shown = showValues ? rows : [];
    const space = list => list.length === 0 ? 0 : vertical ? 16 : maxWidth(list.map(d => d.text)) + 10;
    const f = categoryFrame({
        width, height, vertical,
        labels: rows.map(d => String(d.label ?? '')),
        scale,
        before: space(shown.filter(d => d.value < 0)) + DOT,
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
