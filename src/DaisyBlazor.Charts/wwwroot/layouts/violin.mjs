import { area, curveCatmullRom, max } from '../d3.mjs';
import { r2 } from './_shared.mjs';
import { boxStats, grouped, kde, sorted, steps } from './_distribution.mjs';

const SAMPLES = 48;

// spec: { groups: [{ name, values }], width, height, horizontal, valueTitle, bandwidth }
// One violin per group: a Gaussian kernel density of its values (bandwidth in value units; 0 or
// missing uses Silverman's rule), mirrored around the band centre and cut at the group's own range.
// Widths share one scale, so the widest point of all violins fills its band.
export default function violin({ groups, width, height, horizontal = false, valueTitle, bandwidth = 0 }) {
    const valid = (groups ?? [])
        .map((g, index) => ({ name: String(g.name ?? ''), index, values: sorted(g.values) }))
        .filter(g => g.values.length > 0);
    const { plot, value, band, xTicks, yTicks } = grouped({
        width, height, names: valid.map(g => g.name), horizontal, valueTitle, padding: 0.15,
        values: valid.flatMap(g => [g.values[0], g.values[g.values.length - 1]]),
    });
    if (valid.length === 0) {
        return { plot, xTicks: [], yTicks: [], horizontal, groups: [] };
    }

    const curves = valid.map(g => {
        const at = steps(g.values[0], g.values[g.values.length - 1], SAMPLES);
        return { at, density: kde(g.values, bandwidth, at) };
    });
    const peak = max(curves, c => max(c.density)) || 1;
    const half = band.bandwidth() / 2;

    return {
        plot, xTicks, yTicks, horizontal,
        groups: valid.map((g, i) => {
            const center = band(i) + half;
            const w = d => (d / peak) * half;
            const shape = horizontal
                ? area().x(v => value(v)).y0((_, k) => center - w(curves[i].density[k])).y1((_, k) => center + w(curves[i].density[k]))
                : area().y(v => value(v)).x0((_, k) => center - w(curves[i].density[k])).x1((_, k) => center + w(curves[i].density[k]));
            const s = boxStats(g.values);
            return {
                name: g.name, index: g.index, count: s.count, q1: s.q1, median: s.median, q3: s.q3,
                center: r2(center),
                path: shape.curve(curveCatmullRom).digits(2)(curves[i].at),
                pos: { q1: r2(value(s.q1)), median: r2(value(s.median)), q3: r2(value(s.q3)) },
            };
        }),
    };
}
