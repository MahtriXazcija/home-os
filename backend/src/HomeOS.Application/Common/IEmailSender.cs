namespace HomeOS.Application.Common;

public record EmailSendResult(bool Success, string? Error);

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken);
}
