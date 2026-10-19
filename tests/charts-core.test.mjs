// node --test tests/ — the line, bar, pie and sparkline layouts, and the measured mode of charts.mjs.
// These hold the geometry the bUnit tests used to check in the C# markup (ticks, gaps, stacking,
// zero-crossing bars, arcs): the Razor components now only draw what these return.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout, observe, update, unobserve } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';
import { compact, fitTrend } from '../src/DaisyBlazor.Charts/wwwroot/layouts/_shared.mjs';
// Sweeps call the layouts directly, so the replay fixture does not record thousands of specs.
import barLayout from '../src/DaisyBlazor.Charts/wwwroot/layouts/bar.mjs';
import lineLayout from '../src/DaisyBlazor.Charts/wwwroot/layouts/line.mjs';

const size = { width: 640, height: 256 };
const moves = path => path.split('M').filter(Boolean).length;

test('line: the axis rounds out to nice ticks', async () => {
    const { yTicks } = await layout('line', { series: [[12, 30, 44, 20]], ...size });
    assert.deepEqual(yTicks.map(t => t.label), ['0', '10', '20', '30', '40', '50']);
});

test('compact tick text for large values', async () => {
    assert.deepEqual([0, 2.5, 0.1 + 0.2, 1500, 50000, -1500, 2_400_000, 1234.5].map(compact),
        ['0', '2.5', '0.3', '1.5k', '50k', '-1.5k', '2.4M', '1.23k']);
    const { yTicks } = await layout('bar', { categories: ['Free', 'Pro'], series: [[48200, 12000]], ...size });
    assert.equal(yTicks.at(-1).label, '50k');
});

test('line: a slot per label, points in the middle of their slot', async () => {
    const { plot, xTicks, series } = await layout('line', { series: [[1, 2]], labels: ['A', 'B', 'C', 'D'], showGrid: false, ...size });
    const slot = (plot.right - plot.left) / 4;
    assert.equal(xTicks.length, 4);
    assert.equal(xTicks[3].pos, plot.left + slot * 3.5);
    assert.equal(series[0].points[0].x, plot.left + slot / 2);
});

test('line: long axes thin their labels so they never overlap', async () => {
    const labels = Array.from({ length: 60 }, (_, i) => `Day ${i + 1}`);
    const { xTicks } = await layout('line', { series: [labels.map(() => 1)], labels, width: 300, height: 200 });
    assert.ok(xTicks.length < 15);
    for (let i = 1; i < xTicks.length; i++) {
        assert.ok(xTicks[i].pos - xTicks[i - 1].pos >= 'Day 60'.length * 6.6);
    }
});

test('line: NaN leaves a gap, an isolated point is flagged for a marker', async () => {
    const { series } = await layout('line', { series: [[1, 2, null, 3, 4, null, 5]], ...size });
    assert.equal(moves(series[0].line), 3);
    assert.doesNotMatch(series[0].line, /NaN/);
    assert.deepEqual(series[0].points.map(p => p.index), [0, 1, 3, 4, 6]);
    assert.deepEqual(series[0].points.map(p => p.isolated), [false, false, false, false, true]);
    const single = await layout('line', { series: [[5]], ...size });
    assert.equal(single.series[0].points[0].isolated, true);
});

test('line: with negative data the area closes at the zero line', async () => {
    const { yTicks, series } = await layout('line', { series: [[-20, -10, -40]], ...size });
    const zero = yTicks.find(t => t.label === '0');
    assert.equal(zero.pos, 8); // zero is the top of the plot
    assert.match(series[0].area, new RegExp(`L[\\d.]+,${zero.pos}.*Z$`));
});

test('line: stacked series sit on the running total, missing values count as 0', async () => {
    const { yTicks, series } = await layout('line', { series: [[20, 40], [30, 60]], stacked: true, ...size });
    assert.equal(yTicks.at(-1).label, '100');
    const y = v => yTicks.find(t => t.label === String(v))?.pos;
    // B's points are drawn at A+B, its band runs back along A.
    assert.deepEqual(series[1].points.map(p => p.y), [(y(40) + y(60)) / 2, y(100)]);
    assert.equal(series[0].points[1].y, y(40));
    const a = series[0].points.map(p => `${p.x},${p.y}`);
    assert.ok(series[1].area.includes(a[1]) && series[1].area.includes(a[0]));

    const gaps = await layout('line', { series: [[20, null], [40]], stacked: true, ...size });
    assert.doesNotMatch(gaps.series[1].line, /NaN/);
    assert.equal(moves(gaps.series[1].line), 1); // stacked lines have no gaps
    assert.equal(gaps.series[0].points.length, 1); // but markers only where the series has a value
});

