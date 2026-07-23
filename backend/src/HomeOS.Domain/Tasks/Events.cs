using HomeOS.Domain.Common;

namespace HomeOS.Domain.Tasks;

// See docs/app-sdk.md §2.
public record TaskCreatedEvent(Guid TaskId, Guid HouseholdId, string Title, DateTime? DueDateUtc, Guid? AssignedToUserId) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}

public record TaskCompletedEvent(Guid TaskId, Guid HouseholdId, Guid CompletedByUserId) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}

public record BoardCardMovedEvent(Guid TaskId, Guid HouseholdId, HomeTaskStatus FromStatus, HomeTaskStatus ToStatus) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
