using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Tasks.Queries;

public record GetTasksQuery(Guid HouseholdId, Guid? BoardId = null) : IRequest<List<TaskDto>>;

public class GetTasksQueryHandler(IAppDbContext db) : IRequestHandler<GetTasksQuery, List<TaskDto>>
{
    public async Task<List<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = db.Tasks.Where(t => t.HouseholdId == request.HouseholdId);
        if (request.BoardId is not null)
        {
            query = query.Where(t => t.BoardId == request.BoardId);
        }

        var tasks = await query.OrderBy(t => t.DueDateUtc).ThenByDescending(t => t.CreatedAtUtc).ToListAsync(cancellationToken);
        return tasks.Select(TaskDto.From).ToList();
    }
}