test('line: unstacked axis follows the largest single value; empty input is safe', async () => {
    const { yTicks } = await layout('line', { series: [[20, 40], [30, 60]], ...size });
    assert.equal(yTicks.at(-1).label, '60');
    const empty = await layout('line', { series: [], ...size });
    assert.deepEqual(empty.series, []);
    assert.deepEqual(empty.yTicks.map(t => t.label), ['0', '0.2', '0.4', '0.6', '0.8', '1']);
});

test('bar: negative bars run down from the zero line', async () => {
    const { bars, yTicks } = await layout('bar', { categories: ['Up', 'Down'], series: [[10, -10]], ...size });
    const zero = yTicks.find(t => t.label === '0').pos;
    assert.ok(Math.abs(bars[0].y + bars[0].height - zero) < 0.01);
    assert.equal(bars[1].y, zero);
    assert.equal(bars[0].height, bars[1].height);
});

test('bar: grouped bars share their category band, missing values draw nothing', async () => {
    const { bars } = await layout('bar', { categories: ['Q1', 'Q2'], series: [[1, 2], [3, null]], ...size });
    assert.equal(bars.length, 3);
    const [a, b] = bars.filter(r => r.c === 0).sort((p, q) => p.s - q.s);
    assert.ok(Math.abs(a.x + a.width - b.x) < 0.01);
});

test('bar: horizontal keeps long category labels whole and has no value row without the grid', async () => {
    const out = await layout('bar', { categories: ['North America', 'Asia Pacific'], series: [[10, 5]], horizontal: true, ...size });
    assert.deepEqual(out.yTicks.map(l => l.label), ['North America', 'Asia Pacific']);
    assert.ok(out.plot.left > 'North America'.length * 6.6);

    const bare = await layout('bar', { categories: ['A'], series: [[1]], horizontal: true, showGrid: false, ...size });
    assert.deepEqual(bare.xTicks, []);
    assert.equal(bare.plot.bottom, 256 - 4);

    const narrow = await layout('bar', { categories: ['A very long region name indeed'], series: [[1]], horizontal: true, width: 200, height: 100 });
    assert.match(narrow.yTicks[0].label, /…$/);
    assert.equal(narrow.yTicks[0].full, 'A very long region name indeed');
});

test('bar: stacked series share one bar, negatives stack down from zero so mixed signs diverge', async () => {
    const { bars, yTicks } = await layout('bar', { categories: ['Q1', 'Q2'], series: [[10, 20], [30, -10], [-5, 15]], stacked: true, ...size });
    const y = v => yTicks.find(t => t.label === String(v))?.pos;
    const at = (c, s) => bars.find(b => b.c === c && b.s === s);
    const zero = y(0);
    // Q1: 10 then 30 on top (top at 40), -5 below zero.
    assert.ok(Math.abs(at(0, 0).y + at(0, 0).height - zero) < 0.01);
    assert.ok(Math.abs(at(0, 1).y + at(0, 1).height - at(0, 0).y) < 0.01);
    assert.ok(Math.abs(at(0, 1).y - y(40)) < 0.01);
    assert.equal(at(0, 2).y, zero);
    // Q2: 20 + 15 = 35 up, -10 down; every series of a category sits in the same x slot.
    assert.ok(Math.abs(at(1, 2).y + at(1, 2).height - at(1, 0).y) < 0.01);
    assert.equal(at(1, 1).y, zero);
    assert.ok(bars.filter(b => b.c === 1).every(b => b.x === at(1, 0).x && b.width === at(1, 0).width));
    assert.ok(yTicks.some(t => t.value < 0));
});

