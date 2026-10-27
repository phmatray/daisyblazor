// node --test tests/ — the "over time" layouts computed by the bundled D3 module.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';
// Sweeps call the layouts directly, so the replay fixture does not record thousands of specs.
import streamLayout from '../src/DaisyBlazor.Charts/wwwroot/layouts/stream.mjs';
import candlestickLayout from '../src/DaisyBlazor.Charts/wwwroot/layouts/candlestick.mjs';

const W = 640, H = 360;
const inside = (x, y, w = W, h = H) => x >= 0 && x <= w && y >= 0 && y <= h;
// Tick labels must not overlap: neighbours at least the half-widths apart (~0.6em glyphs, 11 units).
const spaced = ticks => ticks.every((t, i) => i === 0 || t.pos - ticks[i - 1].pos >= (t.label.length + ticks[i - 1].label.length) * 3.3);

test('calendar-heatmap: one cell per day, weeks as columns, Monday first', async () => {
    const { cells, months, weekdays, max } = await layout('calendar-heatmap', {
        days: [{ date: '2026-01-05', value: 2 }, { date: '2026-01-05', value: 1 }, { date: '2026-03-02', value: 12 }],
        width: W, height: 100,
    });
    assert.equal(cells.length, 57); // Jan 5 to Mar 2 inclusive
    assert.equal(cells[0].date, '2026-01-05');
    assert.equal(cells[0].value, 3); // same day summed
    assert.equal(cells[0].label, 'Mon, Jan 5, 2026');
    assert.equal(max, 12);
    // Jan 5 2026 is a Monday: top row. Jan 11 (Sunday) is the bottom row of the same column.
    assert.equal(cells[0].y, cells[7].y);
    assert.equal(cells[6].x, cells[0].x);
    assert.ok(cells[6].y > cells[5].y);
    assert.ok(cells[7].x > cells[0].x);
    assert.deepEqual(months.map(m => m.label), ['Jan', 'Feb', 'Mar']);
    assert.deepEqual(weekdays.map(w => w.label), ['Mon', 'Wed', 'Fri']);
    for (const c of cells) {
        assert.ok(inside(c.x, c.y, W, 100) && inside(c.x + c.size, c.y + c.size, W, 100));
    }
});

test('calendar-heatmap: levels quantize positive values, zero is level 0', async () => {
    const { cells, thresholds } = await layout('calendar-heatmap', {
        days: [1, 0, 4, 8].map((value, i) => ({ date: `2026-02-0${i + 1}`, value })), levels: 5, width: W, height: 100,
    });
    assert.deepEqual(cells.map(c => c.level), [1, 0, 3, 4]); // 4 sits on the second threshold
    assert.deepEqual(thresholds, [2, 4, 6]);
});

test('calendar-heatmap: a Sunday week start moves Sunday to the top row', async () => {
    const { cells } = await layout('calendar-heatmap', { days: [], from: '2026-01-04', to: '2026-01-10', weekStart: 0, width: W, height: 100 });
    assert.equal(cells.length, 7);
    assert.deepEqual(new Set(cells.map(c => c.x)).size, 1); // Jan 4 (Sunday) to Jan 10 is one column
    assert.ok(cells.every((c, i) => i === 0 || c.y > cells[i - 1].y));
});

test('calendar-heatmap: dates with a time keep their calendar day; nothing to show is empty', async () => {
    const { cells } = await layout('calendar-heatmap', { days: [{ date: '2026-06-01T23:30:00', value: 1 }], width: W, height: 100 });
    assert.equal(cells[0].date, '2026-06-01');
    assert.deepEqual((await layout('calendar-heatmap', { days: [], width: W, height: 100 })).cells, []);
    assert.deepEqual((await layout('calendar-heatmap', { days: [{ date: 'nope', value: 1 }], width: W, height: 100 })).cells, []);
});

