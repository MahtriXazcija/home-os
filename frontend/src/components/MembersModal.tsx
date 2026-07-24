import { useEffect, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { removeMember, type Household } from "../api/household";
import { ApiError } from "../api/client";
import Icon from "./Icon";

interface MembersModalProps {
  open: boolean;
  household: Household;
  currentUserId: string;
  isAdmin: boolean;
  onClose: () => void;
}

export default function MembersModal({ open, household, currentUserId, isAdmin, onClose }: MembersModalProps) {
  const queryClient = useQueryClient();
  const [confirmingId, setConfirmingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setConfirmingId(null);
      setError(null);
    }
  }, [open]);

  useEffect(() => {
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    if (open) window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open, onClose]);

  const removeMutation = useMutation({
    mutationFn: (userId: string) => removeMember(household.id, userId),
    onSuccess: () => {
      setConfirmingId(null);
      queryClient.invalidateQueries({ queryKey: ["my-household"] });
    },
    onError: (err) => {
      setError(err instanceof ApiError ? err.message : "Could not remove that member.");
      setConfirmingId(null);
    },
  });

  if (!open) return null;

  const sorted = [...household.members].sort((a, b) => (a.role === b.role ? 0 : a.role === "Owner" ? -1 : 1));

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-head">
          <span className="app-icon"><Icon name="users" /></span>
          <h2>Members</h2>
        </div>
        <p className="dek" style={{ margin: "0 0 14px" }}>{household.name} · {household.members.length} member{household.members.length === 1 ? "" : "s"}</p>

        {error && <p className="auth-error">{error}</p>}

        <div className="members-list">
          {sorted.map((m) => {
            const isSelf = m.userId === currentUserId;
            const initial = m.displayName.trim().charAt(0).toUpperCase();
            return (
              <div key={m.userId} className="member-row">
                <span className="user-avatar">{initial}</span>
                <div className="member-row-info">
                  <span className="member-row-name">{m.displayName}{isSelf ? " (you)" : ""}</span>
                  <span className={`pill role-pill-inline${m.role === "Owner" ? "" : " member-pill"}`}>
                    {m.role === "Owner" ? "Administrator" : "Member"}
                  </span>
                </div>
                {isAdmin && m.role !== "Owner" && (
                  confirmingId === m.userId ? (
                    <div className="member-row-confirm">
                      <button type="button" className="link-button member-confirm-yes" onClick={() => removeMutation.mutate(m.userId)} disabled={removeMutation.isPending}>
                        Confirm
                      </button>
                      <button type="button" className="link-button" onClick={() => setConfirmingId(null)}>Cancel</button>
                    </div>
                  ) : (
                    <button type="button" className="link-button member-remove-btn" onClick={() => setConfirmingId(m.userId)}>
                      Remove
                    </button>
                  )
                )}
              </div>
            );
          })}
        </div>

        <div className="modal-actions">
          <button type="button" onClick={onClose}>Close</button>
        </div>
      </div>
    </div>
  );
}
