import { geoAlbers, geoEqualEarth, geoEquirectangular, geoMercator, geoNaturalEarth1, geoPath } from '../d3/d3-geo.mjs';
import { finite } from './_shared.mjs';

// Choropleth map. spec: { features: [{ id, name, geometry }], values: { [id]: number },
// projection, color, noDataColor, width, height }. The projection is fitted to the features (to
// the whole globe when there are none) and every feature becomes an SVG path filled on a
// sequential scale from a light tint of `color` (lowest value) to `color` itself (highest);
// features without a finite value get `noDataColor`. Features spanning (nearly) the whole globe
// make a world map: the globe is fitted instead and its outline comes back in `sphere`.

const PROJECTIONS = {
    'natural-earth': geoNaturalEarth1,
    'equal-earth': geoEqualEarth,
    mercator: geoMercator,
    equirectangular: geoEquirectangular,
    albers: geoAlbers,
};

const PAD = 4;

/** The fill of a value at `t` (0 = lowest, 1 = highest) along the scale of `color`. */
export const shade = (color, t) => `color-mix(in oklab, ${color} ${Math.round(15 + 85 * t)}%, var(--color-base-100))`;

export default function choropleth({ features = [], values = {}, projection = 'natural-earth', color = 'var(--color-primary)', noDataColor = 'var(--color-base-300)', width, height }) {
    const shapes = (features ?? []).filter(f => f?.geometry).map(f => ({ type: 'Feature', id: f.id, name: f.name, geometry: f.geometry }));
    const sphere = { type: 'Sphere' };
    const extent = [[PAD, PAD], [width - PAD, height - PAD]];
    const proj = (PROJECTIONS[projection] ?? geoNaturalEarth1)()
        .fitExtent(extent, shapes.length ? { type: 'FeatureCollection', features: shapes } : sphere);
    // Two decimals are plenty in a 640-unit box and keep the paths short.
    const path = geoPath(proj).digits(2);

    // Features that cover (nearly) the whole globe make a world map: fit the globe instead and outline it.
    const [[x0, y0], [x1, y1]] = path.bounds(sphere);
    const world = x1 - x0 <= (width - 2 * PAD) * 1.1 && y1 - y0 <= (height - 2 * PAD) * 1.1;
    if (world) {
        proj.fitExtent(extent, sphere);
    }

    const value = f => {
        const v = values?.[f.id];
        return finite(v) ? v : null;
    };
    const known = shapes.map(value).filter(v => v !== null);
    const min = known.length ? Math.min(...known) : null;
    const max = known.length ? Math.max(...known) : null;
    const t = v => (max > min ? (v - min) / (max - min) : 1);

    return {
        sphere: world ? path(sphere) ?? '' : '',
        min,
        max,
        low: shade(color, 0),
        high: shade(color, 1),
        features: shapes.map(f => {
            const v = value(f);
            return { id: String(f.id ?? ''), name: String(f.name ?? f.id ?? ''), value: v, path: path(f) ?? '', fill: v === null ? noDataColor : shade(color, t(v)) };
        }).filter(f => f.path !== ''),
    };
}
