import { Link } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { apiGet, type ApiStatus } from "../api/client";
import { useHousehold } from "../hooks/useHousehold";
import { getTasks } from "../api/tasks";
import { getCalendar } from "../api/calendar";
import { getMyReminders } from "../api/reminders";
import { getBills } from "../api/finance";
import Icon from "../components/Icon";

function startOfDay(d: Date) {
  return new Date(d.getFullYear(), d.getMonth(), d.getDate());
}
function endOfDay(d: Date) {
  return new Date(d.getFullYear(), d.getMonth(), d.getDate() + 1);
}

export default function Dashboard() {
  const { data: status, isLoading, isError } = useQuery({
    queryKey: ["api-status"],
    queryFn: () => apiGet<ApiStatus>("/api/status"),
  });

  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";

  const { data: tasks } = useQuery({
    queryKey: ["tasks", householdId],
    queryFn: () => getTasks(householdId),
    enabled: !!householdId,
  });

  const todayStart = startOfDay(new Date()).toISOString();
  const todayEnd = endOfDay(new Date()).toISOString();
  const { data: todayItems } = useQuery({
    queryKey: ["calendar", householdId, todayStart, todayEnd],
    queryFn: () => getCalendar(householdId, todayStart, todayEnd),
    enabled: !!householdId,
  });

  const now = new Date();
  const dueTasks = (tasks ?? [])
    .filter((t) => !t.isCompleted && t.dueDateUtc)
    .sort((a, b) => new Date(a.dueDateUtc!).getTime() - new Date(b.dueDateUtc!).getTime());
  const overdue = dueTasks.filter((t) => new Date(t.dueDateUtc!) < now);
  const upcoming = dueTasks.filter((t) => new Date(t.dueDateUtc!) >= now).slice(0, 5);
  const todayEvents = (todayItems ?? []).filter((i) => i.kind === "event");

  const { data: reminders } = useQuery({ queryKey: ["reminders"], queryFn: getMyReminders });
  const upcomingReminders = [...(reminders ?? [])]
    .sort((a, b) => new Date(a.remindAtUtc).getTime() - new Date(b.remindAtUtc).getTime())
    .slice(0, 5);

  const { data: bills } = useQuery({ queryKey: ["bills", householdId], queryFn: () => getBills(householdId), enabled: !!householdId });
  const upcomingBills = [...(bills ?? [])]
    .sort((a, b) => new Date(a.dueDateUtc).getTime() - new Date(b.dueDateUtc).getTime())
    .slice(0, 5);

  return (
    <div>
      <h1>Today</h1>
      <p className="dek">Everything due, happening, or coming up — in one place.</p>

      <div className="card-grid">
        <section className="card">
          <div className="card-title-row">
            <span className="app-icon app-icon-tasks"><Icon name="check-square" /></span>
            <h2>Tasks due</h2>
          </div>
          {overdue.length === 0 && upcoming.length === 0 && <p className="empty">Nothing due — you're clear.</p>}
          {overdue.length > 0 && (
            <ul className="dashboard-list">
              {overdue.map((t) => (
                <li key={t.id} className="dashboard-list-item overdue">{t.title}</li>
              ))}
            </ul>
          )}
          {upcoming.length > 0 && (
            <ul className="dashboard-list">
              {upcoming.map((t) => (
                <li key={t.id} className="dashboard-list-item">{t.title}</li>
              ))}
            </ul>
          )}
          {(overdue.length > 0 || upcoming.length > 0) && <Link className="card-link" to="/tasks">View all tasks →</Link>}
        </section>

        <section className="card">
          <div className="card-title-row">
            <span className="app-icon app-icon-calendar"><Icon name="calendar" /></span>
            <h2>Today's events</h2>
          </div>
          {todayEvents.length === 0 && <p className="empty">Nothing on the calendar today.</p>}
          {todayEvents.length > 0 && (
            <ul className="dashboard-list">
              {todayEvents.map((e) => (
                <li key={e.id} className="dashboard-list-item">{e.title}</li>
              ))}
            </ul>
          )}
          <Link className="card-link" to="/calendar">Open calendar →</Link>
        </section>

        <section className="card">
          <div className="card-title-row">
            <span className="app-icon app-icon-finance"><Icon name="dollar-sign" /></span>
            <h2>Upcoming bills</h2>
          </div>
          {upcomingBills.length === 0 && <p className="empty">Nothing due.</p>}
          {upcomingBills.length > 0 && (
            <ul className="dashboard-list">
              {upcomingBills.map((b) => (
                <li key={b.id} className="dashboard-list-item">{b.title} — {b.amount.toLocaleString(undefined, { style: "currency", currency: "USD" })}</li>
              ))}
            </ul>
          )}
          <Link className="card-link" to="/finance">Open finance →</Link>
        </section>
        <section className="card">
          <div className="card-title-row">
            <span className="app-icon app-icon-reminders"><Icon name="bell" /></span>
            <h2>Active reminders</h2>
          </div>
          {upcomingReminders.length === 0 && <p className="empty">Nothing scheduled.</p>}
          {upcomingReminders.length > 0 && (
            <ul className="dashboard-list">
              {upcomingReminders.map((r) => (
                <li key={r.id} className="dashboard-list-item">{r.title}</li>
              ))}
            </ul>
          )}
          <Link className="card-link" to="/reminders">View all reminders →</Link>
        </section>
      </div>

      <div className="status-strip">
        {isLoading && <span>Connecting to API…</span>}
        {isError && <span className="status-bad">API unreachable — is the backend running?</span>}
        {status && (
          <span className="status-ok">
            Connected to {status.service} · {new Date(status.utc).toLocaleTimeString()}
          </span>
        )}
      </div>
    </div>
  );
}
