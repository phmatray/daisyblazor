import { pack } from '../d3.mjs';
import { build, groups, base, EMPTY } from './_hierarchy.mjs';

// spec: { root: { name, value, children }, width, height, padding }
// Every node is a circle (x0..x1, y0..y1 is its bounding box); leaves get room for a centred label.
export default function circlePacking({ root, width, height, padding = 3 }) {
    const h = build(root);
    if (!h) {
        return EMPTY;
    }
    const pad = Number.isFinite(padding) && padding >= 0 ? padding : 3;
    pack().size([width - 2, height - 2]).padding(pad)(h);

    const nodes = h.descendants()
        .filter(d => d.value > 0 && d.r > 0)
        .map(d => ({
            ...base(d),
            x0: d.x + 1 - d.r, y0: d.y + 1 - d.r, x1: d.x + 1 + d.r, y1: d.y + 1 + d.r, d: null,
            labelX: d.x + 1,
            labelY: d.y + 1,
            labelAngle: 0,
            room: d.children ? 0 : d.r * 1.8 - 4,
            thickness: d.children ? 0 : d.r,
        }));
    return { nodes, groups: groups(h), cx: 0, cy: 0 };
}
