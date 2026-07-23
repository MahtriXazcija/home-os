namespace HomeOS.Infrastructure.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string BrevoApiKey { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Home OS";
}
