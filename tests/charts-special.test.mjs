// node --test tests/ — the specialised layouts (Smith chart, 3D columns, choropleth map) computed by the bundled D3 module.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';
import { gamma } from '../src/DaisyBlazor.Charts/wwwroot/layouts/smith.mjs';
import { prism } from '../src/DaisyBlazor.Charts/wwwroot/layouts/column3d.mjs';
import { shade } from '../src/DaisyBlazor.Charts/wwwroot/layouts/choropleth.mjs';

const W = 640, H = 360;
const near = (a, b, eps = 1e-6) => assert.ok(Math.abs(a - b) < eps, `${a} ≈ ${b}`);
const inside = (x, y, w = W, h = H) => assert.ok(x >= 0 && x <= w && y >= 0 && y <= h, `(${x}, ${y}) inside ${w}×${h}`);

test('smith places known impedances at their reflection coefficient', async () => {
    near(gamma(1, 0).re, 0);
    near(gamma(0, 0).re, -1);
    assert.deepEqual(gamma(Infinity, 0), { re: 1, im: 0 });
    // z = 1 + j1 → Γ = (1 + 2j) / 5
    near(gamma(1, 1).re, 0.2);
    near(gamma(1, 1).im, 0.4);

    const points = [{ r: 1, x: 0 }, { r: 1e9, x: 0 }, { r: 0, x: 0 }, { r: 1, x: 1 }, { r: -1, x: 0 }, { r: null, x: 0 }];
    const geo = await layout('smith', { series: [{ points }], width: 640, height: 640 });
    const [match, open, short, inductive] = geo.series[0].points;
    assert.equal(geo.series[0].points.length, 4, 'negative and missing resistances are left out');
    // Matched load in the centre, open circuit at the right edge, short circuit at the left.
    assert.deepEqual([match.x, match.y, match.magnitude], [geo.cx, geo.cy, 0]);
    near(open.x, geo.cx + geo.radius, 0.01);
    near(open.y, geo.cy);
    near(open.magnitude, 1, 1e-4);
    near(short.x, geo.cx - geo.radius, 0.01);
    assert.equal(short.angle, 180);
    near(inductive.x, geo.cx + 0.2 * geo.radius, 0.01);
    near(inductive.y, geo.cy - 0.4 * geo.radius, 0.01);
    near(inductive.magnitude, Math.sqrt(0.2), 1e-4);
    near(inductive.angle, Math.atan2(0.4, 0.2) * 180 / Math.PI, 0.01);
    assert.ok(geo.series[0].path.startsWith(`M${geo.cx},${geo.cy}L`));

    // The r = 1 circle passes through the centre and the open circuit.
    const one = geo.circles.find(c => c.value === 1);
    near(one.cx - one.r, geo.cx, 0.02);
    near(one.cx + one.r, geo.cx + geo.radius, 0.02);
    // Every reactance arc starts at the open circuit and ends on the rim; its label stays in the box.
    for (const a of geo.arcs) {
        const [, x1, y1, x2, y2] = a.path.match(/^M([\d.]+),([\d.]+)A.*,([\d.]+),([\d.]+)$/).map(Number);
        near(x1, geo.cx + geo.radius, 0.01);
        near(y1, geo.cy, 0.01);
        near(Math.hypot(x2 - geo.cx, y2 - geo.cy), geo.radius, 0.05);
    }
    assert.equal(geo.arcs.length, 10);
    for (const l of geo.labels) {
        inside(l.x, l.y, 640, 640);
    }
});

test('smith admittance mirrors the grid but not the points', async () => {
    const spec = { series: [{ points: [{ r: 1, x: 1 }] }], width: 640, height: 640 };
    const z = await layout('smith', spec);
    const y = await layout('smith', { ...spec, admittance: true });
    assert.deepEqual(y.series, z.series);
    near(y.circles[0].cx, 2 * z.cx - z.circles[0].cx, 0.02);
    assert.equal(y.labels[0].anchor, 'end');
    assert.deepEqual((await layout('smith', { series: [], width: 640, height: 640 })).series, []);
});

