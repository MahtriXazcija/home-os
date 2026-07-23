using System.Security.Claims;
using HomeOS.Application.Households;
using HomeOS.Application.Households.Commands;
using HomeOS.Application.Households.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record CreateHouseholdRequest(string Name);
public record InviteMemberRequest(string Email);
public record AcceptInvitationRequest(string Token);

[ApiController]
[Route("api/households")]
[Authorize]
public class HouseholdsController(ISender sender) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    private string CurrentUserDisplayName => User.FindFirstValue("displayName") ?? "Member";

    [HttpPost]
    public async Task<ActionResult<HouseholdDto>> Create(CreateHouseholdRequest request, CancellationToken ct)
    {
        try
        {
            var household = await sender.Send(new CreateHouseholdCommand(request.Name, CurrentUserId, CurrentUserDisplayName), ct);
            return Ok(household);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("mine")]
    public async Task<ActionResult<HouseholdDto?>> Mine(CancellationToken ct)
    {
        var household = await sender.Send(new GetMyHouseholdQuery(CurrentUserId), ct);
        return household is null ? NotFound() : Ok(household);
    }

    [HttpPost("{id:guid}/invitations")]
    public async Task<ActionResult<InvitationDto>> Invite(Guid id, InviteMemberRequest request, CancellationToken ct)
    {
        try
        {
            var invitation = await sender.Send(new InviteMemberCommand(id, request.Email, CurrentUserId), ct);
            return Ok(invitation);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("invitations/accept")]
    public async Task<ActionResult<HouseholdDto>> Accept(AcceptInvitationRequest request, CancellationToken ct)
    {
        try
        {
            var household = await sender.Send(new AcceptInvitationCommand(request.Token, CurrentUserId, CurrentUserDisplayName), ct);
            return Ok(household);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
