using HomeOS.Domain.Boards;

namespace HomeOS.Application.Boards;

public record BoardDto(Guid Id, Guid HouseholdId, string Name, DateTime CreatedAtUtc)
{
    public static BoardDto From(Board board) => new(board.Id, board.HouseholdId, board.Name, board.CreatedAtUtc);
}
