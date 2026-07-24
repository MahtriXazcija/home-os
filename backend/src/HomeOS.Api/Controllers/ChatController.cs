using System.Security.Claims;
using HomeOS.Application.Chat;
using HomeOS.Application.Chat.Commands;
using HomeOS.Application.Chat.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record SendChatMessageRequest(Guid HouseholdId, string Content);
public record MarkChatReadRequest(Guid HouseholdId);

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController(ISender sender) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<List<ChatMessageDto>>> Get([FromQuery] Guid householdId, CancellationToken ct)
    {
        var messages = await sender.Send(new GetChatMessagesQuery(householdId), ct);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<ActionResult<ChatMessageDto>> Send(SendChatMessageRequest request, CancellationToken ct)
    {
        var message = await sender.Send(new SendChatMessageCommand(request.HouseholdId, CurrentUserId, request.Content), ct);
        return Ok(message);
    }

    [HttpGet("read-state")]
    public async Task<ActionResult<List<ChatReadStateDto>>> GetReadState([FromQuery] Guid householdId, CancellationToken ct)
    {
        var states = await sender.Send(new GetChatReadStatesQuery(householdId), ct);
        return Ok(states);
    }

    [HttpPost("read")]
    public async Task<IActionResult> MarkRead(MarkChatReadRequest request, CancellationToken ct)
    {
        await sender.Send(new MarkChatReadCommand(request.HouseholdId, CurrentUserId), ct);
        return NoContent();
    }
}
