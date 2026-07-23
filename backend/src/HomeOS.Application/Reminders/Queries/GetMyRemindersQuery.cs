using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Reminders.Queries;

public record GetMyRemindersQuery(Guid UserId) : IRequest<List<ReminderDto>>;

public class GetMyRemindersQueryHandler(IAppDbContext db) : IRequestHandler<GetMyRemindersQuery, List<ReminderDto>>
{
    public async Task<List<ReminderDto>> Handle(GetMyRemindersQuery request, CancellationToken cancellationToken)
    {
        var reminders = await db.Reminders
            .Where(r => r.TargetUserId == request.UserId && !r.IsFired)
            .OrderBy(r => r.RemindAtUtc)
            .ToListAsync(cancellationToken);

        return reminders.Select(ReminderDto.From).ToList();
    }
}
