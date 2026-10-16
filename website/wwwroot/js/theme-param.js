// ?theme=<name> preselects a theme (shareable links, screenshots); must run before theme-init.js.
(function () {
    var t = new URLSearchParams(location.search).get('theme');
    if (t) { try { localStorage.setItem('daisy-theme', t); } catch (e) { /* storage blocked */ } }
})();
