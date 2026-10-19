// Prerender the published docs site for search engines and link previews.
//
//   node website/scripts/prerender.mjs <publish/wwwroot> <base path, e.g. /daisyblazor/> <site url>
//
// Boots the WebAssembly app in Chromium, follows every internal link from the home page and saves
// each page as static HTML (charts.html, components/barchart.html…) with its own title, description,
// canonical URL and rendered content. GitHub Pages serves charts.html for /charts with a 200, where
// the SPA fallback answered every deep link with 404.html. Blazor replaces the saved content when it
// starts. Also writes 404.html (the plain shell, so unknown links still boot the app), sitemap.xml
// and robots.txt.
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { chromium } from 'playwright';

const [root, basePath = '/', siteUrl = 'https://phmatray.github.io/daisyblazor/'] = process.argv.slice(2);
if (!root) {
    console.error('usage: prerender.mjs <wwwroot> [base path] [site url]');
    process.exit(2);
}

const shell = fs.readFileSync(path.join(root, 'index.html'), 'utf8');
const types = { '.html': 'text/html', '.js': 'text/javascript', '.mjs': 'text/javascript', '.css': 'text/css', '.json': 'application/json', '.wasm': 'application/wasm', '.svg': 'image/svg+xml', '.png': 'image/png' };

// Serve the publish output at "/" with the SPA fallback, as the dev server does.
const server = http.createServer((req, res) => {
    let file = path.join(root, decodeURIComponent(req.url.split('?')[0]));
    if (!file.startsWith(root) || !fs.existsSync(file) || fs.statSync(file).isDirectory()) {
        res.writeHead(200, { 'content-type': 'text/html' });
        res.end(shell);
        return;
    }
    res.writeHead(200, { 'content-type': types[path.extname(file)] ?? 'application/octet-stream' });
    fs.createReadStream(file).pipe(res);
});
await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const origin = `http://127.0.0.1:${server.address().port}`;

const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });
const errors = [];
page.on('pageerror', e => errors.push(e.message));

await page.goto(origin + '/', { waitUntil: 'networkidle' });

