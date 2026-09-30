# Proposal

## Why

With frontend integration commencing on Azure Static Web Apps (SWA) and container deployment running on Azure Container Apps (ACA), the API requires proper cross-origin request handling and unauthenticated health monitoring probes. The initial MVP implementation used a custom raw `ApiKeyMiddleware` that blindly rejects requests lacking `X-Api-Key` before routing occurs, breaking browser CORS preflight (`OPTIONS`) requests and preventing container platform liveness/readiness probes from executing without credentials. Migrating to ASP.NET Core's native authentication and authorization pipeline resolves these issues idiomatically.

## What Changes

- Implement `ApiKeyAuthenticationHandler` deriving from `AuthenticationHandler<ApiKeyAuthenticationOptions>` to handle `X-Api-Key` validation and create an `AuthenticationTicket` with a populated `ClaimsPrincipal` (`ClaimTypes.NameIdentifier`).
- Emit RFC 7807 `ProblemDetails` for 401 Unauthorized responses via `HandleChallengeAsync` using `IProblemDetailsService`.
- Secure `TransactionsController` using the standard `[Authorize]` attribute.
- Configure Cross-Origin Resource Sharing (CORS) in DI and register `app.UseCors()` in the pipeline before authentication/authorization, supporting configured frontend origins (Azure SWA production, SWA PR preview domains, and localhost).
- Register `builder.Services.AddHealthChecks()` and map `app.MapHealthChecks("/health")` as an unauthenticated public endpoint across all environments for ACA container probes.
- Remove obsolete `ApiKeyMiddleware.cs` and its corresponding unit tests (`ApiKeyMiddlewareTests.cs`).
- Add unit tests for `ApiKeyAuthenticationHandler` and integration tests validating CORS preflights, unauthenticated `/health` responses, and authorized controller access.

## Capabilities

### New Capabilities
- `cors`: Defines Cross-Origin Resource Sharing policies for allowed origins, headers, methods, and preflight response behavior.
- `health-checks`: Defines public, unauthenticated health check endpoints for container orchestrators and monitoring tools.

### Modified Capabilities
- `authentication`: Updates authentication mechanism from custom raw middleware to native ASP.NET Core authentication and endpoint authorization (`[Authorize]`), ensuring preflights and anonymous routes are unblocked.

## Impact

- **API Layer**: `src/Coin.API/Authentication/ApiKeyAuthenticationHandler.cs` (new), `src/Coin.API/Program.cs` (pipeline update), `src/Coin.API/Controllers/TransactionsController.cs` (`[Authorize]`), `src/Coin.API/Middleware/ApiKeyMiddleware.cs` (removed).
- **Configuration**: `src/Coin.API/appsettings.json` and `src/Coin.API/appsettings.Development.json` (CORS origin settings).
- **Tests**: `tests/Coin.API.UnitTests/Authentication/ApiKeyAuthenticationHandlerTests.cs` (new), `tests/Coin.API.UnitTests/Middleware/ApiKeyMiddlewareTests.cs` (removed), `tests/Coin.IntegrationTests/` (new test cases for CORS, health check, and authorization).