test('bar: percent stacks each category to 100%, horizontal too', async () => {
    const { bars, xTicks, plot } = await layout('bar', { categories: ['Q1', 'Q2'], series: [[1, 30], [3, 10]], percent: true, horizontal: true, ...size });
    assert.equal(xTicks.at(-1).label, '100%');
    for (const c of [0, 1]) {
        const own = bars.filter(b => b.c === c);
        assert.ok(Math.abs(own.reduce((sum, b) => sum + b.width, 0) - (plot.right - plot.left)) < 0.05);
        assert.ok(own.every(b => b.y === own[0].y));
    }
    const q1 = bars.filter(b => b.c === 0);
    assert.ok(Math.abs(q1[1].width / q1[0].width - 3) < 0.01); // 25% and 75%
});

test('bar: stacks put several stacks side by side in a category, percent per stack', async () => {
    const { bars, yTicks } = await layout('bar', { categories: ['Q1'], series: [[10], [30], [20], [30]], stacks: [0, 0, 1, 1], percent: true, ...size });
    const at = s => bars.find(b => b.s === s);
    assert.equal(at(0).x, at(1).x);
    assert.equal(at(2).x, at(3).x);
    assert.ok(at(2).x > at(0).x + at(0).width - 0.01);
    const y = v => yTicks.find(t => t.label === `${v}%`)?.pos;
    assert.ok(Math.abs(at(1).y - y(100)) < 0.01 && Math.abs(at(3).y - y(100)) < 0.01);
    assert.ok(Math.abs(at(2).y - y(40)) < 0.01); // 20 of 50

    // Without stacked, stacks are ignored: one bar per series side by side.
    const grouped = await layout('bar', { categories: ['Q1'], series: [[1], [2]], stacks: [0, 0], ...size });
    assert.notEqual(grouped.bars[0].x, grouped.bars[1].x);
});

test('line: percent stacks every index to 100%, negatives count as 0; step draws step-after paths', async () => {
    const { yTicks, series } = await layout('line', { series: [[20, 0, 5], [60, 30, -5]], percent: true, ...size });
    assert.equal(yTicks.at(-1).label, '100%');
    const y = v => yTicks.find(t => t.label === `${v}%`)?.pos;
    assert.deepEqual(series[1].points.map(p => p.y), [y(100), y(100), y(100)]);
    assert.equal(series[0].points[0].y, y(20) + (y(40) - y(20)) / 2 * 0.5); // 25% of 80

    const step = await layout('line', { series: [[1, 3, 2]], step: true, smooth: true, showGrid: false, ...size });
    assert.doesNotMatch(step.series[0].line, /C/); // step wins over smooth
    const [p0, p1] = step.series[0].points;
    assert.match(step.series[0].line, new RegExp(`^M${p0.x},${p0.y}L${p1.x},${p0.y}L${p1.x},${p1.y}`));
});

// Every triple of small integers: floating-point drift in the shares must never push the axis past 100%.
const triples = [];
for (let a = 1; a <= 25; a++) for (let b = 1; b <= 25; b += 2) for (let c = 1; c <= 9; c += 2) triples.push([a, b, c]);

test('bar: 100% stacks keep the axis at exactly 100%, whatever the rounding (vertical, horizontal, negatives)', async () => {
    // The reported case, through layout() so the replay fixture holds it: 1/11 + 1/11 + 9/11 > 1.
    assert.equal((await layout('bar', { categories: ['A'], series: [[1], [1], [9]], percent: true, ...size })).yTicks.at(-1).label, '100%');
    for (const [a, b, c] of triples) {
        for (const [horizontal, sign] of [[false, 1], [true, 1], [false, -1]]) {
            const { xTicks, yTicks } = barLayout({ categories: ['A'], series: [[a * sign], [b * sign], [c * sign]], percent: true, horizontal, ...size });
            const values = (horizontal ? xTicks : yTicks).map(t => t.value);
            assert.equal(sign > 0 ? Math.max(...values) : Math.min(...values), 100 * sign, `${a},${b},${c}`);
        }
    }
});

test('line: 100% stacks keep the axis at exactly 100%, whatever the rounding', async () => {
    assert.equal((await layout('line', { series: [[6, 1], [23, 1], [1, 1]], percent: true, ...size })).yTicks.at(-1).label, '100%');
    for (const [a, b, c] of triples) {
        const { yTicks } = lineLayout({ series: [[a, 1], [b, 1], [c, 1]], percent: true, ...size });
        assert.equal(yTicks.at(-1).value, 100, `${a},${b},${c}`);
    }
});

