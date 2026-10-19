// node --test tests/ — the range selector's overview, computed by the bundled D3 module.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';

const W = 600, H = 80;

test('range-selector: the overview spans the plot from the first to the last point, sorted, read as UTC', async () => {
    const points = [
        { time: '2026-10-03T00:00:00', value: 30 },
        { time: '2026-10-01T00:00:00', value: 10 },
        { time: '2026-10-02T00:00:00', value: null },
        { time: '2026-10-05T00:00:00', value: 20 },
    ];
    const out = await layout('range-selector', { points, width: W, height: H, smooth: false });
    assert.deepEqual(out.plot, { left: 8, top: 4, right: W - 8, bottom: H - 22 });
    assert.equal(out.width, W);
    assert.equal(out.height, H);
    // The first point is at the plot's left, the last at its right, and Oct 3 halfway (2 of 4 days).
    const xs = [...out.line.matchAll(/[ML]([\d.]+),/g)].map(m => Number(m[1]));
    assert.deepEqual(xs, [8, 300, 592]);
    // Values grow from zero: the biggest reaches the top, the area closes on the bottom.
    const ys = [...out.line.matchAll(/,([\d.]+)/g)].map(m => Number(m[1]));
    assert.ok(ys[1] < ys[2] && ys[2] < ys[0]);
    assert.match(out.area, /58Z$/);
    assert.ok(out.xTicks.length >= 2 && out.xTicks.every(t => t.pos >= 8 && t.pos <= 592));
    assert.equal(out.xTicks[0].label, 'Oct'); // the first of a month is labelled by the month
});

test('range-selector: one point gets a day of domain; nothing valid is an empty overview', async () => {
    const one = await layout('range-selector', { points: [{ time: '2026-10-01T09:00:00', value: 5 }], width: W, height: H });
    assert.match(one.line, /^M8,/);
    const empty = await layout('range-selector', { points: [{ time: 'x', value: 1 }], width: W, height: H });
    assert.equal(empty.line, '');
    assert.equal(empty.area, '');
    assert.deepEqual(empty.xTicks, []);
    assert.equal(empty.plot.right, W - 8);
});
