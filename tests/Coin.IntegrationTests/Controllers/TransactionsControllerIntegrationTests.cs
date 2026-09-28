namespace Coin.IntegrationTests.Controllers;

using System.Net;
using System.Net.Http.Json;
using Coin.Application.DTOs.Transactions;
using Coin.Domain.Enums;
using Coin.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

public sealed class TransactionsControllerIntegrationTests(CoinWebApplicationFactory factory)
    : IClassFixture<CoinWebApplicationFactory>
{
    private readonly CoinWebApplicationFactory _factory = factory;

    [Fact]
    public async Task Request_WithoutApiKeyHeader_Returns401UnauthorizedProblemDetails()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/v1/transactions");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Equal("API Key is missing.", problem.Detail);
    }

    [Fact]
    public async Task Request_WithInvalidApiKeyHeader_Returns401UnauthorizedProblemDetails()
    {
        // Arrange
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "invalid-key-value");

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/v1/transactions");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Equal("API Key is invalid.", problem.Detail);
    }

    [Fact]
    public async Task FullCrudLifecycle_ExecutesSuccessfullyAgainstRealDatabase()
    {
        // Arrange
        using HttpClient client = _factory.CreateAuthenticatedClient();
        await _factory.ResetDatabaseAsync();

        DateTime now = DateTime.UtcNow;
        CreateTransactionDto createDto = new(
            "Monthly Salary",
            5000.00m,
            now,
            TransactionType.Income,
            TransactionCategory.Salary);

        // 1. CREATE (POST /api/v1/transactions)
        HttpResponseMessage postResponse = await client.PostAsJsonAsync("/api/v1/transactions", createDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        TransactionResponseDto? created = await postResponse.Content.ReadFromJsonAsync<TransactionResponseDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(CoinWebApplicationFactory.DefaultUserId, created.UserId);
        Assert.Equal("Monthly Salary", created.Description);
        Assert.Equal(5000.00m, created.Amount);
        Assert.Equal(5000.00m, created.SignedAmount);
        Assert.Equal(TransactionType.Income, created.Type);
        Assert.Equal(TransactionCategory.Salary, created.Category);
        Assert.Contains(created.Id.ToString(), postResponse.Headers.Location.ToString());

        Guid transactionId = created.Id;

        // 2. GET BY ID (GET /api/v1/transactions/{id})
        HttpResponseMessage getByIdResponse = await client.GetAsync($"/api/v1/transactions/{transactionId}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        TransactionResponseDto? fetched = await getByIdResponse.Content.ReadFromJsonAsync<TransactionResponseDto>();
        Assert.NotNull(fetched);
        Assert.Equal(transactionId, fetched.Id);
        Assert.Equal("Monthly Salary", fetched.Description);

        // 3. UPDATE (PUT /api/v1/transactions/{id})
        UpdateTransactionDto updateDto = new(
            "Promoted Monthly Salary",
            6500.00m,
            now.AddHours(1),
            TransactionType.Income,
            TransactionCategory.Salary);

        HttpResponseMessage putResponse = await client.PutAsJsonAsync($"/api/v1/transactions/{transactionId}", updateDto);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        // Verify update persisted
        HttpResponseMessage getUpdatedResponse = await client.GetAsync($"/api/v1/transactions/{transactionId}");
        Assert.Equal(HttpStatusCode.OK, getUpdatedResponse.StatusCode);
        TransactionResponseDto? updated = await getUpdatedResponse.Content.ReadFromJsonAsync<TransactionResponseDto>();
        Assert.NotNull(updated);
        Assert.Equal("Promoted Monthly Salary", updated.Description);
        Assert.Equal(6500.00m, updated.Amount);
        Assert.Equal(6500.00m, updated.SignedAmount);

        // 4. LIST (GET /api/v1/transactions)
        HttpResponseMessage listResponse = await client.GetAsync("/api/v1/transactions");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        List<TransactionResponseDto>? transactions = await listResponse.Content.ReadFromJsonAsync<List<TransactionResponseDto>>();
        Assert.NotNull(transactions);
        Assert.Single(transactions);
        Assert.Equal(transactionId, transactions[0].Id);

        // Filter with matching range
        HttpResponseMessage filteredResponse = await client.GetAsync(
            $"/api/v1/transactions?startDate={now.AddDays(-1):yyyy-MM-dd}&endDate={now.AddDays(1):yyyy-MM-dd}");
        Assert.Equal(HttpStatusCode.OK, filteredResponse.StatusCode);
        List<TransactionResponseDto>? filtered = await filteredResponse.Content.ReadFromJsonAsync<List<TransactionResponseDto>>();
        Assert.NotNull(filtered);
        Assert.Single(filtered);

        // Filter with non-matching range
        HttpResponseMessage emptyFilteredResponse = await client.GetAsync(
            $"/api/v1/transactions?startDate={now.AddYears(-5):yyyy-MM-dd}&endDate={now.AddYears(-4):yyyy-MM-dd}");
        Assert.Equal(HttpStatusCode.OK, emptyFilteredResponse.StatusCode);
        List<TransactionResponseDto>? emptyFiltered = await emptyFilteredResponse.Content.ReadFromJsonAsync<List<TransactionResponseDto>>();
        Assert.NotNull(emptyFiltered);
        Assert.Empty(emptyFiltered);

        // 5. DELETE (DELETE /api/v1/transactions/{id})
        HttpResponseMessage deleteResponse = await client.DeleteAsync($"/api/v1/transactions/{transactionId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 6. SUBSEQUENT GET BY ID -> 404 NOT FOUND (Soft-deleted)
        HttpResponseMessage getAfterDelete = await client.GetAsync($"/api/v1/transactions/{transactionId}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
        ProblemDetails? notFoundProblem = await getAfterDelete.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(notFoundProblem);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundProblem.Status);

        // 7. SUBSEQUENT DELETE -> 404 NOT FOUND
        HttpResponseMessage deleteAgainResponse = await client.DeleteAsync($"/api/v1/transactions/{transactionId}");
        Assert.Equal(HttpStatusCode.NotFound, deleteAgainResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WhenPayloadViolatesDomainRules_Returns400BadRequestProblemDetails()
    {
        // Arrange
        using HttpClient client = _factory.CreateAuthenticatedClient();

        CreateTransactionDto invalidDto = new(
            "Invalid Amount",
            -100.00m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/transactions", invalidDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Contains("Amount must be strictly greater than zero", problem.Detail);
    }

    [Fact]
    public async Task Update_WhenPayloadViolatesDomainRules_Returns400BadRequestProblemDetails()
    {
        // Arrange
        using HttpClient client = _factory.CreateAuthenticatedClient();
        await _factory.ResetDatabaseAsync();

        CreateTransactionDto createDto = new(
            "Valid Transaction",
            100.00m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        HttpResponseMessage postResponse = await client.PostAsJsonAsync("/api/v1/transactions", createDto);
        TransactionResponseDto? created = await postResponse.Content.ReadFromJsonAsync<TransactionResponseDto>();
        Assert.NotNull(created);

        UpdateTransactionDto invalidUpdate = new(
            "", // Empty description violates domain rule
            100.00m,
            DateTime.UtcNow,
            TransactionType.Expense,
            TransactionCategory.Food);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/v1/transactions/{created.Id}", invalidUpdate);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Contains("Description cannot be empty", problem.Detail);
    }

    [Fact]
    public async Task GetById_WhenTransactionDoesNotExist_Returns404NotFoundProblemDetails()
    {
        // Arrange
        using HttpClient client = _factory.CreateAuthenticatedClient();
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/v1/transactions/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("Not Found", problem.Title);
        Assert.Contains(nonExistentId.ToString(), problem.Detail);
    }
}
