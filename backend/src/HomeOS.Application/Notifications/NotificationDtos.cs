using HomeOS.Domain.Notifications;

namespace HomeOS.Application.Notifications;

public record NotificationDto(Guid Id, string Category, string Title, string? Message, bool IsRead, DateTime CreatedAtUtc)
{
    public static NotificationDto From(Notification n) => new(n.Id, n.Category.ToString(), n.Title, n.Message, n.IsRead, n.CreatedAtUtc);
}

public record NotificationPreferenceDto(string Category, bool EmailEnabled);
