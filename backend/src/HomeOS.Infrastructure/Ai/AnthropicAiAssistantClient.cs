using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HomeOS.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeOS.Infrastructure.Ai;

public class AnthropicAiAssistantClient(HttpClient httpClient, IOptions<AnthropicOptions> options, ILogger<AnthropicAiAssistantClient> logger) : IAiAssistantClient
{
    private readonly AnthropicOptions _options = options.Value;

    public async Task<string> AskAsync(string systemPrompt, IReadOnlyList<AiChatTurn> history, string userMessage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return "The page assistant isn't configured yet — ask an administrator to add an Anthropic API key.";
        }

        var messages = new List<object>();
        foreach (var turn in history)
        {
            messages.Add(new { role = turn.Role, content = turn.Content });
        }
        messages.Add(new { role = "user", content = userMessage });

        var payload = new
        {
            model = _options.Model,
            max_tokens = 700,
            system = systemPrompt,
            messages
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.TryAddWithoutValidation("x-api-key", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Anthropic API call threw");
            return "Sorry, I couldn't reach the AI assistant right now.";
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Anthropic API call failed ({Status}): {Body}", response.StatusCode, body);
            return "Sorry, I couldn't reach the AI assistant right now.";
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();
            return text ?? "Sorry, I didn't get a response.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not parse Anthropic response: {Body}", body);
            return "Sorry, something went wrong reading the AI response.";
        }
    }

    public async Task<AiDiagnosticResult> DiagnoseAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new AiDiagnosticResult(false, 0, "No Anthropic:ApiKey configured.");
        }

        var payload = new
        {
            model = _options.Model,
            max_tokens = 16,
            messages = new[] { new { role = "user", content = "Say hello in exactly 3 words." } }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.TryAddWithoutValidation("x-api-key", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("anthropic-version", "2023-06-01");
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return new AiDiagnosticResult(response.IsSuccessStatusCode, (int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            return new AiDiagnosticResult(false, -1, ex.ToString());
        }
    }
}
