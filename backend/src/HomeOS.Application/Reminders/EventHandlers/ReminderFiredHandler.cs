using HomeOS.Application.Common;
using HomeOS.Application.Notifications;
using HomeOS.Domain.Notifications;
using HomeOS.Domain.Reminders;
using MediatR;

namespace HomeOS.Application.Reminders.EventHandlers;

public class ReminderFiredHandler(INotificationDispatcher dispatcher) : INotificationHandler<DomainEventNotification<ReminderFiredEvent>>
{
    public Task Handle(DomainEventNotification<ReminderFiredEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        return dispatcher.DispatchAsync(evt.TargetUserId, evt.HouseholdId, NotificationCategory.ReminderFired, evt.Title, evt.Message, cancellationToken);
    }
}
