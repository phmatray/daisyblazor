// node --test tests/ — financial chart maths (Heikin-Ashi, price channel, Renko, Kagi, line break,
// point & figure) against hand-worked series, and the layouts that draw them.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';
import { heikinAshi, priceChannel } from '../src/DaisyBlazor.Charts/wwwroot/layouts/candlestick.mjs';
import priceActionLayout, { atr, renko, kagi, lineBreak, pointAndFigure } from '../src/DaisyBlazor.Charts/wwwroot/layouts/price-action.mjs';

const W = 640, H = 360;
const at = i => new Date(Date.UTC(2026, 9, 19, 9 + i)).toISOString().slice(0, 19);
const pick = (list, ...keys) => list.map(o => Object.fromEntries(keys.map(k => [k, o[k]])));

test('heikin-ashi: close = (O+H+L+C)/4, open = (prev HA open + prev HA close)/2 seeded with (O+C)/2, high/low include them', () => {
    const ha = heikinAshi([
        { open: 10, high: 12, low: 9, close: 11 },
        { open: 11, high: 14, low: 10, close: 13 },
        { open: 13, high: 13, low: 8, close: 9 },
    ]);
    // 1: close 42/4 = 10.5, open (10 + 11)/2 = 10.5. 2: close 48/4 = 12, open (10.5 + 10.5)/2 = 10.5,
    // low min(10, 10.5, 12) = 10. 3: close 43/4 = 10.75, open (10.5 + 12)/2 = 11.25, high max(13, 11.25, 10.75).
    assert.deepEqual(ha, [
        { open: 10.5, high: 12, low: 9, close: 10.5 },
        { open: 10.5, high: 14, low: 10, close: 12 },
        { open: 11.25, high: 13, low: 8, close: 10.75 },
    ]);
});

test('price channel: highest high and lowest low of the last n periods, midpoint between', () => {
    const p = priceChannel([5, 7, 6, 4], [3, 4, 2, 1], 2);
    assert.deepEqual(p.upper, [null, 7, 7, 6]);
    assert.deepEqual(p.lower, [null, 3, 2, 1]);
    assert.deepEqual(p.middle, [null, 5, 4.5, 3.5]);
});

test('atr: Wilder average of true ranges at the last price', () => {
    const prices = [{ high: 11, low: 9, close: 10 }, { high: 12, low: 10, close: 11 }, { high: 14, low: 11, close: 13 }, { high: 13, low: 10, close: 10 }];
    // Ranges 2, 3, 3 (the last: |10 − 13|). First average (2 + 3)/2 = 2.5, then (2.5 + 3)/2 = 2.75.
    assert.equal(atr(prices, 2), 2.75);
    assert.equal(atr(prices, 14), 8 / 3); // fewer ranges than the period: their mean
    assert.equal(atr(prices.slice(0, 1), 14), 0);
});

test('renko: a box to go on, two to reverse', () => {
    const bricks = renko([10, 10.5, 11.2, 13, 12.4, 11, 9.9], 1);
    assert.deepEqual(pick(bricks, 'up', 'open', 'close', 'from', 'to'), [
        { up: true, open: 10, close: 11, from: 0, to: 2 },
        { up: true, open: 11, close: 12, from: 2, to: 3 },
        { up: true, open: 12, close: 13, from: 2, to: 3 },
        // 12.4 is neither a box above 13 nor a box below 12; 11 is: the reversal starts at 12.
        { up: false, open: 12, close: 11, from: 3, to: 5 },
        { up: false, open: 11, close: 10, from: 5, to: 6 },
    ]);
});

