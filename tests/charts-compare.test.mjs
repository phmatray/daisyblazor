// node --test tests/ — the "compare" chart layouts computed by the bundled D3 module.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';

const W = 640;
const H = 360;

// Every coordinate of a layout result, found by walking it.
function coords(value, out = []) {
    if (Array.isArray(value)) {
        value.forEach(v => coords(v, out));
    } else if (value && typeof value === 'object') {
        for (const [k, v] of Object.entries(value)) {
            if (/^(x|y|x1|x2|y1|y2|labelX|labelY|cx|cy)$/.test(k) && typeof v === 'number') {
                out.push([k, v]);
            } else {
                coords(v, out);
            }
        }
    }
    return out;
}

function assertInside(result, width = W, height = H) {
    for (const [k, v] of coords(result)) {
        assert.ok(Number.isFinite(v), `${k} is finite`);
        const limit = k.startsWith('x') || k === 'labelX' || k === 'cx' ? width : height;
        assert.ok(v >= 0 && v <= limit, `${k}=${v} inside 0..${limit}`);
    }
    assert.ok(!JSON.stringify(result).includes('NaN'));
}

// Labels as boxes, using the same width estimate the layouts use, must not overlap.
function box(l, size = 12) {
    const w = l.text.length * size * 0.6;
    const x = l.anchor === 'end' ? l.x - w : l.anchor === 'middle' ? l.x - w / 2 : l.x;
    return { x0: x, x1: x + w, y0: l.y - size * 0.8, y1: l.y };
}
// A category label down the left of a horizontal frame, placed as the axes component draws it.
const category = (r, i) => ({ text: r.yTicks[i].label, x: r.yAxisAt - 6, y: r.yTicks[i].pos + 4, anchor: 'end' });
const overlap = (a, b) => a.x0 < b.x1 && b.x0 < a.x1 && a.y0 < b.y1 && b.y0 < a.y1;

test('radar: one spoke per axis from the top, rings on nice ticks, names inside the box', async () => {
    const r = await layout('radar', {
        axes: ['Seats', 'Storage', 'API calls', 'Support', 'Uptime'],
        series: [[80, 60, 90, 40, 70], [50, 85, 40, 75, 95]],
        width: W, height: H, levels: 4, grid: 'polygon',
    });
    assert.equal(r.axes.length, 5);
    assert.equal(r.axes[0].x, r.cx);
    assert.ok(r.axes[0].y < r.cy);
    assert.equal(r.axes[0].anchor, 'middle');
    assert.equal(r.axes[1].anchor, 'start');
    assert.equal(r.axes[4].anchor, 'end');
    assert.deepEqual(r.rings.map(g => g.value), [20, 40, 60, 80, 100]);
    assert.equal(r.rings.at(-1).r, r.radius);
    assert.match(r.rings[0].path, /^M[\d.]+,[\d.]+L.*Z$/);
    assert.equal(r.series.length, 2);
    assert.equal(r.series[0].points.length, 5);
    assert.equal(r.series[0].points[0].y, Math.round((r.cy - r.radius * 0.8) * 100) / 100);
    assertInside(r);
    for (const a of r.axes) {
        const b = box({ text: a.name, x: a.labelX, y: a.labelY, anchor: a.anchor });
        assert.ok(b.x0 >= 0 && b.x1 <= W, `${a.name} fits across`);
    }
});

test('radar: circular grid, fixed max clamps, missing values sit at the centre', async () => {
    const r = await layout('radar', { axes: ['A', 'B', 'C'], series: [[5, null, 20]], width: 400, height: 400, max: 10, levels: 2, grid: 'circle' });
    assert.deepEqual(r.rings.map(g => g.value), [5, 10]);
    assert.match(r.rings[0].path, /A/);
    assert.equal(r.series[0].points[1].value, null);
    assert.deepEqual([r.series[0].points[1].x, r.series[0].points[1].y], [r.cx, r.cy]);
    const far = r.series[0].points[2];
    assert.ok(Math.hypot(far.x - r.cx, far.y - r.cy) <= r.radius + 0.01);
});

test('radar: fewer than three axes or no series gives an empty layout', async () => {
    const empty = { cx: 0, cy: 0, radius: 0, axes: [], rings: [], series: [] };
    assert.deepEqual(await layout('radar', { axes: ['A', 'B'], series: [[1, 2]], width: W, height: H }), empty);
    assert.deepEqual(await layout('radar', { axes: ['A', 'B', 'C'], series: [], width: W, height: H }), empty);
});

