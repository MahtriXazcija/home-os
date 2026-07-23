using System.Security.Claims;
using HomeOS.Application.Calendar;
using HomeOS.Application.Calendar.Commands;
using HomeOS.Application.Calendar.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record CreateCalendarEventRequest(Guid HouseholdId, string Title, string? Description, DateTime StartUtc, DateTime EndUtc, bool IsAllDay);

[ApiController]
[Route("api/calendar")]
[Authorize]
public class CalendarController(ISender sender) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<List<CalendarItemDto>>> Get([FromQuery] Guid householdId, [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
    {
        var items = await sender.Send(new GetCalendarQuery(householdId, fromUtc, toUtc), ct);
        return Ok(items);
    }

    [HttpPost("events")]
    public async Task<ActionResult<CalendarEventDto>> CreateEvent(CreateCalendarEventRequest request, CancellationToken ct)
    {
        var evt = await sender.Send(new CreateCalendarEventCommand(
            request.HouseholdId, request.Title, request.Description, request.StartUtc, request.EndUtc, request.IsAllDay, CurrentUserId), ct);
        return Ok(evt);
    }
}
