import { scaleUtc, scaleBand } from '../d3/d3-scale.mjs';
import { fit, maxWidth, r2 } from './_shared.mjs';
import { parseDate, labelOf, tickLabel, timeTickDates } from './_time.mjs';

const EMPTY = { rows: [], groups: [], today: null, xTicks: [], plot: { left: 0, top: 0, right: 0, bottom: 0 } };

// spec: { tasks: [{ name, start, end, group, progress, row, color }], today, width, height, headers }
// Tasks need a name and a valid start; a missing or earlier end makes a milestone (start = end).
// Tasks with the same `row` key share one row (a lane) labelled with that key; other tasks get a row
// each. With `headers` (the default), a row whose tasks all share a group is listed under a header
// row for that group, groups in order of first appearance; other rows come first. Without headers
// rows keep their order and groups only colour the bars. Progress is a fraction (0..1) and is clamped.
// Each task row carries `index`, its position in `tasks`, and every x tick its date (`value`, in
// epoch milliseconds), so .NET can write the dates in the reader's culture.
// `from` / `to` pin the ends of the time axis (otherwise the tasks' span): bars are clipped to it, and
// tasks wholly outside it are left out (GanttChart counts its rows from the same tasks).
export default function gantt({ tasks, today, width, height, headers = true, from = null, to = null }) {
    const lo = parseDate(from);
    const hi = parseDate(to);
    const valid = (tasks ?? [])
        .map((t, index) => {
            const start = parseDate(t?.start);
            const end = parseDate(t?.end);
            return { ...t, index, start, end: end && start && end > start ? end : start };
        })
        .filter(t => t.name && t.start && (!lo || +t.end >= +lo) && (!hi || +t.start <= +hi));
    if (valid.length === 0) {
        return EMPTY;
    }

    const groups = [...new Set(valid.map(t => t.group).filter(g => g))];
    const lanes = new Map();
    for (const t of valid) {
        const key = t.row ? `row:${t.row}` : t.index;
        if (!lanes.has(key)) {
            lanes.set(key, { label: t.row || t.name, tasks: [] });
        }
        lanes.get(key).tasks.push(t);
    }
    const laneList = [...lanes.values()].map(l => {
        const g = l.tasks[0].group;
        return { ...l, group: headers && g && l.tasks.every(t => t.group === g) ? g : null };
    });
    const rows = [
        ...laneList.filter(l => !l.group),
        ...groups.flatMap(g => {
            const of = laneList.filter(l => l.group === g);
            return of.length ? [{ header: g }, ...of] : [];
        }),
    ];

    const widest = maxWidth(rows.map(r => r.header ?? r.label));
    const labelRoom = Math.min(widest, width * 0.35); // rows under a header are indented 8 more units
    const plot = { left: labelRoom + 20, top: 18, right: width - 24, bottom: height - 24 };

    const first = lo ?? new Date(Math.min(...valid.map(t => t.start)));
    const last = hi ?? new Date(Math.max(...valid.map(t => t.end)));
    const x = scaleUtc()
        .domain([first, +last > +first ? last : new Date(+first + 86400000)])
        .range([plot.left, plot.right])
        .clamp(Boolean(lo || hi));
    const band = scaleBand().domain(rows.map((_, i) => i)).range([plot.top, plot.bottom]).paddingInner(0.3).paddingOuter(0.15);
    const now = parseDate(today);

    return {
        // One entry per header and per task; the tasks of a lane follow each other and share its y.
        rows: rows.flatMap((r, i) => {
            const y = r2(band(i));
            const h = r2(band.bandwidth());
            if (r.header) {
                return [{ header: true, name: r.header, label: fit(r.header, labelRoom), group: groups.indexOf(r.header), y, h }];
            }
            const label = fit(r.label, labelRoom);
            return r.tasks.map(t => {
                const x0 = x(t.start);
                const x1 = Math.max(x(t.end), x0 + 2);
                const progress = typeof t.progress === 'number' && Number.isFinite(t.progress) ? Math.min(1, Math.max(0, t.progress)) : null;
                return {
                    header: false, name: t.name, label, group: t.group ? groups.indexOf(t.group) : -1,
                    milestone: +t.start === +t.end, start: labelOf(t.start), end: labelOf(t.end), progress,
                    x0: r2(x0), x1: r2(x1), y, h, color: t.color || null, indent: r.group != null, index: t.index,
                };
            });
        }),
        groups,
        today: now && now >= x.domain()[0] && now <= x.domain()[1] ? { pos: r2(x(now)), label: labelOf(now) } : null,
        // Every task boundary counts as a data point, so a long plan still gets enough ticks.
        xTicks: timeTickDates(x, valid.flatMap(t => [t.start, t.end])).map(d => ({ pos: r2(x(d)), label: tickLabel(d), value: +d })),
        plot,
    };
}
