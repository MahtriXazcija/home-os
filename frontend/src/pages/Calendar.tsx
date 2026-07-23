import { useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import { createCalendarEvent, getCalendar, type CalendarItem } from "../api/calendar";

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
function sameDay(a: Date, b: Date) {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
}

export default function CalendarPage() {
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();

  const [cursor, setCursor] = useState(() => new Date());
  const [newEventTitle, setNewEventTitle] = useState("");
  const [newEventDate, setNewEventDate] = useState("");

  const monthStart = startOfMonth(cursor);
  const gridStart = startOfCalendarGrid(cursor);
  const days = useMemo(() => Array.from({ length: 42 }, (_, i) => {
    const d = new Date(gridStart);
    d.setDate(gridStart.getDate() + i);
    return d;
  }), [gridStart]);

  const fromUtc = days[0].toISOString();
  const toUtc = new Date(days[41].getTime() + 24 * 60 * 60 * 1000).toISOString();

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
    (items ?? []).filter((item) => sameDay(new Date(item.startUtc), day));

  return (
    <div>
      <h1>Calendar</h1>
      <p className="dek">Events and task due dates, together — a task due this month shows up here automatically.</p>

      <div className="calendar-toolbar">
        <button type="button" className="link-button" onClick={() => setCursor(new Date(cursor.getFullYear(), cursor.getMonth() - 1, 1))}>← Prev</button>
        <span className="calendar-month-label">{monthStart.toLocaleDateString(undefined, { month: "long", year: "numeric" })}</span>
        <button type="button" className="link-button" onClick={() => setCursor(new Date(cursor.getFullYear(), cursor.getMonth() + 1, 1))}>Next →</button>

        <form className="calendar-add-form" onSubmit={handleAddEvent}>
          <input placeholder="Event title" value={newEventTitle} onChange={(e) => setNewEventTitle(e.target.value)} />
          <input type="date" value={newEventDate} onChange={(e) => setNewEventDate(e.target.value)} />
          <button type="submit" disabled={createEventMutation.isPending}>Add event</button>
        </form>
      </div>

      <div className="calendar-weekdays">
        {["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"].map((d) => <div key={d}>{d}</div>)}
      </div>

      <div className="calendar-grid">
        {days.map((day) => (
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
  );
}
