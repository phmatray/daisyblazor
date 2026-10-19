import { scaleLinear } from '../d3/d3-scale.mjs';
import { dodge, r2 } from './_shared.mjs';

// spec: { stages: [{ label, value }], width, height, shape: 'funnel' | 'pyramid' | 'rectangle', gap }
// funnel: equal-height stages, each as wide as its value and narrowing to the next one.
// pyramid: a triangle cut into stages whose heights follow their values, the first stage at the base.
// rectangle: equal-height stages, each a centred rectangle as wide as its value (no slanted sides).
export default function funnel({ stages, width, height, shape = 'funnel', gap = 4 }) {
    const valid = (stages ?? []).map((s, index) => ({ ...s, index })).filter(s => Number.isFinite(s.value) && s.value >= 0);
    const max = Math.max(0, ...valid.map(s => s.value));
    if (valid.length === 0 || max === 0) {
        return { stages: [] };
    }

    // Names go in the left gutter, value and conversion in the right one.
    const left = 150, right = width - 150, mid = (left + right) / 2, top = 4, bottom = height - 4;
    const n = valid.length;
    const conversion = i => (i > 0 && valid[i - 1].value > 0 ? valid[i].value / valid[i - 1].value : null);
    const share = s => (valid[0].value > 0 ? s.value / valid[0].value : 0);

    let out;
    if (shape === 'pyramid') {
        const total = valid.reduce((sum, d) => sum + d.value, 0);
        const usable = bottom - top - gap * (n - 1);
        const half = y => ((y - top) / (bottom - top)) * (right - left) / 2;
        let y1 = bottom;
        out = valid.map((s, i) => {
            const y0 = y1 - (s.value / total) * usable;
            const stage = { index: s.index, label: s.label, value: s.value, conversion: conversion(i), share: share(s), y0, y1,
                path: poly([[mid - half(y0), y0], [mid + half(y0), y0], [mid + half(y1), y1], [mid - half(y1), y1]]) };
            y1 = y0 - gap;
            return stage;
        });
    } else {
        const w = scaleLinear().domain([0, max]).range([0, right - left]);
        const h = (bottom - top - gap * (n - 1)) / n;
        out = valid.map((s, i) => {
            const y0 = top + i * (h + gap), y1 = y0 + h;
            const a = Math.max(2, w(s.value)) / 2, b = shape === 'rectangle' ? a : Math.max(2, w((valid[i + 1] ?? s).value)) / 2;
            return { index: s.index, label: s.label, value: s.value, conversion: conversion(i), share: share(s), y0, y1,
                path: poly([[mid - a, y0], [mid + a, y0], [mid + b, y1], [mid - b, y1]]) };
        });
    }

    // Each label pair takes two lines; labels move apart where stages are thinner than that.
    const ys = dodge(out.map(s => (s.y0 + s.y1) / 2), 34, top + 14, bottom - 18);
    return { stages: out.map((s, i) => ({ ...s, labelY: ys[i], nameX: left - 10, valueX: right + 10 })) };
}

const poly = points => 'M' + points.map(([x, y]) => `${r2(x)},${r2(y)}`).join('L') + 'Z';