test('heatmap: rows and columns in first-appearance order, a sequential t from zero to max', async () => {
    const out = await layout('heatmap', {
        cells: [
            { row: 'Mon', column: '09', value: 10 }, { row: 'Mon', column: '10', value: 30 },
            { row: 'Tue', column: '09', value: 20 }, { row: 'Tue', column: '09', value: 0 },
            { row: 'Tue', column: '10', value: null },
        ],
        width: W, height: H,
    });
    assert.deepEqual(out.rows.map(r => r.full), ['Mon', 'Tue']);
    assert.deepEqual(out.columns.map(c => c.full), ['09', '10']);
    assert.equal(out.cells.length, 3);
    assert.deepEqual(out.cells.map(c => c.t), [0.33, 1, 0.67]);
    assert.equal(out.min, 0);
    assert.equal(out.max, 30);
    for (const c of out.cells) {
        assert.ok(c.x >= out.plot.left && c.x + c.w <= W && c.y >= out.plot.top && c.y + c.h <= H);
    }
});

test('heatmap: the longest row name that fits the gutter is never cut, and keeps a margin at the left edge', async () => {
    // The docs "levels" example: "Platform" came out as "Platfor…" with plenty of room (left - 8 lost a rounding error).
    const rows = ['Platform', 'Billing', 'Search', 'Mobile', 'Data'];
    const out = await layout('heatmap', { cells: rows.map(row => ({ row, column: 'Mon', value: 1 })), width: 716, height: 400 });
    assert.deepEqual(out.rows.map(r => r.label), rows);
    // Row names are drawn ending 6 units left of the plot; the estimate needs slack before x = 0.
    const wed = await layout('heatmap', { cells: ['Mon', 'Wed'].map(row => ({ row, column: '00', value: 1 })), width: 306, height: 172 });
    assert.ok(wed.plot.left - 6 >= 2.15 * 11 * 1.15, 'room for a "Wed" 15 % wider than estimated');
});

test('heatmap: given orders win, long row names are shortened, crowded column labels thinned', async () => {
    const columns = Array.from({ length: 60 }, (_, i) => `Column ${i}`);
    const out = await layout('heatmap', {
        cells: columns.map(column => ({ row: 'A very long row name that would never fit beside the grid', column, value: 1 })),
        rows: ['A very long row name that would never fit beside the grid'], columns: [...columns].reverse(),
        width: W, height: H,
    });
    assert.equal(out.columns[0].full, 'Column 59');
    assert.ok(out.rows[0].label.endsWith('…'));
    assert.ok(out.rows[0].label.length * 6.6 <= out.plot.left);
    const shown = out.columns.filter(c => c.label !== '');
    assert.ok(shown.length < columns.length);
    assert.ok(shown.every((c, i) => i === 0 || c.pos - shown[i - 1].pos >= c.label.length * 6));
    assert.deepEqual((await layout('heatmap', { cells: [], width: W, height: H })).cells, []);
});

test('range-area: a band and a center line on a time axis, sorted by date, read as UTC', async () => {
    const out = await layout('range-area', {
        points: [
            { date: '2026-10-03T00:00:00', low: 90, high: 310, center: 140 },
            { date: '2026-10-01T00:00:00', low: 100, high: 300, center: 150 },
            { date: '2026-10-02T00:00:00', low: 280, high: 120, center: null },
        ],
        width: W, height: H,
    });
    assert.deepEqual(out.points.map(p => p.label), ['Oct 1, 2026', 'Oct 2, 2026', 'Oct 3, 2026']);
    assert.deepEqual([out.points[1].low, out.points[1].high], [120, 280]); // swapped into order
    assert.match(out.band, /^M[\d.]+,[\d.]+/);
    assert.ok(out.center.startsWith('M'));
    assert.ok(out.points[0].x < out.points[1].x && out.points[1].x < out.points[2].x);
    assert.ok(out.yTicks.some(t => t.label === '0')); // the value axis starts at zero
    assert.ok(out.xTicks.every(t => !/AM|PM|:/.test(t.label))); // daily data gets no hour ticks
    assert.ok(spaced(out.xTicks));
    assert.ok(out.plot.left > 0 && out.plot.right < W && out.plot.bottom < H);
});

