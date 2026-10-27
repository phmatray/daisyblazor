// node --test tests/ — dialog.js against a stubbed document/MutationObserver (issue #101).
import test from 'node:test';
import assert from 'node:assert/strict';

const observers = [];
globalThis.MutationObserver = class {
    constructor(cb) { this.cb = cb; this.active = false; observers.push(this); }
    observe() { this.active = true; }
    disconnect() { this.active = false; }
};
const opener = { isConnected: true, focused: 0, focus() { this.focused++; } };
globalThis.document = { activeElement: opener, body: {} };

const { open, close } = await import('../src/DaisyBlazor.Components/wwwroot/dialog.js');

const fakeDialog = (box = null) => ({
    open: false, isConnected: true,
    showModal() { this.open = true; }, close() { this.open = false; }, addEventListener() {},
    querySelector: () => box,
});

const fakeBox = (inside = []) => ({
    focused: 0, focus() { this.focused++; document.activeElement = this; },
    contains(el) { return el === this || inside.includes(el); },
});

test('a dialog removed from the DOM while open gives focus back to its opener', () => {
    opener.focused = 0;
    const dialog = fakeDialog();
    open(dialog);
    dialog.isConnected = false;
    observers.at(-1).cb();
    assert.equal(opener.focused, 1);
    assert.equal(observers.at(-1).active, false);
});

test('close restores focus once and stops watching', () => {
    opener.focused = 0;
    const dialog = fakeDialog();
    open(dialog);
    close(dialog);
    assert.equal(dialog.open, false);
    assert.equal(opener.focused, 1);
    assert.equal(observers.at(-1).active, false);
});

test('a content-only dialog focuses its box, not the backdrop button (#163)', () => {
    const backdropButton = {};
    const box = fakeBox();
    const dialog = fakeDialog(box);
    dialog.showModal = function () { this.open = true; document.activeElement = backdropButton; };
    open(dialog);
    assert.equal(box.focused, 1);
    assert.equal(document.activeElement, box);
    close(dialog);
});

test('a dialog whose box already holds focus keeps it', () => {
    const field = {};
    const box = fakeBox([field]);
    const dialog = fakeDialog(box);
    dialog.showModal = function () { this.open = true; document.activeElement = field; };
    open(dialog);
    assert.equal(box.focused, 0);
    close(dialog);
});
