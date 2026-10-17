// Static-SSR fallback for Drawer. Without a circuit the C# toggle button, Escape and focus handling
// are inert, and the panel keeps the inert attribute it was rendered with. Load once, anywhere:
//   <script src="_content/DaisyBlazor.Components/drawer.js"></script>
// Only drawers rendered statically (data-drawer-static) are touched; interactive ones stay in C#.
// Any button[aria-controls=<side id>] toggles the drawer, wherever it sits (e.g. in a navbar).
(function () {
    function parts(el) {
        var root = el && el.closest && el.closest('.drawer[data-drawer-static]');
        if (!root) return null;
        var side = root.querySelector(':scope > .drawer-side');
        return {
            root: root,
            toggle: root.querySelector(':scope > .drawer-toggle'),
            side: side,
            buttons: side && side.id
                ? Array.prototype.slice.call(document.querySelectorAll('button[aria-controls="' + side.id + '"]'))
                : []
        };
    }

    function sync(p, focus) {
        var open = p.toggle.checked;
        // Same rule as Drawer.razor: a responsive drawer-open class keeps the panel reachable.
        p.side.inert = !open && !/(^|[\s:])drawer-open(\s|$)/.test(p.root.className);
        p.buttons.forEach(function (b) { b.setAttribute('aria-expanded', open ? 'true' : 'false'); });
        if (focus === false) return;
        if (open) p.side.focus();
        else {
            var back = p.root.__daisyOpener && p.root.__daisyOpener.isConnected ? p.root.__daisyOpener : p.buttons[0];
            if (back) back.focus();
        }
    }

    function set(p, open, focus) {
        if (p.toggle.checked === open) return;
        p.toggle.checked = open;
        sync(p, focus);
    }

    function closeOnNavigate() {
        document.querySelectorAll('.drawer[data-drawer-static][data-drawer-close-on-navigate]').forEach(function (root) {
            var p = parts(root);
            if (p) set(p, false, false);
        });
    }

    // The overlay label (and any custom label[for]) toggles the checkbox natively.
    document.addEventListener('change', function (e) {
        var p = e.target.classList && e.target.classList.contains('drawer-toggle') && parts(e.target);
        if (p && p.toggle === e.target) sync(p);
    });

    document.addEventListener('click', function (e) {
        var btn = e.target.closest && e.target.closest('button[aria-controls]');
        var side = btn && document.getElementById(btn.getAttribute('aria-controls'));
        var p = side && side.classList.contains('drawer-side') && parts(side);
        if (p && p.side === side) {
            p.root.__daisyOpener = btn;
            set(p, !p.toggle.checked);
            return;
        }
        // CloseOnNavigation: following a link inside the panel closes it.
        var link = e.target.closest && e.target.closest('a[href]');
        p = link && parts(link);
        if (p && p.side.contains(link) && p.root.hasAttribute('data-drawer-close-on-navigate')) set(p, false, false);
    });

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') return;
        var p = parts(e.target);
        if (p && p.side.contains(e.target)) set(p, false);
    });

    window.addEventListener('popstate', closeOnNavigate);
})();
