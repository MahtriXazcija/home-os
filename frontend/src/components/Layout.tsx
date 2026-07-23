import { NavLink, Outlet } from "react-router-dom";

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
      </aside>
      <div className="main-column">
        <header className="topbar">
          <input className="quick-capture" placeholder="Quick capture — add a task, note, or reminder…" />
        </header>
        <main className="content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
