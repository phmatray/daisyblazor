import { finite, maxWidth, r2, ticks, valueScale } from './_compare.mjs';

// spec: { rows: [{ label, value, target, ranges: [number] }], width, height, vertical }
// One bullet per row, each on its own scale (zero to the largest of value, target and ranges) with
// its own ticks. Ranges are ascending thresholds drawn as shaded bands behind a thin value bar;
// the target is a short line across the bar. Rows without a finite value are skipped.
export default function bullet({ rows, width, height, vertical = false }) {
    const list = (rows ?? [])
        .filter(d => finite(d.value))
        .map(d => {
            const ranges = (d.ranges ?? []).filter(finite).sort((a, b) => a - b);
            const target = finite(d.target) ? d.target : null;
            const scale = valueScale([d.value, target, ...ranges], { count: 4 });
            return { label: String(d.label ?? ''), value: d.value, target, ranges, scale, ticks: ticks(scale, 4) };
        });
    if (list.length === 0) {
        return { rows: [] };
    }

    const n = list.length;
    const tickW = Math.max(...list.map(r => maxWidth(r.ticks.map(t => t.label), 11)));

    const out = list.map((r, i) => {
        const [lo, hi] = r.scale.domain();
        const edges = [lo, ...r.ranges.filter(v => v > lo && v <= hi)];
        if (edges.length === 1) {
            edges.push(hi);
        }
        const bands = edges.slice(1).map((v, level) => [edges[level], v, level]);

        if (!vertical) {
            const x0 = Math.max(maxWidth(list.map(d => d.label), 12) + 12, tickW / 2 + 2);
            const x1 = width - tickW / 2 - 4;
            const step = (height - 4) / n;
            const h = Math.min(40, Math.max(8, step - 28));
            const top = 2 + i * step + (step - 16 - h) / 2;
            const s = r.scale.range([x0, x1]);
            return {
                label: r.label, value: r.value, target: r.target,
                name: { text: r.label, x: r2(x0 - 10), y: r2(top + h / 2 + 4), anchor: 'end', rotate: 0 },
                ranges: bands.map(([a, b, level]) => ({ x: r2(s(a)), y: r2(top), width: r2(s(b) - s(a)), height: r2(h), level })),
                bar: { x: r2(Math.min(s(0), s(r.value))), y: r2(top + h / 3), width: r2(Math.abs(s(r.value) - s(0))), height: r2(h / 3) },
                marker: r.target === null ? null : { x1: r2(s(r.target)), y1: r2(top + h * 0.15), x2: r2(s(r.target)), y2: r2(top + h * 0.85) },
                ticks: r.ticks.map(t => {
                    const x = r2(s(t.value));
                    return { label: t.label, x, y: r2(top + h + 14), anchor: 'middle', x1: x, y1: r2(top + h), x2: x, y2: r2(top + h + 3) };
                }),
            };
        }

        const step = (width - 4) / n;
        const tw = maxWidth(r.ticks.map(t => t.label), 11);
        const w = Math.min(40, Math.max(8, step - tw - 24));
        const bx = 2 + i * step + (step - (tw + 6 + w)) / 2 + tw + 6;
        const s = r.scale.range([height - 26, 8]);
        return {
            label: r.label, value: r.value, target: r.target,
            name: { text: r.label, x: r2(bx + w / 2), y: r2(height - 8), anchor: 'middle', rotate: 0 },
            ranges: bands.map(([a, b, level]) => ({ x: r2(bx), y: r2(s(b)), width: r2(w), height: r2(s(a) - s(b)), level })),
            bar: { x: r2(bx + w / 3), y: r2(Math.min(s(0), s(r.value))), width: r2(w / 3), height: r2(Math.abs(s(r.value) - s(0))) },
            marker: r.target === null ? null : { x1: r2(bx + w * 0.15), y1: r2(s(r.target)), x2: r2(bx + w * 0.85), y2: r2(s(r.target)) },
            ticks: r.ticks.map(t => {
                const y = r2(s(t.value));
                return { label: t.label, x: r2(bx - 6), y: r2(y + 4), anchor: 'end', x1: r2(bx - 3), y1: y, x2: r2(bx), y2: y };
            }),
        };
    });

    return { rows: out };
}