test('kagi: turns on the reversal amount, yang above the last shoulder, yin below the last waist', () => {
    const lines = kagi([10, 11, 13, 12, 10.5, 14, 15, 11, 9, 8, 12, 16], () => 2);
    assert.deepEqual(pick(lines, 'up', 'open', 'close', 'yang'), [
        { up: true, open: 10, close: 13, yang: true }, // rises first: yang
        { up: false, open: 13, close: 10.5, yang: true }, // no waist yet
        { up: true, open: 10.5, close: 15, yang: true },
        { up: false, open: 15, close: 8, yang: false }, // passes the waist at 10.5
        { up: true, open: 8, close: 16, yang: true }, // passes the shoulder at 15
    ]);
    assert.deepEqual(lines[3].pieces, [{ from: 15, to: 10.5, yang: true }, { from: 10.5, to: 8, yang: false }]);
    assert.deepEqual(lines[4].pieces, [{ from: 8, to: 15, yang: false }, { from: 15, to: 16, yang: true }]);
    // 4 % of the line's end: 104 − 101 = 3 < 4.16 holds, 104 − 99 = 5 turns.
    const pct = kagi([100, 104, 101, 99], v => v * 0.04);
    assert.deepEqual(pick(pct, 'up', 'open', 'close'), [{ up: true, open: 100, close: 104 }, { up: false, open: 104, close: 99 }]);
});

test('line break: reversals must pass the extreme of the last n lines', () => {
    const lines = lineBreak([10, 11, 12, 13, 12.5, 11.5, 9.5, 9, 10.5, 13.5], 3);
    assert.deepEqual(pick(lines, 'up', 'open', 'close', 'from', 'to'), [
        { up: true, open: 10, close: 11, from: 0, to: 1 },
        { up: true, open: 11, close: 12, from: 1, to: 2 },
        { up: true, open: 12, close: 13, from: 2, to: 3 },
        // 12.5 and 11.5 stay above the lowest of the last three lines (10); 9.5 breaks it.
        { up: false, open: 12, close: 9.5, from: 3, to: 6 },
        { up: false, open: 9.5, close: 9, from: 6, to: 7 },
        // 10.5 stays under the highest of the last three (13); 13.5 breaks it.
        { up: true, open: 9.5, close: 13.5, from: 7, to: 9 },
    ]);
    // A two-line break reverses on less: 11.5 is under the lowest of the last two lines (11).
    assert.equal(lineBreak([10, 11, 12, 13, 10.5], 2).at(-1).up, false);
});

test('point & figure: X and O columns, a 3-box reversal starting one box from the end', () => {
    const cols = pointAndFigure([10.2, 11.5, 13.7, 12.1, 10.9, 9.6, 11.2, 12.8, 14.3], 1, 3);
    assert.deepEqual(pick(cols, 'up', 'lo', 'hi', 'from', 'to'), [
        { up: true, lo: 10, hi: 13, from: 0, to: 2 },
        { up: false, lo: 10, hi: 12, from: 2, to: 5 }, // 9.6 fills down to 10, three boxes under 13
        { up: true, lo: 11, hi: 14, from: 5, to: 8 },
    ]);
});

const prices = closes => closes.map((close, i) => ({ time: at(i), open: close, high: close, low: close, close }));

test('price-action layout: Renko bricks numbered on the x axis, prices on the y axis, dates of each move', async () => {
    const out = await layout('price-action', { prices: prices([10, 10.5, 11.2, 13, 12.4, 11, 9.9]), kind: 'renko', boxSize: 1, width: W, height: H });
    assert.equal(out.kind, 'renko');
    assert.equal(out.boxSize, 1);
    assert.deepEqual(out.items.map(i => [i.index, i.up, i.open, i.close]), [[1, true, 10, 11], [2, true, 11, 12], [3, true, 12, 13], [4, false, 12, 11], [5, false, 11, 10]]);
    assert.equal(out.items[0].from, 'Oct 19, 2026 09:00');
    assert.equal(out.items[0].to, 'Oct 19, 2026 11:00');
    assert.deepEqual(out.xTicks.map(t => t.label), ['1', '2', '3', '4', '5']);
    for (const it of out.items) {
        assert.ok(it.x - it.width / 2 >= out.plot.left && it.x + it.width / 2 <= out.plot.right);
        assert.ok(Math.min(it.yOpen, it.yClose) >= out.plot.top && Math.max(it.yOpen, it.yClose) <= out.plot.bottom);
    }
    assert.ok(out.items[2].yClose < out.items[0].yOpen); // higher prices are higher up
    assert.equal(out.items[1].x - out.items[0].x, out.items[2].x - out.items[1].x);
});

