using HomeOS.Application.Common;
using HomeOS.Domain.Finance;
using HomeOS.Domain.Reminders;
using HomeOS.Domain.Tasks;
using MediatR;

namespace HomeOS.Application.Finance.EventHandlers;

/// <summary>
/// This is the task brief's own flagship example, literally: "a bill can
/// create a task." A new bill spawns a "Pay X" task (due the same day, so
/// it shows on Tasks/Kanban/Calendar) and a reminder a few days out — both
/// through the existing Tasks and Reminders capabilities, not bespoke
/// Finance-module code.
/// </summary>
public class BillCreatedHandler(IAppDbContext db) : INotificationHandler<DomainEventNotification<BillCreatedEvent>>
{
    private const int ReminderDaysBefore = 3;

    public async Task Handle(DomainEventNotification<BillCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var task = TaskItem.Create(
            evt.HouseholdId, $"Pay {evt.Title}", evt.CreatedByUserId,
            description: $"{evt.Amount:C}", dueDateUtc: evt.DueDateUtc);
        db.Tasks.Add(task);

        var remindAt = evt.DueDateUtc.AddDays(-ReminderDaysBefore);
        if (remindAt > DateTime.UtcNow)
        {
            var reminder = Reminder.Create(
                evt.HouseholdId, evt.CreatedByUserId, $"Bill due soon: {evt.Title}", remindAt, evt.CreatedByUserId,
                message: $"{evt.Amount:C} due {evt.DueDateUtc:MMM d}", sourceType: "bill", sourceId: evt.BillId);
            db.Reminders.Add(reminder);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
