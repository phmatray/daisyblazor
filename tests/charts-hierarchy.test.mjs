// node --test tests/ — the "Hierarchy & flow" layouts computed by the bundled D3 module:
// treemap, sunburst, circle-packing, icicle, chord, network, arc-diagram.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';

const root = {
    name: 'Storage',
    children: [
        { name: 'Platform', children: [{ name: 'Logs', value: 40 }, { name: 'Backups', value: 20 }] },
        { name: 'Data', value: 30 },
        { name: 'Broken', value: -5 },
        { name: 'Unknown', value: null },
    ],
};
const W = 640;
const H = 360;
const close = (a, b, eps = 1e-6) => assert.ok(Math.abs(a - b) < eps, `${a} ≈ ${b}`);
const area = n => (n.x1 - n.x0) * (n.y1 - n.y0);

const links = [
    { source: 'Gateway', target: 'Auth', value: 4 },
    { source: 'Gateway', target: 'Projects', value: 2 },
    { source: 'Projects', target: 'Auth', value: 1 },
    { source: 'Auth', target: 'Auth', value: 3 },
    { source: 'Projects', target: 'Storage', value: 0 },
    { source: 'Projects', target: 'Search', value: Number.NaN },
];

test('hierarchy layouts drop non-positive leaves, sum parents and colour by top-level branch', async () => {
    for (const kind of ['treemap', 'sunburst', 'circle-packing', 'icicle']) {
        const { nodes, groups } = await layout(kind, { root, width: W, height: H });
        assert.deepEqual(groups, ['Platform', 'Data'], kind);
        assert.ok(!nodes.some(n => n.name === 'Broken' || n.name === 'Unknown'), kind);
        const platform = nodes.find(n => n.name === 'Platform');
        assert.equal(platform.value, 60, kind);
        assert.equal(platform.leaf, false, kind);
        assert.equal(nodes.find(n => n.name === 'Logs').group, 0, kind);
        assert.equal(nodes.find(n => n.name === 'Data').group, 1, kind);
        assert.equal(nodes.find(n => n.name === 'Backups').path, 'Platform / Backups', kind);
    }
});

test('treemap: leaf areas follow their values and stay inside parents and the chart', async () => {
    const { nodes } = await layout('treemap', { root, width: W, height: H, padding: 0 });
    assert.ok(!nodes.some(n => n.depth === 0), 'the root is not drawn');
    const logs = nodes.find(n => n.name === 'Logs');
    const backups = nodes.find(n => n.name === 'Backups');
    const platform = nodes.find(n => n.name === 'Platform');
    close(area(logs) / area(backups), 2, 0.01);
    for (const n of nodes) {
        assert.ok(n.x0 >= 0 && n.y0 >= 0 && n.x1 <= W && n.y1 <= H, n.name);
    }
    assert.ok(logs.y0 >= platform.y0 + 18, 'children start below the parent header');
    assert.ok(logs.x0 >= platform.x0 && logs.x1 <= platform.x1 && logs.y1 <= platform.y1);
});

test('treemap: tiling changes the cut', async () => {
    const flat = { name: 'r', children: [{ name: 'a', value: 1 }, { name: 'b', value: 1 }, { name: 'c', value: 2 }] };
    const dice = await layout('treemap', { root: flat, width: W, height: H, tile: 'dice', padding: 0 });
    assert.ok(dice.nodes.every(n => n.y0 === 0 && n.y1 === H), 'dice: full-height columns');
    const slice = await layout('treemap', { root: flat, width: W, height: H, tile: 'slice', padding: 0 });
    assert.ok(slice.nodes.every(n => n.x0 === 0 && n.x1 === W), 'slice: full-width rows');
});

test('icicle: one band per level, the root spanning the whole first column or row', async () => {
    const h = await layout('icicle', { root, width: W, height: H });
    const top = h.nodes.find(n => n.depth === 0);
    assert.deepEqual([top.x0, top.y0, top.y1], [0, 0, H]);
    close(top.x1, W / 3);
    const logs = h.nodes.find(n => n.name === 'Logs');
    close(logs.x0, (2 * W) / 3);
    close((logs.y1 - logs.y0) / H, 40 / 90);

    const v = await layout('icicle', { root, width: W, height: H, orientation: 'vertical' });
    const vtop = v.nodes.find(n => n.depth === 0);
    assert.deepEqual([vtop.x0, vtop.x1, vtop.y0], [0, W, 0]);
    close(vtop.y1, H / 3);
});

