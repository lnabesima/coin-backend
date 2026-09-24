# Proposal

## Why

Incoming API requests currently lack authentication and tenant context. To protect system endpoints from unauthorized access and supply a consistent tenant identity (`UserId`) to downstream application services, an API Key authentication middleware must be introduced.

## What Changes

- Add `ApiKeyMiddleware` in `src/Coin.API/Middleware/ApiKeyMiddleware.cs` that intercepts HTTP requests, extracts the `X-Api-Key` header, and compares it securely against the configured API key using constant-time byte comparison.
- Inject the authenticated `UserId` (configured via `Authentication:DefaultUserId`) into `HttpContext.Items["UserId"]` for valid requests.
- Return HTTP 401 Unauthorized formatted as RFC 7807 `ProblemDetails` when the `X-Api-Key` header is missing, empty, or invalid.
- Bypass authentication for exploratory documentation routes (`/scalar`, `/openapi`) which are only mapped in development, allowing developers to view API docs in their browser without manual header injection. All actual API calls made through the UI still require `X-Api-Key`.
- Define empty placeholders in `appsettings.json` (`"ApiKey": ""`, `"DefaultUserId": ""`) to document the configuration contract without exposing secrets in Git.
- Provide disposable local development credentials in `appsettings.Development.json` for developer portability. Production secrets will be injected via environment variables (`Authentication__ApiKey`, `Authentication__DefaultUserId`).
- Register the middleware in the ASP.NET Core pipeline in `src/Coin.API/Program.cs`.
- Add comprehensive unit tests covering missing key, invalid key, valid key with `UserId` injection, and documentation route exclusions.

## Capabilities

### New Capabilities
- `authentication`: Defines the contract and behavior for incoming API request authentication via `X-Api-Key`, tenant identity resolution, and 401 Unauthorized ProblemDetails responses.

### Modified Capabilities
<!-- None -->

## Impact

- **API Layer**: `src/Coin.API/Middleware/ApiKeyMiddleware.cs`, `src/Coin.API/Program.cs`.
- **Configuration**: `src/Coin.API/appsettings.json` (placeholders), `src/Coin.API/appsettings.Development.json` (local dev key).
- **Testing**: Adds test coverage for middleware authentication and error responses.
- **Breaking Changes**: All API routes will now require a valid `X-Api-Key` header.
