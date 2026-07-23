using HomeOS.Domain.Common;

namespace HomeOS.Domain.Households;

// See docs/app-sdk.md §2 — these are the events other apps can subscribe to.
public record HouseholdCreatedEvent(Guid HouseholdId, Guid CreatedByUserId) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}

public record MemberInvitedEvent(Guid HouseholdId, Guid InvitationId, string Email, Guid InvitedByUserId) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}

public record MemberJoinedEvent(Guid HouseholdId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
