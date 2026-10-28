// node --test tests/ — RowHeight: each row chart's .NET RowBands (count, padding, chrome) must give
// a layout whose rows are exactly RowHeight apart. The heights below are the formula the components
// use (RowHeight × (count + padding) + chrome, see ChartRows in D3Chart.cs) with their constants; a
// layout that changes its frame breaks the matching case here, pointing at the component to update.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';

const W = 900;
const ROW = 24;
const LABEL_ROW = 11 * 1.2 + 6;
const heightFor = (count, padding, chrome) => Math.round((ROW * (count + padding) + chrome) * 100) / 100;

// The distinct row positions, top to bottom, and the gaps between them.
function steps(positions) {
    const ys = [...new Set(positions.map(p => Math.round(p * 100) / 100))].sort((a, b) => a - b);
    return ys.slice(1).map((y, i) => y - ys[i]);
}

function assertSteps(positions, label, tolerance = 0.02) {
    const gaps = steps(positions);
    assert.ok(gaps.length > 0, `${label}: more than one row`);
    for (const gap of gaps) {
        assert.ok(Math.abs(gap - ROW) <= tolerance, `${label}: rows ${gap} apart, not ${ROW}`);
    }
}

const groups = [{ name: 'A', values: [1, 2, 3, 4] }, { name: 'B', values: [2, 3, 5] }, { name: 'C', values: [1, 4] }, { name: 'D', values: [] }];

test('row height: category-frame charts (rangebar, lollipop, dumbbell) step their rows by RowHeight', async () => {
    const items = ['A', 'B', 'C', 'D', 'E'].map((label, i) => ({ label, low: i, high: i + 2 }));
    const range = await layout('rangebar', { items, width: W, height: heightFor(5, 0.3, 26) });
    assertSteps(range.yTicks.map(t => t.pos), 'rangebar');

    const points = ['A', 'B', 'C', 'D'].map((label, i) => ({ label, value: i + 1, text: String(i + 1) })).concat([{ label: 'X', value: null }]);
    const lollipop = await layout('lollipop', { items: points, width: W, height: heightFor(4, 0.2, 26) });
    assertSteps(lollipop.yTicks.map(t => t.pos), 'lollipop');

    const pairs = ['A', 'B', 'C'].map((label, i) => ({ label, start: i, end: i + 3 }));
    const dumbbell = await layout('dumbbell', { items: pairs, width: W, height: heightFor(3, 0.2, 26) });
    assertSteps(dumbbell.yTicks.map(t => t.pos), 'dumbbell');
});

test('row height: bullet rows keep their tick line, or share one axis line', async () => {
    const rows = ['A', 'B', 'C'].map((label, i) => ({ label, value: i + 1, ranges: [1, 2, 3 + i] }));
    const own = await layout('bullet', { rows, width: W, height: heightFor(3, 0, 4), sharedAxis: false });
    assertSteps(own.rows.map(r => r.plot.top), 'bullet');
    const shared = await layout('bullet', { rows, width: W, height: heightFor(3, 0, 20), sharedAxis: true });
    assertSteps(shared.rows.map(r => r.plot.top), 'bullet, shared axis');
});

