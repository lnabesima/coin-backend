namespace Coin.API.Configuration;

public class CorsOptions
{
    public const string SectionName = "Cors";
    public const string PolicyName = "FrontendPolicy";

    public string[] AllowedOrigins { get; set; } = [];
}
