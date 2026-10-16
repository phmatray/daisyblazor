// ES module used by ThemeProvider (imported via IJSRuntime; no eval, CSP-friendly).
const darkQuery = () => (window.matchMedia ? window.matchMedia('(prefers-color-scheme: dark)') : null);

/** Returns the saved preference (a theme name or "system"), or null. */
export function get(storageKey) {
    try { return localStorage.getItem(storageKey); } catch { return null; }
}

/** Whether the OS currently prefers a dark color scheme. */
export function prefersDark() {
    return !!darkQuery()?.matches;
}

/** Applies the resolved theme to <html data-theme> and persists the preference to localStorage + cookies. */
export function set(storageKey, cookieKey, darkModeCookieKey, preference, theme) {
    document.documentElement.setAttribute('data-theme', theme);
    try { localStorage.setItem(storageKey, preference); } catch { /* storage blocked */ }
    document.cookie = `${cookieKey}=${encodeURIComponent(preference)};path=/;max-age=31536000;samesite=lax`;
    document.cookie = `${darkModeCookieKey}=${encodeURIComponent(theme)};path=/;max-age=31536000;samesite=lax`;
}

/** Calls dotnetRef.OnSystemThemeChanged(isDark) on OS light/dark changes. Returns a handle with dispose(). */
export function subscribe(dotnetRef) {
    const mq = darkQuery();
    if (!mq) return null;
    const handler = e => dotnetRef.invokeMethodAsync('OnSystemThemeChanged', e.matches);
    mq.addEventListener('change', handler);
    return { dispose: () => mq.removeEventListener('change', handler) };
}
