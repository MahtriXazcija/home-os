using HomeOS.Domain.Chat;

namespace HomeOS.Application.Chat;

public record ChatReadStateDto(Guid UserId, DateTime LastReadAtUtc)
{
    public static ChatReadStateDto From(ChatReadState r) => new(r.UserId, r.LastReadAtUtc);
}
