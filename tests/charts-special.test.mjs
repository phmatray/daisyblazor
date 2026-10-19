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

const hours = [0, 3, 6, 9, 12, 15, 18, 21].map(v => ({ value: v, label: `${v}:00` }));

test('polar places angle 0 at 12 o\'clock and turns clockwise over angleMax', async () => {
    const points = [{ a: 0, r: 40 }, { a: 6, r: 40 }, { a: 12, r: 40 }, { a: 18, r: 40 }, { a: null, r: 3 }, { a: 3, r: null }];
    const geo = await layout('polar', { series: [{ points }], width: 640, height: 640, angleMax: 24, angleTicks: hours, levels: 4 });
    const [top, right, bottom, left] = geo.series[0].points;
    assert.equal(geo.series[0].points.length, 4, 'points without a finite angle and radius are skipped');
    near(top.x, geo.cx, 0.01);
    assert.ok(top.y < geo.cy);
    assert.ok(right.x > geo.cx);
    near(right.y, geo.cy, 0.01);
    assert.ok(bottom.y > geo.cy);
    assert.ok(left.x < geo.cx);
    // The radius scale is nice from zero: the outer ring is at the top tick, which is 40 here.
    assert.equal(geo.rings.at(-1).r, geo.radius);
    near(geo.cy - top.y, geo.radius * 40 / geo.rings.at(-1).value, 0.02);
    assert.equal(geo.spokes.length, 8);
    assert.deepEqual(geo.spokes.map(s => s.anchor).slice(0, 3), ['middle', 'start', 'start']);
    for (const s of geo.spokes) {
        inside(s.labelX, s.labelY, 640, 640);
    }
    assert.match(geo.series[0].path, /^M[^Z]*$/, 'a line stays open');
});

test('polar start angle, direction, closed lines, areas, splines and markers', async () => {
    const points = [{ a: 0, r: 1 }, { a: 90, r: 2 }, { a: 180, r: 1 }, { a: 270, r: 2 }];
    const base = { series: [{ points }], width: W, height: H };
    const ccw = await layout('polar', { ...base, clockwise: false, startAngle: 90 });
    // Angle 0 at 3 o'clock, angle 90 at 12 o'clock when turning anticlockwise.
    near(ccw.series[0].points[0].y, ccw.cy, 0.01);
    assert.ok(ccw.series[0].points[0].x > ccw.cx);
    near(ccw.series[0].points[1].x, ccw.cx, 0.01);
    assert.ok(ccw.series[0].points[1].y < ccw.cy);

    assert.match((await layout('polar', { ...base, closed: true })).series[0].path, /Z$/);
    assert.match((await layout('polar', { ...base, mode: 'area' })).series[0].path, /Z$/);
    assert.match((await layout('polar', { ...base, mode: 'area', smooth: true })).series[0].path, /C/);
    assert.match((await layout('polar', { ...base, smooth: true })).series[0].path, /^M[^Z]*C[^Z]*$/);
    const markers = await layout('polar', { ...base, mode: 'marker' });
    assert.equal(markers.series[0].path, '');
    assert.equal(markers.series[0].points.length, 4);
    assert.deepEqual(await layout('polar', { series: [], width: 0, height: 0 }), { cx: 0, cy: 0, radius: 0, rings: [], spokes: [], series: [] });
});

