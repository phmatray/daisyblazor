// node --test tests/ — drawer.js static fallback against a hand-rolled DOM stub (issue #105).
import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

const src = readFileSync(new URL('../src/DaisyBlazor.Components/wwwroot/drawer.js', import.meta.url), 'utf8');

function setup() {
    const listeners = {};
    const node = (cls, parent, attrs = {}) => ({
        parent, attrs, className: cls, isConnected: true, inert: true,
        classList: { contains: c => cls.split(' ').includes(c) },
        hasAttribute: a => a in attrs, getAttribute: a => attrs[a], setAttribute(a, v) { attrs[a] = v; },
        closest(sel) {
            for (let n = this; n; n = n.parent) if (n.matches(sel)) return n;
            return null;
        },
        matches(sel) {
            if (sel === '.drawer[data-drawer-static]') return cls.includes('drawer ') && 'data-drawer-static' in attrs;
            if (sel === 'button[aria-controls]') return cls === 'btn' && 'aria-controls' in attrs;
            if (sel === 'a[href]') return cls === 'link';
            return false;
        },
        contains(n) { for (; n; n = n.parent) if (n === this) return true; return false; },
        focus() { doc.focused = this; },
    });
    const root = node('drawer x', null, { 'data-drawer-static': '', 'data-drawer-close-on-navigate': '' });
    const toggle = node('drawer-toggle', root); toggle.checked = false;
    const side = node('drawer-side', root); side.id = 'nav-side';
    const navbar = node('navbar', root);
    const button = node('btn', navbar, { 'aria-controls': 'nav-side' }); // not a direct child of drawer-content
    const link = node('link', side);
    root.querySelector = s => (s.includes('drawer-toggle') ? toggle : side);
    const doc = {
        addEventListener: (t, fn) => { listeners[t] = fn; },
        querySelectorAll: s => (s.startsWith('button') ? [button] : [root]),
        getElementById: id => (id === 'nav-side' ? side : null),
    };
    vm.runInNewContext(src, { document: doc, window: { addEventListener: (t, fn) => { listeners[t] = fn; } } });
    return { listeners, toggle, side, button, link, doc };
}

test('a toggle anywhere with aria-controls opens and closes the drawer', () => {
    const { listeners, toggle, side, button, doc } = setup();
    listeners.click({ target: button });
    assert.equal(toggle.checked, true);
    assert.equal(side.inert, false);
    assert.equal(button.attrs['aria-expanded'], 'true');
    listeners.click({ target: button });
    assert.equal(toggle.checked, false);
    assert.equal(doc.focused, button);
});

test('CloseOnNavigation closes on a panel link click and on popstate', () => {
    const { listeners, toggle, button, link } = setup();
    listeners.click({ target: button });
    listeners.click({ target: link });
    assert.equal(toggle.checked, false);
    listeners.click({ target: button });
    listeners.popstate();
    assert.equal(toggle.checked, false);
});
