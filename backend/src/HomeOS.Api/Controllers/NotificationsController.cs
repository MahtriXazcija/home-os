using System.Security.Claims;
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
public class NotificationsController(ISender sender) : ControllerBase
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
}
