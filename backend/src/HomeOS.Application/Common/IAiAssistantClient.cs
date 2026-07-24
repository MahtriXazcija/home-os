namespace HomeOS.Application.Common;

public record AiChatTurn(string Role, string Content);

public record AiDiagnosticResult(bool Success, int StatusCode, string Body);

public interface IAiAssistantClient
{
    Task<string> AskAsync(string systemPrompt, IReadOnlyList<AiChatTurn> history, string userMessage, CancellationToken cancellationToken);

    /// <summary>Temporary diagnostic: makes a minimal real call and reports exactly what the provider said, since app logs aren't reachable from outside Render.</summary>
    Task<AiDiagnosticResult> DiagnoseAsync(CancellationToken cancellationToken);
}
