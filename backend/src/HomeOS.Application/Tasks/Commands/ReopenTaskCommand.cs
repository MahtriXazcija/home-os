using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Tasks.Commands;

public record ReopenTaskCommand(Guid TaskId) : IRequest<TaskDto>;

public class ReopenTaskCommandHandler(IAppDbContext db) : IRequestHandler<ReopenTaskCommand, TaskDto>
{
    public async Task<TaskDto> Handle(ReopenTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken)
            ?? throw new InvalidOperationException("Task not found.");

        task.Reopen();
        await db.SaveChangesAsync(cancellationToken);

        return TaskDto.From(task);
    }
}
