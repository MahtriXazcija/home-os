namespace HomeOS.Application.Common;

public interface IFrontendLinkBuilder
{
    string BuildInvitationLink(string token);
    string BuildPasswordResetLink(string email, string token);
}