test('range-area: labels make a category axis; no valid point is empty', async () => {
    const out = await layout('range-area', {
        points: ['Mon', 'Tue', 'Wed'].map((label, i) => ({ label, low: i, high: i + 2 })), smooth: false, width: W, height: H,
    });
    assert.deepEqual(out.xTicks.map(t => t.label), ['Mon', 'Tue', 'Wed']);
    assert.equal(out.center, '');
    assert.doesNotMatch(out.band, /C/); // straight segments when not smooth
    assert.deepEqual((await layout('range-area', { points: [{ label: 'x', low: null, high: 1 }], width: W, height: H })).points, []);
});

test('range-area: step draws the band and the center line as step-after segments, over smooth', async () => {
    const out = await layout('range-area', {
        points: ['Mon', 'Tue'].map((label, i) => ({ label, low: i, high: i + 4, center: i + 2 })), step: true, width: W, height: H,
    });
    const [a, b] = out.points;
    assert.doesNotMatch(out.band + out.center, /C/);
    assert.match(out.center, /^M([\d.]+),([\d.]+)L([\d.]+),\2L\3,[\d.]+$/);
    assert.equal(out.center.match(/L([\d.]+),/)[1], String(b.x));
    assert.ok(a.x < b.x);
});

test('stream: wiggle has no value axis, stacked and percent stack from zero', async () => {
    const series = [{ name: 'Billing', values: [10, 20, 30] }, { name: 'Search', values: [30, -5, null] }];
    const spec = { series, labels: ['W1', 'W2', 'W3'], width: W, height: H };

    const wiggle = await layout('stream', { ...spec, offset: 'wiggle' });
    assert.equal(wiggle.layers.length, 2);
    assert.deepEqual(wiggle.layers.map(l => l.index), [0, 1]);
    assert.deepEqual(wiggle.yTicks, []);
    assert.deepEqual(wiggle.columns[1].values, [20, 0]); // negative counts as 0

    const stacked = await layout('stream', { ...spec, offset: 'stacked' });
    assert.equal(stacked.yTicks[0].label, '0');
    assert.equal(stacked.yTicks[0].pos, stacked.plot.bottom);

    const percent = await layout('stream', { ...spec, offset: 'percent' });
    assert.deepEqual([percent.yTicks[0].label, percent.yTicks.at(-1).label], ['0%', '100%']);
    assert.equal(percent.yTicks.at(-1).pos, percent.plot.top);
    for (const l of percent.layers) {
        assert.match(l.path, /^M/);
        if (l.showLabel) {
            assert.ok(l.labelX >= percent.plot.left && l.labelX <= percent.plot.right);
        }
    }
});

test('stream: percent keeps the axis at exactly 100%, whatever the rounding', async () => {
    for (let a = 1; a <= 25; a++) for (let b = 1; b <= 25; b += 2) for (let c = 1; c <= 9; c += 2) {
        const series = [[a, 1], [b, 1], [c, 1]].map((values, i) => ({ name: `S${i}`, values }));
        const out = streamLayout({ series, labels: ['W1', 'W2'], offset: 'percent', width: W, height: H });
        assert.equal(out.yTicks.at(-1).label, '100%', `${a},${b},${c}`);
        assert.equal(out.yTicks.at(-1).pos, out.plot.top, `${a},${b},${c}`);
    }
});

test('stream: dates make a time axis; no series is empty', async () => {
    const dates = Array.from({ length: 12 }, (_, i) => new Date(Date.UTC(2026, 0, 5 + 7 * i)).toISOString().slice(0, 19));
    const out = await layout('stream', { series: [{ name: 'A', values: dates.map((_, i) => i + 1) }], dates, width: W, height: H });
    assert.ok(out.xTicks.length >= 2 && spaced(out.xTicks));
    assert.equal(out.columns[0].label, 'Jan 5, 2026');
    assert.deepEqual((await layout('stream', { series: [], width: W, height: H })).layers, []);
});

