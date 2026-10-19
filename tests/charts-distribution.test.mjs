// node --test tests/ — the distribution layouts computed by the bundled D3 module.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';
import { boxStats, kde } from '../src/DaisyBlazor.Charts/wwwroot/layouts/_distribution.mjs';
import { binOf } from '../src/DaisyBlazor.Charts/wwwroot/layouts/hexbin.mjs';
import { dodge } from '../src/DaisyBlazor.Charts/wwwroot/layouts/beeswarm.mjs';
import scatter from '../src/DaisyBlazor.Charts/wwwroot/layouts/scatter.mjs';
import { fitTrend } from '../src/DaisyBlazor.Charts/wwwroot/layouts/_shared.mjs';
import { idw, surface } from '../src/DaisyBlazor.Charts/wwwroot/layouts/contour.mjs';

const size = { width: 640, height: 360 };

// A deterministic spread of values (linear congruential generator).
function lcg(seed) {
    let s = seed;
    return () => (s = (s * 1664525 + 1013904223) % 4294967296) / 4294967296;
}

const inside = (plot, x, y) => x >= plot.left - 1e-6 && x <= plot.right + 1e-6 && y >= plot.top - 1e-6 && y <= plot.bottom + 1e-6;

function assertAxes({ plot, xTicks, yTicks }) {
    assert.ok(plot.left > 0 && plot.right < size.width && plot.top > 0 && plot.bottom < size.height);
    for (const t of xTicks) {
        assert.ok(t.pos >= plot.left - 1e-6 && t.pos <= plot.right + 1e-6, `x tick ${t.label} at ${t.pos}`);
    }
    for (const t of yTicks) {
        assert.ok(t.pos >= plot.top - 1e-6 && t.pos <= plot.bottom + 1e-6, `y tick ${t.label} at ${t.pos}`);
    }
}

test('box stats: quartiles, Tukey whiskers and outliers', () => {
    const s = boxStats([1, 2, 3, 4, 5, 6, 7, 8, 9, 100, NaN, Infinity]);
    assert.equal(s.count, 10);
    assert.equal(s.q1, 3.25);
    assert.equal(s.median, 5.5);
    assert.equal(s.q3, 7.75);
    assert.equal(s.low, 1);
    assert.equal(s.high, 9);
    assert.deepEqual(s.outliers, [100]);
    assert.equal(boxStats([NaN]), null);
    assert.deepEqual([boxStats([4]).q1, boxStats([4]).high], [4, 4]);
});

test('kernel density integrates to about one', () => {
    const at = Array.from({ length: 2001 }, (_, i) => -10 + i * 0.01);
    const d = kde([0, 1, 2], 0.5, at);
    const total = d.reduce((a, b) => a + b, 0) * 0.01;
    assert.ok(Math.abs(total - 1) < 1e-3, `total ${total}`);
});

test('scatter: points inside the plot, series in order, Voronoi cells, log scale drops non-positive', async () => {
    const points = [
        { x: 1, y: 10, series: 'EU' }, { x: 2, y: 30, series: 'US' }, { x: 3, y: 20, series: 'EU' }, { x: NaN, y: 1 },
    ];
    const out = await layout('scatter', { points, ...size, xTitle: 'Seats', yTitle: 'Calls' });
    assertAxes(out);
    assert.deepEqual(out.series, ['EU', 'US']);
    assert.equal(out.points.length, 3);
    for (const p of out.points) {
        assert.ok(inside(out.plot, p.cx, p.cy));
        assert.match(p.cell, /^M[\d.]+,[\d.]+L.*Z$/);
    }
    assert.ok(out.points[1].cy < out.points[0].cy, 'higher y is drawn higher');

    const log = await layout('scatter', { points: [{ x: 5, y: 0 }, { x: 1, y: 5 }, { x: 10, y: 6 }, { x: 1000, y: 7 }], ...size, xLog: true, yLog: true });
    assert.equal(log.points.length, 3);
    assert.deepEqual(log.xTicks.filter(t => ['1', '10', '100', '1,000'].includes(t.label)).length, 4);
    const [, a, b] = log.points;
    assert.ok(Math.abs((b.cx - a.cx) - 2 * (log.xTicks.find(t => t.label === '100').pos - log.xTicks.find(t => t.label === '10').pos)) < 0.05);
});

