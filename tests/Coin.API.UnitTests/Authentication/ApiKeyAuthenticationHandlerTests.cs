namespace Coin.API.UnitTests.Authentication;

using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Coin.API.Authentication;
using Coin.API.Configuration;
using Coin.API.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

public class ApiKeyAuthenticationHandlerTests
{
    private readonly string _validApiKey = "test-secret-api-key-12345";
    private readonly Guid _defaultUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly IOptionsMonitor<ApiKeyAuthenticationOptions> _optionsMonitor;
    private readonly AuthenticationScheme _scheme;

    public ApiKeyAuthenticationHandlerTests()
    {
        ApiKeyAuthenticationOptions options = new()
        {
            ApiKey = _validApiKey,
            DefaultUserId = _defaultUserId
        };

        _optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        _optionsMonitor.Get(Arg.Any<string>()).Returns(options);
        _optionsMonitor.CurrentValue.Returns(options);

        _scheme = new AuthenticationScheme(
            ApiKeyAuthenticationOptions.DefaultScheme,
            ApiKeyAuthenticationOptions.DefaultScheme,
            typeof(ApiKeyAuthenticationHandler));
    }

    private static DefaultHttpContext CreateHttpContext(string path = "/api/v1/transactions")
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private async Task<ApiKeyAuthenticationHandler> CreateHandlerAsync(HttpContext context)
    {
        ApiKeyAuthenticationHandler handler = new(
            _optionsMonitor,
            NullLoggerFactory.Instance,
            UrlEncoder.Default);

        await handler.InitializeAsync(_scheme, context);
        return handler;
    }

    [Fact]
    public async Task AuthenticateAsync_WhenValidApiKeyProvided_ReturnsSuccessWithClaimsAndUserId()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        context.Request.Headers[ApiKeyAuthenticationHandler.ApiKeyHeaderName] = _validApiKey;
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context);

        // Act
        AuthenticateResult result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Principal);
        Assert.Equal(_defaultUserId.ToString(), result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(_defaultUserId, context.GetUserId());
        Assert.Equal(_defaultUserId, context.TryGetUserId());
    }

    [Fact]
    public async Task AuthenticateAsync_WhenApiKeyHeaderIsMissing_ReturnsNoResult()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context);

        // Act
        AuthenticateResult result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.None);
        Assert.False(result.Succeeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AuthenticateAsync_WhenApiKeyHeaderIsEmptyOrWhitespace_ReturnsNoResult(string emptyKey)
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        context.Request.Headers[ApiKeyAuthenticationHandler.ApiKeyHeaderName] = emptyKey;
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context);

        // Act
        AuthenticateResult result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.None);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenApiKeyIsInvalid_ReturnsFailResult()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        context.Request.Headers[ApiKeyAuthenticationHandler.ApiKeyHeaderName] = "wrong-api-key";
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context);

        // Act
        AuthenticateResult result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Equal("API Key is invalid.", result.Failure.Message);
    }

    [Fact]
    public async Task ChallengeAsync_WhenMissingApiKey_Emits401ProblemDetailsWithMissingDetail()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context);

        // Authenticate first so the failure detail item is set
        await handler.AuthenticateAsync();

        // Act
        await handler.ChallengeAsync(new AuthenticationProperties());

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        ProblemDetails? problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Equal("API Key is missing.", problem.Detail);
        Assert.Equal(context.Request.Path.Value, problem.Instance);
    }

    [Fact]
    public async Task ChallengeAsync_WhenInvalidApiKey_Emits401ProblemDetailsWithInvalidDetail()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        context.Request.Headers[ApiKeyAuthenticationHandler.ApiKeyHeaderName] = "wrong-api-key";
        ApiKeyAuthenticationHandler handler = await CreateHandlerAsync(context);

        // Authenticate first so the failure detail item is set
        await handler.AuthenticateAsync();

        // Act
        await handler.ChallengeAsync(new AuthenticationProperties());

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        ProblemDetails? problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Equal("API Key is invalid.", problem.Detail);
        Assert.Equal(context.Request.Path.Value, problem.Instance);
    }
}
