namespace HomeOS.Domain.Reminders;

/// <summary>
/// Deliberately its own enum rather than reusing Tasks.RecurrenceRule — Reminders is a
/// platform capability (see docs/app-sdk.md §3), and platform capabilities shouldn't
/// depend on a specific built-in app's types.
/// </summary>
public enum ReminderRecurrence
{
    None,
    Daily,
    Weekly,
    Monthly
}
