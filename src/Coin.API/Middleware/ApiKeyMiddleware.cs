namespace Coin.API.Middleware;

using System.Security.Cryptography;
using System.Text;
using Coin.API.Configuration;
using Coin.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

public class ApiKeyMiddleware(
    RequestDelegate next,
    IOptions<ApiKeyAuthenticationOptions> options,
    IHostEnvironment environment)
{
    public const string ApiKeyHeaderName = "X-Api-Key";

    private readonly RequestDelegate _next = next;
    private readonly ApiKeyAuthenticationOptions _options = options.Value;
    private readonly IHostEnvironment _environment = environment;

    public async Task InvokeAsync(HttpContext context)
    {
        PathString path = context.Request.Path;
        if (_environment.IsDevelopment() &&
            (path.StartsWithSegments("/scalar", StringComparison.OrdinalIgnoreCase) ||
             path.StartsWithSegments("/openapi", StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey) ||
            string.IsNullOrWhiteSpace(extractedApiKey))
        {
            await context.WriteUnauthorizedProblemAsync("API Key is missing.");
            return;
        }

        string expectedKey = _options.ApiKey;
        if (string.IsNullOrWhiteSpace(expectedKey) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(extractedApiKey.ToString()),
                Encoding.UTF8.GetBytes(expectedKey)))
        {
            await context.WriteUnauthorizedProblemAsync("API Key is invalid.");
            return;
        }

        context.Items[HttpContextExtensions.UserIdItemKey] = _options.DefaultUserId;
        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<ApiKeyMiddleware>();
    }
}
