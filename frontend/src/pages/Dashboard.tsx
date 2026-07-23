import { useQuery } from "@tanstack/react-query";
import { apiGet, type ApiStatus } from "../api/client";

export default function Dashboard() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ["api-status"],
    queryFn: () => apiGet<ApiStatus>("/api/status"),
  });

  return (
    <div>
      <h1>Today</h1>
      <p className="dek">Everything due, happening, or coming up — in one place.</p>

      <div className="card-grid">
        <section className="card">
          <h2>Tasks due</h2>
          <p className="empty">No tasks yet — this ships in Phase 2.</p>
        </section>
        <section className="card">
          <h2>Today's events</h2>
          <p className="empty">No events yet — this ships in Phase 2.</p>
        </section>
        <section className="card">
          <h2>Upcoming bills</h2>
          <p className="empty">No bills yet — this ships in Phase 4.</p>
        </section>
        <section className="card">
          <h2>Active reminders</h2>
          <p className="empty">No reminders yet — this ships in Phase 3.</p>
        </section>
      </div>

      <div className="status-strip">
        {isLoading && <span>Connecting to API…</span>}
        {isError && <span className="status-bad">API unreachable — is the backend running?</span>}
        {data && (
          <span className="status-ok">
            Connected to {data.service} · {new Date(data.utc).toLocaleTimeString()}
          </span>
        )}
      </div>
    </div>
  );
}
