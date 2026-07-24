using HomeOS.Application.Common;
using Microsoft.Extensions.Options;

namespace HomeOS.Infrastructure.Web;

public class FrontendLinkBuilder(IOptions<FrontendOptions> options) : IFrontendLinkBuilder
{
    private readonly FrontendOptions _options = options.Value;

    public string BuildInvitationLink(string token) =>
        $"{_options.BaseUrl.TrimEnd('/')}/onboarding?token={token}";

    public string BuildPasswordResetLink(string email, string token) =>
        $"{_options.BaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
}