test('scatter: bubbles scale by area, largest drawn first', async () => {
    const out = await layout('scatter', { points: [{ x: 1, y: 1, size: 100 }, { x: 2, y: 2, size: 400 }, { x: 3, y: 3, size: 25 }], ...size, minRadius: 3, maxRadius: 20 });
    assert.deepEqual(out.points.map(p => p.size), [400, 100, 25]);
    assert.equal(out.points[0].r, 20);
    assert.equal(out.points[2].r, 3);
});

test('contour: nested density bands in the plot', async () => {
    const rand = lcg(7);
    const points = Array.from({ length: 200 }, () => ({ x: 50 + (rand() + rand() - 1) * 20, y: 200 + (rand() + rand() - 1) * 60 }));
    const out = await layout('contour', { points, ...size, thresholds: 8, bandwidth: 18 });
    assertAxes(out);
    assert.ok(out.contours.length >= 3);
    for (let i = 1; i < out.contours.length; i++) {
        assert.ok(out.contours[i].value > out.contours[i - 1].value);
    }
    assert.match(out.contours[0].path, /^M/);
    assert.deepEqual(out.points, []);
});

test('hexbin: counts add up, bins on the hex grid', async () => {
    const rand = lcg(3);
    const points = Array.from({ length: 300 }, () => ({ x: rand() * 100, y: rand() * 50 }));
    const out = await layout('hexbin', { points, ...size, radius: 10 });
    assertAxes(out);
    assert.equal(out.bins.reduce((a, b) => a + b.count, 0), 300);
    assert.equal(out.maxCount, out.bins[out.bins.length - 1].count);
    assert.match(out.hexagon, /^M8\.66,-5L8\.66,5L0,10L-8\.66,5L-8\.66,-5L0,-10Z$/);
    for (const b of out.bins) {
        const row = (b.cy - out.plot.top) / 15;
        assert.ok(Math.abs(row - Math.round(row)) < 1e-2, 'rows every 1.5 r');
    }
});

test('hexbin: a point goes to the nearest hexagon centre', () => {
    const r = 10, dx = r * Math.sqrt(3), dy = 15;
    const rand = lcg(11);
    for (let n = 0; n < 500; n++) {
        const px = rand() * 200, py = rand() * 200;
        const [i, j] = binOf(px, py, r);
        const d = Math.hypot(px - (i + (j & 1) / 2) * dx, py - j * dy);
        for (const [ni, nj] of [[i - 1, j], [i + 1, j], [i, j - 1], [i, j + 1], [i - 1, j - 1], [i + 1, j + 1], [i - 1, j + 1], [i + 1, j - 1]]) {
            assert.ok(d <= Math.hypot(px - (ni + (nj & 1) / 2) * dx, py - nj * dy) + 1e-9);
        }
    }
});

test('box plot: vertical and horizontal geometry', async () => {
    const groups = [{ name: 'EU', values: [1, 2, 3, 4, 5, 6, 7, 8, 9, 100] }, { name: 'Empty', values: [] }, { name: 'US', values: [10, 20, 30] }];
    const v = await layout('boxplot', { groups, ...size, valueTitle: 'ms' });
    assertAxes(v);
    assert.deepEqual(v.groups.map(g => [g.name, g.index]), [['EU', 0], ['US', 2]]);
    const eu = v.groups[0];
    assert.equal(eu.median, 5.5);
    assert.deepEqual(eu.outliers.map(o => o.value), [100]);
    assert.ok(eu.pos.high < eu.pos.q3 && eu.pos.q3 < eu.pos.median && eu.pos.median < eu.pos.q1 && eu.pos.q1 < eu.pos.low, 'values grow upwards');
    assert.deepEqual(v.xTicks.map(t => t.label), ['EU', 'US']);

    const h = await layout('boxplot', { groups, ...size, horizontal: true });
    assert.ok(h.groups[0].pos.low < h.groups[0].pos.high, 'values grow rightwards');
    assert.deepEqual(h.yTicks.map(t => t.label), ['EU', 'US']);
    assert.ok(h.plot.left > 'Empty'.length, 'room for group names');
    assert.deepEqual((await layout('boxplot', { groups: [], ...size })).groups, []);
});

