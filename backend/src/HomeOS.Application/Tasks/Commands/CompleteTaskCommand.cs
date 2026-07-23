using HomeOS.Application.Common;
using HomeOS.Domain.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Tasks.Commands;

public record CompleteTaskCommand(Guid TaskId, Guid CompletedByUserId) : IRequest<TaskDto>;

public class CompleteTaskCommandHandler(IAppDbContext db) : IRequestHandler<CompleteTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        var nextDueDate = task.NextDueDate();
        task.Complete(request.CompletedByUserId);

        if (nextDueDate is not null)
        {
            var next = TaskItem.Create(
                task.HouseholdId,
                task.Title,
                task.CreatedByUserId,
                task.Description,
                nextDueDate,
                task.Priority,
                task.AssignedToUserId,
                task.BoardId,
                task.ParentTaskId,
                task.Recurrence,
                task.Tags);
            db.Tasks.Add(next);
        }

        await db.SaveChangesAsync(cancellationToken);
        return TaskDto.From(task);
    }
}
