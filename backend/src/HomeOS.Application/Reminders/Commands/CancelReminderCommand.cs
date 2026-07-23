using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Reminders.Commands;

public record CancelReminderCommand(Guid ReminderId) : IRequest;

public class CancelReminderCommandHandler(IAppDbContext db) : IRequestHandler<CancelReminderCommand>
{
    public async Task Handle(CancelReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = await db.Reminders.FirstOrDefaultAsync(r => r.Id == request.ReminderId, cancellationToken);
        if (reminder is null) return;

        db.Reminders.Remove(reminder);
        await db.SaveChangesAsync(cancellationToken);
    }
}
