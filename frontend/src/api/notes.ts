import { apiDelete, apiGet, apiPost, apiPut } from "./client";

export interface Note {
  id: string;
  householdId: string;
  title: string | null;
  content: string;
  tags: string[];
  journalDate: string | null;
  linkedType: string | null;
  linkedId: string | null;
  createdByUserId: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CreateNoteInput {
  householdId: string;
  content: string;
  title?: string;
  tags?: string[];
  journalDate?: string | null;
  linkedType?: string | null;
  linkedId?: string | null;
}

export const getNotes = (householdId: string) => apiGet<Note[]>(`/api/notes?householdId=${householdId}`);
export const createNote = (input: CreateNoteInput) => apiPost<Note>("/api/notes", input);
export const updateNote = (id: string, content: string, title: string | undefined, tags: string[]) =>
  apiPut<Note>(`/api/notes/${id}`, { content, title, tags });
export const deleteNote = (id: string) => apiDelete(`/api/notes/${id}`);
