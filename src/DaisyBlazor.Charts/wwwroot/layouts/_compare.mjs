// Helpers shared by the category "compare" layouts (lollipop, dumbbell, range bars). The leading
// underscore keeps it out of reach of charts.mjs's layout(kind).
import { scaleBand } from '../d3/d3-scale.mjs';
import { maxWidth, r2, ticks } from './_shared.mjs';

// A band per category along one side, a value axis along the other, with margins sized to the
// category labels, the tick labels and any value text drawn beyond the low (`before`) or high
// (`after`) end of the value axis. The category axis sits beyond the `before` text.
export function categoryFrame({ width, height, labels, scale, vertical = false, before = 0, after = 0, padding = 0.3, tickCount = 5 }) {
    const tickW = maxWidth(ticks(scale, tickCount).map(t => t.label));
    const labelW = maxWidth(labels);
    const index = labels.map((_, i) => i);
    const center = band => labels.map((label, i) => ({ pos: r2(band(i) + band.bandwidth() / 2), label }));

    if (!vertical) {
        const x0 = Math.max(labelW + 10 + before, tickW / 2 + 2);
        const x1 = width - Math.max(after, tickW / 2) - 4;
        const y0 = 4;
        const y1 = height - 22;
        scale.range([x0, x1]);
        const band = scaleBand(index, [y0, y1]).padding(padding);
        return {
            band, scale,
            plot: { left: r2(x0), top: y0, right: r2(x1), bottom: y1 },
            xTicks: ticks(scale, tickCount),
            yTicks: center(band),
            xAxisAt: y1,
            yAxisAt: r2(x0 - before),
            labelAngle: 0,
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
    // The top tick label is centred on the plot's top edge: leave it half a line and a gap.
    const y0 = 10 + after;
    const y1 = height - labelH - before;
    scale.range([y1, y0]);
    const band = scaleBand(index, [x0, x1]).padding(padding);
    return {
        band, scale,
        plot: { left: r2(x0), top: r2(y0), right: x1, bottom: r2(y1) },
        xTicks: center(band),
        yTicks: ticks(scale, tickCount),
        xAxisAt: r2(y1 + before),
        yAxisAt: r2(x0),
        labelAngle: rotate ? -40 : 0,
    };
}

// The frame's plain-data part, as sent back to .NET.
export const frameOut = ({ plot, xTicks, yTicks, xAxisAt, yAxisAt, labelAngle }) => ({ plot, xTicks, yTicks, xAxisAt, yAxisAt, labelAngle });

// Where a value's text goes beside a mark at (x, y): past the end facing away from the other end.
export function valueText(text, x, y, vertical, high) {
    return vertical
        ? { text, x: r2(x), y: r2(high ? y - 8 : y + 16), anchor: 'middle' }
        : { text, x: r2(high ? x + 8 : x - 8), y: r2(y + 4), anchor: high ? 'start' : 'end' };
}