test('candlestick: periods sorted, bodies inside the plot, rising and falling', async () => {
    const candles = [
        { time: '2026-10-01T10:00:00', open: 55, high: 60, low: 20, close: 25 },
        { time: '2026-10-01T09:00:00', open: 40, high: 70, low: 30, close: 55 },
        { time: '2026-10-01T11:00:00', open: 25, high: null, low: 20, close: 30 },
    ];
    const out = await layout('candlestick', { candles, width: W, height: H });
    assert.equal(out.candles.length, 2);
    assert.deepEqual(out.candles.map(c => c.up), [true, false]);
    assert.equal(out.candles[0].label, 'Oct 1, 2026 09:00');
    const [up] = out.candles;
    assert.ok(up.yHigh < up.yClose && up.yClose < up.yOpen && up.yOpen < up.yLow);
    for (const c of out.candles) {
        assert.ok(c.x - c.width / 2 >= out.plot.left && c.x + c.width / 2 <= out.plot.right);
        assert.ok(c.yHigh >= out.plot.top && c.yLow <= out.plot.bottom);
    }
    assert.deepEqual(out.xTicks.map(t => t.label), ['09:00', '10:00']);
    assert.deepEqual((await layout('candlestick', { candles: [], width: W, height: H })).candles, []);
});

test('candlestick: one period still gets a width and a domain', async () => {
    const out = await layout('candlestick', { candles: [{ time: '2026-10-01T09:00:00', open: 5, high: 5, low: 5, close: 5 }], width: W, height: H });
    assert.equal(out.candles.length, 1);
    assert.ok(out.candles[0].width > 0 && Number.isFinite(out.candles[0].yOpen));
});

test('gantt: ungrouped tasks first, then a header per group; milestones and progress', async () => {
    const out = await layout('gantt', {
        tasks: [
            { name: 'Design', start: '2026-10-01T00:00:00', end: '2026-10-10T00:00:00', group: 'Plan', progress: 1.4 },
            { name: 'Kick-off', start: '2026-09-28T00:00:00', end: null },
            { name: 'Build', start: '2026-10-08T00:00:00', end: '2026-10-30T00:00:00', group: 'Ship', progress: 0.25 },
            { name: 'No start', start: null, end: '2026-10-30T00:00:00' },
        ],
        today: '2026-10-19T00:00:00', width: W, height: H,
    });
    assert.deepEqual(out.rows.map(r => r.name), ['Kick-off', 'Plan', 'Design', 'Ship', 'Build']);
    assert.deepEqual(out.rows.map(r => r.header), [false, true, false, true, false]);
    assert.deepEqual(out.groups, ['Plan', 'Ship']);
    const [kickoff, , design, , build] = out.rows;
    assert.equal(kickoff.milestone, true);
    assert.equal(kickoff.x0, out.plot.left);
    assert.equal(design.progress, 1); // clamped
    assert.equal(build.progress, 0.25);
    assert.equal(build.x1, out.plot.right);
    assert.ok(design.x0 < build.x0);
    assert.ok(out.rows.every((r, i) => i === 0 || r.y >= out.rows[i - 1].y + out.rows[i - 1].h));
    assert.equal(out.today.label, 'Oct 19, 2026');
    assert.ok(out.today.pos > build.x0 && out.today.pos < build.x1);
    assert.ok(spaced(out.xTicks));
});

test('gantt: a today outside the plan is not drawn; no tasks is empty', async () => {
    const out = await layout('gantt', { tasks: [{ name: 'A', start: '2026-01-01', end: '2026-01-05' }], today: '2027-01-01T00:00:00', width: W, height: H });
    assert.equal(out.today, null);
    assert.deepEqual((await layout('gantt', { tasks: [], width: W, height: H })).rows, []);
});

test('bump: the highest value ranks first, names at both ends, gaps break the line', async () => {
    const out = await layout('bump', {
        series: [
            { name: 'Starter', values: [30, 50, 20] },
            { name: 'Pro', values: [40, 20, null] },
            { name: 'Team', values: [10, 60, 70] },
        ],
        labels: ['Q1', 'Q2', 'Q3'], width: W, height: H,
    });
    const rank = name => out.lines.find(l => l.name === name).points.map(p => p.rank);
    assert.deepEqual(rank('Starter'), [2, 2, 2]);
    assert.deepEqual(rank('Pro'), [1, 3]);
    assert.deepEqual(rank('Team'), [3, 1, 1]);
    assert.equal(out.ranks, 3);
    const pro = out.lines[1];
    assert.equal(pro.end, null);
    assert.ok(pro.start.x === out.plot.left);
    const team = out.lines[2];
    assert.ok(team.end.y < team.start.y); // rank 1 is higher up
    assert.match(team.path, /C/); // smooth bump curve
    assert.ok(out.plot.left >= 'Starter'.length * 7.2 && out.plot.right <= W - 'Starter'.length * 7.2);
    assert.deepEqual(out.xTicks.map(t => t.label), ['Q1', 'Q2', 'Q3']);
});

