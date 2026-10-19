// node --test tests/ — the combo (bars and lines) layout computed by the bundled D3 module.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';

const size = { width: 640, height: 360 };
const labels = ['W1', 'W2', 'W3', 'W4'];

// The value at a y position, read off two ticks of the same scale.
const valueAt = (ticks, pos) => {
    const [a, b] = ticks;
    return a.value + (pos - a.pos) * (b.value - a.value) / (b.pos - a.pos);
};

test('combo: grouped bars side by side, a line through the category centres', async () => {
    const out = await layout('combo', {
        labels, ...size,
        bars: [{ name: 'Deploys', data: [12, 18, 9, 22] }, { name: 'Rollbacks', data: [1, 3, null, 2] }],
        lines: [{ name: 'Lead time', data: [10, 14, 8, 20] }],
    });
    assert.equal(out.bars.length, 7, 'the missing value is skipped');
    assert.deepEqual(out.y2Ticks, []);
    const w1 = out.bars.filter(b => b.index === 0);
    assert.ok(w1[0].x + w1[0].width <= w1[1].x, 'side by side');
    assert.ok(Math.abs(w1[0].y + w1[0].height - out.plot.bottom) < 0.01, 'grows from zero at the bottom');
    assert.ok(Math.abs(valueAt(out.yTicks, w1[0].y) - 12) < 0.05);
    assert.equal(out.yTicks[0].value, 0);

    const [line] = out.lines;
    assert.equal(line.points.length, 4);
    assert.deepEqual(line.points.map(p => p.cx), out.xTicks.map(t => t.pos));
    assert.ok(Math.abs(valueAt(out.yTicks, line.points[3].cy) - 20) < 0.05, 'on the bars\' scale');
    assert.match(line.path, /^M[\d.]+,[\d.]+(L[\d.]+,[\d.]+){3}$/);
    for (const t of out.xTicks) {
        assert.ok(t.pos > out.plot.left && t.pos < out.plot.right);
    }
});

test('combo: stacked bars add up, negatives stack below zero', async () => {
    const out = await layout('combo', {
        labels: ['Mon', 'Tue'], ...size, stackBars: true,
        bars: [{ name: 'API', data: [5, 4] }, { name: 'Web', data: [3, -2] }, { name: 'Jobs', data: [2, -1] }],
    });
    const mon = out.bars.filter(b => b.index === 0);
    assert.ok(mon.every(b => b.x === mon[0].x && b.width === mon[0].width), 'one bar per category');
    assert.ok(Math.abs(mon[1].y + mon[1].height - mon[0].y) < 0.01, 'Web sits on API');
    assert.ok(Math.abs(valueAt(out.yTicks, mon[2].y) - 10) < 0.05, 'the stack reaches 5 + 3 + 2');
    const tue = out.bars.filter(b => b.index === 1);
    const zero = out.yTicks.find(t => t.value === 0).pos;
    assert.ok(Math.abs(tue[1].y - zero) < 0.01, 'Web hangs from zero');
    assert.ok(Math.abs(tue[2].y - (tue[1].y + tue[1].height)) < 0.01, 'Jobs hangs below Web');
    assert.ok(Math.abs(valueAt(out.yTicks, tue[2].y + tue[2].height) + 3) < 0.05);
});

test('combo: a secondary axis gives the lines their own nice ticks on the right', async () => {
    const out = await layout('combo', {
        labels, ...size, secondaryAxis: true, smooth: true,
        bars: [{ name: 'Deploys', data: [120, 180, 90, 220] }],
        lines: [{ name: 'Failure rate', data: [2.5, 4.1, 3.2, null] }, { name: 'Rollback rate', data: [1, null, 2, 1.5] }],
    });
    assert.deepEqual(out.y2Ticks.map(t => t.value), [0, 1, 2, 3, 4, 5]);
    assert.ok(out.yTicks.at(-1).value >= 220 && out.yTicks.at(-1).value <= 250);
    assert.ok(out.plot.right < size.width - 20, 'room for the right labels');
    const [line, other] = out.lines;
    assert.equal(line.points.length, 3, 'the missing value is skipped');
    assert.ok(Math.abs(valueAt(out.y2Ticks, line.points[1].cy) - 4.1) < 0.01);
    assert.ok(line.path.includes('C'), 'smooth curve');
    assert.equal(other.path.match(/M/g).length, 2, 'broken at the gap');
});

test('combo: crowded labels thin out, empty input gives no plot', async () => {
    const many = Array.from({ length: 52 }, (_, i) => `Week ${i + 1}`);
    const out = await layout('combo', { labels: many, ...size, bars: [{ name: 'Deploys', data: many.map((_, i) => i) }] });
    const shown = out.xTicks.filter(t => t.label !== '');
    assert.ok(shown.length < many.length && shown.length > 3);
    assert.ok(out.xTicks.every(t => t.full.startsWith('Week')));
    const empty = await layout('combo', { ...size });
    assert.equal(empty.plot, null);
    assert.deepEqual(empty.bars, []);
});
