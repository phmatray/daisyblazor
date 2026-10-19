import { grouped, sorted, r2 } from './_distribution.mjs';

// spec: { groups: [{ name, values }], width, height, valueTitle, radius }
// Values run along the x axis, one row per group. Each dot is placed, in value order, at the
// smallest offset from its row's centre line that does not overlap a dot already placed, so the
// result is the same for the same input. A row too crowded for its band is squeezed to fit.
export default function beeswarm({ groups, width, height, valueTitle, radius = 4 }) {
    const r = radius > 0 ? radius : 4;
    const valid = (groups ?? [])
        .map((g, index) => ({ name: String(g.name ?? ''), index, values: sorted(g.values) }))
        .filter(g => g.values.length > 0);
    const { plot, value, band, xTicks, yTicks } = grouped({
        width, height, names: valid.map(g => g.name), horizontal: true, valueTitle, padding: 0.08,
        values: valid.flatMap(g => [g.values[0], g.values[g.values.length - 1]]),
    });
    if (valid.length === 0) {
        return { plot, xTicks: [], yTicks: [], radius: r, groups: [] };
    }

    const room = Math.max(0, band.bandwidth() / 2 - r);
    return {
        plot, xTicks, yTicks: valid.length > 1 || valid[0].name ? yTicks : [], radius: r,
        groups: valid.map((g, i) => {
            const center = band(i) + band.bandwidth() / 2;
            const xs = g.values.map(v => value(v));
            const ys = dodge(xs, r);
            const spread = Math.max(0, ...ys.map(Math.abs));
            const squeeze = spread > room ? room / spread : 1;
            return {
                name: g.name, index: g.index, center: r2(center),
                dots: g.values.map((v, k) => ({ value: v, cx: r2(xs[k]), cy: r2(center + ys[k] * squeeze) })),
            };
        }),
    };
}

// Offsets (from the centre line) for circles of radius r at the ascending positions xs.
// ponytail: O(n²) neighbour scan, fine for a few thousand dots; use a sweep window if rows get bigger.
export function dodge(xs, r) {
    const d2 = (2 * r) ** 2;
    const placed = [];
    const ys = [];
    for (const x of xs) {
        const near = placed.filter(p => Math.abs(p.x - x) < 2 * r);
        const candidates = [0];
        for (const p of near) {
            const dy = Math.sqrt(d2 - (p.x - x) ** 2);
            candidates.push(p.y + dy, p.y - dy);
        }
        // Closest to the centre first; on a tie, above before below.
        candidates.sort((a, b) => Math.abs(a) - Math.abs(b) || a - b);
        const y = candidates.find(c => near.every(p => (p.x - x) ** 2 + (p.y - c) ** 2 >= d2 - 1e-6)) ?? candidates[candidates.length - 1];
        placed.push({ x, y });
        ys.push(y);
    }
    return ys;
}
