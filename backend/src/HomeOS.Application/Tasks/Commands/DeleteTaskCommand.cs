using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Tasks.Commands;

public record DeleteTaskCommand(Guid TaskId) : IRequest;

public class DeleteTaskCommandHandler(IAppDbContext db) : IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);
        if (task is null) return;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
    }
}
