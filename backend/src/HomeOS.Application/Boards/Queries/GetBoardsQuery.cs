using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Boards.Queries;

public record GetBoardsQuery(Guid HouseholdId) : IRequest<List<BoardDto>>;

public class GetBoardsQueryHandler(IAppDbContext db) : IRequestHandler<GetBoardsQuery, List<BoardDto>>
{
    public async Task<List<BoardDto>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        var boards = await db.Boards
            .Where(b => b.HouseholdId == request.HouseholdId)
            .OrderBy(b => b.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return boards.Select(BoardDto.From).ToList();
    }
}
