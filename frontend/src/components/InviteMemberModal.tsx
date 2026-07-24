import { useEffect, useState, type FormEvent } from "react";
import { inviteMember } from "../api/household";
import { ApiError } from "../api/client";
import Icon from "./Icon";

interface InviteMemberModalProps {
  open: boolean;
  householdId: string;
  onClose: () => void;
}

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export default function InviteMemberModal({ open, householdId, onClose }: InviteMemberModalProps) {
  const [email, setEmail] = useState("");
  const [fieldError, setFieldError] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [inviteLink, setInviteLink] = useState<string | null>(null);

  useEffect(() => {
    if (open) {
      setEmail("");
      setFieldError(null);
      setError(null);
      setInviteLink(null);
    }
  }, [open]);

  useEffect(() => {
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") onClose();
    }
    if (open) window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open, onClose]);

  if (!open) return null;

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setFieldError(null);

    const trimmed = email.trim();
    if (!EMAIL_PATTERN.test(trimmed)) {
      setFieldError("Enter a valid email address.");
      return;
    }

    setIsSubmitting(true);
    try {
      const invitation = await inviteMember(householdId, trimmed);
      const link = `${window.location.origin}/onboarding?token=${invitation.token}`;
      setInviteLink(link);
      await navigator.clipboard.writeText(link).catch(() => {});
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not create invite.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        {!inviteLink ? (
          <form onSubmit={handleSubmit}>
            <div className="modal-head">
              <span className="app-icon"><Icon name="mail" /></span>
              <h2>Invite a member</h2>
            </div>
            <p className="dek" style={{ margin: "0 0 16px" }}>
              We'll email them a link to join your household.
            </p>
            <label>
              Email address
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="name@example.com"
                autoFocus
                required
              />
              {fieldError && <span className="field-error">{fieldError}</span>}
            </label>
            {error && <p className="auth-error">{error}</p>}
            <div className="modal-actions">
              <button type="button" className="link-button modal-cancel" onClick={onClose}>Cancel</button>
              <button type="submit" disabled={isSubmitting}>{isSubmitting ? "Sending…" : "Send invite"}</button>
            </div>
          </form>
        ) : (
          <div>
            <div className="modal-head">
              <span className="app-icon"><Icon name="mail" /></span>
              <h2>Invite sent</h2>
            </div>
            <p className="dek" style={{ margin: "0 0 10px" }}>
              We emailed {email.trim()} a link to join. It's also copied to your clipboard:
            </p>
            <div className="invite-link-box">{inviteLink}</div>
            <div className="modal-actions">
              <button type="button" onClick={onClose}>Done</button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
