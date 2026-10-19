// node --test tests/ — the distribution layouts computed by the bundled D3 module.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';
import { boxStats, kde } from '../src/DaisyBlazor.Charts/wwwroot/layouts/_distribution.mjs';
import { binOf } from '../src/DaisyBlazor.Charts/wwwroot/layouts/hexbin.mjs';
import { dodge } from '../src/DaisyBlazor.Charts/wwwroot/layouts/beeswarm.mjs';

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