test('violin: mirrored density within the band, quartile markers', async () => {
    const rand = lcg(5);
    const groups = [{ name: 'A', values: Array.from({ length: 80 }, () => 100 + rand() * 50) }, { name: 'B', values: [5, 5, 5] }];
    const out = await layout('violin', { groups, ...size });
    assertAxes(out);
    assert.equal(out.groups.length, 2);
    for (const g of out.groups) {
        assert.match(g.path, /^M[\d.-]+,[\d.-]+/);
        assert.ok(g.pos.q3 <= g.pos.median && g.pos.median <= g.pos.q1);
    }
    assert.deepEqual((await layout('violin', { groups: [{ name: 'X', values: [NaN] }], ...size })).groups, []);
});

test('histogram: counts every value, edges are nice, median marker', async () => {
    const values = [12, 15, 18, 21, 22, 25, 31, 33, 38, 47, 52, 58, NaN];
    const out = await layout('histogram', { values, ...size, bins: 5, showMedian: true });
    assertAxes(out);
    assert.equal(out.bars.reduce((a, b) => a + b.count, 0), 12);
    assert.deepEqual(out.bars.map(b => b.x0), [10, 20, 30, 40, 50]);
    assert.equal(out.median.value, 28);
    assert.ok(out.median.pos > out.bars[1].left && out.median.pos < out.bars[1].right);
    for (const b of out.bars) {
        assert.ok(b.top <= b.bottom && b.left < b.right && inside(out.plot, b.left, b.top));
    }
    const none = await layout('histogram', { values: [], ...size, showMedian: true });
    assert.deepEqual([none.bars, none.median], [[], null]);
});

test('beeswarm: dots never overlap and stay in their row', async () => {
    const rand = lcg(9);
    const groups = [{ name: 'Free', values: Array.from({ length: 120 }, () => Math.round(rand() * 40)) }, { name: 'Pro', values: [3, 3, 3, 3] }];
    const out = await layout('beeswarm', { groups, ...size, radius: 3 });
    assertAxes(out);
    const rows = out.groups.map(g => g.center);
    for (const [gi, g] of out.groups.entries()) {
        const half = (rows[1] - rows[0]) / 2;
        for (const d of g.dots) {
            assert.ok(Math.abs(d.cy - rows[gi]) <= half, 'inside its row');
        }
    }
    const pro = out.groups[1].dots;
    for (let i = 0; i < pro.length; i++) {
        for (let k = i + 1; k < pro.length; k++) {
            assert.ok(Math.hypot(pro[i].cx - pro[k].cx, pro[i].cy - pro[k].cy) >= 5.99);
        }
    }
    const again = await layout('beeswarm', { groups, ...size, radius: 3 });
    assert.deepEqual(again, out, 'deterministic');
});

test('dodge places equal values side by side around the centre', () => {
    assert.deepEqual(dodge([0, 0, 0], 1).map(y => Math.round(y)), [0, -2, 2]);
    assert.deepEqual(dodge([0, 10], 1), [0, 0]);
});

