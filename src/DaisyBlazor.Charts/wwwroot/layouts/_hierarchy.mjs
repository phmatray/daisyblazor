import { hierarchy } from '../d3.mjs';

// Shared by the hierarchy layouts (treemap, sunburst, circle-packing, icicle). The leading
// underscore keeps it out of reach of layout(kind), whose kinds start with a letter.

export const EMPTY = { nodes: [], groups: [], cx: 0, cy: 0 };

const positive = v => (Number.isFinite(v) && v > 0 ? v : 0);
const kids = d => (Array.isArray(d.children) && d.children.length > 0 ? d.children : null);

// A summed, largest-first d3 hierarchy of { name, value, children }, or null when nothing has a
// positive value. Only leaves carry value; a parent is the sum of its children.
export function build(root) {
    if (!root || typeof root !== 'object') {
        return null;
    }
    const h = hierarchy(root, kids)
        .sum(d => (kids(d) ? 0 : positive(d.value)))
        .sort((a, b) => b.value - a.value);
    if (!(h.value > 0)) {
        return null;
    }
    // Colour group = index of the top-level branch; the root itself is -1 (unless it is the only node).
    h.group = h.children ? -1 : 0;
    (h.children ?? []).forEach((c, i) => c.each(d => { d.group = i; }));
    return h;
}

// Top-level branch names, in group order, for a legend.
export function groups(h) {
    return (h.children ?? [h]).filter(c => c.value > 0).map(c => String(c.data.name ?? ''));
}

// The plain fields every hierarchy node returns; geometry and label room are added by each layout.
export function base(d) {
    const chain = d.ancestors().reverse();
    return {
        name: String(d.data.name ?? ''),
        path: (chain.length > 1 ? chain.slice(1) : chain).map(a => String(a.data.name ?? '')).join(' / '),
        depth: d.depth,
        group: d.group,
        leaf: !d.children,
        value: d.value,
    };
}