test('bump: ranks given as data are kept; no series is empty', async () => {
    const out = await layout('bump', { series: [{ name: 'A', values: [2, 1] }, { name: 'B', values: [1, 2] }], valuesAreRanks: true, width: W, height: H });
    assert.deepEqual(out.lines[0].points.map(p => p.rank), [2, 1]);
    assert.deepEqual(out.xTicks.map(t => t.label), ['1', '2']);
    assert.deepEqual((await layout('bump', { series: [], width: W, height: H })).lines, []);
});

test('candlestick indicators: SMA, EMA, Bollinger (population σ), RSI (Wilder) and MACD of fixed series', async () => {
    const { sma, ema, bollinger, rsi, macd } = await import('../src/DaisyBlazor.Charts/wwwroot/layouts/candlestick.mjs');
    const near = (actual, expected) => assert.deepEqual(actual.map(v => (v === null ? null : Math.round(v * 1e6) / 1e6)), expected);

    near(sma([1, 2, 3, 4, 5], 3), [null, null, 2, 3, 4]);
    // α = 2 / (3 + 1) = 0.5, seeded with SMA(1, 2, 3) = 2: then 0.5·4 + 0.5·2 = 3, 0.5·5 + 0.5·3 = 4.
    near(ema([1, 2, 3, 4, 5], 3), [null, null, 2, 3, 4]);
    near(ema([10, 20, 10, 20], 2), [null, 15, 11.666667, 17.222222]);

    // Mean 4, population σ = √((4 + 0 + 4) / 3) = 1.632993: bands at 4 ± 2σ.
    const b = bollinger([2, 4, 6], 3, 2);
    near(b.middle, [null, null, 4]);
    near(b.upper, [null, null, 7.265986]);
    near(b.lower, [null, null, 0.734014]);

    // Changes +1, −1, +1, +1. First averages: gain 0.5, loss 0.5 → 50; then Wilder's
    // (avg·(n − 1) + change) / n: gain 0.75 / loss 0.25 → 75, gain 0.875 / loss 0.125 → 87.5.
    near(rsi([1, 2, 1, 2, 3], 2), [null, null, 50, 75, 87.5]);
    near(rsi([1, 2, 3], 2), [null, null, 100]); // no losses
    near(rsi([5, 5, 5], 2), [null, null, 50]); // flat

    // On a straight line an EMA lags by a constant: EMA(2) by 0.5, EMA(3) by 1, so the MACD is 0.5,
    // its signal 0.5 and the histogram 0.
    const m = macd([1, 2, 3, 4, 5, 6], 2, 3, 2);
    near(m.main, [null, null, 0.5, 0.5, 0.5, 0.5]);
    near(m.signal, [null, null, null, 0.5, 0.5, 0.5]);
    near(m.histogram, [null, null, null, 0, 0, 0]);
});

const hourly = (n, f) => Array.from({ length: n }, (_, i) => {
    const open = f(i), close = f(i + 1);
    return { time: `2026-10-19T${String(i).padStart(2, '0')}:00:00`, open, high: Math.max(open, close) + 2, low: Math.min(open, close) - 2, close, volume: 100 + i * 10 };
});

