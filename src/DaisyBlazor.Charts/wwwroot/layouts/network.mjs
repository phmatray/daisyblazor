import { max } from '../d3/d3-array.mjs';
import { forceSimulation, forceLink, forceManyBody, forceX, forceY, forceCollide } from '../d3/d3-force.mjs';
import { scaleSqrt } from '../d3/d3-scale.mjs';
import { graph, nameWidth } from './_graph.mjs';

const EMPTY = { nodes: [], links: [], groups: [], labelRoom: 0 };

// spec: { links: [{ source, target, value }], nodes: [{ name, group }], width, height, iterations, labels }
// A force-directed graph. The simulation runs a fixed number of ticks here, before anything is
// drawn, and d3-force seeds its own jitter, so the same input always gives the same picture.
export default function network({ links, nodes, width, height, iterations = 300, labels = true }) {
    const g = graph(links, nodes);
    if (g.nodes.length === 0) {
        return EMPTY;
    }
    const sim = g.nodes.map(n => ({ ...n, r: 5 + 2.5 * Math.sqrt(n.degree) }));
    const edges = g.links.map(l => ({ ...l }));
    const simulation = forceSimulation(sim)
        .force('link', forceLink(edges).distance(36))
        .force('charge', forceManyBody().strength(-140))
        .force('x', forceX().strength(0.06))
        .force('y', forceY().strength(0.06 * (width / height)))
        .force('collide', forceCollide(d => d.r + 6))
        .stop();
    const ticks = Number.isFinite(iterations) ? Math.max(0, Math.min(1000, Math.round(iterations))) : 300;
    for (let i = 0; i < ticks; i++) {
        simulation.tick();
    }

    // Stretch the result over the chart, leaving room for a name to the right of every node.
    const labelRoom = labels ? Math.min(width * 0.25, max(sim, d => nameWidth(d.name)) + 8) : 0;
    const rMax = max(sim, d => d.r);
    const fit = (v, lo, hi, a, b) => (hi - lo < 1e-6 ? (a + b) / 2 : a + ((v - lo) / (hi - lo)) * (b - a));
    const [x0, x1] = [Math.min(...sim.map(d => d.x)), Math.max(...sim.map(d => d.x))];
    const [y0, y1] = [Math.min(...sim.map(d => d.y)), Math.max(...sim.map(d => d.y))];
    const left = rMax + 4;
    const right = width - labelRoom - rMax - 4;
    const top = rMax + 8;
    const bottom = height - rMax - 8;
    for (const d of sim) {
        d.x = fit(d.x, x0, x1, left, right);
        d.y = fit(d.y, y0, y1, top, bottom);
    }

    const stroke = scaleSqrt().domain([0, max(edges, l => l.value) ?? 1]).range([1, 5]);
    return {
        nodes: sim.map(d => ({ name: d.name, group: d.group, groupIndex: d.groupIndex, degree: d.degree, x: d.x, y: d.y, r: d.r })),
        links: edges.map(l => ({
            source: l.source.index,
            target: l.target.index,
            value: l.value,
            width: stroke(l.value),
            path: `M${l.source.x},${l.source.y}L${l.target.x},${l.target.y}`,
        })),
        groups: g.groups,
        labelRoom,
    };
}
