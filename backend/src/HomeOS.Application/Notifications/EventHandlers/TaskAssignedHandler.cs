using HomeOS.Application.Common;
using HomeOS.Domain.Notifications;
using HomeOS.Domain.Tasks;
using MediatR;

namespace HomeOS.Application.Notifications.EventHandlers;

/// <summary>
/// Notifications subscribing to Tasks' event rather than Tasks calling into
/// Notifications directly — this is "apps cooperate without knowing about
/// each other" from docs/app-sdk.md in practice, using the same
/// DomainEventNotification&lt;T&gt; bus a third-party app would.
/// </summary>
public class TaskAssignedHandler(INotificationDispatcher dispatcher) : INotificationHandler<DomainEventNotification<TaskCreatedEvent>>
{
    public Task Handle(DomainEventNotification<TaskCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        if (evt.AssignedToUserId is null) return Task.CompletedTask;

        var due = evt.DueDateUtc is null ? "" : $" (due {evt.DueDateUtc:MMM d})";
        return dispatcher.DispatchAsync(
            evt.AssignedToUserId.Value, evt.HouseholdId, NotificationCategory.TaskAssigned,
            $"Assigned to you: {evt.Title}", $"{evt.Title}{due}", cancellationToken);
    }
}
