namespace Coin.API.Authentication;

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Coin.API.Configuration;
using Coin.API.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    public const string ApiKeyHeaderName = "X-Api-Key";
    private const string AuthFailureDetailItemKey = "AuthFailureDetail";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey) ||
            string.IsNullOrWhiteSpace(extractedApiKey))
        {
            Context.Items[AuthFailureDetailItemKey] = "API Key is missing.";
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string expectedKey = Options.ApiKey;
        if (string.IsNullOrWhiteSpace(expectedKey) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(extractedApiKey.ToString()),
                Encoding.UTF8.GetBytes(expectedKey)))
        {
            Context.Items[AuthFailureDetailItemKey] = "API Key is invalid.";
            return Task.FromResult(AuthenticateResult.Fail("API Key is invalid."));
        }

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, Options.DefaultUserId.ToString())
        ];

        ClaimsIdentity identity = new(claims, Scheme.Name);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, Scheme.Name);

        Context.Items[HttpContextExtensions.UserIdItemKey] = Options.DefaultUserId;

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (!Response.HasStarted)
        {
            string detail = Context.Items.TryGetValue(AuthFailureDetailItemKey, out var item) && item is string msg
                ? msg
                : "API Key is missing.";

            await Context.WriteUnauthorizedProblemAsync(detail);
        }
    }
}