test('candlestick: overlays on the price panel follow the candles, values line up with them', async () => {
    const candles = hourly(24, i => 50 + 10 * Math.sin(i / 3));
    const out = await layout('candlestick', {
        candles, width: W, height: H,
        indicators: [{ kind: 'sma', period: 5 }, { kind: 'bollinger', period: 5, stdDev: 2 }, { kind: 'ema', period: 5 }, { kind: 'nope' }],
    });
    assert.equal(out.indicators.length, 3);
    assert.deepEqual(out.indicators.map(i => [i.index, i.kind, i.panel]), [[0, 'sma', -1], [1, 'bollinger', -1], [2, 'ema', -1]]);
    assert.equal(out.panels.length, 0);
    assert.equal(out.plot.bottom, H - 24); // nothing under the price panel
    const [smaOut, boll] = out.indicators;
    assert.equal(smaOut.values.length, 24);
    assert.deepEqual(smaOut.values.slice(0, 4), [null, null, null, null]);
    const closes = candles.map(c => c.close);
    assert.equal(smaOut.values[4], Math.round(closes.slice(0, 5).reduce((s, v) => s + v, 0) / 5 * 100) / 100);
    assert.match(smaOut.line, /^M[\d.]+,[\d.]+L/);
    assert.ok(boll.band.length > 0 && boll.upper.length > 0 && boll.lower.length > 0);
    // The line starts at the fifth candle.
    assert.equal(r(Number(smaOut.line.slice(1).split(',')[0])), r(out.candles[4].x));
    // The bands widen the price scale, so every candle still sits inside the plot.
    for (const c of out.candles) {
        assert.ok(c.yHigh >= out.plot.top && c.yLow <= out.plot.bottom);
    }
});

test('candlestick: the price panel keeps at least 40% of the height, however many panels and however small', async () => {
    const candles = hourly(24, i => 50 + 10 * Math.sin(i / 3));
    const kinds = ['rsi', 'macd', 'rsi', 'macd', 'rsi', 'macd'];
    // The reported case (aspect ratio 3: volume, RSI, MACD and a second RSI), through layout() for the replay fixture.
    const reported = await layout('candlestick', { candles, width: W, height: 213, showVolume: true, indicators: [{ kind: 'rsi' }, { kind: 'macd', period: 3 }, { kind: 'rsi', period: 5 }] });
    assert.ok(reported.plot.bottom - reported.plot.top >= (213 - 24 - 10) * 0.4 - 0.01);
    for (const height of [60, 100, 160, 213, 300, 360]) {
        for (let n = 1; n <= kinds.length; n++) {
            const out = candlestickLayout({
                candles, width: W, height, showVolume: true, indicators: kinds.slice(0, n).map(kind => ({ kind, period: 3 })),
            });
            const bottom = height - 24;
            const label = `height ${height}, ${n} indicators`;
            assert.ok(out.plot.bottom - out.plot.top >= (bottom - out.plot.top) * 0.4 - 0.01, label);
            let prev = out.plot.bottom;
            for (const p of out.panels) {
                assert.ok(p.top >= prev && p.bottom >= p.top, label);
                prev = p.bottom;
            }
            assert.ok(Math.abs(prev - bottom) < 0.01, label);
        }
    }
});

