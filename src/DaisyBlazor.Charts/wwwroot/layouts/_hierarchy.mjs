import { hierarchy } from '../d3/d3-hierarchy.mjs';

// Shared by the hierarchy layouts (treemap, sunburst, circle-packing, icicle). The leading
// underscore keeps it out of reach of layout(kind), whose kinds start with a letter.

export const EMPTY = { nodes: [], groups: [], cx: 0, cy: 0 };

const positive = v => (Number.isFinite(v) && v > 0 ? v : 0);
const kids = d => (Array.isArray(d.children) && d.children.length > 0 ? d.children : null);

// A summed, largest-first d3 hierarchy of { name, value, children }, or null when nothing has a
// positive value. Only leaves carry value; a parent is the sum of its children. keepOrder leaves the
// children in input order (the strip treemap tiling is defined by that order).
export function build(root, keepOrder = false) {
    if (!root || typeof root !== 'object') {
        return null;
    }
    const h = hierarchy(root, kids).sum(d => (kids(d) ? 0 : positive(d.value)));
    if (!keepOrder) {
        h.sort((a, b) => b.value - a.value);
    }
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
// `names` is the node's place in the whole tree, as the names below the root (the root has none):
// a list, so a name may hold any text, '/' included. `path` is the same joined by ' / ' for tooltips.
export function base(d) {
    const chain = (d.full ?? d).ancestors().reverse();
    return {
        name: String(d.data.name ?? ''),
        path: (chain.length > 1 ? chain.slice(1) : chain).map(a => String(a.data.name ?? '')).join(' / '),
        names: chain.slice(1).map(a => String(a.data.name ?? '')),
        depth: d.depth,
        group: d.group,
        leaf: !d.children,
        value: d.value,
    };
}

// The subtree to lay out for a focus path, the list of names below the root (['Platform', 'api']),
// re-rooted so D3 gives it the full size. An unknown path is the whole tree; a leaf is its parent.
// Nodes keep their value, colour group and (in `full`) their place in the whole tree, for paths.
export function focus(h, path) {
    if (!Array.isArray(path) || path.length === 0) {
        return h;
    }
    let f = h;
    for (const name of path) {
        f = f.children?.find(c => String(c.data.name ?? '') === name);
        if (!f) {
            return h;
        }
    }
    while (!f.children) {
        f = f.parent;
    }
    return f === h ? h : hierarchy(f, n => n.children).eachBefore(n => {
        const o = n.data;
        Object.assign(n, { data: o.data, value: o.value, group: o.group, full: o });
    });
}

// What a layout echoes when the spec asked for a focus: the names of the node it laid out ([] for
// the root). Specs without a focus get nothing, so their result is unchanged.
export function echo(view, path) {
    return Array.isArray(path)
        ? { focus: (view.full ?? view).ancestors().reverse().slice(1).map(a => String(a.data.name ?? '')) }
        : {};
}
