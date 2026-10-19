import { scaleLinear } from '../d3/d3-scale.mjs';
import { finite, r2, ticks, valueScale } from './_shared.mjs';
import { parseDate, labelOf, yAxisWidth } from './_time.mjs';

// Time-independent price charts: Renko, Kagi, line break and point & figure. Each algorithm takes
// the closes in time order and returns its items (bricks, lines or columns) with the indices of the
// prices that started (`from`) and completed (`to`) them; the x axis is the item's number, not time.

const EPS = 1e-9;
// A box so small it would draw more than this many boxes is enlarged (Renko, point & figure).
const MAX_BOXES = 2000;

/**
 * Average true range with Wilder's smoothing, at the last price: true range = max(H − L, |H − previous C|,
 * |L − previous C|) from the second price on; the first average is the mean of the first `n` ranges,
 * then avg = (avg·(n − 1) + range) / n. With fewer than `n` ranges, their mean. 0 for a single price.
 */
export function atr(prices, n) {
    const ranges = prices.slice(1).map((p, i) => {
        const prev = prices[i].close;
        return Math.max(p.high - p.low, Math.abs(p.high - prev), Math.abs(p.low - prev));
    });
    if (ranges.length === 0) {
        return 0;
    }
    const first = ranges.slice(0, n);
    let avg = first.reduce((s, v) => s + v, 0) / first.length;
    for (const v of ranges.slice(n)) {
        avg = (avg * (n - 1) + v) / n;
    }
    return avg;
}

/**
 * Renko bricks of `box`, from the first close: a rising brick each time a close is a whole box above
 * the top of the last brick, a falling one each time it is a whole box below its bottom. So a trend
 * goes on after one box and reverses after two (one past the last brick's open).
 */
export function renko(closes, box) {
    const out = [];
    const base = closes[0];
    let hi = 0, lo = 0, start = 0; // the last brick's top and bottom, in boxes from the base
    for (let i = 1; i < closes.length; i++) {
        const level = (closes[i] - base) / box;
        const up = Math.floor(level + EPS) - hi, down = lo - Math.ceil(level - EPS);
        if (up >= 1) {
            for (let k = 0; k < up; k++) {
                out.push({ up: true, open: base + (hi + k) * box, close: base + (hi + k + 1) * box, from: start, to: i });
            }
            hi += up;
            lo = hi - 1;
            start = i;
        } else if (down >= 1) {
            for (let k = 0; k < down; k++) {
                out.push({ up: false, open: base + (lo - k) * box, close: base + (lo - k - 1) * box, from: start, to: i });
            }
            lo -= down;
            hi = lo + 1;
            start = i;
        }
    }
    return out;
}

/**
 * Kagi lines. The first line starts at the first close once a close is `amount(start)` away; a line
 * goes on while closes extend it and turns when a close comes back `amount(extreme)` or more from its
 * end. Thickness: the first line is thick (yang) when it rises, thin (yin) when it falls; a rising line
 * turns thick where it passes the previous shoulder (the top of the last rising line), a falling line
 * turns thin where it passes the previous waist (the bottom of the last falling line). Each line gets
 * `pieces` ([{ from, to, yang }]) and `yang`, its thickness at its end.
 */
export function kagi(closes, amount) {
    const out = [];
    for (let i = 1; i < closes.length; i++) {
        const c = closes[i];
        const last = out[out.length - 1];
        if (!last) {
            const move = Math.abs(c - closes[0]);
            if (move > 0 && move >= amount(closes[0]) - EPS) {
                out.push({ up: c > closes[0], open: closes[0], close: c, from: 0, to: i });
            }
        } else if (last.up ? c > last.close : c < last.close) {
            last.close = c;
            last.to = i;
        } else {
            const back = last.up ? last.close - c : c - last.close;
            if (back > 0 && back >= amount(last.close) - EPS) {
                out.push({ up: !last.up, open: last.close, close: c, from: last.to, to: i });
            }
        }
    }

    let yang = out[0]?.up ?? true, shoulder = null, waist = null;
    for (const line of out) {
        const pivot = line.up ? (!yang && shoulder !== null && line.close > shoulder ? shoulder : null)
            : (yang && waist !== null && line.close < waist ? waist : null);
        if (pivot === null) {
            line.pieces = [{ from: line.open, to: line.close, yang }];
        } else {
            line.pieces = [{ from: line.open, to: pivot, yang }, { from: pivot, to: line.close, yang: !yang }];
            yang = !yang;
        }
        line.yang = yang;
        if (line.up) {
            shoulder = line.close;
        } else {
            waist = line.close;
        }
    }
    return out;
}

