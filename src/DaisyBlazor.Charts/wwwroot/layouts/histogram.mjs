import { bin, max, median, thresholdSturges } from '../d3.mjs';
import { valueScale, r2 } from './_shared.mjs';
import { cartesian, sorted } from './_distribution.mjs';

// spec: { values, width, height, xTitle, yTitle, bins, showMedian }
// Counts the values into bins with d3.bin. `bins` is a hint (d3 picks nice round edges near it);
// 0 or missing uses Sturges' rule. Bars hold the bin edges, the count and their rectangle.
export default function histogram({ values, width, height, xTitle, yTitle, bins = 0, showMedian = false }) {
    const s = sorted(values);
    const x = valueScale(s);
    const hint = bins >= 1 ? Math.round(bins) : (s.length ? thresholdSturges(s) : 10);
    const counted = s.length ? bin().domain(x.domain()).thresholds(x.ticks(hint).filter(t => t < x.domain()[1]))(s) : [];
    const y = valueScale([max(counted, b => b.length)], { zero: true });
    // The median label sits in its own row above the plot.
    const { plot, xTicks, yTicks } = cartesian({ width, height, x, y, xTitle, yTitle, extraTop: showMedian ? 16 : 0 });
    if (s.length === 0) {
        return { plot, xTicks, yTicks, bars: [], median: null };
    }

    const m = median(s);
    return {
        plot, xTicks, yTicks,
        bars: counted.map(b => ({
            x0: b.x0, x1: b.x1, count: b.length,
            left: r2(x(b.x0)), right: r2(x(b.x1)), top: r2(y(b.length)), bottom: r2(y(0)),
        })),
        median: showMedian ? { value: m, pos: r2(x(m)) } : null,
    };
}
