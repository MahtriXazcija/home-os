using HomeOS.Domain.Common;

namespace HomeOS.Domain.Calendar;

public record CalendarEventCreatedEvent(Guid EventId, Guid HouseholdId, DateTime StartUtc, DateTime EndUtc) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
