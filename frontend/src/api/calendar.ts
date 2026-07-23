import { apiGet, apiPost } from "./client";

export interface CalendarItem {
  id: string;
  kind: "event" | "task";
  title: string;
  startUtc: string;
  endUtc: string;
  isAllDay: boolean;
}

export interface CalendarEvent {
  id: string;
  householdId: string;
  title: string;
  description: string | null;
  startUtc: string;
  endUtc: string;
  isAllDay: boolean;
  createdByUserId: string;
}

export const getCalendar = (householdId: string, fromUtc: string, toUtc: string) =>
  apiGet<CalendarItem[]>(`/api/calendar?householdId=${householdId}&fromUtc=${fromUtc}&toUtc=${toUtc}`);

export const createCalendarEvent = (input: {
  householdId: string;
  title: string;
  description?: string;
  startUtc: string;
  endUtc: string;
  isAllDay: boolean;
}) => apiPost<CalendarEvent>("/api/calendar/events", input);
