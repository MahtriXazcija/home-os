using HomeOS.Application.Apps;
using HomeOS.Application.Apps.Commands;
using HomeOS.Application.Apps.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record InstallAppRequest(Guid HouseholdId, List<string> GrantedPermissions);
public record UninstallAppRequest(Guid HouseholdId);

[ApiController]
[Route("api/apps")]
[Authorize]
public class AppsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AppDto>>> Get([FromQuery] Guid householdId, CancellationToken ct)
    {
        return Ok(await sender.Send(new GetHouseholdAppsQuery(householdId), ct));
    }

    [HttpPost("{id}/install")]
    public async Task<ActionResult<AppDto>> Install(string id, InstallAppRequest request, CancellationToken ct)
    {
        try
        {
            var app = await sender.Send(new InstallAppCommand(request.HouseholdId, id, request.GrantedPermissions), ct);
            return Ok(app);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/uninstall")]
    public async Task<IActionResult> Uninstall(string id, UninstallAppRequest request, CancellationToken ct)
    {
        try
        {
            await sender.Send(new UninstallAppCommand(request.HouseholdId, id), ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