/**
 * Line break with `n` lines (3 for a three-line break). The first line runs from the first close to
 * the first different close. Then a close above the last line's top draws a rising line from that top
 * when the last line rose, or when it is also above every one of the last `n` lines (a reversal);
 * likewise down. Other closes draw nothing.
 */
export function lineBreak(closes, n) {
    const out = [];
    for (let i = 1; i < closes.length; i++) {
        const c = closes[i];
        const last = out[out.length - 1];
        if (!last) {
            if (c !== closes[0]) {
                out.push({ up: c > closes[0], open: closes[0], close: c, from: 0, to: i });
            }
            continue;
        }
        const recent = out.slice(-n);
        const top = Math.max(last.open, last.close), bottom = Math.min(last.open, last.close);
        if (c > top && (last.up || c > Math.max(...recent.map(l => Math.max(l.open, l.close))))) {
            out.push({ up: true, open: top, close: c, from: last.to, to: i });
        } else if (c < bottom && (!last.up || c < Math.min(...recent.map(l => Math.min(l.open, l.close))))) {
            out.push({ up: false, open: bottom, close: c, from: last.to, to: i });
        }
    }
    return out;
}

/**
 * Point & figure columns of boxes of `box`, numbered k for the price k·box. An X column fills up to
 * floor(close / box), an O column down to ceil(close / box). The first column starts at the first close's
 * box once a close fills a box beyond it. A column reverses when a close is `reversal` boxes past its
 * end: the new column starts one box away from the old column's end (so it holds `reversal` boxes at least).
 * Each column has `up` (X), `lo` and `hi` box numbers.
 */
export function pointAndFigure(closes, box, reversal) {
    const out = [];
    const floor = c => Math.floor(c / box + EPS), ceil = c => Math.ceil(c / box - EPS);
    for (let i = 1; i < closes.length; i++) {
        const c = closes[i];
        const last = out[out.length - 1];
        if (!last) {
            if (floor(c) > floor(closes[0])) {
                out.push({ up: true, lo: floor(closes[0]), hi: floor(c), from: 0, to: i });
            } else if (ceil(c) < ceil(closes[0])) {
                out.push({ up: false, lo: ceil(c), hi: ceil(closes[0]), from: 0, to: i });
            }
        } else if (last.up) {
            if (floor(c) > last.hi) {
                last.hi = floor(c);
                last.to = i;
            } else if (ceil(c) <= last.hi - reversal) {
                out.push({ up: false, lo: ceil(c), hi: last.hi - 1, from: last.to, to: i });
            }
        } else if (ceil(c) < last.lo) {
            last.lo = ceil(c);
            last.to = i;
        } else if (floor(c) >= last.lo + reversal) {
            out.push({ up: true, lo: last.lo + 1, hi: floor(c), from: last.to, to: i });
        }
    }
    return out;
}

const KINDS = ['renko', 'kagi', 'linebreak', 'pointandfigure'];
const whole = (v, fallback) => (finite(v) && v >= 1 ? Math.floor(v) : fallback);
const EMPTY = { kind: 'renko', boxSize: 0, reversal: 0, items: [], xTicks: [], yTicks: [], plot: { left: 0, top: 0, right: 0, bottom: 0 } };

