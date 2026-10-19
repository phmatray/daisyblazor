// Entry point for the D3-backed charts. A chart calls layout(kind, spec) after it renders; the
// matching file in ./layouts/ computes the geometry with D3 and returns plain data (numbers and
// SVG path strings). The Razor component draws the SVG itself, with the theme's colours.
const KIND = /^[a-z][a-z0-9-]*$/;

export async function layout(kind, spec) {
    if (!KIND.test(kind)) {
        throw new Error(`Unknown chart layout "${kind}".`);
    }
    const module = await import(`./layouts/${kind}.mjs`);
    return module.default(spec);
}

// Measured mode: the chart's box has a CSS size, and the layout runs at its real pixel size so
// text and strokes are drawn 1:1. observe() lays out now and again (debounced) whenever the box
// is resized, handing the new layout to dotnetRef.OnMeasuredLayout; update() swaps the spec.
// A box with no size yet (hidden, collapsed) gives null until it gets one.
const observed = new Map();
const RESIZE_DELAY = 100;

async function measure(state, force) {
    const width = Math.round(state.element.clientWidth);
    const height = Math.round(state.element.clientHeight);
    if (width < 1 || height < 1) {
        state.width = state.height = 0; // lay out again once it has a size, even the old one
        return undefined;
    }
    if (!force && width === state.width && height === state.height) {
        return undefined;
    }
    state.width = width;
    state.height = height;
    return { ...(await layout(state.kind, { ...state.spec, width, height })), width, height };
}

export async function observe(element, kind, spec, dotnetRef) {
    unobserve(element);
    const state = { element, kind, spec, dotnetRef, width: 0, height: 0, timer: 0 };
    state.observer = new ResizeObserver(() => {
        clearTimeout(state.timer);
        state.timer = setTimeout(async () => {
            const result = await measure(state, false);
            if (result && observed.get(element) === state) {
                state.dotnetRef.invokeMethodAsync('OnMeasuredLayout', result);
            }
        }, RESIZE_DELAY);
    });
    state.observer.observe(element);
    observed.set(element, state);
    return (await measure(state, true)) ?? null;
}

export async function update(element, spec) {
    const state = observed.get(element);
    if (!state) {
        return null;
    }
    state.spec = spec;
    return (await measure(state, true)) ?? null;
}

export function unobserve(element) {
    const state = observed.get(element);
    if (state) {
        clearTimeout(state.timer);
        state.observer.disconnect();
        observed.delete(element);
    }
}
