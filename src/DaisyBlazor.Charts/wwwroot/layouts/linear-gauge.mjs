import { scaleLinear } from '../d3/d3-scale.mjs';
import { r2 } from './_shared.mjs';

const EMPTY = { track: null, fill: null, bands: [], pointer: null, marker: null, ticks: [], value: 0, fontSize: 12 };

// spec: { min, max, value, bands: [{ from, to }], marker, vertical, width, height }
// Horizontal: pointer and value above the track, ticks below. Vertical: ticks on the left, pointer and
// value on the right, min at the bottom. A vertical gauge is meant to be shown narrow, so its text and
// track are sized from the width. The value and the marker are clamped to [min, max].
export default function linearGauge({ min = 0, max = 100, value, bands, marker, vertical = false, width, height }) {
    if (!Number.isFinite(min) || !Number.isFinite(max) || !(max > min)) {
        return EMPTY;
    }

    const u = vertical ? width / 180 : 1;
    const fs = 12 * u, t = vertical ? 20 * u : Math.min(28, Math.max(8, height * 0.18));
    const clamp = v => Math.min(max, Math.max(min, v));
    const v = clamp(Number.isFinite(value) ? value : min);
    const along = vertical ? height : width, cross = vertical ? width : height;
    const pad = vertical ? fs : 24;
    const pos = scaleLinear().domain([min, max]).range(vertical ? [along - pad, pad] : [pad, along - pad]);

    // Lay the pieces out across the track: pointer side, track, tick side; then centre the stack.
    const pointerSide = 16 * u + (vertical ? 4.2 * fs : fs);
    const tickSide = 12 * u + (vertical ? 3.6 * fs : fs);
    const start = (cross - (pointerSide + t + tickSide)) / 2;
    const c0 = start + (vertical ? tickSide : pointerSide), c1 = c0 + t;
    const toward = vertical ? 1 : -1; // direction of the pointer side across the track
    const edge = vertical ? c1 : c0;  // track edge on the pointer side
    const xy = (s, c) => (vertical ? [c, s] : [s, c]);
    const rect = (sA, sB, cA, cB) => {
        const [xa, ya] = xy(sA, cA), [xb, yb] = xy(sB, cB);
        return { x: Math.min(xa, xb), y: Math.min(ya, yb), width: Math.abs(xb - xa), height: Math.abs(yb - ya) };
    };

    const sv = pos(v);
    const tip = xy(sv, edge + toward * 2 * u), b1 = xy(sv - 6 * u, edge + toward * 12 * u), b2 = xy(sv + 6 * u, edge + toward * 12 * u);
    const nearStart = (sv - Math.min(...pos.range())) < 3 * fs, nearEnd = (Math.max(...pos.range()) - sv) < 3 * fs;
    const [lx, ly] = vertical
        ? [edge + 16 * u, sv + 0.35 * fs]
        : [sv, edge - 16 * u];
    const pointer = {
        path: `M${r2(tip[0])},${r2(tip[1])}L${r2(b1[0])},${r2(b1[1])}L${r2(b2[0])},${r2(b2[1])}Z`,
        labelX: lx,
        labelY: ly,
        anchor: vertical ? 'start' : nearStart ? 'start' : nearEnd ? 'end' : 'middle',
    };

    let outMarker = null;
    if (Number.isFinite(marker)) {
        const sm = pos(clamp(marker)), [x1, y1] = xy(sm, c0 - 4 * u), [x2, y2] = xy(sm, c1 + 4 * u);
        outMarker = { value: clamp(marker), x1, y1, x2, y2 };
    }

    const tickEdge = vertical ? c0 : c1, away = -toward;
    const ticks = pos.ticks(vertical ? 5 : 8).map(tv => {
        const s = pos(tv), [x1, y1] = xy(s, tickEdge + away * 2 * u), [x2, y2] = xy(s, tickEdge + away * 8 * u);
        const [labelX, labelY] = vertical ? [tickEdge - 12 * u, s + 0.35 * fs] : [s, tickEdge + 12 * u + 0.8 * fs];
        return { value: tv, x1, y1, x2, y2, labelX, labelY, anchor: vertical ? 'end' : 'middle' };
    });

    return {
        track: rect(pos(min), pos(max), c0, c1),
        fill: rect(pos(min), sv, c0 + t * 0.3, c0 + t * 0.7),
        bands: (bands ?? [])
            .map((b, index) => ({ index, from: clamp(b?.from), to: clamp(b?.to) }))
            .filter(b => Number.isFinite(b.from) && Number.isFinite(b.to) && b.to > b.from)
            .map(b => ({ ...b, ...rect(pos(b.from), pos(b.to), c0, c1) })),
        pointer,
        marker: outMarker,
        ticks,
        value: v,
        fontSize: fs,
    };
}
