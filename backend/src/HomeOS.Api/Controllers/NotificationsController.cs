using System.Security.Claims;
using HomeOS.Application.Common;
using HomeOS.Application.Notifications;
using HomeOS.Application.Notifications.Commands;
using HomeOS.Application.Notifications.Queries;
using HomeOS.Domain.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record SetPreferenceRequest(NotificationCategory Category, bool EmailEnabled);

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(ISender sender, IEmailSender emailSender, IUserDirectory userDirectory) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> Mine(CancellationToken ct)
    {
        var notifications = await sender.Send(new GetMyNotificationsQuery(CurrentUserId), ct);
        return Ok(notifications);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await sender.Send(new MarkNotificationReadCommand(id), ct);
        return NoContent();
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<List<NotificationPreferenceDto>>> Preferences(CancellationToken ct)
    {
        var prefs = await sender.Send(new GetNotificationPreferencesQuery(CurrentUserId), ct);
        return Ok(prefs);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> SetPreference(SetPreferenceRequest request, CancellationToken ct)
    {
        await sender.Send(new SetNotificationPreferenceCommand(CurrentUserId, request.Category, request.EmailEnabled), ct);
        return NoContent();
    }

    /// <summary>Temporary diagnostic: sends a real email to the caller and reports exactly what Brevo said, since app logs aren't reachable from outside Render.</summary>
    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail(CancellationToken ct)
    {
        var contact = await userDirectory.GetContactAsync(CurrentUserId, ct);
        if (contact is null) return NotFound("Current user not found.");

        var result = await emailSender.SendAsync(contact.Email, contact.DisplayName, "Home OS test email", "<p>If you can read this, Brevo is wired up correctly.</p>", ct);
        return Ok(new { toEmail = contact.Email, result.Success, result.Error });
    }
}
