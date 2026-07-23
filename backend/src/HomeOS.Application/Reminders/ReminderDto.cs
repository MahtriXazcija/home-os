using HomeOS.Domain.Reminders;

namespace HomeOS.Application.Reminders;

public record ReminderDto(
    Guid Id,
    Guid HouseholdId,
    Guid TargetUserId,
    string Title,
    string? Message,
    DateTime RemindAtUtc,
    string Recurrence,
    string? SourceType,
    Guid? SourceId,
    bool IsFired)
{
    public static ReminderDto From(Reminder r) => new(
        r.Id, r.HouseholdId, r.TargetUserId, r.Title, r.Message, r.RemindAtUtc,
        r.Recurrence.ToString(), r.SourceType, r.SourceId, r.IsFired);
}
