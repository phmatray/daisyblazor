import { partition } from '../d3/d3-hierarchy.mjs';
import { arc } from '../d3/d3-shape.mjs';
import { build, groups, base, EMPTY } from './_hierarchy.mjs';

// spec: { root: { name, value, children }, width, height }
// One ring per level around a centre disc for the root. Paths are relative to (cx, cy); labels run
// along the radius, flipped on the left half so they read left to right.
export default function sunburst({ root, width, height }) {
    const h = build(root);
    if (!h) {
        return EMPTY;
    }
    const radius = Math.max(10, Math.min(width, height) / 2 - 2);
    const ring = radius / (h.height + 1);
    partition().size([2 * Math.PI, h.height + 1])(h);

    const shape = arc()
        .startAngle(d => d.x0)
        .endAngle(d => d.x1)
        .padAngle(d => Math.min((d.x1 - d.x0) / 2, 0.004))
        .padRadius(radius / 2)
        .innerRadius(d => d.y0 * ring)
        .outerRadius(d => Math.max(d.y0 * ring, d.y1 * ring - 1));

    const nodes = h.descendants()
        .filter(d => d.value > 0 && d.x1 - d.x0 > 0.001)
        .map(d => {
            if (d.depth === 0) {
                return { ...base(d), x0: -ring, y0: -ring, x1: ring, y1: ring, d: shape(d), labelX: 0, labelY: 0, labelAngle: 0, room: ring * 2 - 12, thickness: ring };
            }
            const angle = (d.x0 + d.x1) / 2;
            const mid = ((d.y0 + d.y1) / 2) * ring;
            const degrees = (angle * 180) / Math.PI;
            return {
                ...base(d),
                x0: d.x0, y0: d.y0 * ring, x1: d.x1, y1: d.y1 * ring, d: shape(d),
                labelX: Math.sin(angle) * mid,
                labelY: -Math.cos(angle) * mid,
                labelAngle: degrees < 180 ? degrees - 90 : degrees - 270,
                room: ring - 8,
                thickness: (d.x1 - d.x0) * mid,
            };
        });
    return { nodes, groups: groups(h), cx: width / 2, cy: height / 2 };
}
