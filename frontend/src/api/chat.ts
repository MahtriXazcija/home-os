import { apiGet, apiPost } from "./client";

export interface ChatMessage {
  id: string;
  householdId: string;
  senderUserId: string;
  content: string;
  createdAtUtc: string;
}

export const getChatMessages = (householdId: string) => apiGet<ChatMessage[]>(`/api/chat?householdId=${householdId}`);

export const sendChatMessage = (householdId: string, content: string) =>
  apiPost<ChatMessage>("/api/chat", { householdId, content });