test('pie: slices clockwise from 12 o\'clock in input order, with percentages', async () => {
    const { slices } = await layout('pie', { values: [30, null, -5, 10], size: 200 });
    assert.deepEqual(slices.map(s => s.index), [0, 3]);
    assert.deepEqual(slices.map(s => s.percent), [75, 25]);
    assert.equal(slices[0].startAngle, 0);
    assert.equal(slices[1].endAngle, 2 * Math.PI);
    assert.match(slices[0].path, /L0,0Z$/); // a pie wedge closes at the centre
});

test('pie: a single slice is a full circle; a ring has a hole; no data gives no slices', async () => {
    const full = await layout('pie', { values: [5], size: 200 });
    assert.equal(full.slices.length, 1);
    assert.doesNotMatch(full.slices[0].path, /NaN/);
    const ring = await layout('pie', { values: [1, 1], size: 200, innerRatio: 0.5 });
    assert.equal(ring.innerRadius, 47);
    assert.doesNotMatch(ring.slices[0].path, /L0,0Z/);
    assert.deepEqual((await layout('pie', { values: [0, null], size: 200 })).slices, []);
});

test('pie: the centre label shrinks to fit the hole', async () => {
    const short = await layout('pie', { values: [1], size: 200, innerRatio: 0.6, centerLabel: '72%' });
    const long = await layout('pie', { values: [1], size: 200, innerRatio: 0.35, centerLabel: '€12,480' });
    assert.equal(short.centerFontSize, 22);
    assert.ok(long.centerFontSize < 22);
});

test('spark: spans its own range, flat data is centred, NaN leaves a gap', async () => {
    const rising = await layout('spark', { data: [1, 2, 3], pad: 2, width: 80, height: 24 });
    assert.equal(rising.line, 'M2,22L40,12L78,2');
    assert.deepEqual(rising.end, { x: 78, y: 2 });
    assert.match(rising.area, /L78,22L40,22L2,22Z$/);

    const flat = await layout('spark', { data: [3, 3, 3], pad: 2, width: 80, height: 24 });
    assert.equal(flat.line, 'M2,12L40,12L78,12');

    const gap = await layout('spark', { data: [5, 6, null, 7, 8], pad: 2, width: 80, height: 24 });
    assert.equal(moves(gap.line), 2);
    const lone = await layout('spark', { data: [5, null, 7, null, 8], pad: 2, width: 80, height: 24 });
    assert.match(lone.line, /M40,\d+(\.\d+)?Z/); // a lone value is drawn as a dot
    assert.deepEqual(await layout('spark', { data: [null], width: 80, height: 24 }), { line: '', area: '', end: null });
});

const near = (a, b, eps = 1e-6) => assert.ok(Math.abs(a - b) < eps, `${a} is not near ${b}`);

test('pie: exploded slices move out along their bisector and the radius shrinks to fit', async () => {
    const plain = await layout('pie', { values: [1, 1, 2], size: 200 });
    const geo = await layout('pie', { values: [1, 1, 2], size: 200, explode: [0, 2], explodeOffset: 0.1 });
    near(geo.radius, 94 / 1.1);
    assert.equal(plain.slices[0].dx, undefined, 'without explode no slice has an offset');
    const [first, second, third] = geo.slices;
    // The first slice spans 12 to 3 o'clock: it moves up and right, by a tenth of the radius.
    assert.ok(first.dx > 0 && first.dy < 0);
    near(Math.hypot(first.dx, first.dy), geo.radius * 0.1, 0.01);
    assert.equal(second.dx, undefined);
    // The half-pie third slice spans 6 to 12 o'clock through 9: it moves left.
    near(third.dy, 0, 0.01);
    assert.ok(third.dx < 0);
});

test('pie: rings are concentric, outermost first, sharing the hole, each its own whole', async () => {
    const geo = await layout('pie', { values: [], size: 200, innerRatio: 0.5, rings: [[1, 1], [2, 1, 1, null]] });
    assert.equal(geo.innerRadius, 47);
    assert.deepEqual(geo.slices.map(s => [s.ring, s.index, s.percent]), [[0, 0, 50], [0, 1, 50], [1, 0, 50], [1, 1, 25], [1, 2, 25]]);
    assert.match(geo.slices[0].path, /^M0,-94A94,94/);
    assert.match(geo.slices[0].path, /A70\.5,70\.5/);
    assert.match(geo.slices[2].path, /^M0,-70\.5A70\.5,70\.5/);
    assert.match(geo.slices[2].path, /A47,47/);
});