test('candlestick: RSI, MACD and volume panels stack under the price panel on one time axis', async () => {
    const candles = hourly(24, i => 50 + 10 * Math.sin(i / 3));
    candles[3].volume = null;
    const out = await layout('candlestick', {
        candles, width: W, height: H, showVolume: true,
        indicators: [{ kind: 'rsi', period: 5 }, { kind: 'macd', period: 3, slowPeriod: 6, signalPeriod: 3 }],
    });
    assert.deepEqual(out.panels.map(p => p.kind), ['rsi', 'macd', 'volume']);
    assert.deepEqual(out.indicators.map(i => i.panel), [0, 1]);
    // Panels sit under each other, below the price panel, and end where the time axis is drawn.
    let prev = out.plot.bottom;
    for (const p of out.panels) {
        assert.ok(p.top > prev && p.bottom > p.top);
        prev = p.bottom;
    }
    assert.equal(prev, H - 24);
    const [rsiPanel, macdPanel, volume] = out.panels;
    assert.deepEqual(rsiPanel.yTicks.map(t => t.label), ['30', '70']);
    assert.equal(rsiPanel.guides.length, 2);
    assert.equal(macdPanel.guides.length, 1);
    assert.ok(out.indicators[1].signal.length > 0 && out.indicators[1].bars.length > 0);
    // One volume bar per candle with a volume, under its candle, as wide as its body, standing on the panel's bottom.
    assert.equal(volume.bars.length, 23);
    assert.equal(out.candles[3].volume, null);
    assert.equal(out.candles[4].volume, 140);
    const bar = volume.bars[3]; // the fifth candle
    assert.equal(bar.value, 140);
    assert.equal(r(bar.x + bar.width / 2), r(out.candles[4].x));
    assert.equal(bar.width, out.candles[4].width);
    assert.equal(r(bar.y + bar.height), r(volume.bottom));
    assert.equal(bar.up, out.candles[4].up);
    const tallest = volume.bars.reduce((a, b) => (b.value > a.value ? b : a));
    assert.ok(tallest.y >= volume.top && tallest.value === 330);
    // The left margin fits the widest axis of all panels; without extras the output has no panels.
    const plain = await layout('candlestick', { candles, width: W, height: H });
    assert.ok(out.plot.left >= plain.plot.left);
    assert.deepEqual(out.xTicks.map(t => t.label), plain.xTicks.map(t => t.label));
    assert.equal(plain.panels, undefined);
    assert.equal(plain.indicators, undefined);
    assert.equal('volume' in plain.candles[0], false);
});

const r = v => Math.round(v * 10) / 10;

test('heatmap: levels quantize the values into equal steps, with the thresholds between them', async () => {
    const out = await layout('heatmap', {
        cells: [0, 2, 5, 7, 10].map((value, i) => ({ row: 'API', column: `h${i}`, value })),
        levels: 4, width: W, height: H,
    });
    assert.deepEqual(out.thresholds, [2.5, 5, 7.5]);
    assert.deepEqual(out.cells.map(c => c.t), [0, 0, 0.67, 0.67, 1]);
    const continuous = await layout('heatmap', { cells: [{ row: 'A', column: 'x', value: 1 }], width: W, height: H });
    assert.ok(!('thresholds' in continuous));
});

test('heatmap: the scale starts at zero, or at the given min and max, clamping values outside', async () => {
    const cells = [40, 70, 100].map((value, i) => ({ row: 'A', column: `c${i}`, value }));
    const zero = await layout('heatmap', { cells, width: W, height: H });
    assert.deepEqual([zero.min, zero.max], [0, 100]);
    assert.deepEqual(zero.cells.map(c => c.t), [0.4, 0.7, 1], 'the smallest value is not drawn as empty');
    const fixed = await layout('heatmap', { cells, min: 50, max: 80, width: W, height: H });
    assert.deepEqual([fixed.min, fixed.max], [50, 80]);
    assert.deepEqual(fixed.cells.map(c => c.t), [0, 0.67, 1]);
    const negative = await layout('heatmap', { cells: [-5, 5].map((value, i) => ({ row: 'A', column: `c${i}`, value })), width: W, height: H });
    assert.deepEqual([negative.min, negative.max], [-5, 5]);
});

test('heatmap: level thresholds stay exact for small values; a flat domain is one step', async () => {
    const small = await layout('heatmap', {
        cells: [0.001, 0.005].map((value, i) => ({ row: 'A', column: `c${i}`, value })), min: 0.001, levels: 4, width: W, height: H,
    });
    assert.equal(small.thresholds.length, 3);
    [0.002, 0.003, 0.004].forEach((v, i) => assert.ok(Math.abs(small.thresholds[i] - v) < 1e-12, `threshold ${i}: ${small.thresholds[i]}`));
    assert.deepEqual(small.cells.map(c => c.t), [0, 1]);
    const flat = await layout('heatmap', {
        cells: [5, 5, 5].map((value, i) => ({ row: 'A', column: `c${i}`, value })), min: 5, levels: 4, width: W, height: H,
    });
    assert.deepEqual(flat.thresholds, [], 'one step, no thresholds');
    assert.deepEqual([flat.min, flat.max], [5, 5]);
    assert.deepEqual(flat.cells.map(c => c.t), [1, 1, 1], 'the single step takes the full colour, as its legend swatch');
});
