import { treemap, treemapSquarify, treemapBinary, treemapSlice, treemapDice, treemapSliceDice } from '../d3/d3-hierarchy.mjs';
import { build, groups, base, focus, echo, EMPTY } from './_hierarchy.mjs';

/**
 * Ordered strip tiling (Bederson, Shneiderman and Wattenberg, 2002), as a d3 tile function: the
 * children keep their order and fill horizontal strips from top to bottom, left to right in each
 * strip. A strip takes the next child as long as that does not worsen its average aspect ratio.
 */
export function treemapStrip(parent, x0, y0, x1, y1) {
    const nodes = parent.children ?? [];
    const width = x1 - x0;
    const k = parent.value > 0 && width > 0 ? width * (y1 - y0) / parent.value : 0;
    const sum = row => row.reduce((total, d) => total + d.value, 0);
    // Mean aspect ratio of a strip spanning the full width; a zero-sized tile counts as square.
    const ratio = row => {
        const h = sum(row) * k / width;
        return row.reduce((total, d) => {
            const w = d.value * k / h;
            return total + (w > 0 && h > 0 ? Math.max(w / h, h / w) : 1);
        }, 0) / row.length;
    };
    const rows = [];
    let row = [];
    for (const node of nodes) {
        if (row.length > 0 && k > 0 && ratio([...row, node]) > ratio(row)) {
            rows.push(row);
            row = [];
        }
        row.push(node);
    }
    if (row.length > 0) {
        rows.push(row);
    }
    let y = y0;
    rows.forEach((r, i) => {
        // The last strip ends exactly at the bottom, whatever the rounding.
        const next = i === rows.length - 1 ? y1 : y + (k > 0 ? sum(r) * k / width : (y1 - y0) / rows.length);
        treemapDice({ children: r, value: sum(r) }, x0, y, x1, next);
        y = next;
    });
}

const TILE = { squarify: treemapSquarify, binary: treemapBinary, slice: treemapSlice, dice: treemapDice, slicedice: treemapSliceDice, strip: treemapStrip };
const HEADER = 18;

// spec: { root: { name, value, children }, focus?: ['Branch', 'child'], width, height, tile, padding }
// Every node but the root: leaves are tiles, parents are frames with an 18-unit header for their name.
export default function treemapLayout({ root, focus: path, width, height, tile = 'squarify', padding = 2 }) {
    const full = build(root);
    if (!full) {
        return EMPTY;
    }
    const h = focus(full, path);
    const pad = Number.isFinite(padding) && padding >= 0 ? padding : 2;
    const frame = d => (d.depth > 0 ? pad : 0);
    treemap()
        .size([width, height])
        .tile(TILE[tile] ?? treemapSquarify)
        .paddingInner(pad)
        .paddingTop(d => (d.depth > 0 ? HEADER : 0))
        .paddingRight(frame)
        .paddingBottom(frame)
        .paddingLeft(frame)(h);

    const nodes = h.descendants()
        .filter(d => d.depth > 0 && d.value > 0 && d.x1 - d.x0 > 0.5 && d.y1 - d.y0 > 0.5)
        .map(d => ({
            ...base(d),
            x0: d.x0, y0: d.y0, x1: d.x1, y1: d.y1, d: null,
            labelX: d.x0 + 5,
            labelY: d.y0 + (d.children ? HEADER / 2 : 14),
            labelAngle: 0,
            room: d.x1 - d.x0 - 10,
            thickness: d.children ? Math.min(HEADER, d.y1 - d.y0) : d.y1 - d.y0,
        }));
    return { nodes, groups: groups(full), cx: 0, cy: 0, ...echo(h, path) };
}
