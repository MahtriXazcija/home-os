using HomeOS.Domain.Common;

namespace HomeOS.Domain.Boards;

public class Board : AggregateRoot
{
    public Guid HouseholdId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private Board() { }

    public static Board Create(Guid householdId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Board name is required.", nameof(name));
        }

        return new Board
        {
            HouseholdId = householdId,
            Name = name.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
