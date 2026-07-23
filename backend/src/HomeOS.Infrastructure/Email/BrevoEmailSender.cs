using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HomeOS.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeOS.Infrastructure.Email;

public class BrevoEmailSender(HttpClient httpClient, IOptions<EmailOptions> options, ILogger<BrevoEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BrevoApiKey) || string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            logger.LogWarning("Email not configured — skipping send to {ToEmail}: {Subject}", toEmail, subject);
            return;
        }

        var payload = new
        {
            sender = new { email = _options.FromAddress, name = _options.FromName },
            to = new[] { new { email = toEmail, name = toName } },
            subject,
            htmlContent = htmlBody
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.TryAddWithoutValidation("api-key", _options.BrevoApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Brevo send failed ({Status}) to {ToEmail}: {Body}", response.StatusCode, toEmail, body);
        }
    }
}
