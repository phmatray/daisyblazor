import { finite, maxWidth, r2, valueScale } from './_shared.mjs';
import { categoryFrame, frameOut, valueText } from './_compare.mjs';

const EMPTY = { plot: { left: 0, top: 0, right: 0, bottom: 0 }, xTicks: [], yTicks: [], xAxisAt: 0, yAxisAt: 0, labelAngle: 0, items: [] };

// spec: { items: [{ label, start, end, startText, endText }], width, height, showValues }
// One row per item: a dot at start, a dot at end, a line between. Value texts sit outside the pair,
// the lower one to the left and the higher one to the right. Items need both values finite.
export default function dumbbell({ items, width, height, showValues = true }) {
    const rows = (items ?? []).filter(d => finite(d.start) && finite(d.end));
    if (rows.length === 0) {
        return EMPTY;
    }

    const textW = showValues ? maxWidth(rows.flatMap(d => [d.startText, d.endText])) + 10 : 0;
    const f = categoryFrame({
        width, height,
        labels: rows.map(d => String(d.label ?? '')),
        scale: valueScale(rows.flatMap(d => [d.start, d.end]), { count: 5 }),
        before: textW,
        after: textW,
        padding: 0.2,
    });

    return {
        ...frameOut(f),
        items: rows.map((d, i) => {
            const y = f.band(i) + f.band.bandwidth() / 2;
            const xs = f.scale(d.start);
            const xe = f.scale(d.end);
            const startHigh = d.start > d.end;
            return {
                label: String(d.label ?? ''), start: d.start, end: d.end,
                x1: r2(xs), x2: r2(xe), y: r2(y),
                startText: showValues ? valueText(String(d.startText ?? d.start), xs, y, false, startHigh) : null,
                endText: showValues ? valueText(String(d.endText ?? d.end), xe, y, false, !startHigh) : null,
            };
        }),
    };
}
