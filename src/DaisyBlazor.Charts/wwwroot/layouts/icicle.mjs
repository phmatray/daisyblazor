import { partition } from '../d3/d3-hierarchy.mjs';
import { build, groups, base, focus, echo, EMPTY } from './_hierarchy.mjs';

// spec: { root: { name, value, children }, focus?: "Branch/child", width, height, orientation: 'horizontal' | 'vertical' }
// Horizontal: one column per level, root on the left. Vertical: one row per level, root on top.
export default function icicle({ root, focus: path, width, height, orientation = 'horizontal' }) {
    const full = build(root);
    if (!full) {
        return EMPTY;
    }
    const h = focus(full, path);
    const vertical = orientation === 'vertical';
    partition().size(vertical ? [width, height] : [height, width])(h);

    const nodes = h.descendants()
        .filter(d => d.value > 0 && d.x1 - d.x0 > 0.5)
        .map(d => {
            const [x0, y0, x1, y1] = vertical ? [d.x0, d.y0, d.x1, d.y1] : [d.y0, d.x0, d.y1, d.x1];
            return {
                ...base(d),
                x0, y0, x1, y1, d: null,
                labelX: x0 + 5,
                labelY: y0 + 14,
                labelAngle: 0,
                room: x1 - x0 - 10,
                thickness: y1 - y0,
            };
        });
    return { nodes, groups: groups(full), cx: 0, cy: 0, ...echo(h, path) };
}
