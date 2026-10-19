// Shared by the graph layouts (chord, network, arc-diagram): turns { links, nodes } into indexed
// nodes and valid links. The leading underscore keeps it out of reach of layout(kind).
import { textWidth } from './_shared.mjs';

// Names are drawn bold, about as wide as 12-unit regular text: the room reserved around a chart.
export const nameWidth = s => textWidth(s, 12);

// nodes: [{ name, group }] in the order given, then every other link endpoint by first appearance.
// links: only those with two different, non-empty names and a positive finite value.
export function graph(links, nodes) {
    const valid = (links ?? []).filter(l => l && l.source && l.target && l.source !== l.target
        && Number.isFinite(l.value) && l.value > 0);
    const byName = new Map();
    const add = (name, group) => {
        if (name && !byName.has(name)) {
            byName.set(name, { name: String(name), group: group ?? null, degree: 0, index: byName.size });
        }
    };
    for (const n of nodes ?? []) {
        add(n?.name, n?.group);
    }
    for (const l of valid) {
        add(l.source);
        add(l.target);
    }
    const list = [...byName.values()];
    const edges = valid.map(l => {
        const s = byName.get(l.source);
        const t = byName.get(l.target);
        s.degree++;
        t.degree++;
        return { source: s.index, target: t.index, value: l.value };
    });
    // Groups in order of first appearance; a node without one has group index -1.
    const groups = [...new Set(list.map(n => n.group).filter(g => g))];
    for (const n of list) {
        n.groupIndex = n.group ? groups.indexOf(n.group) : -1;
    }
    return { nodes: list, links: edges, groups };
}
