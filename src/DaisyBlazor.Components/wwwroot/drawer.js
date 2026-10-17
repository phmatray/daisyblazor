// Static-SSR fallback for Drawer. Without a circuit the C# toggle button, Escape and focus handling
// are inert, and the panel keeps the inert attribute it was rendered with. Load once, anywhere:
//   <script src="_content/DaisyBlazor.Components/drawer.js"></script>
// Only drawers rendered statically (data-drawer-static) are touched; interactive ones stay in C#.
(function () {
    function parts(el) {
        var root = el && el.closest && el.closest('.drawer[data-drawer-static]');
        if (!root) return null;
        return {
            root: root,
            toggle: root.querySelector(':scope > .drawer-toggle'),
            side: root.querySelector(':scope > .drawer-side'),
            button: root.querySelector(':scope > .drawer-content > button[aria-controls]')
        };
    }

    function sync(p) {
        var open = p.toggle.checked;
        // Same rule as Drawer.razor: a responsive drawer-open class keeps the panel reachable.
        p.side.inert = !open && !/(^|[\s:])drawer-open(\s|$)/.test(p.root.className);
        if (p.button) p.button.setAttribute('aria-expanded', open ? 'true' : 'false');
        if (open) p.side.focus();
        else if (p.button) p.button.focus();
    }

    function set(p, open) {
        if (p.toggle.checked === open) return;
        p.toggle.checked = open;
        sync(p);
    }

    // The overlay label (and any custom label[for]) toggles the checkbox natively.
    document.addEventListener('change', function (e) {
        var p = e.target.classList && e.target.classList.contains('drawer-toggle') && parts(e.target);
        if (p && p.toggle === e.target) sync(p);
    });

    document.addEventListener('click', function (e) {
        var btn = e.target.closest && e.target.closest('button[aria-controls]');
        var p = btn && parts(btn);
        if (p && p.button === btn) set(p, !p.toggle.checked);
    });

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;
        var p = parts(e.target);
        if (p && p.side.contains(e.target)) set(p, false);
    });
})();
