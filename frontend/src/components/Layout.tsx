import { useEffect, useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getMyHousehold, inviteMember } from "../api/household";
import { useAuth } from "../auth/AuthContext";
import { useApps } from "../hooks/useApps";
import NotificationBell from "./NotificationBell";
import CommandPalette from "./CommandPalette";

export default function Layout() {
  const { user, logout } = useAuth();
  const { data: household } = useQuery({ queryKey: ["my-household"], queryFn: getMyHousehold });
  const { data: apps } = useApps();
  const [inviteLink, setInviteLink] = useState<string | null>(null);
  const [isInviting, setIsInviting] = useState(false);
  const [paletteOpen, setPaletteOpen] = useState(false);

  useEffect(() => {
    function onKeyDown(e: KeyboardEvent) {
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        setPaletteOpen(true);
      }
    }
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, []);

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

  // Nav is driven by which apps are installed — this is the same registry
  // a third-party app would register into, not a hardcoded list per app.
  const installedApps = (apps ?? []).filter((a) => a.isInstalled);

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">Home OS</div>
        <nav>
          {installedApps.map((app) => (
            <NavLink
              key={app.id}
              to={app.navRoute}
              end={app.navRoute === "/"}
              className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
            >
              {app.navLabel}
            </NavLink>
          ))}
          <NavLink to="/apps" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}>
            Manage apps
          </NavLink>
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
          <input
            className="quick-capture"
            placeholder="Search or jump to… (⌘K)"
            onFocus={() => setPaletteOpen(true)}
            readOnly
          />
          <NotificationBell />
        </header>
        <main className="content">
          <Outlet />
        </main>
      </div>
      <CommandPalette open={paletteOpen} onClose={() => setPaletteOpen(false)} />
    </div>
  );
}
