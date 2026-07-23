using System.Security.Claims;
using HomeOS.Application.Reminders;
using HomeOS.Application.Reminders.Commands;
using HomeOS.Application.Reminders.Queries;
using HomeOS.Domain.Reminders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record CreateReminderRequest(
    Guid HouseholdId,
    Guid TargetUserId,
    string Title,
    DateTime RemindAtUtc,
    string? Message,
    ReminderRecurrence Recurrence,
    string? SourceType,
    Guid? SourceId);

[ApiController]
[Route("api/reminders")]
[Authorize]
public class RemindersController(ISender sender) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet("mine")]
    public async Task<ActionResult<List<ReminderDto>>> Mine(CancellationToken ct)
    {
        var reminders = await sender.Send(new GetMyRemindersQuery(CurrentUserId), ct);
        return Ok(reminders);
    }

    [HttpPost]
    public async Task<ActionResult<ReminderDto>> Create(CreateReminderRequest request, CancellationToken ct)
    {
        var reminder = await sender.Send(new CreateReminderCommand(
            request.HouseholdId, request.TargetUserId, request.Title, request.RemindAtUtc, CurrentUserId,
            request.Message, request.Recurrence, request.SourceType, request.SourceId), ct);
        return Ok(reminder);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await sender.Send(new CancelReminderCommand(id), ct);
        return NoContent();
    }
}