test('ridgeline: one curve per group, rows top to bottom, peak within the plot', async () => {
    const groups = [{ name: 'Mon', values: [1, 2, 2, 3] }, { name: 'Tue', values: [4, 5, 5, 6] }, { name: 'Wed', values: [] }];
    const out = await layout('ridgeline', { groups, ...size, overlap: 1 });
    assertAxes(out);
    assert.deepEqual(out.yTicks.map(t => t.label), ['Mon', 'Tue']);
    assert.ok(out.groups[0].baseline < out.groups[1].baseline);
    assert.equal(out.groups[1].baseline, out.plot.bottom);
    for (const g of out.groups) {
        const ys = [...g.line.matchAll(/,(-?[\d.]+)/g)].map(m => Number(m[1]));
        assert.ok(Math.min(...ys) >= out.plot.top - 0.01, 'the peak stays below the top');
    }
    assert.deepEqual((await layout('ridgeline', { groups: [], ...size })).groups, []);
});

test('every layout copes with missing input', async () => {
    for (const kind of ['scatter', 'contour', 'hexbin', 'boxplot', 'violin', 'histogram', 'beeswarm', 'ridgeline']) {
        const out = await layout(kind, { ...size });
        assert.ok(out.plot, kind);
        assert.ok(Array.isArray(out.xTicks) && Array.isArray(out.yTicks), kind);
    }
});

test('scatter trend fits: least-squares coefficients and R² against known values', () => {
    // y = 1/3 + 1.5x, R² = 81/84 (worked by hand).
    const lin = fitTrend('linear', [1, 3, 2], [2, 5, 3]);
    assert.ok(Math.abs(lin.a - 1 / 3) < 1e-12 && Math.abs(lin.b - 1.5) < 1e-12);
    assert.ok(Math.abs(lin.r2 - 81 / 84) < 1e-12);
    // Exact curves are recovered with R² = 1.
    const exp = fitTrend('exponential', [0, 1, 2, 3], [2, 2 * Math.E, 2 * Math.E ** 2, 2 * Math.E ** 3]);
    assert.ok(Math.abs(exp.a - 2) < 1e-9 && Math.abs(exp.b - 1) < 1e-9 && Math.abs(exp.r2 - 1) < 1e-12);
    const log = fitTrend('logarithmic', [1, Math.E, Math.E ** 2], [5, 7, 9]);
    assert.ok(Math.abs(log.a - 5) < 1e-9 && Math.abs(log.b - 2) < 1e-9);
    const pow = fitTrend('power', [1, 2, 4, 8, -1], [3, 12, 48, 192, 3]);
    assert.ok(Math.abs(pow.a - 3) < 1e-9 && Math.abs(pow.b - 2) < 1e-9, 'x ≤ 0 is left out');
    assert.equal(fitTrend('linear', [2, 2], [1, 5]), null, 'all x equal');
    assert.equal(fitTrend('exponential', [1, 2], [0, -1]), null, 'nothing above zero');
    assert.equal(fitTrend('none', [1, 2], [1, 2]), null);
});

