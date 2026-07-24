using HomeOS.Application.Assistant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record AiChatTurnDto(string Role, string Content);
public record AskAssistantRequest(string PageId, string Message, string? DetailLevel, List<AiChatTurnDto>? History);
public record AskAssistantResponse(string Reply);

[ApiController]
[Route("api/ai-assistant")]
[Authorize]
public class AiAssistantController : ControllerBase
{
    [HttpPost("ask")]
    public ActionResult<AskAssistantResponse> Ask(AskAssistantRequest request)
    {
        var reply = PageAssistantResponder.Respond(request.PageId, request.Message, request.DetailLevel);
        return Ok(new AskAssistantResponse(reply));
    }
}
