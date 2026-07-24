using System.Text;
using System.Text.Json;
using HomeOS.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeOS.Infrastructure.Ai;

public class GeminiAiAssistantClient(HttpClient httpClient, IOptions<GeminiOptions> options, ILogger<GeminiAiAssistantClient> logger) : IAiAssistantClient
{
    private readonly GeminiOptions _options = options.Value;

    // Gemini uses "user"/"model" turns instead of "user"/"assistant".
    private static string ToGeminiRole(string role) => role == "assistant" ? "model" : "user";

    public async Task<string> AskAsync(string systemPrompt, IReadOnlyList<AiChatTurn> history, string userMessage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return "The page assistant isn't configured yet — ask an administrator to add a Gemini API key.";
        }

        var contents = new List<object>();
        foreach (var turn in history)
        {
            contents.Add(new { role = ToGeminiRole(turn.Role), parts = new[] { new { text = turn.Content } } });
        }
        contents.Add(new { role = "user", parts = new[] { new { text = userMessage } } });

        var payload = new
        {
            contents,
            systemInstruction = new { parts = new[] { new { text = systemPrompt } } },
            generationConfig = new { maxOutputTokens = 700 }
        };

        var (success, _, body) = await SendAsync(payload, cancellationToken);
        if (!success)
        {
            logger.LogError("Gemini API call failed: {Body}", body);
            return "Sorry, I couldn't reach the AI assistant right now.";
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            return text ?? "Sorry, I didn't get a response.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not parse Gemini response: {Body}", body);
            return "Sorry, something went wrong reading the AI response.";
        }
    }

    public async Task<AiDiagnosticResult> DiagnoseAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new AiDiagnosticResult(false, 0, "No Gemini:ApiKey configured.");
        }

        var payload = new
        {
            contents = new[] { new { role = "user", parts = new[] { new { text = "Say hello in exactly 3 words." } } } },
            generationConfig = new { maxOutputTokens = 16 }
        };

        var (success, status, body) = await SendAsync(payload, cancellationToken);
        return new AiDiagnosticResult(success, status, body);
    }

    private async Task<(bool Success, int StatusCode, string Body)> SendAsync(object payload, CancellationToken cancellationToken)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("x-goog-api-key", _options.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return (response.IsSuccessStatusCode, (int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Gemini API call threw");
            return (false, -1, ex.ToString());
        }
    }
}
