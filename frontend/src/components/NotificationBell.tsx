import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getNotificationPreferences,
  getNotifications,
  markNotificationRead,
  setNotificationPreference,
  type NotificationCategory,
} from "../api/notifications";
import Icon from "./Icon";

const CATEGORY_LABELS: Record<NotificationCategory, string> = {
  ReminderFired: "Reminders",
  TaskAssigned: "Tasks assigned to you",
  BillDue: "Bills coming due",
  SharedWithYou: "Shared with you",
  ChatMessage: "Chat messages",
};

const HISTORY_LIMIT = 5;

function timeAgo(iso: string) {
  const diffMs = Date.now() - new Date(iso).getTime();
  const mins = Math.round(diffMs / 60000);
  if (mins < 1) return "just now";
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.round(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  return `${Math.round(hours / 24)}d ago`;
}

export default function NotificationBell() {
  const [open, setOpen] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const queryClient = useQueryClient();

  const { data: notifications } = useQuery({
    queryKey: ["notifications"],
    queryFn: getNotifications,
    refetchInterval: 60_000,
  });

  const { data: preferences } = useQuery({
    queryKey: ["notification-preferences"],
    queryFn: getNotificationPreferences,
    enabled: showSettings,
  });

  const markReadMutation = useMutation({
    mutationFn: markNotificationRead,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["notifications"] }),
  });

  const setPrefMutation = useMutation({
    mutationFn: ({ category, emailEnabled }: { category: NotificationCategory; emailEnabled: boolean }) =>
      setNotificationPreference(category, emailEnabled),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["notification-preferences"] }),
  });

  const unreadCount = (notifications ?? []).filter((n) => !n.isRead).length;
  const recent = (notifications ?? []).slice(0, HISTORY_LIMIT);

  return (
    <div className="notification-bell">
      <button type="button" className="bell-button" onClick={() => setOpen(!open)}>
        <Icon name="bell" size={17} />
        {unreadCount > 0 && <span className="bell-badge">{unreadCount}</span>}
      </button>

      {open && (
        <div className="notification-panel">
          <div className="notification-panel-head">
            <span>Notifications{unreadCount > 0 ? ` · ${unreadCount} new` : ""}</span>
            <button type="button" className="link-button" onClick={() => setShowSettings(!showSettings)}>
              {showSettings ? "Back" : "Settings"}
            </button>
          </div>

          {!showSettings && (
            <div className="notification-list">
              {recent.length === 0 && <p className="empty">Nothing yet.</p>}
              {recent.map((n) => (
                <button
                  type="button"
                  key={n.id}
                  className={`notification-item${n.isRead ? "" : " unread"}`}
                  onClick={() => !n.isRead && markReadMutation.mutate(n.id)}
                >
                  {!n.isRead && <span className="notification-dot" />}
                  <div className="notification-item-body">
                    <div className="notification-item-title">{n.title}</div>
                    {n.message && <div className="notification-item-message">{n.message}</div>}
                    <div className="notification-item-time">{timeAgo(n.createdAtUtc)}</div>
                  </div>
                </button>
              ))}
            </div>
          )}

          {showSettings && (
            <div className="notification-settings">
              <p className="dek" style={{ marginBottom: 10 }}>Which categories email you.</p>
              {(preferences ?? []).map((p) => (
                <label key={p.category} className="preference-row">
                  <input
                    type="checkbox"
                    checked={p.emailEnabled}
                    onChange={(e) => setPrefMutation.mutate({ category: p.category, emailEnabled: e.target.checked })}
                  />
                  {CATEGORY_LABELS[p.category]}
                </label>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
