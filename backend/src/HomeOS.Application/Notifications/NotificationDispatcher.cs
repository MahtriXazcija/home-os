using System.Net;
using HomeOS.Application.Common;
using HomeOS.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeOS.Application.Notifications;

/// <summary>
/// The single place that turns "something happened" into an in-app
/// notification plus (if the member opted in) an email — the shared
/// "Notifications" capability from docs/app-sdk.md §3. Event handlers call
/// this instead of duplicating the create-notification + check-preference
/// + send-email sequence.
/// </summary>
public interface INotificationDispatcher
{
    Task DispatchAsync(Guid userId, Guid householdId, NotificationCategory category, string title, string? message, CancellationToken cancellationToken);
}

public class NotificationDispatcher(IAppDbContext db, IUserDirectory userDirectory, IEmailSender emailSender, ILogger<NotificationDispatcher> logger) : INotificationDispatcher
{
    public async Task DispatchAsync(Guid userId, Guid householdId, NotificationCategory category, string title, string? message, CancellationToken cancellationToken)
    {
        var inApp = Notification.Create(userId, householdId, category, title, message);
        db.Notifications.Add(inApp);
        await db.SaveChangesAsync(cancellationToken);

        var pref = await db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Category == category, cancellationToken);
        var emailEnabled = pref?.EmailEnabled ?? true;
        if (!emailEnabled) return;

        var contact = await userDirectory.GetContactAsync(userId, cancellationToken);
        if (contact is null) return;

        var html = $"<p>{WebUtility.HtmlEncode(title)}</p>" + (message is null ? "" : $"<p>{WebUtility.HtmlEncode(message)}</p>");
        var result = await emailSender.SendAsync(contact.Email, contact.DisplayName, title, html, cancellationToken);
        if (!result.Success)
        {
            logger.LogError("Notification email to {Email} failed: {Error}", contact.Email, result.Error);
        }
    }
}
