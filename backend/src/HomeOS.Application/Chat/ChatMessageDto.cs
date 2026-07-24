using HomeOS.Domain.Chat;

namespace HomeOS.Application.Chat;

public record ChatMessageDto(Guid Id, Guid HouseholdId, Guid SenderUserId, string Content, DateTime CreatedAtUtc)
{
    public static ChatMessageDto From(ChatMessage m) => new(m.Id, m.HouseholdId, m.SenderUserId, m.Content, m.CreatedAtUtc);
}
