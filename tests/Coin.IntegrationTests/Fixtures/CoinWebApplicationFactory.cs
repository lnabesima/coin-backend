namespace Coin.IntegrationTests.Fixtures;

using Coin.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class CoinWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ValidApiKey = "coin-integration-test-key-98765";
    public static readonly Guid DefaultUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("coin_integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Apply EF Core migrations on the containerized database
        using IServiceScope scope = Services.CreateScope();
        CoinDbContext dbContext = scope.ServiceProvider.GetRequiredService<CoinDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["Authentication:ApiKey"] = ValidApiKey,
                ["Authentication:DefaultUserId"] = DefaultUserId.ToString()
            });
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        HttpClient client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", ValidApiKey);
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        CoinDbContext dbContext = scope.ServiceProvider.GetRequiredService<CoinDbContext>();
        await dbContext.Transactions.IgnoreQueryFilters().ExecuteDeleteAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