// Breadth-first over internal links, moving with Blazor's router so the app boots only once.
const seen = new Set(['/']);
const queue = ['/'];
const saved = [];
const broken = [];
const from = new Map();
while (queue.length > 0) {
    const route = queue.shift();
    if (route !== '/') {
        await page.evaluate(r => window.Blazor.navigateTo(r), route);
    }
    // The page is ready once its canonical link names it and no chart is still laying out.
    await page.waitForFunction(r => {
        const canonical = document.querySelector('link[rel="canonical"]')?.getAttribute('href') ?? '';
        const missing = document.querySelector('meta[name="robots"][content="noindex"]');
        return location.pathname === r && document.querySelector('h1') && (missing || canonical.endsWith(r) || canonical.endsWith(r + '/'));
    }, route, { timeout: 30000 }).catch(e => {
        throw new Error(`${route} did not render`, { cause: e });
    });
    if (await page.locator('meta[name="robots"][content="noindex"]').count() > 0) {
        broken.push(`${route} (linked from ${from.get(route)})`);
        continue;
    }
    await page.waitForFunction(() => !document.querySelector('[aria-busy="true"]'), null, { timeout: 15000 }).catch(() => {});
    await page.waitForTimeout(150);

    const { html, links } = await page.evaluate(site => {
        const doc = document.documentElement.cloneNode(true);
        // The visible breadcrumb, as structured data for search results.
        const crumbs = [...document.querySelectorAll('nav[aria-label="Breadcrumb"] a, nav[aria-label="Breadcrumb"] [aria-current="page"]')];
        if (crumbs.length > 0) {
            const canonical = document.querySelector('link[rel="canonical"]')?.getAttribute('href');
            const ld = doc.ownerDocument.createElement('script');
            ld.type = 'application/ld+json';
            ld.textContent = JSON.stringify({
                '@context': 'https://schema.org', '@type': 'BreadcrumbList',
                itemListElement: crumbs.map((el, i) => ({
                    '@type': 'ListItem', position: i + 1, name: el.textContent.trim(),
                    item: el.href ? site + new URL(el.href).pathname.slice(1) + new URL(el.href).hash : canonical,
                })),
            });
            doc.querySelector('head').append(ld);
        }
        // Head tags from <HeadContent> are re-added by Blazor when it starts: mark them so the shell drops the baked copies first.
        doc.querySelectorAll('head meta[name="description"], head link[rel="canonical"], head meta[property^="og:"], head meta[name^="twitter:"], head meta[name="robots"]')
            .forEach(el => el.setAttribute('data-prerendered', ''));
        doc.querySelector('#shell-description')?.remove();
        // Blazor's render markers inside <title> would read as text once saved; the loading progress is runtime state.
        doc.querySelector('head title').textContent = document.title;
        doc.removeAttribute('style');
        for (const node of [...doc.querySelector('head').childNodes]) {
            if (node.nodeType === Node.COMMENT_NODE) node.remove();
        }
        const links = [...document.querySelectorAll('a[href]')]
            .map(a => new URL(a.href, location.href))
            .filter(u => u.origin === location.origin && !/\.[a-z0-9]+$/i.test(u.pathname))
            .map(u => u.pathname);
        return { html: '<!DOCTYPE html>\n' + doc.outerHTML, links };
    }, siteUrl);

    const file = route === '/' ? 'index.html' : route.slice(1).replace(/\/$/, '') + '.html';
    fs.mkdirSync(path.dirname(path.join(root, file)), { recursive: true });
    fs.writeFileSync(path.join(root, file), html.replace(/<base href="\/">/, `<base href="${basePath}">`));
    // A stale precompressed copy would win over the new file on servers that serve them.
    for (const ext of ['.br', '.gz']) fs.rmSync(path.join(root, file + ext), { force: true });
    saved.push(route);

    for (const link of links) {
        if (!seen.has(link) && link !== '/not-found') {
            seen.add(link);
            from.set(link, route);
            queue.push(link);
        }
    }
}

await browser.close();
server.close();

// A page whose path is also a folder (components, with components/barchart…) must be the folder's
// index.html: GitHub Pages redirects /components to /components/ and never looks at components.html.
const pages = saved.map(r => {
    const name = r.slice(1).replace(/\/$/, '');
    if (r === '/' || !fs.existsSync(path.join(root, name)) || !fs.statSync(path.join(root, name)).isDirectory()) return r;
    fs.renameSync(path.join(root, name + '.html'), path.join(root, name, 'index.html'));
    return r + '/';
});

if (broken.length > 0) {
    console.error(`broken internal links:\n${broken.join('\n')}`);
    process.exit(1);
}
if (errors.length > 0) {
    console.error(`page errors while prerendering:\n${[...new Set(errors)].join('\n')}`);
    process.exit(1);
}

fs.writeFileSync(path.join(root, '404.html'), shell
    .replace('<base href="/" />', `<base href="${basePath}" />`)
    .replace(/<title>[^<]*<\/title>/, '<title>Not found · DaisyBlazor</title>')
    .replace('<meta charset="utf-8" />', '<meta charset="utf-8" />\n    <meta name="robots" content="noindex" data-prerendered />'));
const urls = pages.map(r => siteUrl + (r === '/' ? '' : r.slice(1)));
fs.writeFileSync(path.join(root, 'sitemap.xml'),
    '<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n'
    + urls.map(u => `  <url><loc>${u}</loc><lastmod>${new Date().toISOString().slice(0, 10)}</lastmod></url>`).join('\n') + '\n</urlset>\n');
fs.writeFileSync(path.join(root, 'robots.txt'), `User-agent: *\nAllow: /\n\nSitemap: ${siteUrl}sitemap.xml\n`);
console.log(`prerendered ${saved.length} pages`);
