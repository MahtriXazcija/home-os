import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../auth/AuthContext";
import { getChatMessages, getChatReadStates } from "../api/chat";

/**
 * Shared with Chat.tsx via React Query's cache — both read the same
 * ["chat", householdId] / ["chat-read-state", householdId] keys, so this
 * doesn't double the polling traffic when the Chat page is open.
 */
export function useChatUnread(householdId: string, enabled: boolean) {
  const { user } = useAuth();

  const { data: messages } = useQuery({
    queryKey: ["chat", householdId],
    queryFn: () => getChatMessages(householdId),
    enabled: enabled && !!householdId,
    refetchInterval: 8000,
  });

  const { data: readStates } = useQuery({
    queryKey: ["chat-read-state", householdId],
    queryFn: () => getChatReadStates(householdId),
    enabled: enabled && !!householdId,
    refetchInterval: 8000,
  });

  if (!messages || !user) return 0;

  const myLastRead = readStates?.find((r) => r.userId === user.userId)?.lastReadAtUtc;
  const since = myLastRead ? new Date(myLastRead).getTime() : 0;

  return messages.filter((m) => m.senderUserId !== user.userId && new Date(m.createdAtUtc).getTime() > since).length;
}
