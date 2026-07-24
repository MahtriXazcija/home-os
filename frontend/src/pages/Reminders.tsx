import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import { useAuth } from "../auth/AuthContext";
import { cancelReminder, createReminder, getMyReminders, type Reminder, type ReminderRecurrence } from "../api/reminders";
import Icon, { type IconName } from "../components/Icon";

function startOfDay(d: Date) {
  return new Date(d.getFullYear(), d.getMonth(), d.getDate());
}

export default function Reminders() {
  const { user } = useAuth();
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();

  const { data: reminders, isLoading } = useQuery({
    queryKey: ["reminders"],
    queryFn: getMyReminders,
  });

  const [title, setTitle] = useState("");
  const [remindAt, setRemindAt] = useState("");
  const [targetUserId, setTargetUserId] = useState(user?.userId ?? "");
  const [recurrence, setRecurrence] = useState<ReminderRecurrence>("None");
  const [message, setMessage] = useState("");

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["reminders"] });

  const createMutation = useMutation({
    mutationFn: createReminder,
    onSuccess: () => {
      setTitle("");
      setRemindAt("");
      setMessage("");
      invalidate();
    },
  });

  const cancelMutation = useMutation({ mutationFn: cancelReminder, onSuccess: invalidate });

  function handleCreate(e: FormEvent) {
    e.preventDefault();
    if (!title.trim() || !remindAt || !householdId) return;
    createMutation.mutate({
      householdId,
      targetUserId: targetUserId || user!.userId,
      title,
      remindAtUtc: new Date(remindAt).toISOString(),
      message: message || undefined,
      recurrence,
    });
  }

  const memberName = (userId: string) => household?.members.find((m) => m.userId === userId)?.displayName ?? "—";

  const sorted = [...(reminders ?? [])].sort((a, b) => new Date(a.remindAtUtc).getTime() - new Date(b.remindAtUtc).getTime());

  const now = new Date();
  const today0 = startOfDay(now);
  const tomorrow0 = new Date(today0.getTime() + 24 * 60 * 60 * 1000);
  const weekEnd = new Date(today0.getTime() + 7 * 24 * 60 * 60 * 1000);

  const overdue = sorted.filter((r) => new Date(r.remindAtUtc) < now);
  const todayGroup = sorted.filter((r) => { const t = new Date(r.remindAtUtc); return t >= now && t < tomorrow0; });
  const weekGroup = sorted.filter((r) => { const t = new Date(r.remindAtUtc); return t >= tomorrow0 && t < weekEnd; });
  const laterGroup = sorted.filter((r) => new Date(r.remindAtUtc) >= weekEnd);

  function renderRow(r: Reminder) {
    return (
      <li key={r.id} className="task-row">
        <div className="task-main">
          <span className="task-title">{r.title}</span>
          <div className="task-meta">
            <span className="task-due">{new Date(r.remindAtUtc).toLocaleString(undefined, { month: "short", day: "numeric", hour: "numeric", minute: "2-digit" })}</span>
            <span className="task-assignee">{memberName(r.targetUserId)}</span>
            {r.recurrence !== "None" && <span className="tag">{r.recurrence.toLowerCase()}</span>}
            {r.message && <span className="task-assignee">{r.message}</span>}
          </div>
        </div>
        <button type="button" className="link-button task-delete" onClick={() => cancelMutation.mutate(r.id)}>Cancel</button>
      </li>
    );
  }

  function renderSection(label: string, icon: IconName, items: Reminder[]) {
    if (items.length === 0) return null;
    return (
      <>
        <div className="section-header-row">
          <span className="app-icon"><Icon name={icon} /></span>
          <h2>{label}</h2>
          <span className="section-header-count">{items.length}</span>
        </div>
        <ul className="task-list" style={{ marginBottom: 20 }}>
          {items.map(renderRow)}
        </ul>
      </>
    );
  }

  return (
    <div>
      <h1>Reminders</h1>
      <p className="dek">One-off or recurring, aimed at any member — fires in-app and by email if they've opted in.</p>

      <div className="stat-strip">
        <div className={`stat-chip${overdue.length > 0 ? " warn" : ""}`}>
          <div>
            <div className="stat-chip-num">{overdue.length}</div>
            <div className="stat-chip-label">Overdue</div>
          </div>
        </div>
        <div className="stat-chip">
          <div>
            <div className="stat-chip-num">{todayGroup.length}</div>
            <div className="stat-chip-label">Today</div>
          </div>
        </div>
        <div className="stat-chip">
          <div>
            <div className="stat-chip-num">{weekGroup.length}</div>
            <div className="stat-chip-label">This week</div>
          </div>
        </div>
      </div>

      <div className="quick-add-card">
        <form className="task-form" onSubmit={handleCreate}>
          <input
            className="task-form-title"
            placeholder="Remind about…"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
          <input type="datetime-local" value={remindAt} onChange={(e) => setRemindAt(e.target.value)} required />
          <select value={targetUserId} onChange={(e) => setTargetUserId(e.target.value)}>
            {household?.members.map((m) => (
              <option key={m.userId} value={m.userId}>{m.displayName}{m.userId === user?.userId ? " (you)" : ""}</option>
            ))}
          </select>
          <select value={recurrence} onChange={(e) => setRecurrence(e.target.value as ReminderRecurrence)}>
            <option value="None">One-off</option>
            <option value="Daily">Daily</option>
            <option value="Weekly">Weekly</option>
            <option value="Monthly">Monthly</option>
          </select>
          <input placeholder="note (optional)" value={message} onChange={(e) => setMessage(e.target.value)} />
          <button type="submit" disabled={createMutation.isPending}>Add</button>
        </form>
      </div>

      {isLoading && <p className="empty">Loading reminders…</p>}
      {!isLoading && sorted.length === 0 && <p className="empty">No reminders yet.</p>}

      {renderSection("Overdue", "bell", overdue)}
      {renderSection("Today", "calendar", todayGroup)}
      {renderSection("This week", "calendar", weekGroup)}
      {renderSection("Later", "calendar", laterGroup)}
    </div>
  );
}
