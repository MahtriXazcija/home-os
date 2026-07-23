using HomeOS.Domain.Notes;

namespace HomeOS.Application.Notes;

public record NoteDto(
    Guid Id, Guid HouseholdId, string? Title, string Content, List<string> Tags,
    DateOnly? JournalDate, string? LinkedType, Guid? LinkedId,
    Guid CreatedByUserId, DateTime CreatedAtUtc, DateTime UpdatedAtUtc)
{
    public static NoteDto From(Note n) => new(
        n.Id, n.HouseholdId, n.Title, n.Content, n.Tags, n.JournalDate,
        n.LinkedType, n.LinkedId, n.CreatedByUserId, n.CreatedAtUtc, n.UpdatedAtUtc);
}
