// node --test tests/ — runs theme-init.js against a stubbed document/localStorage (issue #71).
import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

const src = readFileSync(new URL('../src/DaisyBlazor.Components/wwwroot/theme-init.js', import.meta.url), 'utf8');

function run({ stored, dataset = {}, dark = false }) {
    const attrs = {};
    vm.runInNewContext(src, {
        document: { currentScript: { dataset }, documentElement: { setAttribute: (k, v) => { attrs[k] = v; } } },
        localStorage: { getItem: () => stored ?? null },
        window: { matchMedia: () => ({ matches: dark, addEventListener() {} }) },
    });
    return attrs['data-theme'];
}

test('without data-themes, a stored value applies verbatim (back-compat)', () => {
    assert.equal(run({ stored: 'anything' }), 'anything');
});

test('a stored value in data-themes applies', () => {
    assert.equal(run({ stored: 'cupcake', dataset: { themes: 'light dark,cupcake' } }), 'cupcake');
});

test('a stored value not in data-themes falls back to the light/dark default', () => {
    const dataset = { themes: 'brand brand-dark cupcake', light: 'brand', dark: 'brand-dark' };
    assert.equal(run({ stored: 'retired', dataset }), 'brand');
    assert.equal(run({ stored: 'retired', dataset, dark: true }), 'brand-dark');
});

test('system and empty follow the OS', () => {
    assert.equal(run({ stored: 'system', dark: true }), 'dark');
    assert.equal(run({}), 'light');
});
