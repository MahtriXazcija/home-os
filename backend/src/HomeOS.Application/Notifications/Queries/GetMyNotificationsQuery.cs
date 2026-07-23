using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Notifications.Queries;

public record GetMyNotificationsQuery(Guid UserId) : IRequest<List<NotificationDto>>;

public class GetMyNotificationsQueryHandler(IAppDbContext db) : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await db.Notifications
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        return notifications.Select(NotificationDto.From).ToList();
    }
}