test('scatter: showLine joins each series in x order, trendline draws through the scales', async () => {
    const points = [{ x: 3, y: 5, series: 'EU' }, { x: 1, y: 2, series: 'EU' }, { x: 2, y: 3, series: 'EU' }, { x: 1, y: 9, series: 'US' }, { x: 4, y: 1, series: 'US' }];
    const plain = await layout('scatter', { points, ...size });
    assert.equal(plain.lines, undefined);
    assert.equal(plain.trends, undefined);

    const out = await layout('scatter', { points, ...size, showLine: true, trendline: 'linear' });
    const eu = out.points.filter(p => p.series === 0).sort((a, b) => a.x - b.x);
    assert.equal(out.lines[0].path, `M${eu.map(p => `${p.cx},${p.cy}`).join('L')}`);
    assert.equal(out.lines.length, 2);
    assert.equal(out.trends.length, 2);
    const t = out.trends[0];
    assert.equal(t.kind, 'linear');
    assert.ok(Math.abs(t.b - 1.5) < 1e-12 && Math.abs(t.r2 - 81 / 84) < 1e-12);
    // The straight line starts at x = 1 on the fitted value and stays in the plot.
    const [x0, y0] = t.path.slice(1).split('L')[0].split(',').map(Number);
    assert.equal(x0, eu[0].cx);
    const ys = [...t.path.matchAll(/,(-?[\d.]+)/g)].map(m => Number(m[1]));
    assert.ok(ys.every(v => v >= out.plot.top - 1e-6 && v <= out.plot.bottom + 1e-6));
    const [ta, tb] = out.yTicks;
    const yAt = v => ta.pos + (v - ta.value) * (tb.pos - ta.pos) / (tb.value - ta.value);
    assert.ok(Math.abs(y0 - yAt(1 / 3 + 1.5)) < 0.02);

    // On log axes, a power law y = 2x is straight in pixels: the fit is in data space.
    const logOut = await layout('scatter', { points: [1, 10, 100, 1000].map(x => ({ x, y: 2 * x })), ...size, xLog: true, yLog: true, trendline: 'power' });
    const tr = logOut.trends[0];
    assert.ok(Math.abs(tr.a - 2) < 1e-9 && Math.abs(tr.b - 1) < 1e-9);
    const pts = tr.path.slice(1).split('L').map(s => s.split(',').map(Number));
    const slope = (pts.at(-1)[1] - pts[0][1]) / (pts.at(-1)[0] - pts[0][0]);
    assert.ok(pts.every(([px, py]) => Math.abs(py - (pts[0][1] + slope * (px - pts[0][0]))) < 0.05), 'straight on log-log axes');
    // A linear fit on a log x axis bends in pixels.
    const bent = (await layout('scatter', { points: [1, 10, 100, 1000].map(x => ({ x, y: x })), ...size, xLog: true, trendline: 'linear' })).trends[0];
    assert.ok(bent.path.split('L').length > 10);
    assert.deepEqual((await layout('scatter', { ...size, showLine: true, trendline: 'exponential' })).trends, []);
});

test('scatter: smooth draws each series line as a monotone spline through the same points', async () => {
    const points = [{ x: 3, y: 5 }, { x: 1, y: 2 }, { x: 2, y: 3 }, { x: 4, y: 4.5 }];
    const out = await layout('scatter', { points, ...size, showLine: true, smooth: true });
    const path = out.lines[0].path;
    const sorted = out.points.slice().sort((a, b) => a.x - b.x);
    assert.ok(path.startsWith(`M${sorted[0].cx},${sorted[0].cy}C`), path);
    assert.ok(path.endsWith(`${sorted.at(-1).cx},${sorted.at(-1).cy}`));
    assert.equal(path.split('C').length - 1, points.length - 1, 'one curve per gap');
    // Without smooth, straight segments as before.
    assert.doesNotMatch((await layout('scatter', { points, ...size, showLine: true })).lines[0].path, /C/);
});

test('scatter: high density counts every point on a bounded grid, without per-point marks', async () => {
    const rnd = lcg(5);
    const points = Array.from({ length: 200 }, (_, i) => ({ x: rnd() * 50, y: rnd() * rnd() * 20, series: i % 4 === 3 ? 'Web' : 'API' }));
    const out = await layout('scatter', { points, ...size, highDensity: true, cellSize: 4 });
    assert.deepEqual(out.points, []);
    assert.equal(out.cellSize, 4);
    assert.deepEqual(out.series, ['API', 'Web']);
    const counts = out.density.flatMap(d => d.cells.filter((_, i) => i % 3 === 2));
    assert.equal(counts.reduce((a, b) => a + b, 0), points.length, 'cell counts sum to the point count');
    assert.equal(out.density[1].cells.filter((_, i) => i % 3 === 2).reduce((a, b) => a + b, 0), 50);
    const cols = Math.ceil((out.plot.right - out.plot.left) / 4), rows = Math.ceil((out.plot.bottom - out.plot.top) / 4);
    for (const d of out.density) {
        for (let i = 0; i < d.cells.length; i += 3) {
            assert.ok(d.cells[i] >= 0 && d.cells[i] < cols && d.cells[i + 1] >= 0 && d.cells[i + 1] < rows && d.cells[i + 2] > 0);
        }
    }
    assert.equal((await layout('scatter', { points: points.slice(0, 3), ...size })).density, undefined, 'not returned unless asked');
});

