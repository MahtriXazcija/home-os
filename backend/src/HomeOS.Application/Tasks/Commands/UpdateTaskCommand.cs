using HomeOS.Application.Common;
using HomeOS.Domain.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Tasks.Commands;

public record UpdateTaskCommand(
    Guid TaskId,
    string Title,
    string? Description,
    DateTime? DueDateUtc,
    TaskPriority Priority,
    Guid? AssignedToUserId,
    RecurrenceRule Recurrence,
    List<string> Tags) : IRequest<TaskDto>;

public class UpdateTaskCommandHandler(IAppDbContext db) : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        task.UpdateDetails(request.Title, request.Description, request.DueDateUtc, request.Priority, request.AssignedToUserId, request.Recurrence, request.Tags);
        await db.SaveChangesAsync(cancellationToken);

        return TaskDto.From(task);
    }
}
