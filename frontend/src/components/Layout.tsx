import { useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getMyHousehold, inviteMember } from "../api/household";
import { useAuth } from "../auth/AuthContext";
import NotificationBell from "./NotificationBell";

const NAV_ITEMS = [
  { to: "/", label: "Dashboard", end: true },
  { to: "/tasks", label: "Tasks" },
  { to: "/kanban", label: "Kanban" },
  { to: "/calendar", label: "Calendar" },
  { to: "/reminders", label: "Reminders" },
  { to: "/notes", label: "Notes" },
  { to: "/finance", label: "Finance" },
  { to: "/life-admin", label: "Life Admin" },
];

export default function Layout() {
  const { user, logout } = useAuth();
  const { data: household } = useQuery({ queryKey: ["my-household"], queryFn: getMyHousehold });
  const [inviteLink, setInviteLink] = useState<string | null>(null);
  const [isInviting, setIsInviting] = useState(false);

  async function handleInvite() {
    if (!household) return;
    const email = window.prompt("Invite by email:");
    if (!email) return;
    setIsInviting(true);
    try {
      const invitation = await inviteMember(household.id, email);
      const link = `${window.location.origin}/onboarding?token=${invitation.token}`;
      setInviteLink(link);
      await navigator.clipboard.writeText(link).catch(() => {});
    } catch (err) {
      window.alert(err instanceof Error ? err.message : "Could not create invite.");
    } finally {
      setIsInviting(false);
    }
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">Home OS</div>
        <nav>
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-footer">
          {household && (
            <div className="household-info">
              <div className="household-name">{household.name}</div>
              <div className="household-members">{household.members.length} member{household.members.length === 1 ? "" : "s"}</div>
              <button type="button" className="link-button" onClick={handleInvite} disabled={isInviting}>
                {isInviting ? "Inviting…" : "Invite a member"}
              </button>
              {inviteLink && <p className="invite-link-hint">Link copied — share it manually (invites aren't emailed yet).</p>}
            </div>
          )}
          <div className="user-info">
            <span>{user?.displayName}</span>
            <button type="button" className="link-button" onClick={logout}>Sign out</button>
          </div>
        </div>
      </aside>
      <div className="main-column">
        <header className="topbar">
          <input className="quick-capture" placeholder="Quick capture — add a task, note, or reminder…" />
          <NotificationBell />
        </header>
        <main className="content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
