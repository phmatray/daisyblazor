// node --test tests/ — the composition and progress layouts computed by the bundled D3 module.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';

const W = 640, H = 360;
const near = (a, b, eps = 1e-6) => assert.ok(Math.abs(a - b) < eps, `${a} ≈ ${b}`);
const inside = (x, y, w = W, h = H) => assert.ok(x >= 0 && x <= w && y >= 0 && y <= h, `(${x}, ${y}) inside ${w}×${h}`);

test('waterfall keeps a running total through deltas and subtotals', async () => {
    const steps = [
        { label: 'Start', value: 100, kind: 'start' },
        { label: 'New', value: 40, kind: 'delta' },
        { label: 'Lost', value: -25, kind: 'delta' },
        { label: 'Q1', value: 0, kind: 'subtotal' },
        { label: 'Bad', value: NaN, kind: 'delta' },
        { label: 'Back', value: 10, kind: 'delta' },
        { label: 'End', value: 0, kind: 'end' },
    ];
    const { bars, connectors, ticks, zero } = await layout('waterfall', { steps, width: W, height: H });
    assert.deepEqual(bars.map(b => b.label), ['Start', 'New', 'Lost', 'Q1', 'Back', 'End']);
    assert.deepEqual(bars.map(b => b.kind), ['total', 'increase', 'decrease', 'total', 'increase', 'total']);
    assert.deepEqual(bars.map(b => b.total), [100, 140, 115, 115, 125, 125]);
    assert.equal(connectors.length, 5);
    // A delta bar floats between the previous total and the new one; a total stands on zero.
    near(bars[2].y, connectors[1].y1);
    near(bars[3].y + bars[3].height, zero);
    near(bars[0].height / 100, bars[1].height / 40, 1e-3);
    assert.ok(ticks.length > 2 && ticks[0].value === 0);
    for (const b of bars) {
        inside(b.x, b.y);
        inside(b.labelX, b.labelY);
        inside(b.categoryX, b.categoryY);
    }
});

test('waterfall turns sideways and handles a dip below zero', async () => {
    const steps = [{ label: 'Start', value: 10, kind: 'start' }, { label: 'Drop', value: -30, kind: 'delta' }, { label: 'End', kind: 'end' }];
    const { bars, zero } = await layout('waterfall', { steps, width: W, height: H, horizontal: true });
    assert.equal(bars[2].total, -20);
    assert.ok(bars[2].x < zero);
    near(bars[2].x + bars[2].width, zero);
    assert.ok(bars[0].y < bars[1].y && bars[1].y < bars[2].y);
    assert.deepEqual(await layout('waterfall', { steps: [], width: W, height: H }), { bars: [], connectors: [], ticks: [], plot: null, zero: 0 });
});

test('funnel computes the conversion from the previous stage and narrows with the value', async () => {
    const stages = [{ label: 'Trial', value: 1000 }, { label: 'Active', value: 600 }, { label: 'Paid', value: 150 }];
    const { stages: out } = await layout('funnel', { stages, width: W, height: H });
    assert.deepEqual(out.map(s => s.conversion), [null, 0.6, 0.25]);
    assert.deepEqual(out.map(s => s.share), [1, 0.6, 0.15]);
    assert.ok(out[0].y1 < out[1].y0);
    const top = s => s.path.match(/^M([\d.]+),[\d.]+L([\d.]+)/).slice(1).map(Number);
    const [a, b] = top(out[0]), [c, d] = top(out[1]);
    near((d - c) / (b - a), 0.6, 1e-3);
    for (const s of out) {
        inside(s.nameX, s.labelY);
        inside(s.valueX, s.labelY);
    }
});

test('a pyramid stacks stages from the base with heights by value, labels kept apart', async () => {
    const stages = [{ label: 'A', value: 90 }, { label: 'B', value: 8 }, { label: 'C', value: 2 }];
    const { stages: out } = await layout('funnel', { stages, width: W, height: H, shape: 'pyramid' });
    assert.ok(out[0].y0 > out[1].y0 && out[1].y0 > out[2].y0);
    near((out[0].y1 - out[0].y0) / (out[1].y1 - out[1].y0), 90 / 8, 1e-3);
    const ys = out.map(s => s.labelY).sort((p, q) => p - q);
    assert.ok(ys[1] - ys[0] >= 34 - 1e-9 && ys[2] - ys[1] >= 34 - 1e-9);
    assert.deepEqual(await layout('funnel', { stages: [{ label: 'x', value: 0 }], width: W, height: H }), { stages: [] });
});

