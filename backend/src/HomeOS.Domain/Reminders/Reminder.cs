using HomeOS.Domain.Common;

namespace HomeOS.Domain.Reminders;

public class Reminder : AggregateRoot
{
    public Guid HouseholdId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Message { get; private set; }
    public DateTime RemindAtUtc { get; private set; }
    public ReminderRecurrence Recurrence { get; private set; }

    /// <summary>Where this reminder came from — "task", "bill", "manual", an app id, etc. Both optional: a reminder need not point anywhere.</summary>
    public string? SourceType { get; private set; }
    public Guid? SourceId { get; private set; }

    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public bool IsFired { get; private set; }
    public DateTime? FiredAtUtc { get; private set; }

    private Reminder() { }

    public static Reminder Create(
        Guid householdId,
        Guid targetUserId,
        string title,
        DateTime remindAtUtc,
        Guid createdByUserId,
        string? message = null,
        ReminderRecurrence recurrence = ReminderRecurrence.None,
        string? sourceType = null,
        Guid? sourceId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Reminder title is required.", nameof(title));
        }

        return new Reminder
        {
            HouseholdId = householdId,
            TargetUserId = targetUserId,
            Title = title.Trim(),
            Message = message,
            RemindAtUtc = remindAtUtc,
            Recurrence = recurrence,
            SourceType = sourceType,
            SourceId = sourceId,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Fire()
    {
        if (IsFired) return;
        IsFired = true;
        FiredAtUtc = DateTime.UtcNow;
        Raise(new ReminderFiredEvent(Id, HouseholdId, TargetUserId, Title, Message));
    }

    public DateTime? NextRemindAt() => Recurrence switch
    {
        ReminderRecurrence.Daily => RemindAtUtc.AddDays(1),
        ReminderRecurrence.Weekly => RemindAtUtc.AddDays(7),
        ReminderRecurrence.Monthly => RemindAtUtc.AddMonths(1),
        _ => null
    };
}
