import { useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import { createCalendarEvent, getCalendar, type CalendarItem } from "../api/calendar";

type ViewMode = "month" | "week" | "day";

function startOfMonth(d: Date) {
  return new Date(d.getFullYear(), d.getMonth(), 1);
}
function startOfCalendarGrid(d: Date) {
  const start = startOfMonth(d);
  const day = start.getDay();
  const result = new Date(start);
  result.setDate(start.getDate() - day);
  return result;
}
function startOfWeek(d: Date) {
  const result = new Date(d.getFullYear(), d.getMonth(), d.getDate());
  result.setDate(result.getDate() - result.getDay());
  return result;
}
function startOfDay(d: Date) {
  return new Date(d.getFullYear(), d.getMonth(), d.getDate());
}
function addDays(d: Date, n: number) {
  const result = new Date(d);
  result.setDate(result.getDate() + n);
  return result;
}
function sameDay(a: Date, b: Date) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}
function timeLabel(iso: string, isAllDay: boolean) {
  return isAllDay ? "All day" : new Date(iso).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" });
}

export default function CalendarPage() {
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();

  const [viewMode, setViewMode] = useState<ViewMode>("month");
  const [cursor, setCursor] = useState(() => new Date());
  const [newEventTitle, setNewEventTitle] = useState("");
  const [newEventDate, setNewEventDate] = useState("");

  const monthStart = startOfMonth(cursor);
  const gridStart = startOfCalendarGrid(cursor);
  const monthDays = useMemo(() => Array.from({ length: 42 }, (_, i) => addDays(gridStart, i)), [gridStart]);

  const weekStart = startOfWeek(cursor);
  const weekDays = useMemo(() => Array.from({ length: 7 }, (_, i) => addDays(weekStart, i)), [weekStart]);

  const rangeStart = viewMode === "month" ? monthDays[0] : viewMode === "week" ? weekDays[0] : startOfDay(cursor);
  const rangeEndExclusive =
    viewMode === "month" ? addDays(monthDays[41], 1) : viewMode === "week" ? addDays(weekDays[6], 1) : addDays(startOfDay(cursor), 1);

  const fromUtc = rangeStart.toISOString();
  const toUtc = rangeEndExclusive.toISOString();

  const { data: items } = useQuery({
    queryKey: ["calendar", householdId, fromUtc, toUtc],
    queryFn: () => getCalendar(householdId, fromUtc, toUtc),
    enabled: !!householdId,
  });

  const createEventMutation = useMutation({
    mutationFn: createCalendarEvent,
    onSuccess: () => {
      setNewEventTitle("");
      setNewEventDate("");
      queryClient.invalidateQueries({ queryKey: ["calendar", householdId] });
    },
  });

  function handleAddEvent(e: FormEvent) {
    e.preventDefault();
    if (!newEventTitle.trim() || !newEventDate || !householdId) return;
    const start = new Date(newEventDate);
    const end = new Date(start.getTime() + 60 * 60 * 1000);
    createEventMutation.mutate({
      householdId,
      title: newEventTitle,
      startUtc: start.toISOString(),
      endUtc: end.toISOString(),
      isAllDay: false,
    });
  }

  const itemsFor = (day: Date): CalendarItem[] =>
    (items ?? [])
      .filter((item) => sameDay(new Date(item.startUtc), day))
      .sort((a, b) => new Date(a.startUtc).getTime() - new Date(b.startUtc).getTime());

  function goPrev() {
    if (viewMode === "month") setCursor(new Date(cursor.getFullYear(), cursor.getMonth() - 1, 1));
    else if (viewMode === "week") setCursor(addDays(cursor, -7));
    else setCursor(addDays(cursor, -1));
  }
  function goNext() {
    if (viewMode === "month") setCursor(new Date(cursor.getFullYear(), cursor.getMonth() + 1, 1));
    else if (viewMode === "week") setCursor(addDays(cursor, 7));
    else setCursor(addDays(cursor, 1));
  }

  const rangeLabel =
    viewMode === "month"
      ? monthStart.toLocaleDateString(undefined, { month: "long", year: "numeric" })
      : viewMode === "week"
        ? `${weekDays[0].toLocaleDateString(undefined, { month: "short", day: "numeric" })} – ${weekDays[6].toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" })}`
        : cursor.toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric", year: "numeric" });

  return (
    <div>
      <h1>Calendar</h1>
      <p className="dek">Events and task due dates, together — a task due in this range shows up here automatically.</p>

      <div className="calendar-toolbar">
        <div className="view-switch">
          {(["month", "week", "day"] as ViewMode[]).map((mode) => (
            <button
              key={mode}
              type="button"
              className={`view-switch-btn${viewMode === mode ? " active" : ""}`}
              onClick={() => setViewMode(mode)}
            >
              {mode === "month" ? "Month" : mode === "week" ? "Week" : "Day"}
            </button>
          ))}
        </div>

        <button type="button" className="link-button" onClick={goPrev}>← Prev</button>
        <span className="calendar-month-label">{rangeLabel}</span>
        <button type="button" className="link-button" onClick={goNext}>Next →</button>

        <form className="calendar-add-form" onSubmit={handleAddEvent}>
          <input placeholder="Event title" value={newEventTitle} onChange={(e) => setNewEventTitle(e.target.value)} />
          <input type="date" value={newEventDate} onChange={(e) => setNewEventDate(e.target.value)} />
          <button type="submit" disabled={createEventMutation.isPending}>Add event</button>
        </form>
      </div>

      {viewMode === "month" && (
        <div className="calendar-scroll">
          <div className="calendar-weekdays">
            {["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].map((d) => <div key={d}>{d}</div>)}
          </div>

          <div className="calendar-grid">
            {monthDays.map((day) => (
              <div key={day.toISOString()} className={`calendar-cell${day.getMonth() !== monthStart.getMonth() ? " outside-month" : ""}${sameDay(day, new Date()) ? " today" : ""}`}>
                <div className="calendar-cell-date">{day.getDate()}</div>
                <div className="calendar-cell-items">
                  {itemsFor(day).map((item) => (
                    <div key={item.id} className={`calendar-item calendar-item-${item.kind}`} title={item.title}>
                      {item.title}
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {viewMode === "week" && (
        <div className="calendar-scroll">
          <div className="week-agenda">
            {weekDays.map((day) => (
              <div key={day.toISOString()} className={`week-agenda-col${sameDay(day, new Date()) ? " today" : ""}`}>
                <div className="week-agenda-head">
                  {day.toLocaleDateString(undefined, { weekday: "short" })}
                  <span className="week-agenda-date">{day.getDate()}</span>
                </div>
                <div className="week-agenda-items">
                  {itemsFor(day).length === 0 && <p className="empty">—</p>}
                  {itemsFor(day).map((item) => (
                    <div key={item.id} className={`calendar-item calendar-item-${item.kind} week-agenda-item`} title={item.title}>
                      <span className="week-agenda-time">{timeLabel(item.startUtc, item.isAllDay)}</span>
                      {item.title}
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {viewMode === "day" && (
        <div className="day-agenda">
          {itemsFor(cursor).length === 0 && <p className="empty">Nothing scheduled for this day.</p>}
          {itemsFor(cursor).map((item) => (
            <div key={item.id} className="day-agenda-row">
              <span className="day-agenda-time">{timeLabel(item.startUtc, item.isAllDay)}</span>
              <span className={`calendar-item calendar-item-${item.kind} day-agenda-item`}>{item.title}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
