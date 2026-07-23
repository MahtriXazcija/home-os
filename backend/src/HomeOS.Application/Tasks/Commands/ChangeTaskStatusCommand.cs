using HomeOS.Application.Common;
using HomeOS.Domain.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Tasks.Commands;

public record ChangeTaskStatusCommand(Guid TaskId, HomeTaskStatus NewStatus) : IRequest<TaskDto>;

public class ChangeTaskStatusCommandHandler(IAppDbContext db) : IRequestHandler<ChangeTaskStatusCommand, TaskDto>
{
    public async Task<TaskDto> Handle(ChangeTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        task.ChangeStatus(request.NewStatus);
        await db.SaveChangesAsync(cancellationToken);

        return TaskDto.From(task);
    }
}
