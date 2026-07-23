import { useEffect, useState, type DragEvent, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import { getBoards, createBoard, type Board } from "../api/boards";
import { changeTaskStatus, createTask, getTasks, type Task, type TaskStatus } from "../api/tasks";

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
  const [draggedTaskId, setDraggedTaskId] = useState<string | null>(null);

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
      priority: "Medium",
      recurrence: "None",
      tags: [],
      boardId: activeBoardId,
    });
  }

  function handleDrop(e: DragEvent, status: TaskStatus) {
    e.preventDefault();
    if (draggedTaskId) {
      statusMutation.mutate({ id: draggedTaskId, status });
      setDraggedTaskId(null);
    }
  }

  const cardsFor = (status: TaskStatus) => (tasks ?? []).filter((t) => t.status === status);

  return (
    <div>
      <h1>Kanban</h1>
      <p className="dek">Drag cards across columns — the same tasks show up on the Tasks list and Calendar too.</p>

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
          <form className="card-add-form" onSubmit={handleAddCard}>
            <input placeholder="Add a card to To do…" value={newCardTitle} onChange={(e) => setNewCardTitle(e.target.value)} />
            <button type="submit" disabled={createCardMutation.isPending}>Add card</button>
          </form>

          <div className="kanban-columns">
            {COLUMNS.map((col) => (
              <div
                key={col.status}
                className="kanban-column"
                onDragOver={(e) => e.preventDefault()}
                onDrop={(e) => handleDrop(e, col.status)}
              >
                <div className="kanban-column-head">
                  {col.label} <span className="kanban-count">{cardsFor(col.status).length}</span>
                </div>
                <div className="kanban-cards">
                  {cardsFor(col.status).map((task: Task) => (
                    <div
                      key={task.id}
                      className="kanban-card"
                      draggable
                      onDragStart={() => setDraggedTaskId(task.id)}
                    >
                      <div className="kanban-card-title">{task.title}</div>
                      <div className="kanban-card-meta">
                        <span className={`pill priority-${task.priority.toLowerCase()}`}>{task.priority}</span>
                        {task.dueDateUtc && (
                          <span className="task-due">{new Date(task.dueDateUtc).toLocaleDateString(undefined, { month: "short", day: "numeric" })}</span>
                        )}
                      </div>
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
