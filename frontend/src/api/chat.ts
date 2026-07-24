import { apiGet, apiPost } from "./client";

export interface ChatMessage {
  id: string;
  householdId: string;
  senderUserId: string;
  content: string;
  createdAtUtc: string;
}

export interface ChatReadState {
  userId: string;
  lastReadAtUtc: string;
}

export const getChatMessages = (householdId: string) => apiGet<ChatMessage[]>(`/api/chat?householdId=${householdId}`);

export const sendChatMessage = (householdId: string, content: string) =>
  apiPost<ChatMessage>("/api/chat", { householdId, content });

export const getChatReadStates = (householdId: string) =>
  apiGet<ChatReadState[]>(`/api/chat/read-state?householdId=${householdId}`);

export const markChatRead = (householdId: string) => apiPost<void>("/api/chat/read", { householdId });
