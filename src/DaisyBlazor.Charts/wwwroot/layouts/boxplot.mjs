import { r2 } from './_shared.mjs';
import { boxStats, grouped } from './_distribution.mjs';

// spec: { groups: [{ name, values }], width, height, horizontal, valueTitle }
// One box per group from its raw values: quartiles, whiskers at the furthest values within 1.5 IQR of
// the box, and the outliers beyond. `pos` holds each statistic's coordinate on the value axis;
// `center` and `half` place the box across the band axis.
export default function boxplot({ groups, width, height, horizontal = false, valueTitle }) {
    const stats = (groups ?? [])
        .map((g, index) => ({ name: String(g.name ?? ''), index, stats: boxStats(g.values) }))
        .filter(g => g.stats !== null);
    const names = stats.map(g => g.name);
    const { plot, value, band, xTicks, yTicks } = grouped({
        width, height, names, horizontal, valueTitle,
        values: stats.flatMap(g => [g.stats.min, g.stats.max]),
    });
    const half = r2(Math.min(band.bandwidth() / 2, 36));
    return {
        plot, xTicks: stats.length ? xTicks : [], yTicks: stats.length ? yTicks : [], horizontal, half,
        groups: stats.map(({ name, index, stats: s }, i) => ({
            name, index, count: s.count, low: s.low, q1: s.q1, median: s.median, q3: s.q3, high: s.high,
            center: r2(band(i) + band.bandwidth() / 2),
            pos: { low: r2(value(s.low)), q1: r2(value(s.q1)), median: r2(value(s.median)), q3: r2(value(s.q3)), high: r2(value(s.high)) },
            outliers: s.outliers.map(v => ({ value: v, pos: r2(value(v)) })),
        })),
    };
}
