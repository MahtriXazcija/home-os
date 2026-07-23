using HomeOS.Domain.Common;

namespace HomeOS.Domain.Finance;

// See docs/app-sdk.md §2 ("bill.dueSoon") — this is the literal "a bill can
// create a task" example from the task brief, done as an event a handler
// reacts to rather than Bill reaching into Tasks/Reminders directly.
public record BillCreatedEvent(Guid BillId, Guid HouseholdId, string Title, decimal Amount, DateTime DueDateUtc, Guid CreatedByUserId) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
