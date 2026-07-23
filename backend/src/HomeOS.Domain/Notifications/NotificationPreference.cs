using HomeOS.Domain.Common;

namespace HomeOS.Domain.Notifications;

/// <summary>
/// One row per (user, category) that the member has explicitly toggled off.
/// Absence of a row means "on" — every category defaults to opted-in, matching
/// the task brief's "shared by default where it makes sense" principle.
/// </summary>
public class NotificationPreference : Entity
{
    public Guid UserId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public bool EmailEnabled { get; private set; }

    private NotificationPreference() { }

    public static NotificationPreference Create(Guid userId, NotificationCategory category, bool emailEnabled)
    {
        return new NotificationPreference
        {
            UserId = userId,
            Category = category,
            EmailEnabled = emailEnabled
        };
    }

    public void SetEmailEnabled(bool enabled) => EmailEnabled = enabled;
}