test('bullet: each row has its own scale, ranges as bands, a bar and a target', async () => {
    const r = await layout('bullet', {
        rows: [
            { label: 'Active seats', value: 820, target: 900, ranges: [500, 750, 1000] },
            { label: 'Deployments', value: 42, target: 40, ranges: [20, 35, 50] },
        ],
        width: W, height: 200,
    });
    assert.equal(r.rows.length, 2);
    const [seats, deploys] = r.rows;
    assert.deepEqual(seats.ranges.map(b => b.level), [0, 1, 2]);
    assert.equal(seats.ranges[0].x, seats.bar.x);
    assert.equal(seats.xTicks.at(-1).label, '1,000');
    assert.equal(deploys.xTicks.at(-1).label, '50');
    assert.ok(seats.marker.x1 > seats.bar.x + seats.bar.width, 'target past the value');
    assert.ok(deploys.marker.x1 < deploys.bar.x + deploys.bar.width, 'value past the target');
    assert.ok(seats.ranges[0].y + seats.ranges[0].height < deploys.ranges[0].y, 'rows stack');
    assert.deepEqual(seats.yTicks.map(t => t.label), ['Active seats']);
    assert.ok(seats.yTicks[0].pos > seats.plot.top && seats.yTicks[0].pos < seats.plot.bottom);
    assertInside(r, W, 200);
});

test('bullet: vertical columns; no ranges gives one band; rows without a value are skipped', async () => {
    const r = await layout('bullet', {
        rows: [{ label: 'Seats', value: 3, target: null }, { label: 'Bad', value: null }, { label: 'Deploys', value: 7, ranges: [5, 10] }],
        width: 400, height: 300, vertical: true,
    });
    assert.equal(r.rows.length, 2);
    assert.equal(r.rows[0].marker, null);
    assert.equal(r.rows[0].ranges.length, 1);
    assert.ok(r.rows[0].bar.height > 0 && r.rows[0].bar.width > 0);
    assert.ok(r.rows[0].xTicks[0].pos < r.rows[1].xTicks[0].pos);
    assert.equal(r.rows[1].yTicks[0].pos, r.rows[1].plot.bottom);
    assertInside(r, 400, 300);
    assert.deepEqual(await layout('bullet', { rows: [], width: W, height: H }), { rows: [] });
});

test('dumbbell: two dots per row, value texts outside the pair', async () => {
    const r = await layout('dumbbell', {
        items: [
            { label: 'Europe', start: 240, end: 180, startText: '240 ms', endText: '180 ms' },
            { label: 'Asia', start: 310, end: 330, startText: '310 ms', endText: '330 ms' },
            { label: 'Skipped', start: null, end: 3 },
        ],
        width: W, height: 200, showValues: true,
    });
    assert.equal(r.items.length, 2);
    const [eu, asia] = r.items;
    assert.ok(eu.x2 < eu.x1, 'improvement moves left');
    assert.equal(eu.startText.anchor, 'start');
    assert.equal(eu.endText.anchor, 'end');
    assert.equal(asia.startText.anchor, 'end');
    assert.ok(r.yAxisAt < r.plot.left);
    for (const it of r.items) {
        assert.ok(box(it.startText, 11).x0 >= box(category(r, 0), 11).x1 - 0.01 || it.startText.anchor === 'start');
        assert.ok(box(it.endText, 11).x1 <= W && box(it.startText, 11).x1 <= W);
    }
    assertInside(r, W, 200);
    assert.deepEqual((await layout('dumbbell', { items: [], width: W, height: H })).items, []);
});

test('slope: lines between two columns, labels pushed apart, headers on top', async () => {
    const r = await layout('slope', {
        items: [
            { label: 'Support', start: 30, end: 52, startText: '30', endText: '52' },
            { label: 'Billing', start: 31, end: 52.5, startText: '31', endText: '52.5' },
            { label: 'Platform', start: 30.5, end: 20, startText: '30.5', endText: '20' },
        ],
        width: W, height: H, startLabel: 'Q1', endLabel: 'Q4', showValues: true,
    });
    assert.deepEqual(r.headers.map(h => h.text), ['Q1', 'Q4']);
    assert.equal(r.items[0].left.text, 'Support 30');
    assert.equal(r.items[0].right.text, '52 Support');
    for (const side of ['left', 'right']) {
        const labels = r.items.map(i => i[side]);
        for (let a = 0; a < labels.length; a++) {
            for (let b = a + 1; b < labels.length; b++) {
                assert.ok(Math.abs(labels[a].y - labels[b].y) >= 15 - 0.01, `${side} labels apart`);
            }
        }
        for (const l of labels) {
            const b = box(l);
            assert.ok(b.x0 >= 0 && b.x1 <= W, `${l.text} fits`);
        }
    }
    assert.ok(r.items[0].y2 < r.items[0].y1, 'growth goes up');
    assertInside(r);
    assert.deepEqual((await layout('slope', { items: [{ label: 'x', start: NaN, end: 1 }], width: W, height: H })).items, []);
});

