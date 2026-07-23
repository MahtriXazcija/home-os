import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import {
  completeTask,
  createTask,
  deleteTask,
  getTasks,
  reopenTask,
  type RecurrenceRule,
  type Task,
  type TaskPriority,
} from "../api/tasks";

function isOverdue(task: Task) {
  return !task.isCompleted && task.dueDateUtc !== null && new Date(task.dueDateUtc) < new Date();
}

function formatDue(dueDateUtc: string | null) {
  if (!dueDateUtc) return null;
  return new Date(dueDateUtc).toLocaleDateString(undefined, { month: "short", day: "numeric" });
}

export default function Tasks() {
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();

  const { data: tasks, isLoading } = useQuery({
    queryKey: ["tasks", householdId],
    queryFn: () => getTasks(householdId),
    enabled: !!householdId,
  });

  const [title, setTitle] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [priority, setPriority] = useState<TaskPriority>("Medium");
  const [assignedToUserId, setAssignedToUserId] = useState("");
  const [recurrence, setRecurrence] = useState<RecurrenceRule>("None");
  const [tagsInput, setTagsInput] = useState("");

  const invalidateTasks = () => queryClient.invalidateQueries({ queryKey: ["tasks", householdId] });

  const createMutation = useMutation({
    mutationFn: createTask,
    onSuccess: () => {
      setTitle("");
      setDueDate("");
      setTagsInput("");
      invalidateTasks();
    },
  });

  const completeMutation = useMutation({ mutationFn: completeTask, onSuccess: invalidateTasks });
  const reopenMutation = useMutation({ mutationFn: reopenTask, onSuccess: invalidateTasks });
  const deleteMutation = useMutation({ mutationFn: deleteTask, onSuccess: invalidateTasks });

  function handleCreate(e: FormEvent) {
    e.preventDefault();
    if (!title.trim() || !householdId) return;
    createMutation.mutate({
      householdId,
      title,
      dueDateUtc: dueDate ? new Date(dueDate).toISOString() : null,
      priority,
      assignedToUserId: assignedToUserId || null,
      recurrence,
      tags: tagsInput.split(",").map((t) => t.trim()).filter(Boolean),
    });
  }

  const memberName = (userId: string | null) => household?.members.find((m) => m.userId === userId)?.displayName;

  const sorted = [...(tasks ?? [])].sort((a, b) => {
    if (a.isCompleted !== b.isCompleted) return a.isCompleted ? 1 : -1;
    if (!a.dueDateUtc && !b.dueDateUtc) return 0;
    if (!a.dueDateUtc) return 1;
    if (!b.dueDateUtc) return -1;
    return new Date(a.dueDateUtc).getTime() - new Date(b.dueDateUtc).getTime();
  });

  return (
    <div>
      <h1>Tasks</h1>
      <p className="dek">Everything that needs doing, with a due date, a priority, and who's on it.</p>

      <form className="task-form" onSubmit={handleCreate}>
        <input
          className="task-form-title"
          placeholder="Add a task…"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
        <input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
        <select value={priority} onChange={(e) => setPriority(e.target.value as TaskPriority)}>
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
        </select>
        <select value={assignedToUserId} onChange={(e) => setAssignedToUserId(e.target.value)}>
          <option value="">Unassigned</option>
          {household?.members.map((m) => (
            <option key={m.userId} value={m.userId}>{m.displayName}</option>
          ))}
        </select>
        <select value={recurrence} onChange={(e) => setRecurrence(e.target.value as RecurrenceRule)}>
          <option value="None">Doesn't repeat</option>
          <option value="Daily">Daily</option>
          <option value="Weekly">Weekly</option>
          <option value="Monthly">Monthly</option>
        </select>
        <input placeholder="tags, comma, separated" value={tagsInput} onChange={(e) => setTagsInput(e.target.value)} />
        <button type="submit" disabled={createMutation.isPending}>Add</button>
      </form>

      {isLoading && <p className="empty">Loading tasks…</p>}
      {!isLoading && sorted.length === 0 && <p className="empty">No tasks yet — add your first one above.</p>}

      <ul className="task-list">
        {sorted.map((task) => (
          <li key={task.id} className={`task-row${task.isCompleted ? " task-done" : ""}${isOverdue(task) ? " task-overdue" : ""}`}>
            <input
              type="checkbox"
              checked={task.isCompleted}
              onChange={() => (task.isCompleted ? reopenMutation.mutate(task.id) : completeMutation.mutate(task.id))}
            />
            <div className="task-main">
              <span className="task-title">{task.title}</span>
              <div className="task-meta">
                {task.dueDateUtc && <span className="task-due">{formatDue(task.dueDateUtc)}</span>}
                <span className={`pill priority-${task.priority.toLowerCase()}`}>{task.priority}</span>
                {task.assignedToUserId && <span className="task-assignee">{memberName(task.assignedToUserId)}</span>}
                {task.tags.map((tag) => (
                  <span key={tag} className="tag">{tag}</span>
                ))}
              </div>
            </div>
            <button type="button" className="link-button task-delete" onClick={() => deleteMutation.mutate(task.id)}>Delete</button>
          </li>
        ))}
      </ul>
    </div>
  );
}
