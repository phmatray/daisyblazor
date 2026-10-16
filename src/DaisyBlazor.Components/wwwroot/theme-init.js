// Pre-paint theme script for DaisyBlazor's ThemeProvider. Load it synchronously in <head>:
//   <script src="_content/DaisyBlazor.Components/theme-init.js"></script>
// Sets data-theme on <html> from the saved preference (or the OS light/dark setting) before
// first paint, so static SSR and prerendered pages don't flash the wrong theme.
// Optional overrides on the script tag: data-storage-key, data-light, data-dark
// (defaults match ThemeProvider: "daisy-theme", "light", "dark"). No eval, no inline code.
(function () {
    var d = (document.currentScript && document.currentScript.dataset) || {};
    var key = d.storageKey || 'daisy-theme';
    var mq = window.matchMedia ? window.matchMedia('(prefers-color-scheme: dark)') : null;

    function apply() {
        var pref = null;
        try { pref = localStorage.getItem(key); } catch (e) { /* storage blocked */ }
        if (!pref || pref === 'system') {
            pref = mq && mq.matches ? (d.dark || 'dark') : (d.light || 'light');
        }
        document.documentElement.setAttribute('data-theme', pref);
    }

    apply();
    // Keep following the OS while the preference is "system" (also covers pages with no circuit).
    if (mq && mq.addEventListener) mq.addEventListener('change', apply);
})();
