// Helpers shared by the "compare" layouts (radar, bullet, dumbbell, slope, lollipop, parallel,
// rangebar). The leading underscore keeps it out of reach of charts.mjs's layout(kind).
import { scaleLinear, scaleBand } from '../d3.mjs';

export const finite = v => typeof v === 'number' && Number.isFinite(v);

export const r2 = v => Math.round(v * 100) / 100;

// Estimated width of a label in SVG units. Charts draw text in the page font, which the layout
// cannot measure, so margins use a generous average glyph width.
// ponytail: estimate, not measurement; measure with getComputedTextLength if a font ever overflows.
export const textWidth = (s, size = 11) => String(s ?? '').length * size * 0.62;

export const maxWidth = (list, size = 11) => list.reduce((m, s) => Math.max(m, textWidth(s, size)), 0);

// A linear scale over the finite values, extended to nice round ends (and to zero when asked).
export function valueScale(values, { zero = true, count = 5 } = {}) {
    const v = values.filter(finite);
    if (v.length === 0) {
        v.push(0);
    }
    let lo = Math.min(...v, ...(zero ? [0] : []));
    let hi = Math.max(...v, ...(zero ? [0] : []));
    if (lo === hi) {
        lo -= lo === 0 ? 0 : 1;
        hi += 1;
    }
    return scaleLinear().domain([lo, hi]).nice(count);
}

// The scale's ticks, each with its label from the scale's own tick format.
export function ticks(scale, count = 5) {
    const format = scale.tickFormat(count);
    return scale.ticks(count).map(value => ({ value, label: format(value) }));
}

// Moves label positions apart so neighbours are at least `gap` apart, keeping them inside
// [lo, hi] when there is room. Returns the new positions in the input's order.
export function dodge(ys, gap, lo, hi) {
    const order = ys.map((_, i) => i).sort((a, b) => ys[a] - ys[b] || a - b);
    const out = ys.slice();
    let prev = -Infinity;
    for (const i of order) {
        out[i] = Math.max(out[i], prev + gap, lo);
        prev = out[i];
    }
    let next = Infinity;
    for (const i of order.reverse()) {
        out[i] = Math.min(out[i], next - gap, hi);
        next = out[i];
    }
    return out;
}

// Category charts (lollipop, dumbbell, range bars): a band per category along one side, a value
// axis along the other, with margins sized to the category labels, the tick labels and any value
// text drawn beyond the low (`before`) or high (`after`) end of the value axis.
export function categoryFrame({ width, height, labels, scale, vertical = false, before = 0, after = 0, padding = 0.3, tickCount = 5 }) {
    const tickList = ticks(scale, tickCount);
    const tickW = maxWidth(tickList.map(t => t.label), 11);
    const labelW = maxWidth(labels, 12);
    const index = labels.map((_, i) => i);

    if (!vertical) {
        const x0 = Math.max(labelW + 10 + before, tickW / 2 + 2);
        const x1 = width - Math.max(after, tickW / 2) - 4;
        const y0 = 4;
        const y1 = height - 22;
        scale.range([x0, x1]);
        const band = scaleBand(index, [y0, y1]).padding(padding);
        return {
            x0, x1, y0, y1, band, scale,
            ticks: tickList.map(t => {
                const x = r2(scale(t.value));
                return { label: t.label, x, y: y1 + 15, anchor: 'middle', x1: x, y1: y0, x2: x, y2: y1 };
            }),
            categories: labels.map((text, i) => ({ text, x: r2(x0 - before - 8), y: r2(band(i) + band.bandwidth() / 2 + 4), anchor: 'end', rotate: 0 })),
        };
    }

    let x0 = tickW + 10;
    const x1 = width - 4;
    const step = (x1 - x0) / Math.max(1, labels.length);
    const rotate = labelW > step - 6;
    if (rotate) {
        // Labels turn by 40 degrees and hang down-left from their category, so the first one needs room.
        x0 = Math.max(x0, labelW * 0.77 - step / 2 + 4);
    }
    const labelH = rotate ? labelW * 0.65 + 14 : 18;
    const y0 = 4 + after;
    const y1 = height - labelH - before;
    scale.range([y1, y0]);
    const band = scaleBand(index, [x0, x1]).padding(padding);
    return {
        x0, x1, y0, y1, band, scale,
        ticks: tickList.map(t => {
            const y = r2(scale(t.value));
            return { label: t.label, x: r2(x0 - 6), y: r2(y + 4), anchor: 'end', x1: x0, y1: y, x2: x1, y2: y };
        }),
        categories: labels.map((text, i) => ({
            text,
            x: r2(band(i) + band.bandwidth() / 2),
            y: r2(y1 + before + 14),
            anchor: rotate ? 'end' : 'middle',
            rotate: rotate ? -40 : 0,
        })),
    };
}

// The frame's plain-data part, as sent back to .NET.
export const frameOut = f => ({ plot: { x: r2(f.x0), y: r2(f.y0), width: r2(f.x1 - f.x0), height: r2(f.y1 - f.y0) }, ticks: f.ticks, categories: f.categories });

// Where a value's text goes beside a mark at (x, y): past the end facing away from the other end.
export function valueText(text, x, y, vertical, high) {
    return vertical
        ? { text, x: r2(x), y: r2(high ? y - 8 : y + 16), anchor: 'middle', rotate: 0 }
        : { text, x: r2(high ? x + 8 : x - 8), y: r2(y + 4), anchor: high ? 'start' : 'end', rotate: 0 };
}
