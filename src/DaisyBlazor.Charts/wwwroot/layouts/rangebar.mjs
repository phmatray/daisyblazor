import { scaleBand } from '../d3/d3-scale.mjs';
import { finite, maxWidth, r2, valueScale } from './_shared.mjs';
import { categoryFrame, frameOut, valueText } from './_compare.mjs';

const EMPTY = { plot: { left: 0, top: 0, right: 0, bottom: 0 }, xTicks: [], yTicks: [], xAxisAt: 0, yAxisAt: 0, labelAngle: 0, groups: [], bars: [] };

// spec: { items: [{ label, low, high, group, lowText, highText }], width, height, vertical, showValues, formattedTicks, min, max }
// A bar from low to high per item. Items sharing a label sit side by side in its band, one slot per
// distinct group (in order of first appearance). Items need both ends finite; reversed ends are swapped.
// With formattedTicks, .NET writes the tick labels like the value texts, so the tick margin fits those too.
// A finite min / max pins that end of the value axis (otherwise the data's, rounded): bars are clipped
// to it, and bars wholly outside it are left out (RangeBarChart.ViewHeight counts the same rows).
export default function rangebar({ items, width, height, vertical = false, showValues = false, formattedTicks = false, min = null, max = null }) {
    const rows = (items ?? [])
        .filter(d => finite(d.low) && finite(d.high))
        .map(d => d.low <= d.high ? d : { ...d, low: d.high, high: d.low, lowText: d.highText, highText: d.lowText })
        .filter(d => (!finite(min) || d.high >= min) && (!finite(max) || d.low <= max));
    if (rows.length === 0) {
        return EMPTY;
    }

    const labels = [...new Set(rows.map(d => String(d.label ?? '')))];
    const groups = [...new Set(rows.map(d => String(d.group ?? '')))];
    const space = list => !showValues ? 0 : vertical ? 16 : maxWidth(list) + 10;
    const scale = valueScale(rows.flatMap(d => [d.low, d.high]), { count: 5 });
    if (finite(min) || finite(max)) {
        const [lo, hi] = scale.domain();
        const pinned = [finite(min) ? min : lo, finite(max) ? max : hi];
        if (pinned[0] < pinned[1]) {
            scale.domain(pinned).clamp(true);
        }
    }
    const f = categoryFrame({
        width, height, vertical, labels,
        scale,
        before: space(rows.map(d => d.lowText)),
        after: space(rows.map(d => d.highText)),
        padding: groups.length > 1 ? 0.2 : 0.3,
        tickWidth: formattedTicks ? maxWidth(rows.flatMap(d => [String(d.lowText ?? ''), String(d.highText ?? '')])) : 0,
    });
    const inner = scaleBand(groups, [0, f.band.bandwidth()]).padding(groups.length > 1 ? 0.1 : 0);

    return {
        ...frameOut(f),
        groups,
        bars: rows.map(d => {
            const label = String(d.label ?? '');
            const group = String(d.group ?? '');
            const across = f.band(labels.indexOf(label)) + inner(group);
            const size = inner.bandwidth();
            const mid = across + size / 2;
            const a = f.scale(d.low);
            const b = f.scale(d.high);
            const rect = vertical
                ? { x: r2(across), y: r2(b), width: r2(size), height: r2(Math.max(1, a - b)) }
                : { x: r2(a), y: r2(across), width: r2(Math.max(1, b - a)), height: r2(size) };
            return {
                label, group, groupIndex: groups.indexOf(group), low: d.low, high: d.high, ...rect,
                lowText: showValues ? valueText(String(d.lowText ?? d.low), vertical ? mid : a, vertical ? a : mid, vertical, false) : null,
                highText: showValues ? valueText(String(d.highText ?? d.high), vertical ? mid : b, vertical ? b : mid, vertical, true) : null,
            };
        }),
    };
}
