using HomeOS.Application.Common;
using HomeOS.Domain.Boards;
using MediatR;

namespace HomeOS.Application.Boards.Commands;

public record CreateBoardCommand(Guid HouseholdId, string Name) : IRequest<BoardDto>;

public class CreateBoardCommandHandler(IAppDbContext db) : IRequestHandler<CreateBoardCommand, BoardDto>
{
    public async Task<BoardDto> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = Board.Create(request.HouseholdId, request.Name);
        db.Boards.Add(board);
        await db.SaveChangesAsync(cancellationToken);
        return BoardDto.From(board);
    }
}
