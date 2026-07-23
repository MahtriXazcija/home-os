using HomeOS.Application.Common;
using MediatR;
using CalendarEventEntity = HomeOS.Domain.Calendar.CalendarEvent;

namespace HomeOS.Application.Calendar.Commands;

public record CreateCalendarEventCommand(
    Guid HouseholdId,
    string Title,
    string? Description,
    DateTime StartUtc,
    DateTime EndUtc,
    bool IsAllDay,
    Guid CreatedByUserId) : IRequest<CalendarEventDto>;

public class CreateCalendarEventCommandHandler(IAppDbContext db) : IRequestHandler<CreateCalendarEventCommand, CalendarEventDto>
{
    public async Task<CalendarEventDto> Handle(CreateCalendarEventCommand request, CancellationToken cancellationToken)
    {
        var evt = CalendarEventEntity.Create(
            request.HouseholdId, request.Title, request.StartUtc, request.EndUtc, request.IsAllDay, request.CreatedByUserId, request.Description);

        db.CalendarEvents.Add(evt);
        await db.SaveChangesAsync(cancellationToken);

        return CalendarEventDto.From(evt);
    }
}
