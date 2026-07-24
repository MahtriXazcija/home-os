import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { apiGet, type ApiStatus } from "../api/client";
import { useHousehold } from "../hooks/useHousehold";
import { useApps } from "../hooks/useApps";
import { getTasks } from "../api/tasks";
import { getCalendar } from "../api/calendar";
import { getMyReminders } from "../api/reminders";
import { getBills, getFinanceSummary, getTransactions } from "../api/finance";
import Icon, { type IconName } from "../components/Icon";
import CircularGauge from "../components/CircularGauge";
import MiniBarChart from "../components/MiniBarChart";

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
  const { data: apps } = useApps();
  const installedApps = (apps ?? []).filter((a) => a.isInstalled);

  const [now, setNow] = useState(() => new Date());
  useEffect(() => {
    const id = setInterval(() => setNow(new Date()), 30_000);
    return () => clearInterval(id);
  }, []);

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

  const { data: financeSummary } = useQuery({
    queryKey: ["finance-summary", householdId],
    queryFn: () => getFinanceSummary(householdId),
    enabled: !!householdId,
  });
  const { data: transactions } = useQuery({
    queryKey: ["transactions", householdId],
    queryFn: () => getTransactions(householdId),
    enabled: !!householdId,
  });

  // --- stats for the left rail ---
  const tasksDonePercent = tasks && tasks.length > 0
    ? (tasks.filter((t) => t.isCompleted).length / tasks.length) * 100
    : 0;

  const budgetLimitTotal = (financeSummary?.budgets ?? []).reduce((sum, b) => sum + b.monthlyLimit, 0);
  const budgetUsedPercent = budgetLimitTotal > 0
    ? ((financeSummary?.totalExpenseThisMonth ?? 0) / budgetLimitTotal) * 100
    : 0;

  const last7Days = Array.from({ length: 7 }, (_, i) => {
    const d = new Date();
    d.setDate(d.getDate() - (6 - i));
    return d;
  });
  const spendByDay = last7Days.map((d) => {
    const dayStart = startOfDay(d).getTime();
    const dayEnd = endOfDay(d).getTime();
    return (transactions ?? [])
      .filter((t) => t.type === "Expense")
      .filter((t) => {
        const t2 = new Date(t.occurredAtUtc).getTime();
        return t2 >= dayStart && t2 < dayEnd;
      })
      .reduce((sum, t) => sum + t.amount, 0);
  });
  const weekTotal = spendByDay.reduce((a, b) => a + b, 0);
  const dayLabels = last7Days.map((d) => d.toLocaleDateString(undefined, { weekday: "short" }).slice(0, 2));

  // --- "next up" spotlight: earliest item across everything already loaded ---
  interface Candidate { time: number; text: string; link: string; }
  const candidates: Candidate[] = [];
  if (upcoming[0]) candidates.push({ time: new Date(upcoming[0].dueDateUtc!).getTime(), text: upcoming[0].title, link: "/tasks" });
  if (todayEvents[0]) candidates.push({ time: new Date(todayEvents[0].startUtc).getTime(), text: todayEvents[0].title, link: "/calendar" });
  if (upcomingReminders[0]) candidates.push({ time: new Date(upcomingReminders[0].remindAtUtc).getTime(), text: upcomingReminders[0].title, link: "/reminders" });
  if (upcomingBills[0]) candidates.push({ time: new Date(upcomingBills[0].dueDateUtc).getTime(), text: upcomingBills[0].title, link: "/finance" });
  const nextUp = candidates.filter((c) => c.time >= now.getTime()).sort((a, b) => a.time - b.time)[0];

  return (
    <div>
      <h1>Today</h1>
      <p className="dek">Everything due, happening, or coming up — in one place.</p>

      <div className="dashboard-layout">
        <aside className="dash-rail">
          <div className="dash-widget">
            <div className="dash-clock-time">{now.toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit", hour12: false })}</div>
            <div className="dash-clock-date">{now.toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric" })}</div>
          </div>

          <div className="dash-widget">
            <div className="dash-widget-title">Household status</div>
            <div className="dash-gauges">
              <CircularGauge percent={tasksDonePercent} label="Tasks done" />
              <CircularGauge percent={budgetUsedPercent} label="Budget used" />
            </div>
          </div>

          <div className="dash-widget">
            <div className="dash-widget-title">Spending, last 7 days</div>
            <MiniBarChart values={spendByDay} labels={dayLabels} />
            <div className="dash-widget-foot">${weekTotal.toFixed(2)} total</div>
          </div>
        </aside>

        <div className="dash-main">
          <div className="dash-promo-row">
            <div className="dash-promo-card">
              <div>
                <div className="dash-promo-title">{overdue.length > 0 ? `${overdue.length} task${overdue.length === 1 ? "" : "s"} overdue` : "All caught up"}</div>
                <p className="dash-promo-text">
                  {overdue.length > 0 ? "Take care of these before they pile up." : "Nothing overdue right now — nice work."}
                </p>
                <Link className="dash-promo-btn" to="/tasks">View tasks</Link>
              </div>
              <span className="app-icon app-icon-lg dash-promo-icon"><Icon name="check-square" /></span>
            </div>

            <div className="dash-promo-card">
              <div>
                <div className="dash-promo-title">Next up</div>
                <p className="dash-promo-text">{nextUp ? nextUp.text : "Nothing scheduled — enjoy the calm."}</p>
                <Link className="dash-promo-btn" to={nextUp?.link ?? "/calendar"}>{nextUp ? "Open" : "Open calendar"}</Link>
              </div>
              <span className="app-icon app-icon-lg dash-promo-icon"><Icon name="sparkles" /></span>
            </div>
          </div>

          <div className="dash-section-head">
            <h2>Apps</h2>
          </div>
          <div className="dash-app-grid">
            {installedApps.map((app) => (
              <Link key={app.id} to={app.navRoute} className="dash-app-tile">
                <span className="app-icon app-icon-lg"><Icon name={app.icon as IconName} /></span>
                {app.navLabel}
              </Link>
            ))}
            <Link to="/apps" className="dash-app-tile dash-app-tile-add">
              <span className="app-icon app-icon-lg"><Icon name="plus" /></span>
              Manage apps
            </Link>
          </div>

          <div className="dash-section-head">
            <h2>Overview</h2>
          </div>
          <div className="card-grid">
            <section className="card">
              <div className="card-title-row">
                <span className="app-icon"><Icon name="check-square" /></span>
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
                <span className="app-icon"><Icon name="calendar" /></span>
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
                <span className="app-icon"><Icon name="dollar-sign" /></span>
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
                <span className="app-icon"><Icon name="bell" /></span>
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
      </div>
    </div>
  );
}
