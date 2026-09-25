# Tasks

## 1. Test Project Configuration & Testcontainers Fixture

- [x] 1.1 Add `Testcontainers.PostgreSql` package reference to `tests/Coin.IntegrationTests/Coin.IntegrationTests.csproj`
- [x] 1.2 Implement `CoinWebApplicationFactory` in `tests/Coin.IntegrationTests/Fixtures/CoinWebApplicationFactory.cs` managing `PostgreSqlContainer`, applying migrations, and configuring test API Key options
- [x] 1.3 Verify compilation of the integration test fixture with `dotnet build tests/Coin.IntegrationTests`

## 2. End-to-End Integration Tests

- [x] 2.1 Implement authentication integration tests in `tests/Coin.IntegrationTests/Controllers/TransactionsControllerIntegrationTests.cs` verifying 401 Unauthorized ProblemDetails on missing or invalid `X-Api-Key`
- [x] 2.2 Implement full transaction CRUD lifecycle integration test (POST 201 Created with Location header, GET by ID 200 OK, PUT 204 NoContent, GET list 200 OK, DELETE 204 NoContent, subsequent GET 404 Not Found)
- [x] 2.3 Implement domain validation error integration test verifying 400 Bad Request ProblemDetails on invalid payload
- [x] 2.4 Run `dotnet test` and verify that all integration and unit tests pass with 0 failures

## 3. Documentation & Verification

- [x] 3.1 Update `CHANGELOG.md` under `[Unreleased]` with end-to-end integration test suite and Testcontainers
- [x] 3.2 Run `openspec validate e2e-integration-tests` and verify specification and task consistency
