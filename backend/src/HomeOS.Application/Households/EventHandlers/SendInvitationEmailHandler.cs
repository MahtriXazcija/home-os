using System.Net;
using HomeOS.Application.Common;
using HomeOS.Domain.Households;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeOS.Application.Households.EventHandlers;

/// <summary>
/// Invitations go out to an email address that may not have a Home OS
/// account yet, so this can't go through INotificationDispatcher (which
/// requires an existing userId and notification preferences) — it sends
/// directly via IEmailSender instead, unconditionally, since a household
/// invite is a transactional message the invitee explicitly asked for by
/// giving out their email.
/// </summary>
public class SendInvitationEmailHandler(
    IHouseholdRepository repository,
    IUserDirectory userDirectory,
    IEmailSender emailSender,
    IFrontendLinkBuilder linkBuilder,
    ILogger<SendInvitationEmailHandler> logger) : INotificationHandler<DomainEventNotification<MemberInvitedEvent>>
{
    public async Task Handle(DomainEventNotification<MemberInvitedEvent> notification, CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;

        var household = await repository.GetByIdAsync(evt.HouseholdId, cancellationToken);
        var invitation = household?.Invitations.FirstOrDefault(i => i.Id == evt.InvitationId);
        if (household is null || invitation is null)
        {
            return;
        }

        var inviter = await userDirectory.GetContactAsync(evt.InvitedByUserId, cancellationToken);
        var inviterName = inviter?.DisplayName ?? "A household member";
        var link = linkBuilder.BuildInvitationLink(invitation.Token);

        var subject = $"{inviterName} invited you to join \"{household.Name}\" on Home OS";
        var html = $"""
            <p>{WebUtility.HtmlEncode(inviterName)} invited you to join the household
            <strong>{WebUtility.HtmlEncode(household.Name)}</strong> on Home OS.</p>
            <p><a href="{link}">Click here to accept the invitation</a></p>
            <p>Or open Home OS, sign up or sign in, and enter this invite code on the "Join with an
            invite link" screen:</p>
            <p style="font-size:18px; font-weight:600; letter-spacing:0.03em;">{WebUtility.HtmlEncode(invitation.Token)}</p>
            <p style="color:#6e6e73; font-size:12px;">This invitation expires on {invitation.ExpiresAtUtc:yyyy-MM-dd}.</p>
            """;

        var result = await emailSender.SendAsync(evt.Email, evt.Email, subject, html, cancellationToken);
        if (!result.Success)
        {
            logger.LogError("Invitation email to {Email} failed: {Error}", evt.Email, result.Error);
        }
    }
}
