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

/** Scrolls the side navigation (not the page) so the link of the current page sits in its middle. */
export function revealActiveLink(container) {
    const link = container?.querySelector('a.active[aria-current="page"], a.active');
    if (!link) return;
    // The sticky container is the offset parent, so offsetTop is measured from its top.
    container.scrollTop = link.offsetTop - container.clientHeight / 2;
}

// Live covers in the tiles: shrink one that is wider than its tile until it fits, and centre it.
const coverResize = new ResizeObserver(entries => {
    for (const e of entries) fitCover(e.target.closest('[data-cover]'));
});

function fitCover(box) {
    const cover = box?.firstElementChild;
    if (!cover) return;
    cover.style.width = '';
    cover.style.scale = '';
    const base = parseFloat(getComputedStyle(cover).scale) || 1;
    // scrollWidth counts content that spills past the cover's own box; a scroller inside it (a wide table)
    // hides its own overflow, so add that too. A carousel scrolls on purpose and keeps its width.
    let hidden = 0;
    for (const el of cover.querySelectorAll('*')) {
        if (el.scrollWidth > el.clientWidth + 1 && !el.closest('.carousel') && getComputedStyle(el).overflowX !== 'visible') {
            hidden = Math.max(hidden, el.scrollWidth - el.clientWidth);
        }
    }
    const width = Math.max(cover.scrollWidth, cover.offsetWidth) + hidden;
    if (width > cover.offsetWidth) cover.style.width = `${width}px`;
    const room = box.clientWidth - 24;
    const scale = Math.min(base, room / width);
    cover.style.scale = String(scale);
    const height = cover.offsetHeight * scale;
    cover.style.top = `${Math.max(16, (box.clientHeight - height) / 2)}px`;
}

/** Fits every tile cover on the page now and whenever a tile or its cover changes size. */
export function watchCovers() {
    const watch = root => root.querySelectorAll?.('[data-cover]').forEach(box => {
        if (box.dataset.watched) return;
        box.dataset.watched = '1';
        coverResize.observe(box);
        if (box.firstElementChild) coverResize.observe(box.firstElementChild);
    });
    watch(document);
    new MutationObserver(records => records.forEach(r => r.addedNodes.forEach(n => n.nodeType === 1 && watch(n.parentElement ?? n))))
        .observe(document.body, { childList: true, subtree: true });
}
