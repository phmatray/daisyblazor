import { treemap, treemapSquarify, treemapBinary, treemapSlice, treemapDice, treemapSliceDice } from '../d3/d3-hierarchy.mjs';
import { build, groups, base, focus, echo, EMPTY } from './_hierarchy.mjs';

const TILE = { squarify: treemapSquarify, binary: treemapBinary, slice: treemapSlice, dice: treemapDice, slicedice: treemapSliceDice };
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
