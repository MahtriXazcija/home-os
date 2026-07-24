using HomeOS.Domain.Common;

namespace HomeOS.Domain.Chat;

/// <summary>
/// One flat, household-scoped message. No threads, reactions, or read
/// receipts — the platform's own household/member boundary is what scopes
/// "who can see this," so the entity itself stays as simple as Note.
/// </summary>
public class ChatMessage : Entity
{
    public Guid HouseholdId { get; private set; }
    public Guid SenderUserId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private ChatMessage() { }

    public static ChatMessage Create(Guid householdId, Guid senderUserId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content is required.", nameof(content));
        }
        if (content.Length > 2000)
        {
            throw new ArgumentException("Message is too long (max 2000 characters).", nameof(content));
        }

        return new ChatMessage
        {
            HouseholdId = householdId,
            SenderUserId = senderUserId,
            Content = content.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
