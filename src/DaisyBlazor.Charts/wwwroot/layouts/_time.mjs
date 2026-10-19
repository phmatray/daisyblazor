// Shared helpers for the "over time" layouts. The leading underscore keeps this file out of
// charts.mjs's layout names, so it can only be imported by the layouts themselves.
import { extent } from '../d3/d3-array.mjs';
import { scaleUtc, scalePoint } from '../d3/d3-scale.mjs';
import { utcFormat } from '../d3/d3-time-format.mjs';
import { maxWidth, r2, stride, ticks } from './_shared.mjs';

// .NET sends DateTime without an offset ("2026-10-05T00:00:00"), which JavaScript would read as
// local time. Read every date as UTC so the chart never depends on the machine's time zone.
export function parseDate(value) {
    if (typeof value !== 'string' || value === '') {
        return null;
    }
    const iso = /T/.test(value) && !/(Z|[+-]\d\d:?\d\d)$/.test(value) ? `${value}Z` : value;
    const date = new Date(iso);
    return Number.isNaN(date.getTime()) ? null : date;
}

// The left margin a y axis needs for the labels of about `count` ticks of a linear scale.
export const yAxisWidth = (scale, count, format) => maxWidth(ticks(scale, count, format).map(t => t.label)) + 8;

// An x scale over either dates (a UTC time scale) or category labels (a point scale), with tick
// labels thinned so neighbours never touch. Dated data should be sorted (see byDate). `dates` and `labels` line up with the data by index.
export function xScale({ dates, labels, count, x0, x1, pad = 0 }) {
    const parsed = (dates ?? []).map(parseDate);
    if (parsed.length >= count && count > 0 && parsed.slice(0, count).every(d => d)) {
        const [first, last] = extent(parsed.slice(0, count));
        const scale = scaleUtc().domain([first, +first === +last ? new Date(+first + 86400000) : last]).range([x0 + pad, x1 - pad]);
        const ticks = timeTicks(scale, parsed.slice(0, count));
        return { at: i => scale(parsed[i]), ticks, label: i => labelOf(parsed[i]), time: true };
    }

    const names = Array.from({ length: count }, (_, i) => String(labels?.[i] ?? i + 1));
    const scale = scalePoint().domain(names.map((_, i) => i)).range([x0 + pad, x1 - pad]);
    const every = stride(names, scale.step(), 10);
    const shown = names.flatMap((n, i) => (i % every === 0 ? [{ pos: r2(scale(i)), label: n }] : []));
    return { at: i => scale(i), ticks: shown, label: i => names[i], time: false };
}

// Labels for time ticks: a time of day, else a day, else a month, else a year.
const tickTime = utcFormat('%H:%M');
const tickDay = utcFormat('%b %-d');
const tickMonth = utcFormat('%b');
const tickYear = utcFormat('%Y');
const tickLabel = d => (d.getUTCHours() || d.getUTCMinutes() ? tickTime(d)
    : d.getUTCDate() !== 1 ? tickDay(d) : d.getUTCMonth() !== 0 ? tickMonth(d) : tickYear(d));

// Ticks for a time scale: about one per 90 units, never more than there are data points, and
// only at midnight when every data point is at midnight (daily data gets no hour ticks).
export function timeTicks(scale, dates) {
    const [x0, x1] = scale.range();
    const daily = dates.every(d => d.getUTCHours() === 0 && d.getUTCMinutes() === 0);
    const count = Math.max(2, Math.min(Math.floor(Math.abs(x1 - x0) / 90), dates.length));
    return scale.ticks(count)
        .filter(d => !daily || (d.getUTCHours() === 0 && d.getUTCMinutes() === 0))
        .map(d => ({ pos: r2(scale(d)), label: tickLabel(d) }));
}

// The date shown in tooltips: the day, plus the time when it is not midnight.
const dayLabel = utcFormat('%b %-d, %Y');
const timeLabel = utcFormat('%b %-d, %Y %H:%M');

export function labelOf(date) {
    if (!date) {
        return '';
    }
    return date.getUTCHours() === 0 && date.getUTCMinutes() === 0 ? dayLabel(date) : timeLabel(date);
}

// Sorts items by their parsed `date` when every item has one; otherwise keeps the given order.
export function byDate(items, key = 'date') {
    const dated = items.map(item => ({ item, at: parseDate(item[key]) }));
    return dated.every(d => d.at) ? dated.sort((a, b) => a.at - b.at).map(d => d.item) : items;
}
