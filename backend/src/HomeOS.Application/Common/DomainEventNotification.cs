using HomeOS.Domain.Common;
using MediatR;

namespace HomeOS.Application.Common;

/// <summary>
/// Wraps a domain event so it can travel through the MediatR pipeline as a
/// notification — this is the in-process event bus described in
/// docs/app-sdk.md. Dispatched by HomeOsDbContext.SaveChangesAsync after a
/// successful save.
/// </summary>
public class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
