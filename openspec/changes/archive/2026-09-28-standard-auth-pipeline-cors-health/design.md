# Design

## Context

See `proposal.md` for motivation. Currently, `Coin.API` implements a standalone `ApiKeyMiddleware` that intercepts requests before endpoint routing occurs. Because this custom middleware executes before CORS and lacks awareness of endpoint metadata (`[AllowAnonymous]`), browser cross-origin preflight `OPTIONS` requests fail with 401 Unauthorized, and external orchestrator health checks cannot be performed unauthenticated.

## Goals / Non-Goals

**Goals:**
- Implement idiomatic ASP.NET Core authentication using `AuthenticationHandler<ApiKeyAuthenticationOptions>`.
- Establish `ClaimsPrincipal` on `HttpContext.User` containing `ClaimTypes.NameIdentifier` set to `DefaultUserId`.
- Protect resource controllers with standard `[Authorize]` attribute.
- Configure CORS to support frontend consumption from Azure Static Web Apps (production and preview environments) and local development.
- Expose an unauthenticated, lightweight public `/health` endpoint for Azure Container Apps probes.
- Maintain full RFC 7807 `ProblemDetails` compliance on 401 Unauthorized challenge responses.

**Non-Goals:**
- Multi-tenant database-backed API key management (retains single configured key and default user ID from options).
- JWT or OAuth2 identity provider integration.
- Database health check probes (basic liveness probe is sufficient for v0.1.1).

## Decisions

### 1. Native AuthenticationHandler vs Custom Middleware
- **Decision**: Replace `ApiKeyMiddleware` with `ApiKeyAuthenticationHandler` deriving from `AuthenticationHandler<ApiKeyAuthenticationOptions>`.
- **Rationale**: Plugs directly into ASP.NET Core's security architecture (`app.UseAuthentication()`, `app.UseAuthorization()`). Endpoints not requiring authorization (such as `/health` or OpenAPI documentation) or preflight requests (`OPTIONS`) pass through naturally without hardcoded path string checks.
- **Alternatives Considered**: Patching `ApiKeyMiddleware` with `if (HttpMethods.IsOptions(context.Request.Method))` and `if (path == "/health")`. Rejected as fragile and redundant technical debt that works against framework conventions.

### 2. Challenge Handling and ProblemDetails
- **Decision**: Override `HandleChallengeAsync` in `ApiKeyAuthenticationHandler` to write an RFC 7807 `ProblemDetails` response (Status: 401, Title: "Unauthorized", Detail: "API Key is missing or invalid.") using `IProblemDetailsService`.
- **Rationale**: Preserves the API's existing RFC 7807 error contract expected by clients and established in integration tests.
- **Alternatives Considered**: Allowing the default challenge handler to emit an empty 401 status. Rejected because clients rely on structured ProblemDetails.

### 3. CORS Policy Configuration
- **Decision**: Register a CORS policy named `FrontendPolicy` that binds allowed origins from `Cors:AllowedOrigins` configuration, with support for Azure Static Web Apps subdomains (`*.azurestaticapps.net`) and localhost via `SetIsOriginAllowed`. Allowed headers explicitly include `X-Api-Key` and `Content-Type`.
- **Rationale**: Azure SWA generates dynamic preview URLs for pull requests. Rigid static origin lists would break staging and pull request verification.
- **Alternatives Considered**: `AllowAnyOrigin()` with `AllowAnyHeader()`. Rejected for security and hygiene in production.

### 4. Health Check Endpoint
- **Decision**: Use ASP.NET Core native health checks (`builder.Services.AddHealthChecks()` and `app.MapHealthChecks("/health")`).
- **Rationale**: Lightweight, zero external packages, and standard practice for container orchestration platforms like Azure Container Apps.

### 5. Middleware Pipeline Order in Program.cs
- **Decision**: Arrange HTTP pipeline as follows:
  1. `app.UseHttpsRedirection()`
  2. `app.UseCors("FrontendPolicy")`
  3. `app.UseAuthentication()`
  4. `app.UseAuthorization()`
  5. `app.MapHealthChecks("/health")`
  6. `app.MapControllers()`
- **Rationale**: `UseCors` must precede `UseAuthentication` so that preflight `OPTIONS` requests are answered before authentication is evaluated.

## Risks / Trade-offs

- **[Risk] Test Fixture Regressions**: Existing integration tests configure `X-Api-Key` on requests, but tests asserting 401 ProblemDetails might expect specific `Detail` messages from the previous middleware.
  - **Mitigation**: Align `HandleChallengeAsync` ProblemDetails structure with previous specifications and update test assertions accordingly.
- **[Risk] SWA Wildcard Matching Performance**: Using `SetIsOriginAllowed` with URI matching on every cross-origin request.
  - **Mitigation**: Use simple host suffix checking (`uri.Host.EndsWith("azurestaticapps.net") || uri.Host == "localhost"`) to avoid regex overhead.
