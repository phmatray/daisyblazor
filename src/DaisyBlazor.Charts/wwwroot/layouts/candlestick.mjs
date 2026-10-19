import { scaleUtc } from '../d3/d3-scale.mjs';
import { area, line } from '../d3/d3-shape.mjs';
import { compact, finite, r2, ticks, valueScale } from './_shared.mjs';
import { parseDate, labelOf, yAxisWidth, timeTicks } from './_time.mjs';

const EMPTY = { candles: [], xTicks: [], yTicks: [], plot: { left: 0, top: 0, right: 0, bottom: 0 } };

// ---- Indicator maths. Every function takes the closes in time order and returns one value per
// close (null until the window is full), so the results line up with the candles.

/** Simple moving average: the mean of the last `n` closes. */
export function sma(values, n) {
    let sum = 0;
    return values.map((v, i) => {
        sum += v - (i >= n ? values[i - n] : 0);
        return i >= n - 1 ? sum / n : null;
    });
}

/** Exponential moving average, α = 2 / (n + 1), seeded with the SMA of the first `n` values. Nulls in the input are skipped (MACD's signal line). */
export function ema(values, n) {
    const a = 2 / (n + 1);
    const out = values.map(() => null);
    let seen = 0, sum = 0, prev = null;
    values.forEach((v, i) => {
        if (v === null) {
            return;
        }
        seen++;
        if (prev === null) {
            sum += v;
            if (seen === n) {
                prev = out[i] = sum / n;
            }
            return;
        }
        prev = out[i] = a * v + (1 - a) * prev;
    });
    return out;
}

/** Bollinger bands: the SMA of `n` closes ± `k` population standard deviations of those closes. */
export function bollinger(values, n, k) {
    const middle = sma(values, n);
    const sigma = middle.map((m, i) => m === null ? null
        : Math.sqrt(values.slice(i - n + 1, i + 1).reduce((s, v) => s + (v - m) ** 2, 0) / n));
    return {
        middle,
        upper: middle.map((m, i) => m === null ? null : m + k * sigma[i]),
        lower: middle.map((m, i) => m === null ? null : m - k * sigma[i]),
    };
}

/** Relative strength index with Wilder's smoothing: the first average gain and loss are plain means of `n` changes, then avg = (avg·(n−1) + change) / n. */
export function rsi(values, n) {
    const out = values.map(() => null);
    let gain = 0, loss = 0;
    for (let i = 1; i < values.length; i++) {
        const change = values[i] - values[i - 1];
        const up = Math.max(change, 0), down = Math.max(-change, 0);
        if (i <= n) {
            gain += up / n;
            loss += down / n;
        } else {
            gain = (gain * (n - 1) + up) / n;
            loss = (loss * (n - 1) + down) / n;
        }
        if (i >= n) {
            out[i] = loss === 0 ? (gain === 0 ? 50 : 100) : 100 - 100 / (1 + gain / loss);
        }
    }
    return out;
}

/** MACD: EMA(fast) − EMA(slow), its signal line EMA(signal) of the MACD, and the histogram MACD − signal. */
export function macd(values, fast, slow, signal) {
    const f = ema(values, fast), s = ema(values, slow);
    const main = values.map((_, i) => f[i] === null || s[i] === null ? null : f[i] - s[i]);
    const sig = ema(main, signal);
    return { main, signal: sig, histogram: main.map((m, i) => m === null || sig[i] === null ? null : m - sig[i]) };
}

/** Price channel: the highest high and lowest low of the last `n` periods (the current one included) and their midpoint. */
export function priceChannel(highs, lows, n) {
    const window = (values, i, pick) => (i < n - 1 ? null : pick(...values.slice(i - n + 1, i + 1)));
    const upper = highs.map((_, i) => window(highs, i, Math.max));
    const lower = lows.map((_, i) => window(lows, i, Math.min));
    return { upper, lower, middle: upper.map((u, i) => (u === null ? null : (u + lower[i]) / 2)) };
}

/**
 * Heikin-Ashi candles: close = (O + H + L + C) / 4; open = (previous HA open + previous HA close) / 2,
 * the first one (O + C) / 2; high and low = the max and min of the period's high or low and the HA open and close.
 */
export function heikinAshi(candles) {
    let prev = null;
    return candles.map(c => {
        const close = (c.open + c.high + c.low + c.close) / 4;
        const open = prev ? (prev.open + prev.close) / 2 : (c.open + c.close) / 2;
        prev = { open, high: Math.max(c.high, open, close), low: Math.min(c.low, open, close), close };
        return prev;
    });
}

const DEFAULT_PERIOD = { sma: 20, ema: 20, bollinger: 20, rsi: 14, macd: 12, pricechannel: 20 };
const whole = (v, fallback) => (finite(v) && v >= 1 ? Math.floor(v) : fallback);

