namespace HomeOS.Application.Common;

public record AiChatTurn(string Role, string Content);

public interface IAiAssistantClient
{
    Task<string> AskAsync(string systemPrompt, IReadOnlyList<AiChatTurn> history, string userMessage, CancellationToken cancellationToken);
}
