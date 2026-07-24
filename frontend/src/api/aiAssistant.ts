import { apiPost } from "./client";

export interface AiChatTurn {
  role: "user" | "assistant";
  content: string;
}

export interface AskAssistantResponse {
  reply: string;
}

export const askPageAssistant = (
  pageId: string,
  message: string,
  detailLevel: "brief" | "detailed" | null,
  history: AiChatTurn[],
) => apiPost<AskAssistantResponse>("/api/ai-assistant/ask", { pageId, message, detailLevel, history });
