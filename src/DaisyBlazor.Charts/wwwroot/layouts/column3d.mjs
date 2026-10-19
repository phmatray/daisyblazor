import { scaleBand } from '../d3/d3-scale.mjs';
import { LABEL_ROW, finite, r2, textWidth, maxWidth, fit, compact, valueScale, ticks } from './_shared.mjs';

// 3D columns. spec: { categories: [string], series: [[number|null]], stacked, horizontal, depth,
// width, height }. Each column is a prism seen from the front, slightly from above and the right:
// a front rectangle on the value scale, and a top and a right side pushed back along the depth
// direction (30° up and to the right). A floor under the columns and a back wall carry the
// gridlines. Columns come back in drawing order, back to front, so neighbours cover each other
// correctly.

const ANGLE = Math.PI / 6;

/** The top and right side of the prism in front of rectangle (x, y, w, h), pushed back by (dx, dy). */
export function prism(x, y, w, h, dx, dy) {
    const p = pts => pts.map(([a, b]) => `${r2(a)},${r2(b)}`).join(' ');
    return {
        front: p([[x, y], [x + w, y], [x + w, y + h], [x, y + h]]),
        top: p([[x, y], [x + w, y], [x + w + dx, y + dy], [x + dx, y + dy]]),
        side: p([[x + w, y], [x + w + dx, y + dy], [x + w + dx, y + h + dy], [x + w, y + h]]),
    };
}

export default function column3d({ categories = [], series = [], stacked = false, horizontal = false, depth = 0.5, width, height }) {
    const n = categories.length;
    const values = [];
    // Stacked: positives pile up from zero, negatives down from it.
    const ends = series.map(() => []);
    if (stacked) {
        for (let c = 0; c < n; c++) {
            let pos = 0, neg = 0;
            series.forEach((data, s) => {
                const v = data?.[c];
                if (!finite(v)) {
                    return;
                }
                const from = v >= 0 ? pos : neg;
                if (v >= 0) pos += v; else neg += v;
                ends[s][c] = [from, from + v];
                values.push(from + v);
            });
        }
    } else {
        series.forEach((data, s) => (data ?? []).forEach((v, c) => {
            if (finite(v) && c < n) {
                ends[s][c] = [0, v];
                values.push(v);
            }
        }));
    }

    const value = valueScale(values, { zero: true, count: 5 });
    const share = Math.min(1, Math.max(0, finite(depth) ? depth : 0.5));

    // The depth offset depends on the column width, which depends on the room the offset leaves:
    // estimate it from a first pass, keeping the scene at most 1/6 of the box.
    const groups = stacked ? 1 : Math.max(1, series.length);
    const along = horizontal ? height : width;
    const guess = n > 0 ? along / n * 0.7 / groups : 0;
    const back = Math.min(guess * share * 1.25, Math.min(width, height) / 6);
    const dx = r2(back * Math.cos(ANGLE)), dy = r2(-back * Math.sin(ANGLE));

    const top = 8 - dy;
    let left, right, bottom;
    if (horizontal) {
        left = (n > 0 ? Math.min(maxWidth(categories), width * 0.35) : 0) + 8;
        bottom = height - LABEL_ROW;
        right = width - Math.max(dx + 6, textWidth(ticks(value, 5, compact).at(-1)?.label ?? '') / 2 + 2);
        value.range([left, right]);
    } else {
        bottom = height - (n > 0 ? LABEL_ROW : 4);
        value.range([bottom, top]);
        left = Math.max(0, ...ticks(value, 5, compact).map(t => textWidth(t.label))) + 8;
        right = width - dx - 6;
    }
    const valueTicks = ticks(value, 5, compact);

    const band = scaleBand().domain([...categories.keys()]).range(horizontal ? [top, bottom] : [left, right])
        .paddingInner(0.3).paddingOuter(0.2);
    const inner = scaleBand().domain([...series.keys()]).range([0, band.bandwidth()]).paddingInner(stacked ? 0 : 0.08);
    const thick = stacked ? band.bandwidth() : inner.bandwidth();
    // A column is as deep as `depth` of its width, within the scene's depth.
    const k = Math.min(1, back > 0 ? (thick * share) / back : 0);
    const cdx = r2(dx * k), cdy = r2(dy * k);

    const columns = [];
    series.forEach((_, s) => categories.forEach((__, c) => {
        const e = ends[s][c];
        if (!e) {
            return;
        }
        const a = value(e[0]), b = value(e[1]);
        const start = band(c) + (stacked ? 0 : inner(s));
        const box = horizontal
            ? { x: Math.min(a, b), y: start, w: Math.max(1, Math.abs(b - a)), h: thick }
            : { x: start, y: Math.min(a, b), w: thick, h: Math.max(1, Math.abs(b - a)) };
        columns.push({ c, s, value: e[1] - e[0], x: r2(box.x), y: r2(box.y), width: r2(box.w), height: r2(box.h), ...prism(box.x, box.y, box.w, box.h, cdx, cdy) });
    }));
    // Back to front: along the category axis first (left to right, bottom to top), then within a stack.
    columns.sort(horizontal ? (p, q) => q.y - p.y || p.x - q.x : (p, q) => p.x - q.x || q.y - p.y);

    // The floor runs under the plot, the wall at its back.
    const floor = `${r2(left)},${r2(bottom)} ${r2(right)},${r2(bottom)} ${r2(right + dx)},${r2(bottom + dy)} ${r2(left + dx)},${r2(bottom + dy)}`;
    const wall = `${r2(left + dx)},${r2(top + dy)} ${r2(right + dx)},${r2(top + dy)} ${r2(right + dx)},${r2(bottom + dy)} ${r2(left + dx)},${r2(bottom + dy)}`;
    // Each gridline runs along the back wall and comes forward along the floor (horizontal) or the left edge.
    const grid = valueTicks.map(t => horizontal
        ? { value: t.value, path: `M${t.pos},${r2(bottom)}L${r2(t.pos + dx)},${r2(bottom + dy)}L${r2(t.pos + dx)},${r2(top + dy)}` }
        : { value: t.value, path: `M${r2(left)},${t.pos}L${r2(left + dx)},${r2(t.pos + dy)}L${r2(right + dx)},${r2(t.pos + dy)}` });

    const room = horizontal ? left - 8 : band.step() - 4;
    const categoryTicks = categories.map((label, c) => ({
        pos: r2(band(c) + band.bandwidth() / 2),
        label: fit(label, room),
        full: String(label ?? ''),
    }));
    return {
        plot: { left: r2(left), top: r2(top), right: r2(right), bottom: r2(bottom) },
        xTicks: horizontal ? valueTicks : categoryTicks,
        yTicks: horizontal ? categoryTicks : valueTicks,
        floor: n > 0 ? floor : '',
        wall: n > 0 ? wall : '',
        grid: n > 0 ? grid : [],
        columns,
    };
}
