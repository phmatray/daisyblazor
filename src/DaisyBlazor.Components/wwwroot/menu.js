// Menu focus management (WAI-ARIA menu button): on open, remember the activator and move focus to
// the first item; Arrow/Home/End move between items; on close, give focus back to the activator.
const items = root => [...root.querySelectorAll('[role=menu] [role=menuitem]:not([disabled]):not([aria-disabled=true])')];

export function open(root) {
    if (!root) return;
    root.__daisyReturnFocus = document.activeElement;
    if (!root.__daisyKeys) {
        root.__daisyKeys = true;
        root.addEventListener('keydown', e => {
            const list = items(root);
            if (!list.length || !['ArrowDown', 'ArrowUp', 'Home', 'End'].includes(e.key)) return;
            const i = list.indexOf(document.activeElement);
            const next = e.key === 'Home' ? 0
                : e.key === 'End' ? list.length - 1
                : e.key === 'ArrowDown' ? (i + 1) % list.length
                : (i - 1 + list.length) % list.length;
            e.preventDefault();
            list[next].focus();
        });
    }
    items(root)[0]?.focus();
}

export function close(root) {
    if (!root) return;
    const target = root.__daisyReturnFocus;
    root.__daisyReturnFocus = null;
    // Only pull focus back when it was in the menu (now gone, so on <body>) or still on the root.
    const active = document.activeElement;
    const lost = !active || active === document.body || root.contains(active);
    if (lost && target && typeof target.focus === 'function' && target.isConnected) target.focus();
}
