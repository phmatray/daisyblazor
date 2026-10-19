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