test('spark column: bars from zero, negatives hang down, nulls leave a gap', async () => {
    const geo = await layout('spark', { data: [3, -1, null, 0, 1], pad: 2, width: 100, height: 24, kind: 'column' });
    assert.deepEqual([geo.line, geo.area, geo.end], ['', '', null]);
    assert.deepEqual(geo.bars.map(b => b.i), [0, 1, 3, 4]);
    const zero = 2 + (20 * 3) / 4; // y of 0 over [-1, 3] in [2, 22]
    const [up, down, flat, small] = geo.bars;
    assert.deepEqual([up.x, up.width, up.y, up.height], [2.5, 15, 2, zero - 2]);
    assert.deepEqual([down.y, down.height], [zero, 22 - zero]);
    assert.equal(flat.height, 0);
    near(small.y + small.height, zero, 0.01);
    assert.equal(geo.band, undefined);
    assert.equal(geo.trend, undefined);
});

test('spark win/loss: equal bars by sign, zero is a gap; trend and range are ignored', async () => {
    const geo = await layout('spark', { data: [5, -0.1, 0, 2], pad: 1, width: 40, height: 20, kind: 'winloss', trend: true, normal: [0, 1] });
    assert.deepEqual(geo.bars.map(b => [b.i, b.y, b.height]), [[0, 1, 8.5], [1, 10.5, 8.5], [3, 1, 8.5]]);
    assert.equal(geo.band, undefined);
    assert.equal(geo.trend, undefined);
});

test('spark trend line and normal range on lines and columns', async () => {
    const line = await layout('spark', { data: [1, 2, 3], pad: 2, width: 80, height: 24, trend: true, normal: [2, 5] });
    // The range widens the scale to [1, 5]; the band runs from 5 (top) to 2.
    assert.deepEqual(line.band, { y: 2, height: 15 });
    assert.deepEqual(line.trend, { x1: 2, y1: 22, x2: 78, y2: 12 });
    assert.equal(line.line, 'M2,22L40,17L78,12');
    const column = await layout('spark', { data: [2, 4, null, 8], pad: 2, width: 80, height: 24, kind: 'column', trend: true });
    assert.equal(column.trend.x1, 10);
    assert.equal(column.trend.x2, 70);
    assert.ok(column.trend.y2 < column.trend.y1, 'a rising trend');
    assert.equal((await layout('spark', { data: [4, null], pad: 2, width: 80, height: 24, trend: true })).trend, null, 'one value has no trend');
});

test('spark trend is the shared linear fit over the indices, skipping gaps', async () => {
    const data = [1, null, 5, 4, 9];
    const geo = await layout('spark', { data, pad: 2, width: 82, height: 24, trend: true });
    const fit = fitTrend('linear', [0, 2, 3, 4], [1, 5, 4, 9]);
    // Values 1..9 span y 22..2, indices 0..4 span x 2..80.
    const y = v => 22 - (v - 1) * 20 / 8;
    assert.deepEqual(geo.trend, { x1: 2, y1: Math.round(y(fit.f(0)) * 100) / 100, x2: 80, y2: Math.round(y(fit.f(4)) * 100) / 100 });
});

const ends = path => {
    const v = path.slice(1).split('L');
    return [v[0], v.at(-1)].map(s => s.split(',').map(Number));
};

