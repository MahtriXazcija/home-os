using HomeOS.Domain.Common;

namespace HomeOS.Domain.Reminders;

// See docs/app-sdk.md §2. Any app can raise a reminder for a member; the
// platform (not the app) is responsible for firing it, notifying in-app,
// and emailing if the member opted in — apps never build their own.
public record ReminderFiredEvent(Guid ReminderId, Guid HouseholdId, Guid TargetUserId, string Title, string? Message) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
