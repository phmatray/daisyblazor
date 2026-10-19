// Records every layout(kind, spec) call the node chart tests make, with its result, into
// tests/DaisyBlazor.Charts.Server.Tests/layouts.fixture.json. The server-rendering tests replay them
// through the .NET JavaScript engine and expect the same output as the browser engine.
//   node --import ./scripts/record-layouts.mjs --test --test-concurrency=1 "tests/charts-*.test.mjs"
import { register } from 'node:module';

register('data:text/javascript,' + encodeURIComponent(`
export async function resolve(specifier, context, next) {
    const r = await next(specifier, context);
    if (r.url.endsWith('/wwwroot/charts.mjs') && !context.parentURL?.startsWith('data:') && !specifier.includes('?real')) {
        return { url: 'data:text/javascript,' + encodeURIComponent(
            "import { layout as real } from '" + r.url + "?real';" +
            "export async function layout(kind, spec) {" +
            "  const result = await real(kind, spec);" +
            "  (globalThis.__layouts ??= []).push({ kind, spec: JSON.parse(JSON.stringify(spec)), result: JSON.parse(JSON.stringify(result)) });" +
            "  return result; }"), shortCircuit: true };
    }
    return r;
}`));

process.on('exit', async () => {
    const { writeFileSync, readFileSync, existsSync } = await import('node:fs');
    const out = new URL('../tests/DaisyBlazor.Charts.Server.Tests/layouts.fixture.json', import.meta.url);
    const seen = existsSync(out) ? JSON.parse(readFileSync(out, 'utf8')) : [];
    writeFileSync(out, JSON.stringify([...seen, ...(globalThis.__layouts ?? [])]));
});