test('line: a trend per series over the label index, drawn through the label positions', async () => {
    // y = 2 + 2i exactly: the straight trend runs from the first point to the last.
    const out = await layout('line', { series: [[2, 4, 6, 8], [5, 2, 1, 2, 5]], labels: ['Q1', 'Q2', 'Q3', 'Q4', 'Q5'], ...size, trendline: 'linear' });
    const [lin, flat] = out.trends;
    assert.ok(Math.abs(lin.a - 2) < 1e-9 && Math.abs(lin.b - 2) < 1e-9 && lin.r2 === 1);
    const pts = out.series[0].points;
    assert.deepEqual(ends(lin.path), [[pts[0].x, pts[0].y], [pts[3].x, pts[3].y]]);
    // The symmetric bowl has no linear trend: y = 3, R² = 0.
    assert.ok(Math.abs(flat.a - 3) < 1e-9 && Math.abs(flat.b) < 1e-9 && Math.abs(flat.r2) < 1e-12);
    // (i − 2)² + 1 = 5 − 4i + i², recovered by a polynomial of order 2.
    const poly = (await layout('line', { series: [[5, 2, 1, 2, 5]], labels: ['Q1', 'Q2', 'Q3', 'Q4', 'Q5'], ...size, trendline: 'polynomial', trendlineOrder: 2 })).trends[0];
    assert.deepEqual(poly.coefficients.map(c => Math.round(c * 1e9) / 1e9), [5, -4, 1]);
    // Missing values are left out of the fit; stacked and percent draw no trend.
    const gap = (await layout('line', { series: [[1, null, 3]], ...size, trendline: 'linear' })).trends[0];
    assert.ok(Math.abs(gap.b - 1) < 1e-9 && Math.abs(gap.a - 1) < 1e-9);
    assert.equal((await layout('line', { series: [[1, 2]], ...size, stacked: true, trendline: 'linear' })).trends, undefined);
    assert.equal((await layout('line', { series: [[1, 2]], ...size, percent: true, trendline: 'linear' })).trends, undefined);
    // Horizontal: the labels run down y, so the trend ends on the first and last points.
    const h = await layout('line', { series: [[2, 4, 6, 8]], labels: ['Q1', 'Q2', 'Q3', 'Q4'], ...size, horizontal: true, trendline: 'linear' });
    const hp = h.series[0].points;
    assert.deepEqual(ends(h.trends[0].path), [[hp[0].x, hp[0].y], [hp[3].x, hp[3].y]]);
});

test('bar: a moving-average trend through the category centres, also horizontal', async () => {
    const spec = { categories: ['Jan', 'Feb', 'Mar', 'Apr'], series: [[10, 20, 30, 40]], ...size, trendline: 'movingaverage', trendlinePeriod: 2 };
    const out = await layout('bar', spec);
    const t = out.trends[0];
    assert.equal(t.period, 2);
    const v = t.path.slice(1).split('L').map(s => s.split(',').map(Number));
    // Means 15, 25, 35 at Feb, Mar, Apr.
    assert.deepEqual(v.map(([x]) => x), out.xTicks.slice(1).map(tk => tk.pos));
    const [ta, tb] = out.yTicks;
    const yAt = val => ta.pos + (val - ta.value) * (tb.pos - ta.pos) / (tb.value - ta.value);
    [15, 25, 35].forEach((m, i) => assert.ok(Math.abs(v[i][1] - yAt(m)) < 0.02));

    const h = await layout('bar', { ...spec, horizontal: true });
    const hv = h.trends[0].path.slice(1).split('L').map(s => s.split(',').map(Number));
    assert.deepEqual(hv.map(([, y]) => y), h.yTicks.slice(1).map(tk => tk.pos));
    assert.equal((await layout('bar', { ...spec, stacked: true })).trends, undefined);
    // A linear trend on bars: y = 10 + 10c, ending on the first and last bar tops.
    const lin = await layout('bar', { ...spec, trendline: 'linear' });
    assert.deepEqual(ends(lin.trends[0].path).map(([, y]) => y), [lin.bars[0].y, lin.bars[3].y]);
});

test('bar: on grouped bars each trend runs through its own series\' bars', async () => {
    const out = await layout('bar', { categories: ['Q1', 'Q2', 'Q3'], series: [[10, 20, 30], [5, 15, 25]], ...size, trendline: 'linear' });
    for (const s of [0, 1]) {
        const xs = out.trends[s].path.slice(1).split('L').map(p => Number(p.split(',')[0]));
        const centres = out.bars.filter(b => b.s === s).map(b => b.x + b.width / 2);
        assert.ok(Math.abs(xs[0] - centres[0]) < 0.02, `series ${s} starts on its first bar`);
        assert.ok(Math.abs(xs.at(-1) - centres.at(-1)) < 0.02, `series ${s} ends on its last bar`);
    }
});

