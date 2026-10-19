import { scaleUtc, scaleBand } from '../d3.mjs';
import { fit, maxWidth, r2 } from './_shared.mjs';
import { parseDate, labelOf, timeTicks } from './_time.mjs';

const EMPTY = { rows: [], groups: [], today: null, xTicks: [], plot: { left: 0, top: 0, right: 0, bottom: 0 } };

// spec: { tasks: [{ name, start, end, group, progress }], today, width, height }
// Tasks need a name and a valid start; a missing or earlier end makes a milestone (start = end).
// Grouped tasks are listed under a header row per group, groups in order of first appearance;
// tasks without a group come first. Progress is a fraction (0..1) and is clamped.
export default function gantt({ tasks, today, width, height }) {
    const valid = (tasks ?? [])
        .map(t => {
            const start = parseDate(t?.start);
            const end = parseDate(t?.end);
            return { ...t, start, end: end && start && end > start ? end : start };
        })
        .filter(t => t.name && t.start);
    if (valid.length === 0) {
        return EMPTY;
    }

    const groups = [...new Set(valid.map(t => t.group).filter(g => g))];
    const rows = [
        ...valid.filter(t => !t.group).map(t => ({ task: t })),
        ...groups.flatMap(g => [{ header: g }, ...valid.filter(t => t.group === g).map(t => ({ task: t }))]),
    ];

    const widest = maxWidth(rows.map(r => r.header ?? r.task.name));
    const labelRoom = Math.min(widest, width * 0.35); // grouped tasks are indented 8 more units
    const plot = { left: labelRoom + 20, top: 18, right: width - 24, bottom: height - 24 };

    const first = new Date(Math.min(...valid.map(t => t.start)));
    const last = new Date(Math.max(...valid.map(t => t.end)));
    const x = scaleUtc()
        .domain([first, +last > +first ? last : new Date(+first + 86400000)])
        .range([plot.left, plot.right]);
    const band = scaleBand().domain(rows.map((_, i) => i)).range([plot.top, plot.bottom]).paddingInner(0.3).paddingOuter(0.15);
    const now = parseDate(today);

    return {
        rows: rows.map((r, i) => {
            const y = r2(band(i));
            const h = r2(band.bandwidth());
            if (r.header) {
                return { header: true, name: r.header, label: fit(r.header, labelRoom), group: groups.indexOf(r.header), y, h };
            }
            const t = r.task;
            const x0 = x(t.start);
            const x1 = Math.max(x(t.end), x0 + 2);
            const progress = typeof t.progress === 'number' && Number.isFinite(t.progress) ? Math.min(1, Math.max(0, t.progress)) : null;
            return {
                header: false, name: t.name, label: fit(t.name, labelRoom), group: t.group ? groups.indexOf(t.group) : -1,
                milestone: +t.start === +t.end, start: labelOf(t.start), end: labelOf(t.end), progress,
                x0: r2(x0), x1: r2(x1), y, h,
            };
        }),
        groups,
        today: now && now >= x.domain()[0] && now <= x.domain()[1] ? { pos: r2(x(now)), label: labelOf(now) } : null,
        // Every task boundary counts as a data point, so a long plan still gets enough ticks.
        xTicks: timeTicks(x, valid.flatMap(t => [t.start, t.end])),
        plot,
    };
}
