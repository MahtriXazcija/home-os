using HomeOS.Application.Common;
using HomeOS.Domain.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Notifications.Queries;

public record GetNotificationPreferencesQuery(Guid UserId) : IRequest<List<NotificationPreferenceDto>>;

public class GetNotificationPreferencesQueryHandler(IAppDbContext db) : IRequestHandler<GetNotificationPreferencesQuery, List<NotificationPreferenceDto>>
{
    public async Task<List<NotificationPreferenceDto>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var saved = await db.NotificationPreferences
            .Where(p => p.UserId == request.UserId)
            .ToDictionaryAsync(p => p.Category, cancellationToken);

        return Enum.GetValues<NotificationCategory>()
            .Select(category => new NotificationPreferenceDto(
                category.ToString(),
                saved.TryGetValue(category, out var pref) ? pref.EmailEnabled : true))
            .ToList();
    }
}