test('scatter: 100k points in high density stay fast and bounded', () => {
    // Called directly, not through layout(), to keep the recorded server fixture small.
    const rnd = lcg(11);
    const points = Array.from({ length: 100_000 }, () => ({ x: rnd() * 100 + rnd() * 30, y: rnd() * rnd() * 80, series: null }));
    const start = performance.now();
    const out = scatter({ points, ...size, highDensity: true });
    const ms = performance.now() - start;
    assert.ok(ms < 2000, `took ${ms} ms`);
    const cells = out.density[0].cells.length / 3;
    const bound = Math.ceil((out.plot.right - out.plot.left) / 3) * Math.ceil((out.plot.bottom - out.plot.top) / 3);
    assert.ok(cells <= bound, `${cells} cells`);
    assert.equal(out.density[0].cells.filter((_, i) => i % 3 === 2).reduce((a, b) => a + b, 0), 100_000);
    assert.ok(JSON.stringify(out).length < 300_000, 'the result stays small');
});

test('scatter: error bars span ±error on each axis, clamped to the plot', async () => {
    const points = [{ x: 2, y: 10, xError: 0.5, yError: 3 }, { x: 4, y: 20 }, { x: 6, y: 15, yError: 2 }];
    const out = await layout('scatter', { points, ...size });
    const [a, b, c] = out.points;
    const [x0, x1] = out.xTicks, [y0, y1] = out.yTicks;
    const xAt = v => x0.pos + (v - x0.value) * (x1.pos - x0.pos) / (x1.value - x0.value);
    const yAt = v => y0.pos + (v - y0.value) * (y1.pos - y0.pos) / (y1.value - y0.value);
    assert.equal(a.xWhisker.error, 0.5);
    assert.ok(Math.abs(a.xWhisker.from - xAt(1.5)) < 0.02 && Math.abs(a.xWhisker.to - xAt(2.5)) < 0.02);
    assert.ok(Math.abs(a.yWhisker.from - yAt(7)) < 0.02 && Math.abs(a.yWhisker.to - yAt(13)) < 0.02);
    assert.ok(Math.abs((a.yWhisker.from + a.yWhisker.to) / 2 - a.cy) < 0.02, 'centred on the point');
    assert.equal(b.xWhisker, undefined);
    assert.equal(b.yWhisker, undefined);
    assert.equal(c.xWhisker, undefined);
    assert.equal(c.yWhisker.error, 2);
    for (const p of [a, c]) {
        assert.ok(inside(out.plot, p.cx, p.yWhisker.from) && inside(out.plot, p.cx, p.yWhisker.to), 'the scale takes the bars in');
    }
    // On a log axis, a bar reaching zero or below runs to the axis' start (the plot's bottom, less the dot inset).
    const log = await layout('scatter', { points: [{ x: 1, y: 10, yError: 20 }, { x: 2, y: 100 }], ...size, yLog: true });
    assert.equal(log.points[0].yWhisker.from, log.plot.bottom - 4);
});

test('scatter: stacked series sit on the running total at the same x', async () => {
    const points = [
        { x: 1, y: 2, series: 'Compute' }, { x: 2, y: 3, series: 'Compute' },
        { x: 1, y: 5, series: 'Storage' }, { x: 3, y: 4, series: 'Storage' },
        { x: 1, y: 1, series: 'Network' }, { x: 2, y: 2, series: 'Network' },
    ];
    const out = await layout('scatter', { points, ...size, stacked: true, showLine: true });
    const [y0, y1] = out.yTicks;
    const yAt = v => y0.pos + (v - y0.value) * (y1.pos - y0.pos) / (y1.value - y0.value);
    assert.deepEqual(out.points.map(p => [p.y, p.total]), [[2, 2], [3, 3], [5, 7], [4, 4], [1, 8], [2, 5]], 'own y kept, totals stacked; x = 3 starts from 0');
    for (const p of out.points) {
        assert.ok(Math.abs(p.cy - yAt(p.total)) < 0.02);
    }
    assert.equal((await layout('scatter', { points, ...size })).points[0].total, undefined);
});