test('circle-packing: children inside their parent, everything inside the chart', async () => {
    const { nodes } = await layout('circle-packing', { root, width: W, height: H });
    const circle = n => ({ x: (n.x0 + n.x1) / 2, y: (n.y0 + n.y1) / 2, r: (n.x1 - n.x0) / 2 });
    const outer = circle(nodes.find(n => n.depth === 0));
    close(outer.r, (H - 2) / 2);
    for (const n of nodes) {
        assert.ok(n.x0 >= 0 && n.y0 >= 0 && n.x1 <= W && n.y1 <= H, n.name);
    }
    const platform = circle(nodes.find(n => n.name === 'Platform'));
    const logs = circle(nodes.find(n => n.name === 'Logs'));
    assert.ok(Math.hypot(logs.x - platform.x, logs.y - platform.y) + logs.r <= platform.r + 1e-6);
    assert.ok(nodes.find(n => n.name === 'Logs').room > 0, 'leaves get label room');
    assert.equal(nodes.find(n => n.name === 'Platform').room, 0, 'parents are not labelled');
});

test('sunburst: arcs fill the circle in proportion, labels inside the radius', async () => {
    const { nodes, cx, cy } = await layout('sunburst', { root, width: W, height: H });
    assert.deepEqual([cx, cy], [W / 2, H / 2]);
    const depth1 = nodes.filter(n => n.depth === 1);
    close(depth1.reduce((s, n) => s + (n.x1 - n.x0), 0), 2 * Math.PI);
    const platform = nodes.find(n => n.name === 'Platform');
    close((platform.x1 - platform.x0) / (2 * Math.PI), 60 / 90);
    for (const n of nodes) {
        assert.match(n.d, /^M/);
        assert.ok(Math.hypot(n.labelX, n.labelY) < H / 2, n.name);
        assert.ok(n.labelAngle >= -90 && n.labelAngle <= 90, `${n.name} reads left to right`);
    }
});

test('drill-down: a focus path lays out that subtree at full size, coloured by its top-level branch', async () => {
    const deep = {
        name: 'Acme Cloud',
        children: [
            { name: 'Data', value: 30 },
            { name: 'Platform', children: [{ name: 'api', children: [{ name: 'auth', value: 25 }, { name: 'search', value: 15 }] }, { name: 'web', value: 20 }] },
        ],
    };
    for (const kind of ['treemap', 'sunburst', 'icicle']) {
        const whole = await layout(kind, { root: deep, width: W, height: H });
        assert.equal(whole.focus, undefined, `${kind}: no focus asked, none echoed`);

        const { nodes, groups, focus } = await layout(kind, { root: deep, focus: ['Platform', 'api'], width: W, height: H });
        assert.deepEqual(focus, ['Platform', 'api'], kind);
        assert.deepEqual(groups, ['Platform', 'Data'], `${kind}: legend keys stay those of the whole tree`);
        assert.deepEqual(nodes.map(n => n.name).filter(n => n !== 'api').sort(), ['auth', 'search'], kind);
        const auth = nodes.find(n => n.name === 'auth');
        assert.equal(auth.group, 0, `${kind}: colour keyed to the top-level branch`);
        assert.equal(auth.path, 'Platform / api / auth', `${kind}: paths stay those of the whole tree`);
        assert.deepEqual(auth.names, ['Platform', 'api', 'auth'], kind);
        assert.equal(auth.depth, 1, kind);

        const unknown = await layout(kind, { root: deep, focus: ['Platform', 'nope'], width: W, height: H });
        assert.deepEqual(unknown.focus, [], `${kind}: unknown path falls back to the root`);
        assert.deepEqual(unknown.nodes, whole.nodes, kind);

        const leaf = await layout(kind, { root: deep, focus: ['Platform', 'web'], width: W, height: H });
        assert.deepEqual(leaf.focus, ['Platform'], `${kind}: a leaf shows its parent`);
    }

    const { nodes } = await layout('treemap', { root: deep, focus: ['Platform', 'api'], width: W, height: H, padding: 0 });
    close(nodes.reduce((s, n) => s + area(n), 0), W * H, 1e-3);
    const icicle = await layout('icicle', { root: deep, focus: ['Platform'], width: W, height: H });
    const top = icicle.nodes.find(n => n.depth === 0);
    assert.deepEqual([top.name, top.y0, top.y1], ['Platform', 0, H]);
    close(top.x1, W / 3);
});

