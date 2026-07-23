using HomeOS.Application.Common;
using HomeOS.Domain.LifeAdmin;
using HomeOS.Domain.Reminders;
using MediatR;

namespace HomeOS.Application.LifeAdmin.EventHandlers;

/// <summary>"Renewal and expiry dates that trigger reminders automatically" — via the existing Reminders capability, not bespoke logic.</summary>
public class DocumentRenewalHandler(IAppDbContext db) : INotificationHandler<DomainEventNotification<DocumentRenewalScheduledEvent>>
{
    private const int ReminderDaysBefore = 7;

    public async Task Handle(DomainEventNotification<DocumentRenewalScheduledEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        var remindAt = evt.RenewalDateUtc.AddDays(-ReminderDaysBefore);
        if (remindAt <= DateTime.UtcNow) return;

        var reminder = Reminder.Create(
            evt.HouseholdId, evt.CreatedByUserId, $"Renews soon: {evt.Title}", remindAt, evt.CreatedByUserId,
            message: $"Renewal date {evt.RenewalDateUtc:MMM d}", sourceType: "document", sourceId: evt.DocumentId);
        db.Reminders.Add(reminder);

        await db.SaveChangesAsync(cancellationToken);
    }
}
