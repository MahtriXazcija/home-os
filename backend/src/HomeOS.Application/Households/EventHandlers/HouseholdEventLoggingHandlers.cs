using HomeOS.Application.Common;
using HomeOS.Domain.Households;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeOS.Application.Households.EventHandlers;

// Placeholder subscribers proving the event bus works end-to-end (see
// docs/app-sdk.md §2). Real apps built on the platform subscribe the same
// way — through a DomainEventNotification<T> handler, never a direct call
// into the Households module.

public class HouseholdCreatedLoggingHandler(ILogger<HouseholdCreatedLoggingHandler> logger)
    : INotificationHandler<DomainEventNotification<HouseholdCreatedEvent>>
{
    public Task Handle(DomainEventNotification<HouseholdCreatedEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "household.created: {HouseholdId} by {UserId}",
            notification.DomainEvent.HouseholdId,
            notification.DomainEvent.CreatedByUserId);
        return Task.CompletedTask;
    }
}

public class MemberJoinedLoggingHandler(ILogger<MemberJoinedLoggingHandler> logger)
    : INotificationHandler<DomainEventNotification<MemberJoinedEvent>>
{
    public Task Handle(DomainEventNotification<MemberJoinedEvent> notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "member.joined: household {HouseholdId}, user {UserId}",
            notification.DomainEvent.HouseholdId,
            notification.DomainEvent.UserId);
        return Task.CompletedTask;
    }
}