// spec: { prices: [{ time, open?, high?, low?, close }], kind, boxSize?, reversal?, reversalPercent?, lines?, atrPeriod?, width, height }
// Prices need a time and a finite close (a missing high or low reads as the close); they are sorted by time.
// boxSize (Renko, point & figure): null or ≤ 0 takes the ATR(atrPeriod, default 14) at the last price.
// reversal: Kagi's reversal amount (a percentage of the line's end with reversalPercent), null takes the
//   ATR; point & figure's reversal in boxes (default 3). lines: line break's lines (default 3).
// Returns the box and reversal used (`reversal` is the line count for a line break), the items in order
// with their index (from 1), direction, prices, the dates that started and completed them and their
// geometry, Kagi's strokes (thick or thin) and point & figure's marks (box centres).
export default function priceAction({ prices, kind, boxSize, reversal, reversalPercent, lines, atrPeriod, width, height }) {
    const type = KINDS.includes(String(kind ?? '').toLowerCase()) ? String(kind).toLowerCase() : 'renko';
    const valid = (prices ?? [])
        .map(p => ({ at: parseDate(p?.time), close: p?.close, high: finite(p?.high) ? p.high : p?.close, low: finite(p?.low) ? p.low : p?.close }))
        .filter(p => p.at && finite(p.close))
        .sort((a, b) => a.at - b.at);
    if (valid.length === 0) {
        return { ...EMPTY, kind: type };
    }

    const closes = valid.map(p => p.close);
    const auto = atr(valid, whole(atrPeriod, 14));
    const travel = closes.slice(1).reduce((s, c, i) => s + Math.abs(c - closes[i]), 0);
    const box = type === 'renko' || type === 'pointandfigure'
        ? Math.max(finite(boxSize) && boxSize > 0 ? boxSize : auto, travel / MAX_BOXES) : 0;
    const percent = reversalPercent === true;
    const setting = type === 'kagi' ? (finite(reversal) && reversal >= 0 ? reversal : (percent ? 4 : auto))
        : type === 'pointandfigure' ? whole(reversal, 3)
        : type === 'linebreak' ? whole(lines, 3) : 0;

    // Items as { up, open, close, from, to } (+ pieces for Kagi, lo/hi for point & figure).
    let items;
    if (type === 'kagi') {
        items = kagi(closes, percent ? at => Math.abs(at) * setting / 100 : () => setting);
    } else if (type === 'linebreak') {
        items = lineBreak(closes, setting);
    } else if (!(box > 0)) {
        items = [];
    } else if (type === 'renko') {
        items = renko(closes, box);
    } else {
        items = pointAndFigure(closes, box, setting).map(c => ({ ...c, open: (c.up ? c.lo : c.hi) * box, close: (c.up ? c.hi : c.lo) * box }));
    }

    const half = type === 'pointandfigure' ? box / 2 : 0;
    const y = valueScale(items.length ? items.flatMap(it => [it.open - half, it.open + half, it.close - half, it.close + half]) : closes);
    const top = 10, bottom = height - 24;
    const yCount = Math.max(2, Math.floor((bottom - top) / 40));
    const plot = { left: yAxisWidth(y, yCount), top, right: width - 24, bottom };
    y.range([plot.bottom, plot.top]);

    const n = Math.max(1, items.length);
    const x = scaleLinear().domain([0.5, n + 0.5]).range([plot.left, plot.right]);
    const slot = (plot.right - plot.left) / n;
    const cell = type === 'pointandfigure' ? Math.max(1, Math.min(slot, Math.abs(y(0) - y(box))) * 0.9) : 0;
    const itemWidth = type === 'pointandfigure' ? cell : type === 'kagi' ? Math.max(4, slot * 0.6) : Math.max(1, Math.min(64, slot * 0.9));
    const xTicks = x.ticks(Math.max(2, Math.min(n, Math.floor((plot.right - plot.left) / 50))))
        .filter(v => Number.isInteger(v) && v >= 1 && v <= items.length)
        .map(v => ({ pos: r2(x(v)), label: String(v) }));

    const out = {
        kind: type, boxSize: r2(box), reversal: setting,
        items: items.map((it, i) => ({
            index: i + 1, up: it.up, open: r2(it.open), close: r2(it.close),
            from: labelOf(valid[it.from].at), to: labelOf(valid[it.to].at),
            x: r2(x(i + 1)), width: r2(itemWidth), yOpen: r2(y(it.open)), yClose: r2(y(it.close)),
            ...(type === 'pointandfigure' ? { marks: Array.from({ length: it.hi - it.lo + 1 }, (_, k) => r2(y((it.lo + k) * box))) } : {}),
        })),
        xTicks,
        yTicks: ticks(y, yCount),
        plot,
    };
    if (type === 'kagi') {
        // Each line's vertical pieces, then the shoulder or waist joining it to the next line.
        out.strokes = items.flatMap((it, i) => [
            ...it.pieces.map(p => ({ x1: r2(x(i + 1)), y1: r2(y(p.from)), x2: r2(x(i + 1)), y2: r2(y(p.to)), thick: p.yang })),
            ...(i + 1 < items.length ? [{ x1: r2(x(i + 1)), y1: r2(y(it.close)), x2: r2(x(i + 2)), y2: r2(y(it.close)), thick: it.yang }] : []),
        ]);
    }
    return out;
}
