import { useEffect, useRef, useState, type FormEvent } from "react";
import { useLocation } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { askPageAssistant, type AiChatTurn } from "../api/aiAssistant";
import { useApps } from "../hooks/useApps";
import Icon from "./Icon";

function pageIdFromPath(pathname: string): string {
  if (pathname === "/") return "dashboard";
  return pathname.split("/")[1] || "dashboard";
}

const FALLBACK_TITLES: Record<string, string> = {
  dashboard: "Dashboard",
  apps: "Manage apps",
  profile: "Profile",
};

interface Message {
  role: "user" | "assistant";
  content: string;
}

export default function PageAssistant() {
  const location = useLocation();
  const pageId = pageIdFromPath(location.pathname);
  const { data: apps } = useApps();
  const pageTitle = apps?.find((a) => a.id === pageId)?.navLabel ?? FALLBACK_TITLES[pageId] ?? pageId;

  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<Message[]>([]);
  const [draft, setDraft] = useState("");
  const [awaitingDetailChoice, setAwaitingDetailChoice] = useState(false);
  const scrollRef = useRef<HTMLDivElement>(null);

  // This assistant is scoped to "what's on this page" — reset the
  // conversation whenever the page changes rather than carrying context
  // across unrelated modules.
  useEffect(() => {
    setMessages([]);
    setAwaitingDetailChoice(false);
    setOpen(false);
  }, [pageId]);

  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight });
  }, [messages, awaitingDetailChoice]);

  const askMutation = useMutation({
    mutationFn: ({ message, detailLevel }: { message: string; detailLevel: "brief" | "detailed" | null }) => {
      const history: AiChatTurn[] = messages.map((m) => ({ role: m.role, content: m.content }));
      return askPageAssistant(pageId, message, detailLevel, history);
    },
    onSuccess: (res) => {
      setMessages((prev) => [...prev, { role: "assistant", content: res.reply }]);
    },
    onError: () => {
      setMessages((prev) => [...prev, { role: "assistant", content: "Sorry, something went wrong reaching the assistant." }]);
    },
  });

  function sendExplainRequest(detailLevel: "brief" | "detailed") {
    setAwaitingDetailChoice(false);
    setMessages((prev) => [...prev, { role: "user", content: `Explain this page (${detailLevel}).` }]);
    askMutation.mutate({ message: "Explain this page.", detailLevel });
  }

  function handleSend(e: FormEvent) {
    e.preventDefault();
    const text = draft.trim();
    if (!text) return;
    setMessages((prev) => [...prev, { role: "user", content: text }]);
    setDraft("");
    askMutation.mutate({ message: text, detailLevel: null });
  }

  return (
    <>
      <button type="button" className="page-assistant-fab" onClick={() => setOpen(!open)} aria-label="Page assistant">
        <Icon name="sparkles" size={19} />
      </button>

      {open && (
        <div className="page-assistant-panel">
          <div className="page-assistant-head">
            <span className="app-icon"><Icon name="sparkles" /></span>
            <div>
              <div className="page-assistant-title">Page assistant</div>
              <div className="page-assistant-subtitle">{pageTitle}</div>
            </div>
            <button type="button" className="link-button page-assistant-close" onClick={() => setOpen(false)} aria-label="Close">
              <Icon name="chevrons-right" size={14} />
            </button>
          </div>

          <div className="page-assistant-body" ref={scrollRef}>
            {messages.length === 0 && !awaitingDetailChoice && (
              <div className="page-assistant-empty">
                <p className="dek" style={{ margin: "0 0 12px" }}>Ask me anything about the {pageTitle} page, or:</p>
                <button type="button" className="page-assistant-explain-btn" onClick={() => setAwaitingDetailChoice(true)}>
                  <Icon name="sparkles" size={14} /> Explain this page
                </button>
              </div>
            )}

            {messages.map((m, i) => (
              <div key={i} className={`page-assistant-msg ${m.role}`}>{m.content}</div>
            ))}

            {askMutation.isPending && <div className="page-assistant-msg assistant page-assistant-thinking">Thinking…</div>}

            {awaitingDetailChoice && (
              <div className="page-assistant-msg assistant">
                Sure — brief version or a detailed walkthrough?
                <div className="page-assistant-choices">
                  <button type="button" onClick={() => sendExplainRequest("brief")}>Brief</button>
                  <button type="button" onClick={() => sendExplainRequest("detailed")}>Detailed</button>
                </div>
              </div>
            )}
          </div>

          <form className="page-assistant-form" onSubmit={handleSend}>
            <input
              value={draft}
              onChange={(e) => setDraft(e.target.value)}
              placeholder={`Ask about ${pageTitle}…`}
            />
            <button type="submit" disabled={askMutation.isPending || !draft.trim()}>Send</button>
          </form>
        </div>
      )}
    </>
  );
}
