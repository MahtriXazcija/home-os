import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useHousehold } from "../hooks/useHousehold";
import { createNote, deleteNote, getNotes } from "../api/notes";
import Icon from "../components/Icon";

export default function Notes() {
  const { data: household } = useHousehold();
  const householdId = household?.id ?? "";
  const queryClient = useQueryClient();

  const { data: notes, isLoading } = useQuery({
    queryKey: ["notes", householdId],
    queryFn: () => getNotes(householdId),
    enabled: !!householdId,
  });

  const [content, setContent] = useState("");
  const [title, setTitle] = useState("");
  const [tagsInput, setTagsInput] = useState("");
  const [isJournal, setIsJournal] = useState(false);
  const [showJournalOnly, setShowJournalOnly] = useState(false);

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["notes", householdId] });

  const createMutation = useMutation({
    mutationFn: createNote,
    onSuccess: () => {
      setContent("");
      setTitle("");
      setTagsInput("");
      invalidate();
    },
  });

  const deleteMutation = useMutation({ mutationFn: deleteNote, onSuccess: invalidate });

  function handleCreate(e: FormEvent) {
    e.preventDefault();
    if (!content.trim() || !householdId) return;
    createMutation.mutate({
      householdId,
      content,
      title: title || undefined,
      tags: tagsInput.split(",").map((t) => t.trim()).filter(Boolean),
      journalDate: isJournal ? new Date().toISOString().slice(0, 10) : null,
    });
  }

  const all = notes ?? [];
  const todayStr = new Date().toISOString().slice(0, 10);
  const todaysJournal = all.find((n) => n.journalDate === todayStr);
  const journalCount = all.filter((n) => n.journalDate !== null).length;
  const tagCount = new Set(all.flatMap((n) => n.tags)).size;

  const visible = all
    .filter((n) => !showJournalOnly || n.journalDate !== null)
    .filter((n) => n.id !== todaysJournal?.id || showJournalOnly);

  return (
    <div>
      <h1>Notes</h1>
      <p className="dek">Quick notes and a daily journal — link one to a task, bill, or event from its own page.</p>

      <div className="stat-strip">
        <div className="stat-chip">
          <div>
            <div className="stat-chip-num">{all.length}</div>
            <div className="stat-chip-label">Total notes</div>
          </div>
        </div>
        <div className="stat-chip">
          <div>
            <div className="stat-chip-num">{journalCount}</div>
            <div className="stat-chip-label">Journal entries</div>
          </div>
        </div>
        <div className="stat-chip">
          <div>
            <div className="stat-chip-num">{tagCount}</div>
            <div className="stat-chip-label">Tags in use</div>
          </div>
        </div>
      </div>

      <div className="quick-add-card">
        <form className="task-form" onSubmit={handleCreate}>
          <input placeholder="Title (optional)" value={title} onChange={(e) => setTitle(e.target.value)} style={{ flex: "0 0 180px" }} />
          <input
            className="task-form-title"
            placeholder="Write a note…"
            value={content}
            onChange={(e) => setContent(e.target.value)}
          />
          <input placeholder="tags, comma, separated" value={tagsInput} onChange={(e) => setTagsInput(e.target.value)} />
          <label style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 13 }}>
            <input type="checkbox" checked={isJournal} onChange={(e) => setIsJournal(e.target.checked)} />
            Today's journal
          </label>
          <button type="submit" disabled={createMutation.isPending}>Add</button>
        </form>
      </div>

      {!showJournalOnly && todaysJournal && (
        <div className="journal-spotlight">
          <div className="journal-spotlight-label"><Icon name="file-text" size={13} />Today's journal</div>
          <p className="journal-spotlight-content">{todaysJournal.content}</p>
        </div>
      )}

      <label style={{ display: "flex", alignItems: "center", gap: 6, fontSize: 13, marginBottom: 16 }}>
        <input type="checkbox" checked={showJournalOnly} onChange={(e) => setShowJournalOnly(e.target.checked)} />
        Journal entries only
      </label>

      {isLoading && <p className="empty">Loading notes…</p>}
      {!isLoading && visible.length === 0 && <p className="empty">No notes yet.</p>}

      {visible.length > 0 && (
        <div className="section-header-row">
          <span className="app-icon"><Icon name="file-text" /></span>
          <h2>{showJournalOnly ? "Journal" : "All notes"}</h2>
          <span className="section-header-count">{visible.length}</span>
        </div>
      )}

      <div className="note-grid">
        {visible.map((note) => (
          <div key={note.id} className="note-card">
            <div className="note-card-head">
              {note.journalDate && <span className="tag">journal · {note.journalDate}</span>}
              <button type="button" className="link-button" onClick={() => deleteMutation.mutate(note.id)}>Delete</button>
            </div>
            {note.title && <div className="note-title">{note.title}</div>}
            <p className="note-content">{note.content}</p>
            {note.tags.length > 0 && (
              <div className="task-meta">
                {note.tags.map((t) => <span key={t} className="tag">{t}</span>)}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