test('column3d builds prisms from the front rectangle and the depth offset', async () => {
    const p = prism(10, 20, 30, 40, 8, -5);
    assert.equal(p.front, '10,20 40,20 40,60 10,60');
    assert.equal(p.top, '10,20 40,20 48,15 18,15');
    assert.equal(p.side, '40,20 48,15 48,55 40,60');

    const geo = await layout('column3d', { categories: ['Mon', 'Tue', 'Wed'], series: [[10, 20, -5], [5, null, 8]], depth: 0.5, width: W, height: H });
    assert.equal(geo.columns.length, 5);
    // Drawn left to right, so a column's side face is covered by its right neighbour.
    const xs = geo.columns.map(c => c.x);
    assert.deepEqual(xs, [...xs].sort((a, b) => a - b));
    const zero = geo.yTicks.find(t => t.value === 0).pos;
    const neg = geo.columns.find(c => c.value === -5);
    near(neg.y, zero, 0.02);
    const tall = geo.columns.find(c => c.value === 20);
    near(tall.y, geo.plot.top, 0.02);
    // The top face rises to the right by the same offset on every column.
    const offset = c => c.top.split(' ').map(q => q.split(',').map(Number))[3];
    const [ox, oy] = offset(tall);
    assert.ok(ox > tall.x && oy < tall.y);
    for (const c of geo.columns) {
        for (const [x, y] of c.side.split(' ').map(q => q.split(',').map(Number))) {
            inside(x, y);
        }
    }
    assert.ok(geo.wall.length > 0 && geo.floor.length > 0);
    assert.equal(geo.grid.length, geo.yTicks.length);
});

test('column3d stacks values and turns sideways', async () => {
    const geo = await layout('column3d', { categories: ['A', 'B'], series: [[10, 20], [5, 7]], stacked: true, horizontal: true, width: W, height: H });
    const a = geo.columns.filter(c => c.c === 0).sort((p, q) => p.x - q.x);
    near(a[0].x + a[0].width, a[1].x, 0.02);
    assert.equal(a[1].value, 5);
    // Bottom category first, so the bar above covers the top face of the one below.
    assert.ok(geo.columns[0].y > geo.columns.at(-1).y);
    const t = geo.xTicks.find(x => x.value === 0);
    near(a[0].x, t.pos, 0.02);
    const empty = await layout('column3d', { categories: [], series: [], width: W, height: H });
    assert.deepEqual([empty.columns, empty.grid, empty.wall], [[], [], '']);
});

// Two unit squares side by side on the equator, and one far away to stretch the fit.
const square = (lon, lat, size = 10) => ({
    type: 'Polygon',
    coordinates: [[[lon, lat], [lon, lat + size], [lon + size, lat + size], [lon + size, lat], [lon, lat]]],
});

test('choropleth fits the projection to the box and maps values to colours', async () => {
    const features = [
        { id: 'a', name: 'Alpha', geometry: square(0, 0) },
        { id: 'b', name: 'Beta', geometry: square(10, 0) },
        { id: 'c', name: 'Gamma', geometry: square(20, 0) },
        { id: 'd', geometry: square(30, 0) },
    ];
    const values = { a: 10, b: 55, c: 100, zz: 3 };
    const geo = await layout('choropleth', { features, values, projection: 'mercator', color: 'var(--color-primary)', noDataColor: 'gray', width: W, height: H });
    assert.equal(geo.features.length, 4);
    assert.deepEqual([geo.min, geo.max], [10, 100]);
    assert.deepEqual(geo.features.map(f => f.fill), [shade('var(--color-primary)', 0), shade('var(--color-primary)', 0.5), shade('var(--color-primary)', 1), 'gray']);
    assert.equal(geo.features[0].fill, 'color-mix(in oklab, var(--color-primary) 25%, var(--color-base-100))');
    assert.equal(geo.features[2].fill, 'color-mix(in oklab, var(--color-primary) 100%, var(--color-base-100))');
    assert.deepEqual(geo.features.map(f => [f.name, f.value]), [['Alpha', 10], ['Beta', 55], ['Gamma', 100], ['d', null]]);
    assert.equal(geo.sphere, '', 'a region is not a world map');
    // fitExtent: the features touch the padded box on its long side and stay inside it.
    const nums = geo.features.flatMap(f => f.path.match(/-?[\d.]+/g).map(Number));
    const xs = nums.filter((_, i) => i % 2 === 0), ys = nums.filter((_, i) => i % 2 === 1);
    near(Math.min(...xs), 4, 0.02);
    near(Math.max(...xs), W - 4, 0.02);
    assert.ok(Math.min(...ys) >= 4 - 0.01 && Math.max(...ys) <= H - 4 + 0.01);
});

test('choropleth without features or values draws the globe and no scale', async () => {
    const geo = await layout('choropleth', { features: [], values: {}, projection: 'natural-earth', width: W, height: 330 });
    assert.ok(geo.sphere.startsWith('M'));
    assert.deepEqual([geo.min, geo.max, geo.features], [null, null, []]);
    const one = await layout('choropleth', { features: [{ id: 'a', geometry: square(0, 0) }], values: { a: 4 }, projection: 'equal-earth', width: W, height: H });
    assert.equal(one.features[0].fill, shade('var(--color-primary)', 1), 'a single value is the full colour');
    assert.equal(one.features[0].name, 'a');
    assert.equal(one.low, shade('var(--color-primary)', 0));
});
