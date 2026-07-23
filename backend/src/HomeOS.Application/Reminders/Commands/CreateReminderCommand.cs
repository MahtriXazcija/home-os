using HomeOS.Application.Common;
using HomeOS.Domain.Reminders;
using MediatR;

namespace HomeOS.Application.Reminders.Commands;

public record CreateReminderCommand(
    Guid HouseholdId,
    Guid TargetUserId,
    string Title,
    DateTime RemindAtUtc,
    Guid CreatedByUserId,
    string? Message,
    ReminderRecurrence Recurrence,
    string? SourceType,
    Guid? SourceId) : IRequest<ReminderDto>;

public class CreateReminderCommandHandler(IAppDbContext db) : IRequestHandler<CreateReminderCommand, ReminderDto>
{
    public async Task<ReminderDto> Handle(CreateReminderCommand request, CancellationToken cancellationToken)
    {
        var reminder = Reminder.Create(
            request.HouseholdId, request.TargetUserId, request.Title, request.RemindAtUtc,
            request.CreatedByUserId, request.Message, request.Recurrence, request.SourceType, request.SourceId);

        db.Reminders.Add(reminder);
        await db.SaveChangesAsync(cancellationToken);

        return ReminderDto.From(reminder);
    }
}
