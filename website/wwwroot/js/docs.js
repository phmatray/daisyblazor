// Docs-site interop: theme view transitions, the ⌘K shortcut, clipboard, dialogs, scroll spy.

const reducedMotion = () => window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;

/**
 * Starts a circular view transition from (x, y) and resolves once the old frame is captured.
 * The transition's new frame waits until <html data-theme> changes (set by ThemeProvider).
 */
export function beginThemeTransition(x, y) {
    if (!document.startViewTransition || reducedMotion()) return Promise.resolve();
    const root = document.documentElement;
    root.style.setProperty('--vt-x', `${x}px`);
    root.style.setProperty('--vt-y', `${y}px`);
    return new Promise(captured => {
        document.startViewTransition(() => {
            captured();
            return new Promise(done => {
                const mo = new MutationObserver(() => { mo.disconnect(); done(); });
                mo.observe(root, { attributes: true, attributeFilter: ['data-theme'] });
                setTimeout(() => { mo.disconnect(); done(); }, 1200);
            });
        });
    });
}

/** Calls dotnetRef.OpenSearch() on ⌘K / Ctrl+K and "/" (outside inputs). */
export function registerShortcuts(dotnetRef) {
    const handler = e => {
        const typing = /^(INPUT|TEXTAREA|SELECT)$/.test(e.target.tagName) || e.target.isContentEditable;
        if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            dotnetRef.invokeMethodAsync('OpenSearch');
        } else if (e.key === '/' && !typing) {
            e.preventDefault();
            dotnetRef.invokeMethodAsync('OpenSearch');
        }
    };
    window.addEventListener('keydown', handler);
    return { dispose: () => window.removeEventListener('keydown', handler) };
}

export function copy(text) {
    return navigator.clipboard?.writeText(text).then(() => true, () => false) ?? false;
}

export function showModal(el) { if (el && !el.open) el.showModal(); }
export function closeModal(el) { if (el?.open) el.close(); }
export function focus(el) { el?.focus(); }
export function scrollIntoView(el) { el?.scrollIntoView({ block: 'nearest' }); }

/** Highlights the track (table-of-contents entry) whose section is in view. */
export function observeSections(dotnetRef, ids) {
    const els = ids.map(id => document.getElementById(id)).filter(Boolean);
    if (!els.length || !('IntersectionObserver' in window)) return null;
    const io = new IntersectionObserver(entries => {
        const visible = entries.filter(e => e.isIntersecting).sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top);
        if (visible.length) dotnetRef.invokeMethodAsync('OnSectionVisible', visible[0].target.id);
    }, { rootMargin: '-15% 0px -70% 0px' });
    els.forEach(el => io.observe(el));
    return { dispose: () => io.disconnect() };
}

export function scrollToTop() { window.scrollTo({ top: 0, behavior: 'instant' }); }
