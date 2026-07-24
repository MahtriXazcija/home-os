import { useEffect, useRef, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import { useAuth } from "../auth/AuthContext";
import { getChatMessages, sendChatMessage } from "../api/chat";

function timeLabel(iso: string) {
  return new Date(iso).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" });
}

export default function Chat() {
  const { user } = useAuth();
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();
  const [draft, setDraft] = useState("");
  const scrollRef = useRef<HTMLDivElement>(null);

  const { data: messages } = useQuery({
    queryKey: ["chat", householdId],
    queryFn: () => getChatMessages(householdId),
    enabled: !!householdId,
    refetchInterval: 4000,
  });

  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight });
  }, [messages]);

  const sendMutation = useMutation({
    mutationFn: (content: string) => sendChatMessage(householdId, content),
    onSuccess: () => {
      setDraft("");
      queryClient.invalidateQueries({ queryKey: ["chat", householdId] });
    },
  });

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const content = draft.trim();
    if (!content || !householdId) return;
    sendMutation.mutate(content);
  }

  const nameFor = (userId: string) =>
    household?.members.find((m) => m.userId === userId)?.displayName ?? "Member";

  return (
    <div>
      <h1>Chat</h1>
      <p className="dek">Only members of {household?.name ?? "your household"} can see this.</p>

      <div className="chat-panel">
        <div className="chat-messages" ref={scrollRef}>
          {(messages ?? []).length === 0 && <p className="empty">No messages yet — say hello.</p>}
          {(messages ?? []).map((m) => {
            const isOwn = m.senderUserId === user?.userId;
            return (
              <div key={m.id} className={`chat-message${isOwn ? " own" : ""}`}>
                {!isOwn && <div className="chat-message-sender">{nameFor(m.senderUserId)}</div>}
                <div className="chat-bubble">{m.content}</div>
                <div className="chat-message-time">{timeLabel(m.createdAtUtc)}</div>
              </div>
            );
          })}
        </div>
        <form className="chat-form" onSubmit={handleSubmit}>
          <input
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            placeholder="Write a message…"
            maxLength={2000}
          />
          <button type="submit" disabled={sendMutation.isPending || !draft.trim()}>Send</button>
        </form>
      </div>
    </div>
  );
}
