# Tasks

## 1. Configuration & Authentication Handler

- [ ] 1.1 Add CORS configuration options in `appsettings.json` and `appsettings.Development.json` specifying allowed origins and verify options binding.
- [ ] 1.2 Implement `ApiKeyAuthenticationHandler` deriving from `AuthenticationHandler<ApiKeyAuthenticationOptions>` in `src/Coin.API/Authentication/` with constant-time API key verification, `ClaimsPrincipal` creation (`ClaimTypes.NameIdentifier`), and ProblemDetails challenge handling.
- [ ] 1.3 Add unit tests in `tests/Coin.API.UnitTests/Authentication/ApiKeyAuthenticationHandlerTests.cs` covering valid keys, missing keys, invalid keys, and challenge responses, verifying all unit tests pass.

## 2. Security Pipeline, CORS & Health Checks

- [ ] 2.1 Configure CORS policy (`FrontendPolicy`), native authentication scheme, authorization services, and health checks in `src/Coin.API/Program.cs` with proper pipeline ordering (`UseCors` -> `UseAuthentication` -> `UseAuthorization` -> `MapHealthChecks`).
- [ ] 2.2 Add `[Authorize]` attribute to `src/Coin.API/Controllers/TransactionsController.cs` to secure transaction endpoints.
- [ ] 2.3 Remove deprecated `src/Coin.API/Middleware/ApiKeyMiddleware.cs` and `tests/Coin.API.UnitTests/Middleware/ApiKeyMiddlewareTests.cs`.

## 3. Integration Testing & Verification

- [ ] 3.1 Add integration tests in `tests/Coin.IntegrationTests/` validating unauthenticated `/health` returning 200 OK, CORS preflight `OPTIONS` returning 204 No Content with access control headers, and protected transaction endpoints requiring `X-Api-Key`.
- [ ] 3.2 Run the full test suite (`dotnet test`) and verify all unit and integration tests pass with 0 errors and 0 warnings.
