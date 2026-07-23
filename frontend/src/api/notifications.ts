import { apiGet, apiPost, apiPut } from "./client";

export type NotificationCategory = "ReminderFired" | "TaskAssigned" | "BillDue" | "SharedWithYou";

export interface AppNotification {
  id: string;
  category: NotificationCategory;
  title: string;
  message: string | null;
  isRead: boolean;
  createdAtUtc: string;
}

export interface NotificationPreference {
  category: NotificationCategory;
  emailEnabled: boolean;
}

export const getNotifications = () => apiGet<AppNotification[]>("/api/notifications");
export const markNotificationRead = (id: string) => apiPost<void>(`/api/notifications/${id}/read`);
export const getNotificationPreferences = () => apiGet<NotificationPreference[]>("/api/notifications/preferences");
export const setNotificationPreference = (category: NotificationCategory, emailEnabled: boolean) =>
  apiPut<void>("/api/notifications/preferences", { category, emailEnabled });
