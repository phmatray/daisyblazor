// node --test tests/ — the Sankey layout computed by the bundled D3 module.
import test from 'node:test';
import assert from 'node:assert/strict';
import { layout } from '../src/DaisyBlazor.Charts/wwwroot/charts.mjs';

const links = [
    { source: 'Organic', target: 'Starter', value: 30 },
    { source: 'Ads', target: 'Starter', value: 10 },
    { source: 'Starter', target: 'Active', value: 32 },
    { source: 'Starter', target: 'Churned', value: 8 },
];

test('nodes are the distinct names, sized by the flow through them', async () => {
    const { nodes, links: out } = await layout('sankey', { links, width: 600, height: 300 });
    assert.deepEqual(nodes.map(n => n.name), ['Organic', 'Starter', 'Ads', 'Active', 'Churned']);
    assert.equal(nodes[1].value, 40);
    assert.deepEqual(nodes.map(n => n.depth), [0, 1, 0, 2, 2]);
    assert.equal(out.length, 4);
    assert.match(out[0].path, /^M[\d.]+,[\d.]+C/);
    for (const n of nodes) {
        assert.ok(n.x0 >= 0 && n.x1 <= 600 && n.y0 >= 0 && n.y1 <= 300);
    }
});

test('links without a positive value are dropped; no links gives an empty layout', async () => {
    const { nodes } = await layout('sankey', { links: [...links, { source: 'X', target: 'Y', value: 0 }], width: 600, height: 300 });
    assert.ok(!nodes.some(n => n.name === 'X'));
    assert.deepEqual(await layout('sankey', { links: [], width: 600, height: 300 }), { nodes: [], links: [] });
});

test('a cycle is reported, not drawn', async () => {
    await assert.rejects(layout('sankey', { links: [{ source: 'A', target: 'B', value: 1 }, { source: 'B', target: 'A', value: 1 }], width: 100, height: 100 }), /circular/);
});

test('an unknown kind is refused before any import', async () => {
    await assert.rejects(layout('../d3', {}), /Unknown chart layout/);
});
