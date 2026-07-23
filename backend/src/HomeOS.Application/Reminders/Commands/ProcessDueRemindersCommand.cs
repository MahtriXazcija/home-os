using HomeOS.Application.Common;
using HomeOS.Domain.Reminders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Reminders.Commands;

/// <summary>Sent on a timer by the background scanner (Infrastructure) — see ReminderSchedulerService.</summary>
public record ProcessDueRemindersCommand : IRequest<int>;

public class ProcessDueRemindersCommandHandler(IAppDbContext db) : IRequestHandler<ProcessDueRemindersCommand, int>
{
    public async Task<int> Handle(ProcessDueRemindersCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var due = await db.Reminders
            .Where(r => !r.IsFired && r.RemindAtUtc <= now)
            .ToListAsync(cancellationToken);

        foreach (var reminder in due)
        {
            var next = reminder.NextRemindAt();
            reminder.Fire();

            if (next is not null)
            {
                var recurring = Reminder.Create(
                    reminder.HouseholdId, reminder.TargetUserId, reminder.Title, next.Value,
                    reminder.CreatedByUserId, reminder.Message, reminder.Recurrence, reminder.SourceType, reminder.SourceId);
                db.Reminders.Add(recurring);
            }
        }

        if (due.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return due.Count;
    }
}