test('gauge clamps the value, places bands, marker and ticks, and fits the box', async () => {
    const spec = { min: 99, max: 100, value: 120, marker: 99.9, bands: [{ from: 98, to: 99.5 }, { from: 99.5, to: 100 }, { from: 5, to: 6 }], startAngle: -120, endAngle: 120, width: 640, height: 520 };
    const g = await layout('gauge', spec);
    assert.equal(g.value, 100);
    assert.equal(g.fraction, 1);
    assert.deepEqual(g.bands.map(b => [b.index, b.from, b.to]), [[0, 99, 99.5], [1, 99.5, 100]]);
    assert.match(g.track, /^M/);
    assert.ok(g.marker.value === 99.9);
    assert.ok(g.ticks.length >= 3);
    for (const t of g.ticks) {
        inside(t.x, t.y, 640, 520);
    }
    inside(g.cx - g.radius, g.cy - g.radius, 640, 520);
    inside(g.cx + g.radius, g.cy, 640, 520);

    const low = await layout('gauge', { ...spec, value: -5 });
    assert.equal(low.value, 99);
    assert.equal(low.fill, '');
    const half = await layout('gauge', { min: 0, max: 10, value: 5, startAngle: -90, endAngle: 90, width: 640, height: 400 });
    near(half.fraction, 0.5);
    assert.ok(half.valueY < 400 && half.cy < 400);
    assert.equal((await layout('gauge', { min: 5, max: 5, value: 5, width: 640, height: 400 })).track, '');
});

test('linear gauge puts min at the start of the track and ticks along it', async () => {
    const h = await layout('linear-gauge', { min: 0, max: 200, value: 50, marker: 150, bands: [{ from: 160, to: 200 }], width: 640, height: 160 });
    near(h.fill.width / h.track.width, 0.25);
    near(h.marker.x1, h.track.x + h.track.width * 0.75);
    near(h.bands[0].x, h.track.x + h.track.width * 0.8);
    assert.ok(h.ticks.every(t => t.y1 > h.track.y + h.track.height - 1e-9));
    for (const t of h.ticks) {
        inside(t.labelX, t.labelY, 640, 160);
    }
    inside(h.pointer.labelX, h.pointer.labelY, 640, 160);

    const v = await layout('linear-gauge', { min: 0, max: 100, value: 500, vertical: true, width: 640, height: 1280 });
    assert.equal(v.value, 100);
    assert.equal(v.fill.height, v.track.height);
    assert.ok(v.ticks[0].labelY > v.ticks.at(-1).labelY);
    assert.equal((await layout('linear-gauge', { min: 1, max: 0, width: 640, height: 160 })).track, null);
});

test('radial bars nest one ring per item, outer first, with labels left of the rings start', async () => {
    const items = [{ label: 'CPU', value: 72 }, { label: 'Memory', value: 140 }, { label: 'Disk', value: -5 }];
    const { cx, rings } = await layout('radial-bar', { items, max: 100, width: 640, height: 640 });
    assert.deepEqual(rings.map(r => r.fraction), [0.72, 1, 0]);
    assert.equal(rings[2].fill, '');
    assert.ok(rings[0].labelY < rings[1].labelY && rings[1].labelY < rings[2].labelY);
    assert.ok(rings.every(r => r.labelX < cx));
    assert.ok(rings[1].labelY - rings[0].labelY > rings[0].fontSize);
    const auto = await layout('radial-bar', { items: [{ label: 'a', value: 5 }, { label: 'b', value: 10 }], width: 640, height: 640 });
    assert.deepEqual(auto.rings.map(r => r.fraction), [0.5, 1]);
    assert.deepEqual((await layout('radial-bar', { items: [], width: 640, height: 640 })).rings, []);
});