test('scatter: a trend line is not drawn on stacked series, and fits each series on its own values', async () => {
    const points = [
        { x: 1, y: 1, series: 'A' }, { x: 2, y: 2, series: 'A' }, { x: 3, y: 3, series: 'A' },
        { x: 1, y: 10, series: 'B' }, { x: 2, y: 10, series: 'B' }, { x: 3, y: 10, series: 'B' },
    ];
    const stacked = await layout('scatter', { points, ...size, stacked: true, trendline: 'linear' });
    assert.equal(stacked.trends, undefined, 'Trendline is ignored when stacked');
    const plain = await layout('scatter', { points, ...size, trendline: 'linear' });
    assert.deepEqual(plain.trends.map(t => [t.series, Math.round(t.a * 1e9) / 1e9, Math.round(t.b * 1e9) / 1e9]), [[0, 0, 1], [1, 10, 0]]);
});

test('scatter: horizontal and vertical references and bands, reference values taken into the scales', async () => {
    const out = await layout('scatter', {
        points: [{ x: 1, y: 1 }, { x: 5, y: 4 }], ...size,
        references: [{ value: 10, label: 'Limit' }], bands: [{ from: 2, to: 3, label: 'Normal' }],
        xReferences: [{ value: 3, label: 'Split' }], xBands: [{ from: 4, to: 2 }],
    });
    const [x0, x1] = out.xTicks, [y0, y1] = out.yTicks;
    const xAt = v => x0.pos + (v - x0.value) * (x1.pos - x0.pos) / (x1.value - x0.value);
    const yAt = v => y0.pos + (v - y0.value) * (y1.pos - y0.pos) / (y1.value - y0.value);
    const [h] = out.references, [v] = out.xReferences;
    assert.ok(Math.abs(h.y1 - yAt(10)) < 0.02 && h.y1 === h.y2 && h.x1 === out.plot.left && h.x2 === out.plot.right, 'horizontal at y = 10, across the plot');
    assert.ok(Math.abs(v.x1 - xAt(3)) < 0.02 && v.x1 === v.x2 && v.y1 === out.plot.top && v.y2 === out.plot.bottom, 'vertical at x = 3');
    assert.ok(out.yTicks.some(t => t.value >= 10), 'the y axis grows to show the reference');
    assert.ok(Math.abs(out.bands[0].y - yAt(3)) < 0.02 && Math.abs(out.bands[0].height - (yAt(2) - yAt(3))) < 0.02);
    assert.ok(Math.abs(out.xBands[0].x - xAt(2)) < 0.02 && Math.abs(out.xBands[0].width - (xAt(4) - xAt(2))) < 0.02);
    // A log axis drops what it cannot show, keeping each mark's input index.
    const log = await layout('scatter', { points: [{ x: 1, y: 1 }, { x: 10, y: 100 }], ...size, yLog: true, references: [{ value: -1 }, { value: 10 }] });
    assert.deepEqual(log.references.map(r => r.index), [1]);
    const plain = await layout('scatter', { points: [{ x: 1, y: 1 }], ...size });
    for (const key of ['references', 'bands', 'xReferences', 'xBands']) {
        assert.equal(plain[key], undefined, key);
    }
});

