import { scaleQuantize } from '../d3/d3-scale.mjs';
import { utcDay } from '../d3/d3-time.mjs';
import { utcFormat } from '../d3/d3-time-format.mjs';
import { finite, r2 } from './_shared.mjs';
import { parseDate } from './_time.mjs';

const EMPTY = { cells: [], months: [], weekdays: [], thresholds: [], max: 0, cellSize: 0 };
const DAY = 86400000;
const iso = utcFormat('%Y-%m-%d');
const dayLabel = utcFormat('%a, %b %-d, %Y');
const monthLabel = utcFormat('%b');
const weekdayLabel = utcFormat('%a');

// The calendar date of an ISO string ("2026-01-05" or a DateTime), as midnight UTC.
const dayOf = value => {
    const date = parseDate(value);
    return date && utcDay.floor(date);
};

// spec: { days: [{ date, value }], from, to, weekStart (0 = Sunday … 6), levels, width, height,
//         monthNames: [12 names, January first], dayNames: [7 names, Sunday first] }
// One cell per day from `from` to `to` (default: the first and last dated day), weeks as columns
// and weekdays as rows. Values on the same day are summed. Days without a positive value are
// level 0; positive values are quantized into levels 1 … levels-1 between 0 and the largest value.
// Everything is computed in UTC, so the grid never depends on the machine's time zone. The month
// and weekday labels come from monthNames and dayNames (the caller's culture), else English.
export default function calendar({ days, from, to, weekStart = 1, levels = 5, width, height, monthNames, dayNames }) {
    const sums = new Map();
    for (const d of days ?? []) {
        const day = dayOf(d?.date);
        if (day && finite(d.value)) {
            sums.set(+day, (sums.get(+day) ?? 0) + d.value);
        }
    }
    const keys = [...sums.keys()];
    const start = dayOf(from) ?? (keys.length ? new Date(Math.min(...keys)) : null);
    const end = dayOf(to) ?? (keys.length ? new Date(Math.max(...keys)) : null);
    if (!start || !end || end < start) {
        return EMPTY;
    }

    const all = utcDay.range(start, new Date(+end + DAY));
    const offset = d => (d.getUTCDay() - weekStart + 7) % 7;
    const firstWeek = +start - offset(start) * DAY;
    const column = d => Math.floor((+d - firstWeek) / (7 * DAY));
    const columns = column(end) + 1;

    // Weekday names on the left, month names on top; cells are square and as large as both fit.
    const left = 30, top = 16;
    const cell = Math.max(1, Math.min((width - left - 2) / columns, (height - top - 2) / 7));
    const gap = Math.min(2, cell * 0.15);

    const max = Math.max(0, ...all.map(d => sums.get(+d) ?? 0));
    const steps = Math.max(2, Math.round(levels));
    const level = scaleQuantize().domain([0, max > 0 ? max : 1]).range(Array.from({ length: steps - 1 }, (_, i) => i + 1));
    const levelOf = v => (v > 0 ? level(v) : 0);

    // A month is named over the week holding its first day (or the first shown day); a name too
    // close to the next one is dropped so labels never overlap.
    const months = all.filter((d, i) => i === 0 || d.getUTCDate() === 1)
        .map(d => ({ label: monthNames?.[d.getUTCMonth()] ?? monthLabel(d), x: left + column(d) * cell }))
        .filter((m, i, list) => i === list.length - 1 || list[i + 1].x - m.x >= 26)
        .map(m => ({ pos: r2(m.x), label: m.label }));

    return {
        cells: all.map(d => {
            const value = sums.get(+d) ?? 0;
            return {
                date: iso(d), label: dayLabel(d), value, level: levelOf(value),
                x: r2(left + column(d) * cell), y: r2(top + offset(d) * cell), size: r2(cell - gap),
            };
        }),
        months,
        // Monday, Wednesday and Friday are named, wherever the week starts.
        weekdays: [1, 3, 5].map(w => {
            const sample = new Date(Date.UTC(2024, 0, 7 + w)); // 2024-01-07 is a Sunday
            return { pos: r2(top + offset(sample) * cell + (cell - gap) / 2), label: dayNames?.[w] ?? weekdayLabel(sample) };
        }),
        thresholds: level.thresholds().map(r2),
        max,
        cellSize: r2(cell - gap),
    };
}
