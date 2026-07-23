using HomeOS.Application.Common;
using HomeOS.Domain.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Notifications.Commands;

public record SetNotificationPreferenceCommand(Guid UserId, NotificationCategory Category, bool EmailEnabled) : IRequest;

public class SetNotificationPreferenceCommandHandler(IAppDbContext db) : IRequestHandler<SetNotificationPreferenceCommand>
{
    public async Task Handle(SetNotificationPreferenceCommand request, CancellationToken cancellationToken)
    {
        var pref = await db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.Category == request.Category, cancellationToken);

        if (pref is null)
        {
            pref = NotificationPreference.Create(request.UserId, request.Category, request.EmailEnabled);
            db.NotificationPreferences.Add(pref);
        }
        else
        {
            pref.SetEmailEnabled(request.EmailEnabled);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
