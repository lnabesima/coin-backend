namespace Coin.API.Configuration;

public class ApiKeyAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string ApiKey { get; set; } = string.Empty;
    public Guid DefaultUserId { get; set; }
}