test('row height: heatmap rows, from the given row names or the cells', async () => {
    const cells = ['Mon', 'Tue', 'Wed', 'Thu'].flatMap((row, r) => ['00', '06', '12'].map((column, c) => ({ row, column, value: r + c })));
    const r = await layout('heatmap', { cells, width: W, height: heightFor(4, 0.06, 22) });
    assertSteps(r.rows.map(t => t.pos), 'heatmap');
    const named = await layout('heatmap', { cells, rows: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'], width: W, height: heightFor(5, 0.06, 22) });
    assertSteps(named.rows.map(t => t.pos), 'heatmap, named rows');
});

test('row height: gantt lanes and group headers step by RowHeight', async () => {
    const tasks = [
        { name: 'Prep', start: '2026-10-28T06:00:00Z', end: '2026-10-28T07:00:00Z' },
        { name: 'Stand 1', row: 'S1', start: '2026-10-28T06:30:00Z', end: '2026-10-28T08:00:00Z', group: 'Apron' },
        { name: 'Stand 1 bis', row: 'S1', start: '2026-10-28T09:00:00Z', end: '2026-10-28T10:00:00Z', group: 'Apron' },
        { name: 'Stand 2', row: 'S2', start: '2026-10-28T07:00:00Z', end: '2026-10-28T09:00:00Z', group: 'Apron' },
        { name: 'Gate 4', start: '2026-10-28T08:00:00Z', end: '2026-10-28T11:00:00Z', group: 'Gates' },
    ];
    // Lanes: Prep, S1, S2, Gate 4, plus the Apron and Gates headers = 6 rows (as GanttChart counts them).
    const r = await layout('gantt', { tasks, width: W, height: heightFor(6, 2 * 0.15 - 0.3, 42) });
    assertSteps(r.rows.map(row => row.y), 'gantt');
});

test('row height: gantt from/to pin the window, clip bars to it and leave out tasks outside it', async () => {
    const tasks = [
        { name: 'Before', start: '2026-10-28T01:00:00Z', end: '2026-10-28T02:00:00Z' },
        { name: 'Across', start: '2026-10-28T05:00:00Z', end: '2026-10-28T07:00:00Z' },
        { name: 'Inside', start: '2026-10-28T08:00:00Z', end: '2026-10-28T09:00:00Z' },
        { name: 'After', start: '2026-10-28T20:00:00Z', end: '2026-10-28T21:00:00Z' },
    ];
    const r = await layout('gantt', { tasks, width: W, height: 300, from: '2026-10-28T06:00:00Z', to: '2026-10-28T18:00:00Z' });
    assert.deepEqual(r.rows.map(row => row.name), ['Across', 'Inside']);
    assert.equal(r.rows[0].x0, r.plot.left, 'Across is clipped at the window start');
    assert.ok(r.xTicks.every(t => t.value >= Date.parse('2026-10-28T06:00:00Z') && t.value <= Date.parse('2026-10-28T18:00:00Z')), 'ticks stay inside the window');

    const open = await layout('gantt', { tasks, width: W, height: 300 });
    assert.equal(open.rows.length, 4, 'without a window every task is drawn');
});

test('row height: distribution charts step their groups by RowHeight, with or without a value title', async () => {
    const shown = groups.filter(g => g.values.length > 0).length;
    for (const [kind, padding, extra] of [['beeswarm', 0.08, {}], ['violin', 0.15, { horizontal: true }], ['boxplot', 0.3, { horizontal: true }]]) {
        const plain = await layout(kind, { groups, ...extra, width: W, height: heightFor(shown, padding, 34) });
        assertSteps(plain.yTicks.map(t => t.pos), kind);
        const titled = await layout(kind, { groups, ...extra, valueTitle: 'Minutes', width: W, height: heightFor(shown, padding, 52) });
        assertSteps(titled.yTicks.map(t => t.pos), `${kind}, titled`);
    }

    const ridge = await layout('ridgeline', { groups, overlap: 1.6, width: W, height: heightFor(shown, 1.6 - 1, 34) });
    assertSteps(ridge.groups.map(g => g.baseline), 'ridgeline');
});

test('row height: horizontal waterfall and bar step by RowHeight; column3d comes close', async () => {
    const steps5 = [
        { label: 'Open', value: 100, kind: 'start' },
        { label: 'Sales', value: 40, kind: 'delta' },
        { label: 'Costs', value: -25, kind: 'delta' },
        { label: 'Skipped', value: null, kind: 'delta' },
        { label: 'Q1', kind: 'subtotal' },
        { label: 'Close', kind: 'end' },
    ];
    const fall = await layout('waterfall', { steps: steps5, horizontal: true, width: W, height: heightFor(5, 2 * 0.15 - 0.3, 30) });
    assertSteps(fall.bars.map(b => b.y), 'waterfall');

    const categories = ['A', 'B', 'C', 'D', 'E', 'F'];
    const bar = await layout('bar', { categories, series: [[1, 2, 3, 4, 5, 6]], horizontal: true, width: W, height: heightFor(6, 0, 8 + LABEL_ROW) });
    assertSteps(bar.yTicks.map(t => t.pos), 'bar');

    const cube = await layout('column3d', { categories, series: [[1, 2, 3, 4, 5, 6]], horizontal: true, width: W, height: heightFor(6, 2 * 0.2 - 0.3, 8 + LABEL_ROW) });
    for (const gap of steps(cube.yTicks.map(t => t.pos))) {
        assert.ok(gap <= ROW && gap > ROW * 0.8, `column3d: rows ${gap} apart, near ${ROW}`);
    }
});