test('lollipop: sorted descending by default, stems from zero, horizontal or vertical', async () => {
    const items = [{ label: 'Support', value: 42, text: '42' }, { label: 'Platform', value: 61, text: '61' }, { label: 'Billing', value: -12, text: '-12' }];
    const r = await layout('lollipop', { items, width: W, height: H });
    assert.deepEqual(r.items.map(i => i.label), ['Platform', 'Support', 'Billing']);
    assert.deepEqual(r.items.map(i => i.index), [1, 0, 2]);
    const zero = r.items[0].x1;
    assert.ok(r.items.every(i => i.x1 === zero));
    assert.ok(r.items[2].x2 < zero);
    assert.equal(r.items[2].text.anchor, 'end');
    assert.ok(r.xTicks.some(t => t.label === '0' && t.pos === zero));
    assertInside(r);

    const v = await layout('lollipop', { items, width: W, height: H, vertical: true, sort: 'none' });
    assert.deepEqual(v.items.map(i => i.label), ['Support', 'Platform', 'Billing']);
    assert.ok(v.items[0].y2 < v.items[0].y1, 'positive goes up');
    assert.equal(v.labelAngle, 0);
    assertInside(v);
});

test('lollipop: long labels in a narrow vertical chart turn and keep room', async () => {
    const items = Array.from({ length: 12 }, (_, i) => ({ label: `Team number ${i + 1}`, value: i + 1, text: String(i + 1) }));
    const v = await layout('lollipop', { items, width: 400, height: 300, vertical: true });
    assert.equal(v.labelAngle, -40);
    assert.ok(v.plot.bottom < 300 - 50);
    assert.deepEqual((await layout('lollipop', { items: [{ label: 'a', value: null }], width: W, height: H })).items, []);
});

test('parallel: one axis per dimension with its own ticks, a polyline per item', async () => {
    const r = await layout('parallel', {
        dimensions: ['Seats', 'Storage (GB)', 'API calls / min', 'Support hours'],
        items: [
            { label: 'Starter', values: [5, 10, 60, 8] },
            { label: 'Team', values: [25, 100, 300, 24] },
            { label: 'Scale', values: [200, 1000, null, 168] },
        ],
        width: W, height: H,
    });
    assert.equal(r.axes.length, 4);
    assert.ok(r.axes[0].x < r.axes[1].x);
    assert.notDeepEqual(r.axes[0].ticks.map(t => t.label), r.axes[1].ticks.map(t => t.label));
    assert.match(r.lines[0].path, /^M[\d.]+,[\d.]+(L[\d.]+,[\d.]+){3}$/);
    assert.equal((r.lines[2].path.match(/M/g) ?? []).length, 2, 'a missing value breaks the line');
    for (const a of r.axes) {
        const b = box(a.label);
        assert.ok(b.x0 >= 0 && b.x1 <= W, `${a.name} fits`);
    }
    assertInside(r);
    assert.deepEqual(await layout('parallel', { dimensions: ['A'], items: [{ label: 'x', values: [1] }], width: W, height: H }), { axes: [], lines: [] });
});

test('rangebar: bars from low to high, grouped side by side, reversed ends swapped', async () => {
    const r = await layout('rangebar', {
        items: [
            { label: 'Starter', low: 1, high: 5, group: 'Seats' },
            { label: 'Starter', low: 2, high: 10, group: 'Projects' },
            { label: 'Team', low: 25, high: 5, group: 'Seats' },
            { label: 'Team', low: 10, high: 50, group: 'Projects' },
        ],
        width: W, height: H, showValues: true,
    });
    assert.deepEqual(r.groups, ['Seats', 'Projects']);
    assert.deepEqual(r.yTicks.map(c => c.label), ['Starter', 'Team']);
    const [s1, s2, t1] = r.bars;
    assert.equal(t1.low, 5);
    assert.equal(t1.high, 25);
    assert.ok(s2.y > s1.y && s2.y < t1.y, 'groups side by side within a category');
    assert.equal(s1.height, s2.height);
    assert.ok(t1.width > s1.width);
    assert.equal(s1.lowText.anchor, 'end');
    assertInside(r);

    const v = await layout('rangebar', { items: [{ label: 'A', low: 1, high: 3 }], width: W, height: H, vertical: true });
    assert.deepEqual(v.groups, ['']);
    assert.ok(v.bars[0].height > 0);
    assert.deepEqual((await layout('rangebar', { items: [], width: W, height: H })).bars, []);
});