test('price-action layout: without a box size Renko takes the ATR', async () => {
    const candles = [
        { time: at(0), open: 10, high: 11, low: 9, close: 10 }, { time: at(1), open: 10, high: 12, low: 10, close: 11 },
        { time: at(2), open: 11, high: 14, low: 11, close: 13 }, { time: at(3), open: 13, high: 13, low: 10, close: 10 },
    ];
    const out = await layout('price-action', { prices: candles, kind: 'renko', atrPeriod: 2, width: W, height: H });
    assert.equal(out.boxSize, 2.75);
    assert.deepEqual(out.items.map(i => [i.up, i.open, i.close]), [[true, 10, 12.75]]);
});

test('price-action layout: Kagi strokes are thick for yang, thin for yin, joined at the turns', async () => {
    const out = await layout('price-action', { prices: prices([10, 11, 13, 12, 10.5, 14, 15, 11, 9, 8, 12, 16]), kind: 'kagi', reversal: 2, width: W, height: H });
    assert.equal(out.reversal, 2);
    assert.equal(out.items.length, 5);
    // 5 lines, 2 of them split in two, and 4 joins.
    assert.equal(out.strokes.length, 7 + 4);
    const vertical = out.strokes.filter(s => s.x1 === s.x2);
    assert.deepEqual(vertical.map(s => s.thick), [true, true, true, true, false, false, true]);
    const joins = out.strokes.filter(s => s.y1 === s.y2);
    assert.deepEqual(joins.map(s => s.thick), [true, true, true, false]);
    assert.equal(joins[0].x1, out.items[0].x);
    assert.equal(joins[0].x2, out.items[1].x);
    assert.equal(joins[0].y1, out.items[0].yClose);
});

test('price-action layout: line break and point & figure', async () => {
    const lb = await layout('price-action', { prices: prices([10, 11, 12, 13, 12.5, 11.5, 9.5, 9, 10.5, 13.5]), kind: 'lineBreak', width: W, height: H });
    assert.equal(lb.kind, 'linebreak');
    assert.equal(lb.reversal, 3);
    assert.deepEqual(lb.items.map(i => i.up), [true, true, true, false, false, true]);

    const pf = await layout('price-action', { prices: prices([10.2, 11.5, 13.7, 12.1, 10.9, 9.6, 11.2, 12.8, 14.3]), kind: 'pointAndFigure', boxSize: 1, width: W, height: H });
    assert.equal(pf.reversal, 3);
    assert.deepEqual(pf.items.map(i => [i.up, i.open, i.close, i.marks.length]), [[true, 10, 13, 4], [false, 12, 10, 3], [true, 11, 14, 4]]);
    const [x] = pf.items;
    assert.ok(x.marks.every((m, k) => k === 0 || m < x.marks[k - 1])); // X boxes stacked upwards
    assert.ok(Math.abs(x.marks[0] - x.marks[1]) >= x.width); // cells fit their box
    for (const m of pf.items.flatMap(i => i.marks)) {
        assert.ok(m - x.width / 2 >= pf.plot.top && m + x.width / 2 <= pf.plot.bottom);
    }
});

test('price-action: tiny boxes are capped, flat or empty prices give no items', () => {
    const wobble = Array.from({ length: 50 }, (_, i) => 10 + (i % 2) * 5);
    const out = priceActionLayout({ prices: prices(wobble), kind: 'renko', boxSize: 1e-9, width: W, height: H });
    assert.equal(out.boxSize, 0.12); // 49 × 5 travelled / 2000
    assert.ok(out.items.length <= 2000);
    assert.deepEqual(priceActionLayout({ prices: prices([5, 5, 5]), kind: 'pointandfigure', width: W, height: H }).items, []);
    assert.deepEqual(priceActionLayout({ prices: [], kind: 'kagi', width: W, height: H }).items, []);
    assert.equal(priceActionLayout({ prices: prices([1, 2]), kind: 'nope', boxSize: 1, width: W, height: H }).kind, 'renko');
});