function study(spec, closes, index, candles) {
    const kind = String(spec?.kind ?? '').toLowerCase();
    if (!(kind in DEFAULT_PERIOD)) {
        return null;
    }
    const n = whole(spec.period, DEFAULT_PERIOD[kind]);
    switch (kind) {
        case 'sma': return { index, kind, main: sma(closes, n) };
        case 'ema': return { index, kind, main: ema(closes, n) };
        case 'bollinger': {
            const b = bollinger(closes, n, finite(spec.stdDev) ? spec.stdDev : 2);
            return { index, kind, main: b.middle, upper: b.upper, lower: b.lower };
        }
        case 'pricechannel': {
            const p = priceChannel(candles.map(c => c.high), candles.map(c => c.low), n);
            return { index, kind, main: p.middle, upper: p.upper, lower: p.lower };
        }
        case 'rsi': return { index, kind, main: rsi(closes, n), oscillator: true };
        default: {
            const m = macd(closes, n, whole(spec.slowPeriod, 26), whole(spec.signalPeriod, 9));
            return { index, kind, main: m.main, signal: m.signal, histogram: m.histogram, oscillator: true };
        }
    }
}

const present = list => list.filter(v => v !== null);

// spec: { candles: [{ time, open, high, low, close, volume? }], width, height, indicators?, showVolume? }
// Candles need a time and four finite values; they are sorted by time. Each one gets a slot as wide
// as the smallest gap between neighbours, and the domain is padded by half a slot on both sides
// so the first and last bodies stay inside the plot.
// indicators: [{ kind: 'sma'|'ema'|'bollinger'|'rsi'|'macd', period?, stdDev?, slowPeriod?, signalPeriod? }]
// are computed over the closes. SMA, EMA and Bollinger bands are drawn on the price panel; RSI and
// MACD each get a panel under it; with showVolume a volume panel comes last. All panels share the
// time axis, labelled under the bottom one.
// style (only for the styles that change the geometry; candle, OHLC and high–low leave it out):
//   'line' | 'area': the closes as a line (or an area down to the bottom of the plot), the price
//     scale fitted to the closes; candles keep their values for the tooltips.
//   'heikinashi': the candles are replaced by Heikin-Ashi candles (heikinAshi()); indicators still
//     use the real closes, highs and lows.
//   'equivolume' | 'candlevolume': each body's width is proportional to its volume (the largest
//     takes 90% of a slot, at most 48), still centred on its time so the time axis reads as usual.
//     When any candle has no positive volume, every candle falls back to the usual equal width.
export default function candlestick({ candles, width, height, indicators, showVolume, style }) {
    const valid = (candles ?? [])
        .map(c => ({ ...c, at: parseDate(c?.time) }))
        .filter(c => c.at && [c.open, c.high, c.low, c.close].every(finite))
        .sort((a, b) => a.at - b.at);
    if (valid.length === 0) {
        return EMPTY;
    }

    const closes = valid.map(c => c.close);
    const look = String(style ?? '').toLowerCase();
    const ha = look === 'heikinashi' ? heikinAshi(valid) : null;
    const drawn = ha ? valid.map((c, i) => ({ ...c, ...ha[i] })) : valid;
    const closeOnly = look === 'line' || look === 'area';
    const studies = (Array.isArray(indicators) ? indicators : []).map((s, i) => study(s, closes, i, valid)).filter(Boolean);
    const overlays = studies.filter(s => !s.oscillator);
    const volumes = showVolume ? valid.map(c => (finite(c.volume) ? Math.max(0, c.volume) : null)) : [];
    const hasVolume = volumes.some(v => v !== null);

    const lows = closeOnly ? closes : drawn.map(c => Math.min(c.low, c.high, c.open, c.close));
    const highs = closeOnly ? closes : drawn.map(c => Math.max(c.low, c.high, c.open, c.close));
    const y = valueScale([...lows, ...highs, ...overlays.flatMap(s => present([...s.main, ...(s.upper ?? []), ...(s.lower ?? [])]))]);
    const top = 10, bottom = height - 24;

    // Panels under the price panel: a share of the height each (32 at least when there is room), a
    // gap above each. Together they never take more than 60% of the height, so the price panel keeps
    // 40%: when space runs out the panels and their gaps shrink.
    const below = [...studies.filter(s => s.oscillator), ...(hasVolume ? [{ kind: 'volume' }] : [])];
    const room = (bottom - top) * 0.6 / Math.max(1, below.length);
    const gap = Math.min(14, room / 3);
    const share = below.length ? Math.min(Math.max(32, Math.min((bottom - top) * 0.22, room - 14)), room - gap) : 0;
    const priceBottom = bottom - below.length * (share + gap);

    const yCount = Math.max(2, Math.floor((priceBottom - top) / 40));
    const panels = below.map((p, i) => {
        const pTop = priceBottom + gap + i * (share + gap);
        const scale = p.kind === 'rsi' ? valueScale([0, 100])
            : p.kind === 'macd' ? valueScale(present([...p.main, ...p.signal, ...p.histogram, 0]))
            : valueScale(present(volumes), { zero: true });
        return { study: p, scale, top: pTop, bottom: pTop + share };
    });
    const panelFormat = p => (p.study.kind === 'volume' ? compact : undefined);
    const left = Math.max(yAxisWidth(y, yCount), ...panels.map(p => yAxisWidth(p.scale, 2, panelFormat(p))));
    const plot = { left, top, right: width - 24, bottom: priceBottom };
    y.range([plot.bottom, plot.top]);
    panels.forEach(p => p.scale.range([p.bottom, p.top]));

    const gaps = valid.slice(1).map((c, i) => c.at - valid[i].at).filter(g => g > 0);
    const step = gaps.length ? Math.min(...gaps) : 3600000;
    const x = scaleUtc()
        .domain([new Date(+valid[0].at - step / 2), new Date(+valid[valid.length - 1].at + step / 2)])
        .range([plot.left, plot.right]);
    const slot = x(new Date(+x.domain()[0] + step)) - plot.left;
    const bodyWidth = Math.max(1, Math.min(24, slot * 0.7));
    const byVolume = (look === 'equivolume' || look === 'candlevolume') && valid.every(c => finite(c.volume) && c.volume > 0);
    const maxVolume = byVolume ? Math.max(...valid.map(c => c.volume)) : 0;
    const widthOf = c => (byVolume ? Math.max(1, Math.min(48, slot * 0.9) * c.volume / maxVolume) : bodyWidth);

    // Tick labels sit under the plot; keep the ones that land inside it.
    const xTicks = timeTicks(x, valid.map(c => c.at))
        .filter(t => t.pos >= plot.left + 12 && t.pos <= plot.right - 12);

    const out = {
        candles: drawn.map((c, i) => ({
            label: labelOf(c.at), open: c.open, high: c.high, low: c.low, close: c.close,
            up: c.close >= c.open, x: r2(x(c.at)), width: r2(widthOf(c)),
            yOpen: r2(y(c.open)), yClose: r2(y(c.close)),
            yHigh: r2(y(Math.max(c.high, c.low, c.open, c.close))), yLow: r2(y(Math.min(c.high, c.low, c.open, c.close))),
            ...(hasVolume ? { volume: volumes[i] } : {}),
        })),
        xTicks,
        yTicks: ticks(y, yCount),
        plot,
    };
    const xs = valid.map(c => x(c.at));
    if (closeOnly) {
        out.priceLine = line().x((_, i) => xs[i]).y(v => y(v))(closes) ?? '';
        out.priceArea = look === 'area' ? area().x((_, i) => xs[i]).y0(plot.bottom).y1(v => y(v))(closes) ?? '' : '';
    }
    if (studies.length === 0 && !hasVolume) {
        return out;
    }

    const path = (values, scale) => line().defined(v => v !== null).x((_, i) => xs[i]).y(v => scale(v))(values) ?? '';
    // A bar from zero to each value; `up` colours it (a rising value, or a rising candle for volume).
    const bars = (values, scale, upOf) => values.flatMap((v, i) => (v === null ? [] : [{
        x: r2(xs[i] - bodyWidth / 2), y: r2(Math.min(scale(v), scale(0))), width: r2(bodyWidth),
        height: r2(Math.max(1, Math.abs(scale(v) - scale(0)))), up: upOf(v, i), value: r2(v),
    }]));

    out.indicators = studies.map(s => {
        const panel = panels.findIndex(p => p.study === s);
        const scale = panel < 0 ? y : panels[panel].scale;
        return {
            index: s.index, kind: s.kind, panel,
            line: path(s.main, scale),
            signal: s.signal ? path(s.signal, scale) : '',
            upper: s.upper ? path(s.upper, scale) : '',
            lower: s.lower ? path(s.lower, scale) : '',
            band: s.upper
                ? area().defined(i => s.upper[i] !== null).x(i => xs[i]).y0(i => y(s.lower[i])).y1(i => y(s.upper[i]))(xs.map((_, i) => i)) ?? ''
                : '',
            bars: s.histogram ? bars(s.histogram, scale, v => v >= 0) : [],
            values: s.main.map(v => (v === null ? null : r2(v))),
        };
    });
    out.panels = panels.map(p => {
        const kind = p.study.kind;
        return {
            kind, top: r2(p.top), bottom: r2(p.bottom),
            yTicks: kind === 'rsi' ? [30, 70].map(v => ({ value: v, pos: r2(p.scale(v)), label: String(v) })) : ticks(p.scale, 2, panelFormat(p)),
            // The 30 and 70 lines of an RSI, the zero line of a MACD.
            guides: kind === 'rsi' ? [r2(p.scale(30)), r2(p.scale(70))] : kind === 'macd' ? [r2(p.scale(0))] : [],
            bars: kind === 'volume' ? bars(volumes, p.scale, (_, i) => drawn[i].close >= drawn[i].open) : [],
        };
    });
    return out;
}

