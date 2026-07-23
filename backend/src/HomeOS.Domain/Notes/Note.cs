using HomeOS.Domain.Common;

namespace HomeOS.Domain.Notes;

public class Note : AggregateRoot
{
    public Guid HouseholdId { get; private set; }
    public string? Title { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public List<string> Tags { get; private set; } = [];
    public DateOnly? JournalDate { get; private set; }

    /// <summary>Generic link to a task/bill/event — e.g. "task"/taskId. Both optional: most notes link nowhere.</summary>
    public string? LinkedType { get; private set; }
    public Guid? LinkedId { get; private set; }

    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Note() { }

    public static Note Create(
        Guid householdId, string content, Guid createdByUserId,
        string? title = null, List<string>? tags = null, DateOnly? journalDate = null,
        string? linkedType = null, Guid? linkedId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Note content is required.", nameof(content));
        }

        var now = DateTime.UtcNow;
        return new Note
        {
            HouseholdId = householdId,
            Title = title,
            Content = content,
            Tags = tags ?? [],
            JournalDate = journalDate,
            LinkedType = linkedType,
            LinkedId = linkedId,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void Update(string content, string? title, List<string> tags)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Note content is required.", nameof(content));
        }

        Content = content;
        Title = title;
        Tags = tags;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
