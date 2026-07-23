import { apiGet, apiPost } from "./client";

export interface Board {
  id: string;
  householdId: string;
  name: string;
  createdAtUtc: string;
}

export const getBoards = (householdId: string) => apiGet<Board[]>(`/api/boards?householdId=${householdId}`);

export const createBoard = (householdId: string, name: string) =>
  apiPost<Board>("/api/boards", { householdId, name });
