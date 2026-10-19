// node --test tests/ — regressions found by the visual QA pass of the docs site: labels that left
// the chart box or ran into each other, bands that stopped short and sliver bands.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';
import { textWidth } from '../src/DaisyBlazor.Charts/wwwroot/layouts/_shared.mjs';

// The estimated horizontal extent of a label drawn at x with the given text-anchor.
const span = (x, text, anchor, size) => {
    const w = textWidth(text, size);
    return anchor === 'start' ? [x, x + w] : anchor === 'end' ? [x - w, x] : [x - w / 2, x + w / 2];
};
const within = ([a, b], width, what) => assert.ok(a >= 0 && b <= width, `${what} [${a.toFixed(1)}, ${b.toFixed(1)}] inside 0..${width}`);

test('gauge reserves the width of its formatted tick labels beside the arc', async () => {
    const text = ['99.0%', '99.2%', '99.4%', '99.6%', '99.8%', '100.0%'];
    for (const [width, height] of [[640, 640], [640, 512], [640, 400]]) {
        const g = await layout('gauge', { min: 99, max: 100, value: 99.95, startAngle: -120, endAngle: 120, tickText: text, width, height });
        for (const t of g.ticks) {
            within(span(t.x, text.at(-1), t.anchor, g.fontSize), width, `tick ${t.value} at ${width}×${height}`);
        }
    }
    const half = await layout('gauge', { min: 0, max: 100, value: 62, startAngle: -90, endAngle: 90, tickText: ['0%', '100%'], width: 640, height: 400 });
    for (const t of half.ticks) {
        within(span(t.x, '100%', t.anchor, half.fontSize), 640, `half gauge tick ${t.value}`);
    }
});

test('polar area reserves the width of each wedge\'s full label, value included', async () => {
    const names = ['Email', 'Push', 'In-app', 'SMS', 'Integrations', 'Webhooks', 'Notifications', 'Digest'];
    const items = names.map((label, i) => ({ label, value: 20 + i * 11, text: `${label} ${20 + i * 11}` }));
    for (const size of [640, 400]) {
        const p = await layout('polar-area', { items, width: size, height: size });
        for (const w of p.wedges) {
            within(span(w.labelX, items[w.index].text, w.anchor, p.fontSize), size, `"${items[w.index].text}" at ${size}`);
        }
    }
    const rose = await layout('polar-area', { labels: names, texts: names.map(n => `${n} 1,234`), series: [names.map(() => 5)], width: 400, height: 400 });
    for (const w of rose.wedges) {
        within(span(w.labelX, `${names[w.index]} 1,234`, w.anchor, rose.fontSize), 400, `wind rose "${names[w.index]}"`);
    }
});

test('polar leaves the rim ring unlabelled, so its value cannot run into the 0° spoke label', async () => {
    const hours = [0, 3, 6, 9, 12, 15, 18, 21].map(v => ({ value: v, label: `${String(v).padStart(2, '0')}h` }));
    const points = hours.map(h => ({ a: h.value, r: 100 + h.value * 9 }));
    for (const startAngle of [0, 90]) {
        const geo = await layout('polar', { series: [{ points }], width: 640, height: 640, angleMax: 24, angleTicks: hours, startAngle });
        assert.equal(geo.rings.at(-1).r, geo.radius, 'the outer ring is still drawn');
        assert.equal(geo.rings.at(-1).label, '', 'but not labelled');
        assert.ok(geo.rings.slice(0, -1).every(r => r.label !== ''), 'inner rings keep their labels');
    }
});

test('bullet ranges reach the end of the axis when it extends past the last threshold', async () => {
    const r = await layout('bullet', { rows: [{ label: 'Seats', value: 1180, target: 900, ranges: [500, 750, 1000] }], width: 640, height: 120 });
    const [row] = r.rows;
    const last = row.ranges.at(-1);
    assert.equal(row.ranges.length, 3);
    assert.ok(Math.abs(last.x + last.width - row.plot.right) < 0.02, `last band ends at ${last.x + last.width}, axis at ${row.plot.right}`);
    const v = await layout('bullet', { rows: [{ label: 'Seats', value: 1180, ranges: [500, 1000] }], width: 400, height: 300, vertical: true });
    assert.ok(Math.abs(v.rows[0].ranges.at(-1).y - v.rows[0].plot.top) < 0.02);
});

test('contour value bands are at least half a step wide at the ends', async () => {
    // Values 200..281: nice steps of 10 would leave a 280–281 sliver at the top.
    const points = [];
    for (let i = 0; i <= 9; i++) {
        points.push({ x: i, y: 0, value: 200 + i * 9 }, { x: i, y: 1, value: 200 + i * 9 });
    }
    const out = await layout('contour', { points, width: 640, height: 360, thresholds: 8, mode: 'value' });
    const edges = [...out.contours.map(c => c.value), out.max];
    const step = edges[2] - edges[1];
    assert.ok(out.max - out.contours.at(-1).value >= step / 2, `top band ${out.contours.at(-1).value}–${out.max} with step ${step}`);
    assert.ok(edges[1] - edges[0] >= step / 2, `bottom band ${edges[0]}–${edges[1]}`);
});

test('the top tick label has room above the plot in range bars and Marimekko', async () => {
    const bars = await layout('rangebar', { items: [{ label: 'a', low: 2, high: 18 }, { label: 'b', low: 5, high: 20 }], width: 640, height: 360, vertical: true });
    assert.ok(bars.plot.top >= 10, `rangebar plot top ${bars.plot.top}`);
    const mekko = await layout('marimekko', { cells: [{ column: 'A', segment: 'x', value: 3 }, { column: 'A', segment: 'y', value: 1 }], width: 640, height: 360 });
    assert.ok(mekko.plot.top >= 10, `marimekko plot top ${mekko.plot.top}`);
});

test('label widths are estimated on the wide side for wide capitals', () => {
    // "W" is close to a full em in the UI fonts; 0.6 em per glyph left rotated "W2" labels short.
    assert.ok(textWidth('W2') >= 11 * 1.5, `W2 ≈ ${textWidth('W2')}`);
    assert.ok(textWidth('MW') > textWidth('ab'));
    assert.equal(textWidth('0123'), 4 * 11 * 0.6, 'digits keep the old estimate');
});
