using HomeOS.Application.Assistant;
using HomeOS.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record AiChatTurnDto(string Role, string Content);
public record AskAssistantRequest(string PageId, string Message, string? DetailLevel, List<AiChatTurnDto>? History);
public record AskAssistantResponse(string Reply);

[ApiController]
[Route("api/ai-assistant")]
[Authorize]
public class AiAssistantController(IAiAssistantClient client) : ControllerBase
{
    [HttpPost("ask")]
    public async Task<ActionResult<AskAssistantResponse>> Ask(AskAssistantRequest request, CancellationToken ct)
    {
        var page = PageAssistantContext.Describe(request.PageId);

        var detailInstruction = request.DetailLevel switch
        {
            "detailed" => "The user asked for a DETAILED explanation of this page — walk through its purpose and every feature thoroughly, in a few short paragraphs.",
            "brief" => "The user asked for a BRIEF explanation of this page — 2-3 sentences, high level only.",
            _ => "Keep answers concise and conversational unless the user explicitly asks for more detail."
        };

        var systemPrompt = $"""
            You are the in-app help assistant embedded directly inside the "{page.Title}" page of Home OS, a household
            management web app. Only help with this page: what it's for, how to use its features, and troubleshooting
            problems on it. {page.Description}
            {detailInstruction}
            If asked about something unrelated to this page or to Home OS in general, briefly redirect the user back
            to this page's topic instead of answering it. Do not claim to see the user's actual data — you only know
            what this page is for and how it works, not their personal entries.
            """;

        var history = (request.History ?? []).Select(h => new AiChatTurn(h.Role, h.Content)).ToList();
        var reply = await client.AskAsync(systemPrompt, history, request.Message, ct);
        return Ok(new AskAssistantResponse(reply));
    }
}
