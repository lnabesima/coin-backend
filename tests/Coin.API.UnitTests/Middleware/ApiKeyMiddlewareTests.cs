namespace Coin.API.UnitTests.Middleware;

using System.Text.Json;
using Coin.API.Configuration;
using Coin.API.Extensions;
using Coin.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;

public class ApiKeyMiddlewareTests
{
    private readonly string _validApiKey = "test-secret-api-key-12345";
    private readonly Guid _defaultUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly IOptions<ApiKeyAuthenticationOptions> _options;
    private readonly IHostEnvironment _developmentEnvironment;
    private readonly IHostEnvironment _productionEnvironment;

    public ApiKeyMiddlewareTests()
    {
        _options = Options.Create(new ApiKeyAuthenticationOptions
        {
            ApiKey = _validApiKey,
            DefaultUserId = _defaultUserId
        });

        _developmentEnvironment = Substitute.For<IHostEnvironment>();
        _developmentEnvironment.EnvironmentName.Returns(Environments.Development);

        _productionEnvironment = Substitute.For<IHostEnvironment>();
        _productionEnvironment.EnvironmentName.Returns(Environments.Production);
    }

    private static DefaultHttpContext CreateHttpContext(string path = "/api/v1/transactions")
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task InvokeAsync_WhenValidApiKeyProvided_CallsNextAndAttachesUserId()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        context.Request.Headers[ApiKeyMiddleware.ApiKeyHeaderName] = _validApiKey;

        bool nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        ApiKeyMiddleware middleware = new(next, _options, _developmentEnvironment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(_defaultUserId, context.GetUserId());
        Assert.Equal(_defaultUserId, context.TryGetUserId());
    }

    [Fact]
    public async Task InvokeAsync_WhenApiKeyHeaderIsMissing_Returns401ProblemDetailsAndDoesNotCallNext()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        bool nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        ApiKeyMiddleware middleware = new(next, _options, _developmentEnvironment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(nextCalled);
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

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InvokeAsync_WhenApiKeyHeaderIsEmptyOrWhitespace_Returns401ProblemDetails(string emptyKey)
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        context.Request.Headers[ApiKeyMiddleware.ApiKeyHeaderName] = emptyKey;

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        ApiKeyMiddleware middleware = new(next, _options, _developmentEnvironment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenApiKeyIsInvalid_Returns401ProblemDetailsAndDoesNotCallNext()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        context.Request.Headers[ApiKeyMiddleware.ApiKeyHeaderName] = "wrong-api-key";

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        ApiKeyMiddleware middleware = new(next, _options, _developmentEnvironment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        ProblemDetails? problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);
        Assert.NotNull(problem);
        Assert.Equal("API Key is invalid.", problem.Detail);
    }

    [Theory]
    [InlineData("/scalar/v1")]
    [InlineData("/scalar")]
    [InlineData("/openapi/v1.json")]
    [InlineData("/openapi")]
    public async Task InvokeAsync_WhenDocumentationPathInDevelopment_BypassesAuthentication(string docPath)
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext(docPath);
        bool nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        ApiKeyMiddleware middleware = new(next, _options, _developmentEnvironment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenDocumentationPathInProduction_RejectsUnauthorizedWithoutHeader()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext("/scalar/v1");
        bool nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        ApiKeyMiddleware middleware = new(next, _options, _productionEnvironment);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public void GetUserId_WhenUserIdNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        DefaultHttpContext context = new();

        // Act & Assert
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => context.GetUserId());
        Assert.Contains("User ID is not present", ex.Message);
    }

    [Fact]
    public void TryGetUserId_WhenUserIdNotFound_ReturnsNull()
    {
        // Arrange
        DefaultHttpContext context = new();

        // Act
        Guid? result = context.TryGetUserId();

        // Assert
        Assert.Null(result);
    }
}
