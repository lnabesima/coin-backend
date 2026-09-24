# Design

## Context

See `proposal.md` for motivation.

The API layer is built on ASP.NET Core in .NET 10. The application layer services (`ITransactionService`) require a valid `Guid userId` for every operation to guarantee multi-tenant boundary isolation. In upcoming endpoints (Issue #7), controllers will retrieve the authenticated user identifier from `HttpContext.Items`.

## Goals / Non-Goals

**Goals:**
- Provide an ASP.NET Core middleware (`ApiKeyMiddleware`) executing early in the request pipeline.
- Securely validate the `X-Api-Key` request header using constant-time byte comparison (`CryptographicOperations.FixedTimeEquals`).
- Inject the resolved tenant ID as a `System.Guid` into `HttpContext.Items["UserId"]`.
- Return standard RFC 7807 `ProblemDetails` formatted as `application/problem+json` with HTTP status 401 when the key is absent or invalid.
- Permit unauthenticated access exclusively to exploratory documentation routes (`/scalar`, `/openapi`) which are only registered in development, allowing developers to view the API explorer in browsers without manual header injection.
- Support strongly-typed configuration via `ApiKeyOptions` bound from `Authentication`.
- Provide an extension method `context.GetUserId()` for type-safe retrieval.
- Never store secrets in committed base configuration files; use empty placeholders in `appsettings.json`, disposable values in `appsettings.Development.json`, and environment variables in cloud/production environments.

**Non-Goals:**
- Multi-user authentication, JWT tokens, or identity management (deferred to future identity milestones).
- Database-backed dynamic API key storage or key rotation APIs.

## Decisions

### 1. Custom Middleware vs AuthenticationHandler
- **Decision**: Implement a dedicated `ApiKeyMiddleware` registered via `app.UseMiddleware<ApiKeyMiddleware>()`.
- **Rationale**: Minimal footprint, zero external dependencies, full control over short-circuiting and exact RFC 7807 response formatting, directly aligned with Issue #6 scope.
- **Alternative considered**: ASP.NET Core `AuthenticationHandler<AuthenticationSchemeOptions>`. Considered heavier with ceremonial scheme registration not needed for a single API Key model.

### 2. Constant-Time Key Comparison
- **Decision**: Compare the incoming header against the configured key using `CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(providedKey), Encoding.UTF8.GetBytes(expectedKey))`.
- **Rationale**: Standard string equality (`==` or `.Equals()`) short-circuits on the first mismatched byte, creating timing side-channel attack vectors. Fixed-time comparison eliminates this vulnerability.

### 3. Secrets & Configuration Hierarchy
- **Decision**:
  - `appsettings.json` (committed base): empty placeholders only:
    ```json
    "Authentication": {
      "ApiKey": "",
      "DefaultUserId": ""
    }
    ```
  - `appsettings.Development.json`: disposable local development key:
    ```json
    "Authentication": {
      "ApiKey": "coin-development-api-key-secret-2026",
      "DefaultUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
    ```
  - Production / Cloud (e.g. Azure Container Apps): injected via environment variables (`Authentication__ApiKey` and `Authentication__DefaultUserId`).
- **Rationale**: Complies strictly with zero-secrets-in-repo guidelines while providing out-of-the-box local development portability.

### 4. Direct RFC 7807 Serialization on 401
- **Decision**: Return a `ProblemDetails` instance serialized directly with `System.Text.Json` to the response stream when authentication fails.
- **Structure**:
  ```json
  {
    "type": "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
    "title": "Unauthorized",
    "status": 401,
    "detail": "API Key is missing or invalid.",
    "instance": "/api/v1/..."
  }
  ```
- **Rationale**: Complies with RFC 7807 and ASP.NET Core standard error response conventions.

### 5. Whitelisting Public Documentation Endpoints
- **Decision**: Middleware checks `context.Request.Path`: if the path starts with `/scalar` or `/openapi`, it immediately passes to `_next(context)`.
- **Rationale**: These routes are only registered in `app.Environment.IsDevelopment()` (in production they return 404). In development, browsers cannot send custom headers when loading HTML pages; bypass allows the documentation interface to load, while interactive API calls issued by the UI still require `X-Api-Key`.

## Risks / Trade-offs

- **[Key Exposure]** → Mitigated by empty placeholders in `appsettings.json` and injecting production keys via environment variables / Azure secrets.
- **[Public Route Leakage]** → Mitigated by OpenAPI/Scalar being strictly gated behind `app.Environment.IsDevelopment()`.
