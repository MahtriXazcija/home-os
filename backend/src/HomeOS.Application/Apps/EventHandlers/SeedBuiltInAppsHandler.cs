using HomeOS.Application.Common;
using HomeOS.Domain.Apps;
using HomeOS.Domain.Households;
using MediatR;

namespace HomeOS.Application.Apps.EventHandlers;

/// <summary>
/// A new household starts with the full built-in suite installed — proving
/// "a new app installs the same way the built-in ones do" (docs/app-sdk.md §1)
/// means the built-ins have to go through AppInstallation too, not just be
/// implicitly always-on. Each app is granted exactly the permissions its own
/// manifest declares.
/// </summary>
public class SeedBuiltInAppsHandler(IAppDbContext db) : INotificationHandler<DomainEventNotification<HouseholdCreatedEvent>>
{
    public async Task Handle(DomainEventNotification<HouseholdCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var householdId = notification.DomainEvent.HouseholdId;

        foreach (var manifest in AppCatalog.All)
        {
            db.AppInstallations.Add(AppInstallation.Create(householdId, manifest.Id, [.. manifest.Permissions]));
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
