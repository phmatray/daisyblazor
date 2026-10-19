import { scaleUtc } from '../d3.mjs';
import { finite, r2, ticks, valueScale } from './_shared.mjs';
import { parseDate, labelOf, yAxisWidth, timeTicks } from './_time.mjs';

const EMPTY = { candles: [], xTicks: [], yTicks: [], plot: { left: 0, top: 0, right: 0, bottom: 0 } };

// spec: { candles: [{ time, open, high, low, close }], width, height }
// Candles need a time and four finite values; they are sorted by time. Each one gets a slot as wide
// as the smallest gap between neighbours, and the domain is padded by half a slot on both sides
// so the first and last bodies stay inside the plot.
export default function candlestick({ candles, width, height }) {
    const valid = (candles ?? [])
        .map(c => ({ ...c, at: parseDate(c?.time) }))
        .filter(c => c.at && [c.open, c.high, c.low, c.close].every(finite))
        .sort((a, b) => a.at - b.at);
    if (valid.length === 0) {
        return EMPTY;
    }

    const lows = valid.map(c => Math.min(c.low, c.high, c.open, c.close));
    const highs = valid.map(c => Math.max(c.low, c.high, c.open, c.close));
    const y = valueScale([...lows, ...highs]);
    const top = 10, bottom = height - 24;
    const yCount = Math.max(2, Math.floor((bottom - top) / 40));
    const plot = { left: yAxisWidth(y, yCount), top, right: width - 24, bottom };
    y.range([plot.bottom, plot.top]);

    const gaps = valid.slice(1).map((c, i) => c.at - valid[i].at).filter(g => g > 0);
    const step = gaps.length ? Math.min(...gaps) : 3600000;
    const x = scaleUtc()
        .domain([new Date(+valid[0].at - step / 2), new Date(+valid[valid.length - 1].at + step / 2)])
        .range([plot.left, plot.right]);
    const slot = x(new Date(+x.domain()[0] + step)) - plot.left;
    const bodyWidth = Math.max(1, Math.min(24, slot * 0.7));

    // Tick labels sit under the plot; keep the ones that land inside it.
    const xTicks = timeTicks(x, valid.map(c => c.at))
        .filter(t => t.pos >= plot.left + 12 && t.pos <= plot.right - 12);

    return {
        candles: valid.map(c => ({
            label: labelOf(c.at), open: c.open, high: c.high, low: c.low, close: c.close,
            up: c.close >= c.open, x: r2(x(c.at)), width: r2(bodyWidth),
            yOpen: r2(y(c.open)), yClose: r2(y(c.close)),
            yHigh: r2(y(Math.max(c.high, c.low, c.open, c.close))), yLow: r2(y(Math.min(c.high, c.low, c.open, c.close))),
        })),
        xTicks,
        yTicks: ticks(y, yCount),
        plot,
    };
}
