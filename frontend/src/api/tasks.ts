import { apiGet, apiPost, apiPut, apiPatch, apiDelete } from "./client";

export type TaskPriority = "Low" | "Medium" | "High";
export type TaskStatus = "Todo" | "Doing" | "Done";
export type RecurrenceRule = "None" | "Daily" | "Weekly" | "Monthly";

export interface Task {
  id: string;
  householdId: string;
  boardId: string | null;
  parentTaskId: string | null;
  title: string;
  description: string | null;
  dueDateUtc: string | null;
  priority: TaskPriority;
  status: TaskStatus;
  assignedToUserId: string | null;
  recurrence: RecurrenceRule;
  tags: string[];
  isCompleted: boolean;
  completedAtUtc: string | null;
  createdByUserId: string;
  createdAtUtc: string;
}

export interface CreateTaskInput {
  householdId: string;
  title: string;
  description?: string;
  dueDateUtc?: string | null;
  priority: TaskPriority;
  assignedToUserId?: string | null;
  boardId?: string | null;
  parentTaskId?: string | null;
  recurrence: RecurrenceRule;
  tags: string[];
}

export interface UpdateTaskInput {
  title: string;
  description?: string;
  dueDateUtc?: string | null;
  priority: TaskPriority;
  assignedToUserId?: string | null;
  recurrence: RecurrenceRule;
  tags: string[];
}

export const getTasks = (householdId: string, boardId?: string) =>
  apiGet<Task[]>(`/api/tasks?householdId=${householdId}${boardId ? `&boardId=${boardId}` : ""}`);

export const createTask = (input: CreateTaskInput) => apiPost<Task>("/api/tasks", input);

export const updateTask = (id: string, input: UpdateTaskInput) => apiPut<Task>(`/api/tasks/${id}`, input);

export const changeTaskStatus = (id: string, status: TaskStatus) =>
  apiPatch<Task>(`/api/tasks/${id}/status`, { status });

export const completeTask = (id: string) => apiPost<Task>(`/api/tasks/${id}/complete`);

export const reopenTask = (id: string) => apiPost<Task>(`/api/tasks/${id}/reopen`);

export const deleteTask = (id: string) => apiDelete(`/api/tasks/${id}`);
