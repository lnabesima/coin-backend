namespace Coin.IntegrationTests.Security;

using System.Net;
using Coin.IntegrationTests.Fixtures;
using Xunit;

public sealed class CorsAndHealthCheckIntegrationTests(CoinWebApplicationFactory factory)
    : IClassFixture<CoinWebApplicationFactory>
{
    private readonly CoinWebApplicationFactory _factory = factory;

    [Fact]
    public async Task GetHealth_WithoutAuthenticationHeader_Returns200OkWithHealthyStatus()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task OptionsPreflight_FromAzureStaticWebAppsOrigin_Returns204NoContentWithCorsHeaders()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Options, "/api/v1/transactions");
        request.Headers.Add("Origin", "https://salmon-island-012345678.brazilsouth.azurestaticapps.net");
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "X-Api-Key, Content-Type");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal(
            "https://salmon-island-012345678.brazilsouth.azurestaticapps.net",
            response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task OptionsPreflight_FromLocalhostOrigin_Returns204NoContentWithCorsHeaders()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Options, "/api/v1/transactions");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "X-Api-Key");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task OptionsPreflight_FromUnauthorizedOrigin_DoesNotReturnAllowOriginHeader()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Options, "/api/v1/transactions");
        request.Headers.Add("Origin", "https://malicious-website.com");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task GetTransactions_FromAllowedOriginWithApiKey_Returns200AndCorsHeaders()
    {
        // Arrange
        using HttpClient client = _factory.CreateAuthenticatedClient();
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/v1/transactions");
        request.Headers.Add("Origin", "http://localhost:5173");

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        Assert.Equal("http://localhost:5173", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }
}
