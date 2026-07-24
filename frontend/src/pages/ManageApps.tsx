import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useApps } from "../hooks/useApps";
import { installApp, uninstallApp } from "../api/apps";

export default function ManageApps() {
  const { data: apps, householdId } = useApps();
  const queryClient = useQueryClient();

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["apps", householdId] });

  const installMutation = useMutation({
    mutationFn: ({ id, permissions }: { id: string; permissions: string[] }) => installApp(id, householdId, permissions),
    onSuccess: invalidate,
  });
  const uninstallMutation = useMutation({
    mutationFn: (id: string) => uninstallApp(id, householdId),
    onSuccess: invalidate,
  });

  return (
    <div>
      <h1>Manage apps</h1>
      <p className="dek">
        Every module here — built-in or not — installs through the same manifest and only reaches the
        permissions your household grants it. Dashboard and Tasks are core and stay installed.
      </p>

      <div className="app-grid">
        {(apps ?? []).map((app) => (
          <div key={app.id} className="app-card">
            <div className="app-card-head">
              <span className="app-card-name">{app.name}</span>
              {app.isCore ? (
                <span className="pill priority-low">Core</span>
              ) : app.isInstalled ? (
                <button type="button" className="link-button" onClick={() => uninstallMutation.mutate(app.id)}>Uninstall</button>
              ) : (
                <button type="button" className="link-button" onClick={() => installMutation.mutate({ id: app.id, permissions: app.permissions })}>Install</button>
              )}
            </div>
            <p className="app-card-desc">{app.description}</p>
            <div className="app-card-permissions">
              {app.permissions.map((p) => (
                <span key={p} className={`tag${app.isInstalled && app.grantedPermissions.includes(p) ? " tag-granted" : ""}`}>{p}</span>
              ))}
            </div>
            {app.publishes.length > 0 && (
              <div className="app-card-events">publishes: {app.publishes.join(", ")}</div>
            )}
            {app.subscribes.length > 0 && (
              <div className="app-card-events">subscribes: {app.subscribes.join(", ")}</div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