test('contour value mode: interpolation is exact at the points and bounded between them', () => {
    const known = [{ cx: 0, cy: 0, value: 10 }, { cx: 100, cy: 0, value: 30 }, { cx: 50, cy: 80, value: 20 }];
    for (const k of known) {
        assert.equal(idw(known, k.cx, k.cy), k.value);
    }
    const rnd = lcg(3);
    for (let i = 0; i < 200; i++) {
        const v = idw(known, rnd() * 300 - 100, rnd() * 300 - 100);
        assert.ok(v >= 10 && v <= 30, `${v}`);
    }
    assert.ok(Math.abs(idw(known.slice(0, 2), 50, 0) - 20) < 1e-12, 'halfway between two points, their mean');
});

test('contour value surface: linear on the triangles, exact at the points, bounded everywhere', () => {
    // A 10 × 8 grid of 10-unit cells; points at cell centres (5 + 10i, 5 + 10j).
    const at = (i, j, value) => ({ cx: 5 + 10 * i, cy: 5 + 10 * j, value });
    const known = [at(1, 1, 10), at(8, 1, 40), at(1, 6, 20), at(8, 6, 70), at(4, 3, 55)];
    const grid = surface(known, 10, 8, 10, 10);
    for (const k of known) {
        assert.ok(Math.abs(grid[((k.cy - 5) / 10) * 10 + (k.cx - 5) / 10] - k.value) < 1e-9, `exact at ${k.cx},${k.cy}`);
    }
    for (const v of grid) {
        assert.ok(v >= 10 - 1e-9 && v <= 70 + 1e-9, `${v}`);
    }
    // A plane is reproduced exactly inside and outside the hull (outside: on the nearest hull edge).
    const plane = (cx, cy) => 3 + 0.5 * cx + 0.25 * cy;
    const corners = [at(2, 2), at(7, 2), at(2, 5), at(7, 5), at(5, 4)].map(k => ({ ...k, value: plane(k.cx, k.cy) }));
    const flat = surface(corners, 10, 8, 10, 10);
    assert.ok(Math.abs(flat[3 * 10 + 4] - plane(45, 35)) < 1e-9, 'inside the hull');
    assert.ok(Math.abs(flat[0] - plane(25, 25)) < 1e-9, 'a corner cell takes the nearest hull corner');
    // Fewer than three points: inverse-distance weighting.
    assert.deepEqual([...surface([at(0, 0, 1), at(1, 0, 3)], 2, 1, 10, 10)], [1, 3]);
});

test('contour value mode: nested value bands over the whole plot, labels on the lines', async () => {
    const points = [];
    for (let i = 0; i <= 6; i++) {
        for (let j = 0; j <= 4; j++) {
            points.push({ x: i * 10, y: j * 5, value: 40 + 3 * i + 2 * j });
        }
    }
    points.push({ x: 1, y: 1, value: null });
    const out = await layout('contour', { points, ...size, mode: 'value', thresholds: 6, showLines: true, showPoints: true });
    assert.equal(out.max, 40 + 18 + 8);
    assert.equal(out.contours[0].value, 40, 'the lowest band starts at the smallest value');
    for (let i = 1; i < out.contours.length; i++) {
        assert.ok(out.contours[i].value > out.contours[i - 1].value);
    }
    assert.ok(out.contours.length >= 4);
    assert.match(out.contours[0].path, /^M/);
    assert.equal(out.points.length, 35, 'points without a value are skipped');
    assert.equal(out.points[0].value, 40);
    assert.ok(out.labels.length > 0);
    const w = out.plot.right - out.plot.left, h = out.plot.bottom - out.plot.top;
    for (const l of out.labels) {
        assert.ok(l.x > 0 && l.x < w && l.y > 0 && l.y < h);
        assert.ok(out.contours.some(c => c.value === l.value));
    }
    const plain = await layout('contour', { points, ...size, mode: 'value' });
    assert.equal(plain.labels, undefined);
    assert.deepEqual(plain.points, []);
    assert.equal((await layout('contour', { points: points.slice(0, 3), ...size, thresholds: 2 })).max, undefined, 'density mode output unchanged');
    assert.deepEqual((await layout('contour', { ...size, mode: 'value', showLines: true })).labels, []);
});
