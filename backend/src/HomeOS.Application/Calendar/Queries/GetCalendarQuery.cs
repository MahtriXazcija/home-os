using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Calendar.Queries;

public record GetCalendarQuery(Guid HouseholdId, DateTime FromUtc, DateTime ToUtc) : IRequest<List<CalendarItemDto>>;

public class GetCalendarQueryHandler(IAppDbContext db) : IRequestHandler<GetCalendarQuery, List<CalendarItemDto>>
{
    public async Task<List<CalendarItemDto>> Handle(GetCalendarQuery request, CancellationToken cancellationToken)
    {
        var events = await db.CalendarEvents
            .Where(e => e.HouseholdId == request.HouseholdId && e.StartUtc < request.ToUtc && e.EndUtc >= request.FromUtc)
            .Select(e => new CalendarItemDto(e.Id, "event", e.Title, e.StartUtc, e.EndUtc, e.IsAllDay))
            .ToListAsync(cancellationToken);

        var dueTasks = await db.Tasks
            .Where(t => t.HouseholdId == request.HouseholdId
                && t.DueDateUtc != null
                && t.DueDateUtc >= request.FromUtc
                && t.DueDateUtc < request.ToUtc)
            .Select(t => new CalendarItemDto(t.Id, "task", t.Title, t.DueDateUtc!.Value, t.DueDateUtc!.Value, true))
            .ToListAsync(cancellationToken);

        return [.. events, .. dueTasks];
    }
}