const hourly = (n, f) => Array.from({ length: n }, (_, i) => {
    const open = f(i), close = f(i + 1);
    return { time: at(i), open, high: Math.max(open, close) + 2, low: Math.min(open, close) - 2, close, volume: 100 + (i % 5) * 50 };
});

test('candlestick styles: price line and area over the closes, candles kept for tooltips', async () => {
    const candles = hourly(12, i => 50 + 10 * Math.sin(i / 2));
    const lineOut = await layout('candlestick', { candles, width: W, height: H, indicators: [], showVolume: false, style: 'line' });
    assert.match(lineOut.priceLine, /^M[\d.]+,[\d.]+L/);
    assert.equal(lineOut.priceArea, '');
    assert.equal(lineOut.candles.length, 12);
    const areaOut = await layout('candlestick', { candles, width: W, height: H, indicators: [{ kind: 'sma', period: 3 }], showVolume: false, style: 'area' });
    assert.match(areaOut.priceArea, /Z$/);
    assert.equal(areaOut.indicators.length, 1);
    // The scale fits the closes, not the wicks (2 beyond them here), so it spans less than the candles'.
    const plain = await layout('candlestick', { candles, width: W, height: H });
    const span = out => out.yTicks.at(-1).value - out.yTicks[0].value;
    assert.ok(span(lineOut) < span(plain));
    assert.equal(plain.priceLine, undefined);
});

test('candlestick styles: Heikin-Ashi candles, indicators over the real closes', async () => {
    const candles = hourly(6, i => 40 + i * 3);
    const out = await layout('candlestick', { candles, width: W, height: H, indicators: [{ kind: 'sma', period: 2 }], showVolume: false, style: 'heikinAshi' });
    const ha = heikinAshi(candles);
    assert.deepEqual(out.candles.map(c => [c.open, c.close]), ha.map(c => [c.open, c.close]));
    assert.equal(out.indicators[0].values[1], (candles[0].close + candles[1].close) / 2);
});

test('candlestick styles: EquiVolume and CandleVolume widths follow the volume, equal when one is missing', async () => {
    const candles = hourly(6, i => 40 + i * 3);
    const out = await layout('candlestick', { candles, width: W, height: H, indicators: [], showVolume: false, style: 'equivolume' });
    const widest = Math.max(...out.candles.map(c => c.width));
    out.candles.forEach((c, i) => assert.ok(Math.abs(c.width - widest * candles[i].volume / 300) < 0.02));
    assert.ok(widest <= (out.candles[1].x - out.candles[0].x) * 0.9 + 0.01); // never wider than its slot
    const plain = candlestickLayoutWidths(await layout('candlestick', { candles, width: W, height: H }));
    candles[2].volume = null;
    const fallback = await layout('candlestick', { candles, width: W, height: H, indicators: [], showVolume: false, style: 'candlevolume' });
    assert.deepEqual(candlestickLayoutWidths(fallback), plain);
});

const candlestickLayoutWidths = out => out.candles.map(c => c.width);

test('candlestick: price channel band from the highs and lows', async () => {
    const candles = hourly(8, i => 40 + i * 3);
    const out = await layout('candlestick', { candles, width: W, height: H, indicators: [{ kind: 'pricechannel', period: 3 }], showVolume: false });
    const [pc] = out.indicators;
    assert.equal(pc.kind, 'pricechannel');
    assert.equal(pc.panel, -1);
    assert.deepEqual(pc.values.slice(0, 2), [null, null]);
    const hi = Math.max(...candles.slice(0, 3).map(c => c.high)), lo = Math.min(...candles.slice(0, 3).map(c => c.low));
    assert.equal(pc.values[2], Math.round((hi + lo) / 2 * 100) / 100);
    assert.ok(pc.band.length > 0 && pc.upper.length > 0 && pc.lower.length > 0);
});
