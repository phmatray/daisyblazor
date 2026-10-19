import { arc as d3Arc, scaleLinear } from '../d3.mjs';

const EMPTY = { cx: 0, cy: 0, radius: 0, track: '', fill: '', bands: [], marker: null, ticks: [], value: 0, fraction: 0, fontSize: 12, valueFontSize: 12, valueY: 0 };
const RAD = Math.PI / 180;

// spec: { min, max, value, bands: [{ from, to }], marker, startAngle, endAngle, thickness, width, height }
// Angles are in degrees, 0 at 12 o'clock, clockwise. Paths are relative to (cx, cy). The value and
// the marker are clamped to [min, max]; bands are clipped to it.
export default function gauge({ min = 0, max = 100, value, bands, marker, startAngle = -120, endAngle = 120, thickness = 0.2, width, height }) {
    if (!Number.isFinite(min) || !Number.isFinite(max) || !(max > min) || !Number.isFinite(startAngle) || !Number.isFinite(endAngle) || startAngle === endAngle) {
        return EMPTY;
    }

    const clamp = v => Math.min(max, Math.max(min, v));
    const v = clamp(Number.isFinite(value) ? value : min);
    const a0 = startAngle * RAD, a1 = endAngle * RAD;
    const angle = scaleLinear().domain([min, max]).range([a0, a1]);
    const fontSize = Math.max(12, Math.min(width, height) / 28);

    // Fit the arc (and the value text under its centre) in the box, leaving room for the band ring and tick labels.
    const unit = [[0, 0], [-0.5, 0.2], [0.5, 0.2]];
    for (const a of [a0, a1, ...[-2, -1.5, -1, -0.5, 0, 0.5, 1, 1.5, 2].map(k => k * Math.PI).filter(a => a > Math.min(a0, a1) && a < Math.max(a0, a1))]) {
        unit.push([Math.sin(a), -Math.cos(a)]);
    }
    const xs = unit.map(p => p[0]), ys = unit.map(p => p[1]);
    const [x0, x1, y0, y1] = [Math.min(...xs), Math.max(...xs), Math.min(...ys), Math.max(...ys)];
    const padX = 16 + fontSize * 2.4, padY = 16 + fontSize * 1.4;
    const radius = Math.max(10, Math.min((width - 2 * padX) / (x1 - x0), (height - 2 * padY) / (y1 - y0)));
    const cx = width / 2 - radius * (x0 + x1) / 2, cy = height / 2 - radius * (y0 + y1) / 2;
    const inner = radius * (1 - Math.min(0.9, Math.max(0.02, thickness)));

    const arc = d3Arc();
    const ring = (r0, r1, from, to) => arc({ innerRadius: r0, outerRadius: r1, startAngle: angle(from), endAngle: angle(to) });
    const polar = (a, r) => [cx + r * Math.sin(a), cy - r * Math.cos(a)];

    const outBands = (bands ?? [])
        .map((b, index) => ({ index, from: clamp(b?.from), to: clamp(b?.to) }))
        .filter(b => Number.isFinite(b.from) && Number.isFinite(b.to) && b.to > b.from)
        .map(b => ({ ...b, path: ring(radius + 4, radius + 10, b.from, b.to) }));

    let outMarker = null;
    if (Number.isFinite(marker)) {
        const a = angle(clamp(marker));
        const [mx1, my1] = polar(a, inner - 4), [mx2, my2] = polar(a, radius + 12);
        outMarker = { value: clamp(marker), x1: mx1, y1: my1, x2: mx2, y2: my2 };
    }

    const ticks = angle.ticks(Math.max(2, Math.round(Math.abs(endAngle - startAngle) / 50))).map(t => {
        const a = angle(t), [x, y] = polar(a, radius + 16 + fontSize * 0.6), s = Math.sin(a);
        return { value: t, x, y, anchor: s > 0.3 ? 'start' : s < -0.3 ? 'end' : 'middle' };
    });

    return {
        cx, cy, radius,
        track: ring(inner, radius, min, max),
        fill: v > min ? ring(inner, radius, min, v) : '',
        bands: outBands,
        marker: outMarker,
        ticks,
        value: v,
        fraction: (v - min) / (max - min),
        fontSize,
        valueFontSize: radius * 0.28,
        valueY: cy + radius * 0.1,
    };
}
