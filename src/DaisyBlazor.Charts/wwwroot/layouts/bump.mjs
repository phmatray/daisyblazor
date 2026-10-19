import { line, scalePoint, curveBumpX } from '../d3.mjs';
import { finite, textWidth, fit, r2 } from './_time.mjs';

const EMPTY = { lines: [], xTicks: [], ranks: 0, plot: { x0: 0, y0: 0, x1: 0, y1: 0 } };

// spec: { series: [{ name, values }], labels, valuesAreRanks, width, height }
// values[i] belongs to period labels[i]. By default the values are scores and the highest one in a
// period ranks 1st (ties keep series order); with valuesAreRanks they are the ranks themselves.
// A series without a value in a period has no rank there, and its line breaks.
export default function bump({ series, labels, valuesAreRanks = false, width, height }) {
    const named = (series ?? []).filter(s => s && Array.isArray(s.values));
    const count = Math.max(0, ...named.map(s => s.values.length));
    if (named.length === 0 || count === 0) {
        return EMPTY;
    }

    // ranks[s][i]: the rank of series s in period i, or null.
    const ranks = named.map(() => Array(count).fill(null));
    for (let i = 0; i < count; i++) {
        const present = named.map((s, k) => ({ k, v: s.values[i] })).filter(d => finite(d.v));
        present.sort((a, b) => (valuesAreRanks ? a.v - b.v : b.v - a.v) || a.k - b.k);
        present.forEach((d, r) => { ranks[d.k][i] = r + 1; });
    }
    const depth = Math.max(1, ...ranks.flat().filter(r => r));

    // Names sit at both ends of the lines, so the margins are as wide as the longest name.
    const size = 12;
    const side = Math.min(width * 0.25, Math.max(...named.map(s => textWidth(s.name ?? '', size)))) + 14;
    const plot = { x0: side, y0: 12, x1: width - side, y1: height - 26 };
    const periods = Array.from({ length: count }, (_, i) => String(labels?.[i] ?? i + 1));
    const x = scalePoint().domain(periods.map((_, i) => i)).range([plot.x0, plot.x1]);
    const y = scalePoint().domain(Array.from({ length: depth }, (_, r) => r + 1)).range([plot.y0, plot.y1]).padding(0.5);
    const path = line().defined(d => d.rank !== null).x(d => d.x).y(d => d.y).curve(curveBumpX);

    // Period labels are thinned so neighbours never touch.
    const every = Math.max(1, Math.ceil((Math.max(...periods.map(p => textWidth(p))) + 10) / Math.max(1, x.step())));

    return {
        lines: named.map((s, k) => {
            const points = ranks[k].map((rank, i) => ({
                period: periods[i], rank, value: finite(s.values[i]) ? s.values[i] : null,
                x: r2(x(i)), y: rank === null ? 0 : r2(y(rank)),
            }));
            const first = points[0].rank !== null ? points[0] : null;
            const last = points[count - 1].rank !== null ? points[count - 1] : null;
            return {
                index: k, name: s.name ?? '', label: fit(s.name ?? '', side - 14, size), path: path(points) ?? '',
                points: points.filter(p => p.rank !== null),
                start: first && { x: first.x, y: first.y },
                end: last && { x: last.x, y: last.y },
            };
        }),
        xTicks: periods.flatMap((p, i) => (i % every === 0 ? [{ position: r2(x(i)), label: p }] : [])),
        ranks: depth,
        plot,
    };
}
