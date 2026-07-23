using HomeOS.Domain.Common;

namespace HomeOS.Domain.Notifications;

/// <summary>The in-app half of a notification — see docs/app-sdk.md §3 "Notifications" capability.</summary>
public class Notification : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid HouseholdId { get; private set; }
    public NotificationCategory Category { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Message { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Notification() { }

    public static Notification Create(Guid userId, Guid householdId, NotificationCategory category, string title, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Notification title is required.", nameof(title));
        }

        return new Notification
        {
            UserId = userId,
            HouseholdId = householdId,
            Category = category,
            Title = title.Trim(),
            Message = message,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkRead() => IsRead = true;
}
