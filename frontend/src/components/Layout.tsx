import { useEffect, useState } from "react";
import { NavLink, Outlet, useLocation } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getMyHousehold } from "../api/household";
import { useAuth } from "../auth/AuthContext";
import { useApps } from "../hooks/useApps";
import { useChatUnread } from "../hooks/useChatUnread";
import NotificationBell from "./NotificationBell";
import CommandPalette from "./CommandPalette";
import InviteMemberModal from "./InviteMemberModal";
import MembersModal from "./MembersModal";
import Icon, { type IconName } from "./Icon";

const SIDEBAR_COLLAPSED_KEY = "homeos.sidebarCollapsed";

export default function Layout() {
  const { user, logout } = useAuth();
  const { data: household } = useQuery({ queryKey: ["my-household"], queryFn: getMyHousehold });
  const { data: apps } = useApps();
  const [inviteOpen, setInviteOpen] = useState(false);
  const [membersOpen, setMembersOpen] = useState(false);
  const [paletteOpen, setPaletteOpen] = useState(false);
  const [mobileNavOpen, setMobileNavOpen] = useState(false);
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem(SIDEBAR_COLLAPSED_KEY) !== "false");
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

  useEffect(() => {
    localStorage.setItem(SIDEBAR_COLLAPSED_KEY, String(collapsed));
  }, [collapsed]);

  // Nav is driven by which apps are installed — this is the same registry
  // a third-party app would register into, not a hardcoded list per app.
  const installedApps = (apps ?? []).filter((a) => a.isInstalled);
  const chatInstalled = installedApps.some((a) => a.id === "chat");
  const chatUnread = useChatUnread(household?.id ?? "", chatInstalled);

  const myRole = household?.members.find((m) => m.userId === user?.userId)?.role;
  const myRoleLabel = myRole === "Owner" ? "Administrator" : myRole === "Member" ? "Member" : null;
  const initial = (user?.displayName ?? "?").trim().charAt(0).toUpperCase();

  return (
    <div className={`app-shell${collapsed ? " sidebar-collapsed" : ""}`}>
      <div className={`sidebar-backdrop${mobileNavOpen ? " open" : ""}`} onClick={() => setMobileNavOpen(false)} />
      <aside className={`sidebar${mobileNavOpen ? " open" : ""}${collapsed ? " collapsed" : ""}`}>
        <div className="brand">
          <span className="brand-mark"><Icon name="home" size={15} /></span>
          <span className="brand-text">Home OS</span>
          <button
            type="button"
            className="sidebar-toggle"
            onClick={() => setCollapsed(!collapsed)}
            aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
            title={collapsed ? "Expand" : "Collapse"}
          >
            <Icon name={collapsed ? "chevrons-right" : "chevrons-left"} size={15} />
          </button>
        </div>
        <nav>
          {installedApps.map((app) => (
            <NavLink
              key={app.id}
              to={app.navRoute}
              end={app.navRoute === "/"}
              className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
              title={app.navLabel}
            >
              <span className="app-icon"><Icon name={app.icon as IconName} /></span>
              <span className="nav-link-label">{app.navLabel}</span>
              {app.id === "chat" && chatUnread > 0 && <span className="nav-badge">{chatUnread}</span>}
            </NavLink>
          ))}
          <NavLink to="/apps" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")} title="Manage apps">
            <span className="app-icon"><Icon name="grid" /></span>
            <span className="nav-link-label">Manage apps</span>
          </NavLink>
          <NavLink to="/profile" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")} title="Profile">
            <span className="app-icon"><Icon name="user" /></span>
            <span className="nav-link-label">Profile</span>
          </NavLink>
        </nav>

        <div className="sidebar-footer">
          {household && (
            <div className="household-info">
              <div className="household-name">{household.name}</div>
              <button type="button" className="link-button household-members-btn" onClick={() => setMembersOpen(true)}>
                <Icon name="users" size={13} />
                {household.members.length} member{household.members.length === 1 ? "" : "s"}
                {myRoleLabel && <span className="pill role-pill-inline">{myRoleLabel}</span>}
              </button>
              <button type="button" className="link-button" onClick={() => setInviteOpen(true)}>
                <Icon name="mail" size={13} />
                <span className="nav-link-label">Invite a member</span>
              </button>
            </div>
          )}
          <div className="user-info">
            <span className="user-avatar" title={user?.displayName}>{initial}</span>
            <span className="nav-link-label">{user?.displayName}</span>
            <button type="button" className="link-button" onClick={logout} title="Sign out">
              <Icon name="log-out" size={13} />
              <span className="nav-link-label">Sign out</span>
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
      {household && (
        <MembersModal
          open={membersOpen}
          household={household}
          currentUserId={user?.userId ?? ""}
          isAdmin={myRole === "Owner"}
          onClose={() => setMembersOpen(false)}
        />
      )}
    </div>
  );
}
