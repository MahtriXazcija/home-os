using HomeOS.Application.Common;
using HomeOS.Domain.Tasks;
using MediatR;

namespace HomeOS.Application.Tasks.Commands;

public record CreateTaskCommand(
    Guid HouseholdId,
    string Title,
    Guid CreatedByUserId,
    string? Description,
    DateTime? DueDateUtc,
    TaskPriority Priority,
    Guid? AssignedToUserId,
    Guid? BoardId,
    Guid? ParentTaskId,
    RecurrenceRule Recurrence,
    List<string> Tags) : IRequest<TaskDto>;

public class CreateTaskCommandHandler(IAppDbContext db) : IRequestHandler<CreateTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = TaskItem.Create(
            request.HouseholdId,
            request.Title,
            request.CreatedByUserId,
            request.Description,
            request.DueDateUtc,
            request.Priority,
            request.AssignedToUserId,
            request.BoardId,
            request.ParentTaskId,
            request.Recurrence,
            request.Tags);

        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);

        return TaskDto.From(task);
    }
}
