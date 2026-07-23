import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import { useAuth } from "../auth/AuthContext";
import { cancelReminder, createReminder, getMyReminders, type ReminderRecurrence } from "../api/reminders";

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

  return (
    <div>
      <h1>Reminders</h1>
      <p className="dek">One-off or recurring, aimed at any member — fires in-app and by email if they've opted in.</p>

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

      {isLoading && <p className="empty">Loading reminders…</p>}
      {!isLoading && sorted.length === 0 && <p className="empty">No reminders yet.</p>}

      <ul className="task-list">
        {sorted.map((r) => (
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
        ))}
      </ul>
    </div>
  );
}