// Measured mode, against a stubbed ResizeObserver and element.
const observers = [];
globalThis.ResizeObserver = class {
    constructor(callback) { this.callback = callback; this.targets = []; observers.push(this); }
    observe(el) { this.targets.push(el); }
    disconnect() { this.targets = []; this.disconnected = true; }
};
const dotnet = () => ({ calls: [], invokeMethodAsync(name, arg) { this.calls.push([name, arg]); return Promise.resolve(); } });
const wait = ms => new Promise(resolve => setTimeout(resolve, ms));

test('measured: lays out at the element size and again, debounced, when it is resized', async () => {
    const el = { clientWidth: 300, clientHeight: 120 };
    const ref = dotnet();
    const first = await observe(el, 'spark', { data: [1, 2], pad: 2 }, ref);
    assert.equal(first.width, 300);
    assert.equal(first.height, 120);
    assert.equal(first.line, 'M2,118L298,2');

    const ro = observers.at(-1);
    ro.callback(); // initial notification at the same size: nothing to do
    el.clientWidth = 200;
    ro.callback();
    ro.callback();
    await wait(150);
    assert.equal(ref.calls.length, 1);
    assert.equal(ref.calls[0][0], 'OnMeasuredLayout');
    assert.equal(ref.calls[0][1].width, 200);

    const next = await update(el, { data: [2, 1], pad: 2 });
    assert.equal(next.line, 'M2,2L198,118');
    unobserve(el);
    assert.ok(ro.disconnected);
    assert.equal(await update(el, { data: [1] }), null);
});

test('measured: a box without a size waits until it gets one', async () => {
    const el = { clientWidth: 0, clientHeight: 0 };
    const ref = dotnet();
    assert.equal(await observe(el, 'spark', { data: [1, 2] }, ref), null);
    el.clientWidth = 100;
    el.clientHeight = 20;
    observers.at(-1).callback();
    await wait(150);
    assert.equal(ref.calls.length, 1);
    assert.equal(ref.calls[0][1].width, 100);
    unobserve(el);
});

test('frame: observing with no kind reports the box width only, and again when the width changes', async () => {
    const el = { clientWidth: 360, clientHeight: 200 };
    const ref = dotnet();
    assert.equal(await observe(el, null, null, ref), 360);
    const ro = observers.at(-1);
    el.clientHeight = 260; // the chart's own height changing (layout arrived, legend) is not a resize
    ro.callback();
    await wait(150);
    assert.equal(ref.calls.length, 0);
    el.clientWidth = 330;
    ro.callback();
    await wait(150);
    assert.deepEqual(ref.calls, [['OnFrameWidth', 330]]);
    unobserve(el);
});

test('measured: arrow keys on a slider inside the chart do not scroll the page; other keys pass', async () => {
    const listeners = {};
    const el = {
        clientWidth: 300, clientHeight: 120,
        addEventListener(type, fn) { listeners[type] = fn; },
        removeEventListener(type, fn) { if (listeners[type] === fn) delete listeners[type]; },
    };
    await observe(el, 'spark', { data: [1, 2] }, dotnet());
    const key = (k, slider) => {
        const event = { key: k, prevented: false, target: { closest: s => (slider && s === '[role="slider"]' ? {} : null) }, preventDefault() { this.prevented = true; } };
        listeners.keydown(event);
        return event.prevented;
    };
    assert.equal(key('ArrowLeft', true), true);
    assert.equal(key('PageUp', true), true);
    assert.equal(key('End', true), true);
    assert.equal(key('Tab', true), false);
    assert.equal(key('ArrowLeft', false), false);
    unobserve(el);
    assert.equal(listeners.keydown, undefined);
});

