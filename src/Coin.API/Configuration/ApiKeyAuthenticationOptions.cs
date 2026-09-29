using Microsoft.AspNetCore.Authentication;

namespace Coin.API.Configuration;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public const string SectionName = "Authentication";

    public string ApiKey { get; set; } = string.Empty;
    public Guid DefaultUserId { get; set; }
}
