namespace HomeOS.Domain.Notifications;

/// <summary>Matches the categories a member can toggle on/off for email, per the task brief.</summary>
public enum NotificationCategory
{
    ReminderFired,
    TaskAssigned,
    BillDue,
    SharedWithYou,
    ChatMessage
}
