import { useState, type FormEvent } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { createHousehold, acceptInvitation } from "../api/household";
import { ApiError } from "../api/client";
import { useAuth } from "../auth/AuthContext";

export default function Onboarding() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [searchParams] = useSearchParams();

  const [householdName, setHouseholdName] = useState("");
  const [inviteToken, setInviteToken] = useState(searchParams.get("token") ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const household = await createHousehold(householdName);
      queryClient.setQueryData(["my-household"], household);
      navigate("/");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not create household.");
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleJoin(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const household = await acceptInvitation(inviteToken.trim());
      queryClient.setQueryData(["my-household"], household);
      navigate("/");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Could not join household — check the invite link.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="auth-screen">
      <div className="onboarding-stack">
        <div className="onboarding-header">
          <div>
            <h1>Welcome, {user?.displayName}</h1>
            <p className="dek">You're not part of a household yet.</p>
          </div>
          <button className="link-button" onClick={logout} type="button">Sign out</button>
        </div>

        <div className="onboarding-cards">
          <form className="auth-card" onSubmit={handleCreate}>
            <h2>Start a new household</h2>
            <label>
              Household name
              <input
                value={householdName}
                onChange={(e) => setHouseholdName(e.target.value)}
                placeholder="e.g. The Hodžić Home"
                required
                autoFocus
              />
            </label>
            <button type="submit" disabled={isSubmitting}>Create household</button>
          </form>

          <form className="auth-card" onSubmit={handleJoin}>
            <h2>Join with an invite link</h2>
            <label>
              Invite token
              <input
                value={inviteToken}
                onChange={(e) => setInviteToken(e.target.value)}
                placeholder="Paste the token a member sent you"
                required
              />
            </label>
            <button type="submit" disabled={isSubmitting}>Join household</button>
          </form>
        </div>

        {error && <p className="auth-error">{error}</p>}
      </div>
    </div>
  );
}
