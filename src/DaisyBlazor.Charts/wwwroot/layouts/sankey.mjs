import { sankey as d3Sankey, sankeyLinkHorizontal, sankeyJustify, sankeyLeft, sankeyRight, sankeyCenter } from '../d3.mjs';

const ALIGN = { justify: sankeyJustify, left: sankeyLeft, right: sankeyRight, center: sankeyCenter };

// spec: { links: [{ source, target, value }], width, height, nodeWidth, nodePadding, align }
// Nodes are the distinct source/target names, in order of first appearance.
export default function sankey({ links, width, height, nodeWidth = 12, nodePadding = 14, align = 'justify' }) {
    const valid = (links ?? []).filter(l => l.source && l.target && Number.isFinite(l.value) && l.value > 0);
    const names = [...new Set(valid.flatMap(l => [l.source, l.target]))];
    if (valid.length === 0) {
        return { nodes: [], links: [] };
    }

    const graph = d3Sankey()
        .nodeId(d => d.name)
        .nodeWidth(nodeWidth)
        .nodePadding(nodePadding)
        .nodeAlign(ALIGN[align] ?? sankeyJustify)
        .extent([[1, 1], [width - 1, height - 1]])({
            nodes: names.map(name => ({ name })),
            links: valid.map(l => ({ source: l.source, target: l.target, value: l.value })),
        });

    const path = sankeyLinkHorizontal();
    return {
        nodes: graph.nodes.map(n => ({ name: n.name, depth: n.depth, value: n.value, x0: n.x0, y0: n.y0, x1: n.x1, y1: n.y1 })),
        links: graph.links.map(l => ({ source: l.source.index, target: l.target.index, value: l.value, width: l.width, path: path(l) })),
    };
}