test('polar area gives equal angles and radius by the square root of the value', async () => {
    const items = [{ label: 'A', value: 100 }, { label: 'B', value: 25 }, { label: 'C', value: 0 }, { label: 'D', value: 50 }];
    const p = await layout('polar-area', { items, width: 640, height: 640 });
    assert.equal(p.wedges.length, 4);
    const r = w => Math.max(...w.path.match(/-?[\d.]+/g).map(Number).map(Math.abs));
    near(r(p.wedges[1]) / r(p.wedges[0]), 0.5, 0.02);
    assert.equal(p.wedges[0].anchor, 'start');
    assert.equal(p.wedges[3].anchor, 'end');
    for (const w of p.wedges) {
        inside(w.labelX, w.labelY, 640, 640);
    }
    assert.ok(p.rings.length > 0 && p.rings.every(g => g.radius <= p.radius + 1e-9));
    assert.deepEqual((await layout('polar-area', { items: [{ label: 'x', value: 0 }], width: 640, height: 640 })).wedges, []);
});

test('pareto sorts descending, accumulates to 100 % and finds the vital few', async () => {
    const items = [{ label: 'Timeout', value: 10 }, { label: 'Auth', value: 50 }, { label: 'DNS', value: 30 }, { label: 'Other', value: 10 }, { label: 'Zero', value: 0 }];
    const p = await layout('pareto', { items, width: W, height: H });
    assert.deepEqual(p.bars.map(b => b.label), ['Auth', 'DNS', 'Timeout', 'Other']);
    assert.deepEqual(p.bars.map(b => +b.cumulative.toFixed(6)), [0.5, 0.8, 0.9, 1]);
    assert.equal(p.vitalFew, 2);
    near(p.bars[1].cy, p.thresholdY);
    near(p.bars[3].cy, p.plot.top);
    assert.match(p.line, /^M[\d.]+,[\d.]+L/);
    assert.deepEqual(p.shareTicks.map(t => t.value), [0, 0.2, 0.4, 0.6, 0.8, 1]);
    assert.equal((await layout('pareto', { items: [], width: W, height: H })).bars.length, 0);
});

test('marimekko sizes columns by their total and stacks each to 100 %', async () => {
    const cells = [
        { column: 'EU', segment: 'Free', value: 30 }, { column: 'EU', segment: 'Pro', value: 10 },
        { column: 'US', segment: 'Free', value: 20 }, { column: 'US', segment: 'Pro', value: 40 },
        { column: 'US', segment: 'Free', value: 0 },
    ];
    const m = await layout('marimekko', { cells, width: W, height: H, gap: 0 });
    assert.deepEqual(m.columns.map(c => c.share), [0.4, 0.6]);
    assert.deepEqual(m.segments, ['Free', 'Pro']);
    near((m.columns[0].x1 - m.columns[0].x0) / (m.columns[1].x1 - m.columns[1].x0), 40 / 60);
    const us = m.cells.filter(c => c.column === 1);
    assert.deepEqual(us.map(c => c.share), [1 / 3, 2 / 3]);
    near(us[0].y + us[0].height, m.plot.bottom);
    near(us[1].y, m.plot.top);
    assert.equal((await layout('marimekko', { cells: [], width: W, height: H })).cells.length, 0);
});

test('waffle allocates exactly the rounded share of cells, never more than the grid', async () => {
    const thirds = await layout('waffle', { items: [{ label: 'A', value: 1 }, { label: 'B', value: 1 }, { label: 'C', value: 1 }], width: 640, height: 640 });
    assert.deepEqual(thirds.items.map(i => i.cells), [34, 33, 33]);
    assert.equal(thirds.cells.length, 100);
    assert.ok(thirds.cells.every(c => c.item >= 0));

    const seats = await layout('waffle', { items: [{ label: 'Active', value: 412 }, { label: 'Invited', value: 96 }], total: 600, width: 640, height: 640 });
    assert.deepEqual(seats.items.map(i => i.cells), [69, 16]);
    assert.equal(seats.cells.filter(c => c.item === -1).length, 15);
    assert.equal(seats.cells[0].item, 0);
    assert.equal(seats.cells[99].item, -1);
    for (const c of seats.cells) {
        inside(c.x, c.y, 640, 640);
        inside(c.x + seats.size, c.y + seats.size, 640, 640);
    }
    const grid = await layout('waffle', { items: [{ label: 'A', value: 1 }], rows: 5, columns: 20, width: 640, height: 200 });
    assert.equal(grid.cells.length, 100);
    assert.deepEqual((await layout('waffle', { items: [], width: 640, height: 640 })).cells, []);
});
