import { max } from '../d3/d3-array.mjs';
import { scalePoint, scaleSqrt } from '../d3/d3-scale.mjs';
import { graph, nameWidth } from './_graph.mjs';

const EMPTY = { nodes: [], links: [], groups: [], labelRoom: 0 };

// spec: { links: [{ source, target, value }], nodes: [{ name, group }], width, height, sort }
// Nodes sit on one baseline (sort: 'input' keeps their order, 'group' gathers each group, 'degree'
// puts the most connected first); each link is an arc above it. Names hang below, rotated.
export default function arcDiagram({ links, nodes, width, height, sort = 'input' }) {
    const g = graph(links, nodes);
    if (g.nodes.length === 0) {
        return EMPTY;
    }
    const order = [...g.nodes];
    if (sort === 'group') {
        order.sort((a, b) => (a.groupIndex < 0 ? Infinity : a.groupIndex) - (b.groupIndex < 0 ? Infinity : b.groupIndex) || a.index - b.index);
    } else if (sort === 'degree') {
        order.sort((a, b) => b.degree - a.degree || a.index - b.index);
    }

    const labelRoom = Math.min(height * 0.4, max(order, d => nameWidth(d.name)) + 16);
    const baseline = height - labelRoom;
    const x = scalePoint().domain(order.map(d => d.index)).range([12, width - 12]).padding(0.5);
    const r = d => 3 + 1.5 * Math.sqrt(d.degree);

    // Arcs are half-ellipses: as round as possible, flattened when the widest would leave the top.
    const span = max(g.links, l => Math.abs(x(l.target) - x(l.source))) ?? 0;
    const squash = span > 0 ? Math.min(1, (baseline - 8) / (span / 2)) : 1;
    const stroke = scaleSqrt().domain([0, max(g.links, l => l.value) ?? 1]).range([1, 6]);

    return {
        nodes: g.nodes.map(d => ({ name: d.name, group: d.group, groupIndex: d.groupIndex, degree: d.degree, x: x(d.index), y: baseline, r: r(d) })),
        links: g.links.map(l => {
            const [a, b] = [x(l.source), x(l.target)].sort((p, q) => p - q);
            const rx = (b - a) / 2;
            return {
                source: l.source,
                target: l.target,
                value: l.value,
                width: stroke(l.value),
                path: `M${a},${baseline}A${rx},${rx * squash} 0 0,1 ${b},${baseline}`,
            };
        }),
        groups: g.groups,
        labelRoom,
    };
}
