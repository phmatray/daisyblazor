// ?theme=<name> preselects a theme (shareable links, screenshots); must run before theme-init.js.
// ThemeProvider and theme-init.js read localStorage, so the theme goes there for this page view only:
// the visitor's own saved choice comes back when the page is left, unless they picked a theme meanwhile.
(function () {
    var t = new URLSearchParams(location.search).get('theme');
    if (!t) return;
    var key = 'daisy-theme';
    try {
        var saved = localStorage.getItem(key);
        localStorage.setItem(key, t);
        addEventListener('pagehide', function () {
            try {
                if (localStorage.getItem(key) !== t) return;
                if (saved === null) localStorage.removeItem(key); else localStorage.setItem(key, saved);
            } catch (e) { /* storage blocked */ }
        });
    } catch (e) { /* storage blocked */ }
})();
