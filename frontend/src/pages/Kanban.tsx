import { useEffect, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import { getBoards, createBoard, type Board } from "../api/boards";
import { changeTaskStatus, createTask, getTasks, type Task, type TaskPriority, type TaskStatus } from "../api/tasks";

const COLUMNS: { status: TaskStatus; label: string }[] = [
  { status: "Todo", label: "To do" },
  { status: "Doing", label: "Doing" },
  { status: "Done", label: "Done" },
];

export default function Kanban() {
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();

  const { data: boards } = useQuery({
    queryKey: ["boards", householdId],
    queryFn: () => getBoards(householdId),
    enabled: !!householdId,
  });

  const [activeBoardId, setActiveBoardId] = useState<string | null>(null);
  const [newBoardName, setNewBoardName] = useState("");
  const [newCardTitle, setNewCardTitle] = useState("");
  const [newCardDueDate, setNewCardDueDate] = useState("");
  const [newCardPriority, setNewCardPriority] = useState<TaskPriority>("Medium");
  const [newCardAssignee, setNewCardAssignee] = useState("");

  useEffect(() => {
    if (!activeBoardId && boards && boards.length > 0) {
      setActiveBoardId(boards[0].id);
    }
  }, [boards, activeBoardId]);

  const { data: tasks } = useQuery({
    queryKey: ["tasks", householdId, activeBoardId],
    queryFn: () => getTasks(householdId, activeBoardId ?? undefined),
    enabled: !!householdId && !!activeBoardId,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["tasks", householdId] });
    queryClient.invalidateQueries({ queryKey: ["boards", householdId] });
  };

  const createBoardMutation = useMutation({
    mutationFn: (name: string) => createBoard(householdId, name),
    onSuccess: (board: Board) => {
      setNewBoardName("");
      setActiveBoardId(board.id);
      invalidate();
    },
  });

  const createCardMutation = useMutation({
    mutationFn: createTask,
    onSuccess: () => {
      setNewCardTitle("");
      setNewCardDueDate("");
      setNewCardPriority("Medium");
      setNewCardAssignee("");
      invalidate();
    },
  });

  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: TaskStatus }) => changeTaskStatus(id, status),
    onSuccess: invalidate,
  });

  function handleCreateBoard(e: FormEvent) {
    e.preventDefault();
    if (!newBoardName.trim()) return;
    createBoardMutation.mutate(newBoardName);
  }

  function handleAddCard(e: FormEvent) {
    e.preventDefault();
    if (!newCardTitle.trim() || !activeBoardId) return;
    createCardMutation.mutate({
      householdId,
      title: newCardTitle,
      dueDateUtc: newCardDueDate ? new Date(newCardDueDate).toISOString() : null,
      priority: newCardPriority,
      assignedToUserId: newCardAssignee || null,
      recurrence: "None",
      tags: [],
      boardId: activeBoardId,
    });
  }

  const memberName = (userId: string | null) => household?.members.find((m) => m.userId === userId)?.displayName;

  const cardsFor = (status: TaskStatus) => (tasks ?? []).filter((t) => t.status === status);

  return (
    <div>
      <h1>Kanban</h1>
      <p className="dek">Move a card to a different column with the status picker — the same tasks show up on the Tasks list and Calendar too.</p>

      <div className="board-switcher">
        {boards?.map((b) => (
          <button
            key={b.id}
            type="button"
            className={`board-tab${b.id === activeBoardId ? " active" : ""}`}
            onClick={() => setActiveBoardId(b.id)}
          >
            {b.name}
          </button>
        ))}
        <form className="board-new-form" onSubmit={handleCreateBoard}>
          <input placeholder="New board name" value={newBoardName} onChange={(e) => setNewBoardName(e.target.value)} />
          <button type="submit" disabled={createBoardMutation.isPending}>+ Board</button>
        </form>
      </div>

      {!activeBoardId && <p className="empty">Create a board to start organizing tasks visually.</p>}

      {activeBoardId && (
        <>
          <div className="quick-add-card">
            <form className="task-form" onSubmit={handleAddCard}>
              <input
                className="task-form-title"
                placeholder="Add a card to To do…"
                value={newCardTitle}
                onChange={(e) => setNewCardTitle(e.target.value)}
              />
              <input type="date" value={newCardDueDate} onChange={(e) => setNewCardDueDate(e.target.value)} title="Due date (optional)" />
              <select value={newCardPriority} onChange={(e) => setNewCardPriority(e.target.value as TaskPriority)}>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
              <select value={newCardAssignee} onChange={(e) => setNewCardAssignee(e.target.value)}>
                <option value="">Unassigned</option>
                {household?.members.map((m) => (
                  <option key={m.userId} value={m.userId}>{m.displayName}</option>
                ))}
              </select>
              <button type="submit" disabled={createCardMutation.isPending}>Add card</button>
            </form>
          </div>

          <div className="kanban-columns">
            {COLUMNS.map((col) => (
              <div key={col.status} className="kanban-column">
                <div className="kanban-column-head">
                  {col.label} <span className="kanban-count">{cardsFor(col.status).length}</span>
                </div>
                <div className="kanban-cards">
                  {cardsFor(col.status).map((task: Task) => (
                    <div key={task.id} className="kanban-card">
                      <div className="kanban-card-title">{task.title}</div>
                      <div className="kanban-card-meta">
                        <span className={`pill priority-${task.priority.toLowerCase()}`}>{task.priority}</span>
                        {task.dueDateUtc && (
                          <span className="task-due">{new Date(task.dueDateUtc).toLocaleDateString(undefined, { month: "short", day: "numeric" })}</span>
                        )}
                        {task.assignedToUserId && <span className="task-assignee">{memberName(task.assignedToUserId)}</span>}
                      </div>
                      <select
                        className="kanban-card-status"
                        value={task.status}
                        onChange={(e) => statusMutation.mutate({ id: task.id, status: e.target.value as TaskStatus })}
                      >
                        {COLUMNS.map((c) => (
                          <option key={c.status} value={c.status}>{c.label}</option>
                        ))}
                      </select>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
