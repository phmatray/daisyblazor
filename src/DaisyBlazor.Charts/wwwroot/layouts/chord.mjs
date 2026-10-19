import { chord, chordDirected, ribbon, ribbonArrow, arc, descending } from '../d3.mjs';
import { graph, nameWidth } from './_graph.mjs';

const EMPTY = { groups: [], ribbons: [], cx: 0, cy: 0 };

// spec: { links: [{ source, target, value }], width, height, directed, padAngle, labelWidth }
// Undirected: each link is one relation, counted on both of its names. Directed: a ribbon starts
// wide at its source and ends in an arrow at its target. Paths are relative to (cx, cy).
// labelWidth (optional) is the widest label in chart units; by default it is estimated from the names.
export default function chordLayout({ links, width, height, directed = false, padAngle = 0.04, labelWidth }) {
    const g = graph(links, []);
    if (g.links.length === 0) {
        return EMPTY;
    }
    const n = g.nodes.length;
    const matrix = Array.from({ length: n }, () => new Array(n).fill(0));
    for (const l of g.links) {
        matrix[l.source][l.target] += l.value;
        if (!directed) {
            matrix[l.target][l.source] += l.value;
        }
    }

    // Names sit outside the ring, so the radius leaves room for the longest one on each side.
    const widest = Number.isFinite(labelWidth) && labelWidth > 0 ? labelWidth : Math.max(...g.nodes.map(d => nameWidth(d.name)));
    const outer = Math.max(20, Math.min(height / 2 - 20, width / 2 - widest - 16));
    const inner = outer - Math.max(8, outer * 0.07);
    const pad = Number.isFinite(padAngle) && padAngle >= 0 ? Math.min(padAngle, 0.5) : 0.04;

    const chords = (directed ? chordDirected() : chord()).padAngle(pad).sortSubgroups(descending)(matrix);
    const ring = arc().innerRadius(inner).outerRadius(outer);
    const band = (directed ? ribbonArrow().headRadius(Math.min(12, inner * 0.08)) : ribbon()).radius(inner - 1);

    return {
        groups: chords.groups.map(d => {
            const angle = (d.startAngle + d.endAngle) / 2;
            const x = Math.sin(angle) * (outer + 8);
            const y = -Math.cos(angle) * (outer + 8);
            return {
                name: g.nodes[d.index].name,
                value: d.value,
                path: ring(d),
                labelX: x,
                // Pull labels above the top and below the bottom of the ring away from it.
                labelY: y - Math.cos(angle) * 4,
                anchor: Math.abs(x) < outer * 0.15 ? 'middle' : x > 0 ? 'start' : 'end',
            };
        }),
        ribbons: chords.map(d => ({
            source: d.source.index,
            target: d.target.index,
            sourceValue: d.source.value,
            targetValue: d.target.value,
            path: band(d),
        })),
        cx: width / 2,
        cy: height / 2,
    };
}