test('polar stacked puts each series on the running total at its angle', async () => {
    const a = [{ a: 0, r: 10 }, { a: 90, r: 20 }, { a: 180, r: 30 }];
    const b = [{ a: 0, r: 30 }, { a: 90, r: 20 }, { a: 270, r: 5 }];
    const base = { series: [{ points: a }, { points: b }], width: 640, height: 640, levels: 4 };
    const dist = (geo, p) => Math.hypot(p.x - geo.cx, p.y - geo.cy);
    const top = geo => geo.rings.at(-1);
    const unit = geo => geo.radius / top(geo).value;

    const geo = await layout('polar', { ...base, stacked: true });
    // 10 + 30 = 40 and 20 + 20 = 40 at the shared angles; 270 has nothing below it.
    near(dist(geo, geo.series[1].points[0]), 40 * unit(geo), 0.02);
    near(dist(geo, geo.series[1].points[1]), 40 * unit(geo), 0.02);
    near(dist(geo, geo.series[1].points[2]), 5 * unit(geo), 0.02);
    near(dist(geo, geo.series[0].points[2]), 30 * unit(geo), 0.02);
    assert.match(geo.series[1].path, /^M[^Z]*$/, 'a stacked line stays open');

    const area = await layout('polar', { ...base, stacked: true, mode: 'area' });
    assert.equal(area.series[0].path.match(/M/g).length, 1, 'the first band starts at the centre');
    assert.equal(area.series[1].path.match(/M/g).length, 2, 'later bands cut out the total below them');

    const pct = await layout('polar', { ...base, percent: true, mode: 'area', smooth: true });
    assert.equal(top(pct).value, 100);
    // Every angle's top series reaches 100%; at 0 the first series is 10 / 40 = 25%.
    for (const p of pct.series[1].points) {
        near(dist(pct, p), pct.radius, 0.02);
    }
    near(dist(pct, pct.series[0].points[0]), pct.radius * 0.25, 0.02);
    near(dist(pct, pct.series[0].points[2]), pct.radius, 0.02);

    // Off by default: the same series overlap instead.
    const flat = await layout('polar', base);
    near(dist(flat, flat.series[1].points[0]), 30 * unit(flat), 0.02);
});

test('shape draws closed polygons with holes and open polylines on shared axes', async () => {
    const square = [{ x: 0, y: 0 }, { x: 10, y: 0 }, { x: 10, y: 10 }, { x: 0, y: 10 }];
    const hole = [{ x: 4, y: 4 }, { x: 6, y: 4 }, { x: 6, y: 6 }, { x: null, y: 1 }];
    const shapes = [{ rings: [square, hole], closed: true }, { rings: [[{ x: 0, y: 12 }, { x: 10, y: 12 }]], closed: false }, { rings: [[]] }];
    const geo = await layout('shape', { shapes, width: W, height: H, xTitle: 'Metres', yTitle: 'Metres' });
    assert.equal(geo.paths.length, 3);
    assert.equal((geo.paths[0].match(/M/g) ?? []).length, 2, 'one subpath per ring');
    assert.equal((geo.paths[0].match(/Z/g) ?? []).length, 2);
    assert.equal((geo.paths[0].match(/L/g) ?? []).length, 3 + 2, 'the point without x is skipped');
    assert.doesNotMatch(geo.paths[1], /Z/);
    assert.equal(geo.paths[2], '');
    assert.ok(geo.xTicks.length > 1 && geo.yTicks.length > 1);
    const [x0, y0] = geo.paths[0].slice(1).split('L')[0].split(',').map(Number);
    near(x0, geo.plot.left, 0.01);
    near(y0, geo.plot.bottom, 0.01);
});

test('shape keepAspect gives one unit the same length across and up', async () => {
    const wide = [{ rings: [[{ x: 0, y: 0 }, { x: 40, y: 0 }, { x: 40, y: 10 }, { x: 0, y: 10 }]] }];
    const geo = await layout('shape', { shapes: wide, width: W, height: H, keepAspect: true });
    const xs = geo.xTicks, ys = geo.yTicks;
    const perX = (xs[1].pos - xs[0].pos) / (xs[1].value - xs[0].value);
    const perY = (ys[0].pos - ys[1].pos) / (ys[1].value - ys[0].value);
    near(perX, perY, 0.05);
    const free = await layout('shape', { shapes: wide, width: W, height: H });
    assert.notDeepEqual(free.yTicks, geo.yTicks);
});