test('drill-down: names containing "/" or " / " stay whole, the focus and the node paths are lists of names', async () => {
    const tree = {
        name: 'Ops',
        children: [
            { name: 'CI/CD', children: [{ name: 'A / B', children: [{ name: 'x', value: 5 }, { name: 'y', value: 3 }] }, { name: 'z', value: 2 }] },
            { name: 'CI', children: [{ name: 'CD', value: 50 }, { name: 'w', value: 1 }] },
        ],
    };
    for (const kind of ['treemap', 'sunburst', 'icicle', 'circle-packing']) {
        const whole = await layout(kind, { root: tree, width: W, height: H });
        assert.deepEqual(whole.nodes.find(n => n.name === 'A / B').names, ['CI/CD', 'A / B'], kind);
        assert.ok(whole.nodes.filter(n => n.depth === 0).every(n => n.names.length === 0), `${kind}: the root is no name below the root`);
    }
    for (const kind of ['treemap', 'sunburst', 'icicle']) {
        const out = await layout(kind, { root: tree, focus: ['CI/CD', 'A / B'], width: W, height: H });
        assert.deepEqual(out.focus, ['CI/CD', 'A / B'], kind);
        assert.deepEqual(out.nodes.map(n => n.name).filter(n => n !== 'A / B').sort(), ['x', 'y'], kind);
        assert.ok(out.nodes.every(n => n.names.slice(0, 2).join('\n') === 'CI/CD\nA / B'), `${kind}: nodes keep their place in the whole tree`);
    }
});

test('hierarchy layouts return an empty layout for missing or all-zero trees', async () => {
    const empty = { nodes: [], groups: [], cx: 0, cy: 0 };
    for (const kind of ['treemap', 'sunburst', 'circle-packing', 'icicle']) {
        assert.deepEqual(await layout(kind, { root: null, width: W, height: H }), empty, kind);
        assert.deepEqual(await layout(kind, { root: { name: 'r', children: [{ name: 'a', value: 0 }] }, width: W, height: H }), empty, kind);
    }
    const single = await layout('treemap', { root: { name: 'Only', value: 5, children: null }, width: W, height: H });
    assert.equal(single.nodes.length, 0, 'a lone root has nothing to tile');
    assert.deepEqual(single.groups, ['Only']);
});

test('chord: undirected relations count on both nodes; self links and bad values are dropped', async () => {
    const { groups, ribbons, cx, cy } = await layout('chord', { links, width: W, height: H });
    assert.deepEqual(groups.map(g => g.name), ['Gateway', 'Auth', 'Projects']);
    assert.deepEqual(groups.map(g => g.value), [6, 5, 3]);
    assert.equal(ribbons.length, 3);
    assert.deepEqual([cx, cy], [W / 2, H / 2]);
    for (const g of groups) {
        assert.match(g.path, /^M/);
        assert.ok(Math.abs(g.labelY) < H / 2, g.name);
        assert.ok(['start', 'end', 'middle'].includes(g.anchor));
    }
});

test('chord: directed ribbons are one per direction, and a wide label shrinks the ring', async () => {
    const both = [{ source: 'A', target: 'B', value: 3 }, { source: 'B', target: 'A', value: 1 }];
    const d = await layout('chord', { links: both, width: W, height: H, directed: true });
    assert.equal(d.ribbons.length, 2);
    assert.deepEqual(d.groups.map(g => g.value), [4, 4]);
    const u = await layout('chord', { links: both, width: W, height: H });
    assert.equal(u.ribbons.length, 1);
    assert.equal(u.ribbons[0].sourceValue, 4);

    const radius = l => Math.max(...l.groups.map(g => Math.hypot(g.labelX, g.labelY)));
    const wide = await layout('chord', { links: both, width: W, height: H, labelWidth: 250 });
    assert.ok(radius(wide) < radius(u));
    assert.ok(radius(wide) + 250 <= W / 2);
    assert.deepEqual(await layout('chord', { links: [], width: W, height: H }), { groups: [], ribbons: [], cx: 0, cy: 0 });
});

