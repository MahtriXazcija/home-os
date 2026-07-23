import { apiDelete, apiGet, apiPost } from "./client";

export type ReminderRecurrence = "None" | "Daily" | "Weekly" | "Monthly";

export interface Reminder {
  id: string;
  householdId: string;
  targetUserId: string;
  title: string;
  message: string | null;
  remindAtUtc: string;
  recurrence: ReminderRecurrence;
  sourceType: string | null;
  sourceId: string | null;
  isFired: boolean;
}

export interface CreateReminderInput {
  householdId: string;
  targetUserId: string;
  title: string;
  remindAtUtc: string;
  message?: string;
  recurrence: ReminderRecurrence;
  sourceType?: string | null;
  sourceId?: string | null;
}

export const getMyReminders = () => apiGet<Reminder[]>("/api/reminders/mine");
export const createReminder = (input: CreateReminderInput) => apiPost<Reminder>("/api/reminders", input);
export const cancelReminder = (id: string) => apiDelete(`/api/reminders/${id}`);