test('line: horizontal rotates the chart, labels down the side, values along x; step and smooth follow y', async () => {
    const out = await layout('line', { series: [[1, 3, 2]], labels: ['Mon', 'Tue', 'Wed'], horizontal: true, ...size });
    assert.deepEqual(out.yTicks.map(t => t.label), ['Mon', 'Tue', 'Wed']);
    assert.ok(out.plot.left >= 'Mon'.length * 6.6);
    assert.equal(out.xTicks[0].label, '0');
    const x = v => out.xTicks.find(t => t.label === String(v)).pos;
    const [p0, p1, p2] = out.series[0].points;
    assert.deepEqual([p0.x, p1.x, p2.x], [x(1), x(3), x(2)]);
    assert.deepEqual([p0.y, p1.y, p2.y], out.yTicks.map(t => t.pos));
    assert.ok(p0.y < p1.y && p1.y < p2.y, 'categories run top to bottom');
    // The area fills from the zero line on the left.
    assert.match(out.series[0].area, new RegExp(`L${x(0)},${p0.y}Z$`));

    const step = await layout('line', { series: [[1, 3, 2]], labels: ['Mon', 'Tue', 'Wed'], horizontal: true, step: true, ...size });
    const [s0, s1] = step.series[0].points;
    // A step holds its value down to the next label, then moves across.
    assert.match(step.series[0].line, new RegExp(`^M${s0.x},${s0.y}L${s0.x},${s1.y}L${s1.x},${s1.y}`));

    const smooth = await layout('line', { series: [[1, 3, 2], [2, 1, 2]], horizontal: true, smooth: true, stacked: true, area: true, ...size });
    assert.match(smooth.series[0].line, /C/);
    assert.equal(smooth.series[1].points[0].x, smooth.xTicks.find(t => t.label === '3').pos); // stacked on the running total
});

test('line: horizontal percent stacks reach exactly 100% on the x axis', async () => {
    const { xTicks, series, plot } = await layout('line', { series: [[1, 3], [3, 1]], percent: true, horizontal: true, showGrid: true, ...size });
    assert.equal(xTicks.at(-1).label, '100%');
    assert.ok(series[1].points.every(p => p.x === plot.right));
});

test('line and bar: references and bands sit at their values, labels inside the plot', async () => {
    const out = await layout('line', {
        series: [[20, 35, 30]], labels: ['A', 'B', 'C'],
        references: [{ value: 60, label: 'Quota' }, { value: 25, label: 'Target' }], bands: [{ from: 10, to: 30, label: 'Normal' }, { from: 500, to: 900 }],
        ...size,
    });
    const y = v => out.yTicks.find(t => t.label === String(v))?.pos;
    assert.equal(out.yTicks.at(-1).label, '60', 'the axis grows to show the reference');
    const [quota, target] = out.references;
    assert.deepEqual([quota.y1, quota.y2, quota.x1, quota.x2], [y(60), y(60), out.plot.left, out.plot.right]);
    assert.ok(quota.labelY > quota.y1 && quota.labelY - 11 >= out.plot.top, 'a line at the top is labelled below it');
    assert.ok(target.labelY < target.y1 && target.anchor === 'end' && target.labelX < out.plot.right);
    assert.equal(out.bands.length, 1, 'a band outside the axis is dropped');
    const [normal] = out.bands;
    assert.deepEqual([normal.y, normal.y + normal.height].map(Math.round), [y(30), y(10)].map(Math.round));
    assert.ok(normal.labelX > out.plot.left && normal.labelY > normal.y);

    const bar = await layout('bar', { categories: ['A', 'B'], series: [[10, 40]], horizontal: true, references: [{ value: 50, label: 'Limit' }], bands: [{ from: 30, to: 0 }], ...size });
    const x = v => bar.xTicks.find(t => t.label === String(v))?.pos;
    const [limit] = bar.references;
    assert.deepEqual([limit.x1, limit.x2, limit.y1, limit.y2], [x(50), x(50), bar.plot.top, bar.plot.bottom]);
    assert.equal(limit.anchor, 'end', 'a label that would leave the plot turns back');
    assert.ok(limit.labelX - 'Limit'.length * 6.6 >= bar.plot.left);
    assert.deepEqual([bar.bands[0].x, Math.round(bar.bands[0].x + bar.bands[0].width)], [x(0), Math.round(x(30))]);

    const vertical = await layout('bar', { categories: ['A'], series: [[10]], references: [{ value: 4 }], ...size });
    assert.equal(vertical.references[0].y1, vertical.yTicks.find(t => t.label === '4').pos);
    assert.deepEqual(vertical.bands, []);
    // Without references or bands, the result has neither key.
    assert.ok(!('references' in await layout('bar', { categories: ['A'], series: [[10]], ...size })));
});