test('network: deterministic, inside the chart, with room for labels on the right', async () => {
    const nodes = [{ name: 'Gateway', group: 'Edge' }, { name: 'Lonely', group: 'Data' }];
    const a = await layout('network', { links, nodes, width: W, height: H });
    const b = await layout('network', { links, nodes, width: W, height: H });
    assert.deepEqual(a, b);
    assert.deepEqual(a.nodes.map(n => n.name), ['Gateway', 'Lonely', 'Auth', 'Projects']);
    assert.deepEqual(a.groups, ['Edge', 'Data']);
    assert.deepEqual(a.nodes.map(n => n.groupIndex), [0, 1, -1, -1]);
    assert.deepEqual(a.nodes.map(n => n.degree), [2, 0, 2, 2]);
    assert.equal(a.links.length, 3);
    for (const n of a.nodes) {
        assert.ok(n.x - n.r >= 0 && n.x + n.r + a.labelRoom <= W + 1e-6, n.name);
        assert.ok(n.y - n.r >= 0 && n.y + n.r <= H, n.name);
    }
    const g = a.nodes[0];
    assert.ok(a.links[0].path.startsWith(`M${g.x},${g.y}L`));
    assert.equal(a.links[0].width, 5, 'the heaviest link is the widest');

    const unlabelled = await layout('network', { links, width: W, height: H, labels: false });
    assert.equal(unlabelled.labelRoom, 0);
    assert.deepEqual(await layout('network', { links: [], width: W, height: H }), { nodes: [], links: [], groups: [], labelRoom: 0 });
});

test('arc-diagram: nodes on one baseline, arcs above it and inside the chart', async () => {
    const nodes = [{ name: 'Projects', group: 'Core' }, { name: 'Gateway', group: 'Edge' }, { name: 'Auth', group: 'Core' }];
    const { nodes: out, links: arcs, labelRoom } = await layout('arc-diagram', { links, nodes, width: W, height: H });
    const xs = Object.fromEntries(out.map(n => [n.name, n.x]));
    assert.ok(xs.Projects < xs.Gateway && xs.Gateway < xs.Auth, 'input order');
    assert.ok(out.every(n => n.y === H - labelRoom));
    for (const l of arcs) {
        const [, x1, , rx, ry, x2] = l.path.match(/^M([\d.]+),([\d.]+)A([\d.]+),([\d.]+) 0 0,1 ([\d.]+),/).map(Number);
        assert.ok(x1 < x2);
        close(rx, (x2 - x1) / 2);
        assert.ok(H - labelRoom - ry >= 0, 'arc stays below the top');
    }

    const grouped = await layout('arc-diagram', { links, nodes, width: W, height: H, sort: 'group' });
    const gx = Object.fromEntries(grouped.nodes.map(n => [n.name, n.x]));
    assert.ok(gx.Projects < gx.Auth && gx.Auth < gx.Gateway, 'Core nodes side by side, then Edge');
    const byDegree = await layout('arc-diagram', { links: [...links, { source: 'Auth', target: 'X', value: 1 }], width: W, height: H, sort: 'degree' });
    const dx = Object.fromEntries(byDegree.nodes.map(n => [n.name, n.x]));
    assert.ok(dx.Auth < dx.Gateway && dx.Gateway < dx.X);
    assert.deepEqual(await layout('arc-diagram', { links: null, width: W, height: H }), { nodes: [], links: [], groups: [], labelRoom: 0 });
});

test('treemap: strip tiling fills rows in order while they get closer to square', async () => {
    const values = [6, 6, 4, 3, 2, 2, 1];
    const flat = { name: 'r', children: values.map((value, i) => ({ name: 'abcdefg'[i], value })) };
    const { nodes } = await layout('treemap', { root: flat, width: 600, height: 400, tile: 'strip', padding: 0 });
    const rect = n => [n.x0, n.y0, n.x1, n.y1].map(v => Math.round(v * 100) / 100);
    // 24 units on 240 000 px²: a, b, c fill the first strip (adding d would worsen it), d to g the second.
    assert.deepEqual(nodes.map(rect), [
        [0, 0, 225, 266.67], [225, 0, 450, 266.67], [450, 0, 600, 266.67],
        [0, 266.67, 225, 400], [225, 266.67, 375, 400], [375, 266.67, 525, 400], [525, 266.67, 600, 400],
    ]);
});
