using HomeOS.Domain.Common;

namespace HomeOS.Domain.LifeAdmin;

// See docs/app-sdk.md §2 ("document.renewalDue").
public record DocumentRenewalScheduledEvent(Guid DocumentId, Guid HouseholdId, string Title, DateTime RenewalDateUtc, Guid CreatedByUserId) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
