import { useEffect, useState } from "react";
import { NavLink, Outlet, useLocation } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getMyHousehold } from "../api/household";
import { useAuth } from "../auth/AuthContext";
import { useApps } from "../hooks/useApps";
import NotificationBell from "./NotificationBell";
import CommandPalette from "./CommandPalette";
import InviteMemberModal from "./InviteMemberModal";
import Icon, { type IconName } from "./Icon";

export default function Layout() {
  const { user, logout } = useAuth();
  const { data: household } = useQuery({ queryKey: ["my-household"], queryFn: getMyHousehold });
  const { data: apps } = useApps();
  const [inviteOpen, setInviteOpen] = useState(false);
  const [paletteOpen, setPaletteOpen] = useState(false);
  const [mobileNavOpen, setMobileNavOpen] = useState(false);
  const location = useLocation();

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

  // Close the mobile drawer whenever navigation happens.
  useEffect(() => {
    setMobileNavOpen(false);
  }, [location.pathname]);

  // Nav is driven by which apps are installed — this is the same registry
  // a third-party app would register into, not a hardcoded list per app.
  const installedApps = (apps ?? []).filter((a) => a.isInstalled);

  const myRole = household?.members.find((m) => m.userId === user?.userId)?.role;
  const myRoleLabel = myRole === "Owner" ? "Administrator" : myRole === "Member" ? "Member" : null;

  return (
    <div className="app-shell">
      <div className={`sidebar-backdrop${mobileNavOpen ? " open" : ""}`} onClick={() => setMobileNavOpen(false)} />
      <aside className={`sidebar${mobileNavOpen ? " open" : ""}`}>
        <div className="brand">
          <span className="brand-mark"><Icon name="home" size={15} /></span>
          Home OS
        </div>
        <nav>
          {installedApps.map((app) => (
            <NavLink
              key={app.id}
              to={app.navRoute}
              end={app.navRoute === "/"}
              className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
            >
              <span className="app-icon"><Icon name={app.icon as IconName} /></span>
              {app.navLabel}
            </NavLink>
          ))}
          <NavLink to="/apps" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}>
            <span className="app-icon"><Icon name="grid" /></span>
            Manage apps
          </NavLink>
          <NavLink to="/profile" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}>
            <span className="app-icon"><Icon name="user" /></span>
            Profile
          </NavLink>
        </nav>

        <div className="sidebar-footer">
          {household && (
            <div className="household-info">
              <div className="household-name">{household.name}</div>
              <div className="household-members">
                {household.members.length} member{household.members.length === 1 ? "" : "s"}
                {myRoleLabel && <span className="pill role-pill-inline">{myRoleLabel}</span>}
              </div>
              <button type="button" className="link-button" onClick={() => setInviteOpen(true)}>
                <Icon name="mail" size={13} />
                Invite a member
              </button>
            </div>
          )}
          <div className="user-info">
            <span>{user?.displayName}</span>
            <button type="button" className="link-button" onClick={logout}>
              <Icon name="log-out" size={13} />
              Sign out
            </button>
          </div>
        </div>
      </aside>
      <div className="main-column">
        <header className="topbar">
          <button
            type="button"
            className="mobile-menu-button"
            onClick={() => setMobileNavOpen(true)}
            aria-label="Open menu"
          >
            <Icon name="menu" size={18} />
          </button>
          <div className="quick-capture-wrap">
            <Icon name="search" size={15} className="search-icon" />
            <input
              className="quick-capture"
              placeholder="Search or jump to… (⌘K)"
              onFocus={() => setPaletteOpen(true)}
              readOnly
            />
          </div>
          <NotificationBell />
        </header>
        <main className="content">
          <Outlet />
        </main>
      </div>
      <CommandPalette open={paletteOpen} onClose={() => setPaletteOpen(false)} />
      {household && (
        <InviteMemberModal open={inviteOpen} householdId={household.id} onClose={() => setInviteOpen(false)} />
      )}
    </div>
  );
}
