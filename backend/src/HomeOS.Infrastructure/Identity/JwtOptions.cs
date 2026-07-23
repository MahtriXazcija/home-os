namespace HomeOS.Infrastructure.Identity;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "home-os";
    public string Audience { get; set; } = "home-os-client";
    public int ExpiryMinutes { get; set; } = 60 * 24 * 7; // 7 days — no refresh-token flow yet
}
