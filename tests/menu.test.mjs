// node --test tests/ — menu.js focus moves against stubbed elements (issue #109).
import test from 'node:test';
import assert from 'node:assert/strict';

const el = name => ({ name, isConnected: true, focus() { globalThis.document.activeElement = this; } });
const activator = el('activator');
const body = el('body');
const list = [el('a'), el('b'), el('c')];
let onKey;
const root = {
    contains: e => list.includes(e) || e === activator,
    querySelectorAll: () => list,
    addEventListener: (_, fn) => { onKey = fn; },
};
globalThis.document = { activeElement: activator, body };

const { open, close } = await import('../src/DaisyBlazor.Components/wwwroot/menu.js');
const key = k => onKey({ key: k, preventDefault() {} });

test('open focuses the first item; arrows, Home and End move and wrap', () => {
    open(root);
    assert.equal(document.activeElement.name, 'a');
    key('ArrowUp');
    assert.equal(document.activeElement.name, 'c');
    key('ArrowDown');
    assert.equal(document.activeElement.name, 'a');
    key('End');
    assert.equal(document.activeElement.name, 'c');
    key('Home');
    assert.equal(document.activeElement.name, 'a');
});

test('close gives focus back to the activator once the list is gone', () => {
    document.activeElement = activator;
    open(root);
    document.activeElement = body; // the focused item left the DOM
    close(root);
    assert.equal(document.activeElement, activator);
});
