# Tasks

## 1. Configuration & Options

- [x] 1.1 Define `ApiKeyOptions` in `src/Coin.API/Configuration/ApiKeyOptions.cs` and configure `Authentication` section in `src/Coin.API/appsettings.json` and `src/Coin.API/appsettings.Development.json`
- [x] 1.2 Register `ApiKeyOptions` in `src/Coin.API/Program.cs` with validation and verify compilation via `dotnet build`

## 2. Middleware & Extensions

- [x] 2.1 Implement `ApiKeyMiddleware` in `src/Coin.API/Middleware/ApiKeyMiddleware.cs` with constant-time byte comparison (`FixedTimeEquals`), path bypass for `/scalar` and `/openapi`, `UserId` attachment to `HttpContext.Items`, and RFC 7807 `ProblemDetails` on 401 Unauthorized
- [x] 2.2 Implement `HttpContextExtensions` in `src/Coin.API/Extensions/HttpContextExtensions.cs` with `GetUserId(this HttpContext)` helper method
- [x] 2.3 Register `ApiKeyMiddleware` into the HTTP request pipeline in `src/Coin.API/Program.cs` and verify build succeeds with 0 errors and 0 warnings

## 3. Automated Testing

- [x] 3.1 Add unit tests for `ApiKeyMiddleware` covering valid key, missing header, invalid key, route bypass, and `UserId` injection in `tests/Coin.API.UnitTests` or `tests/Coin.IntegrationTests`
- [x] 3.2 Run `dotnet test` and confirm all solution test suites pass with 0 failures

## 4. Documentation & Verification

- [x] 4.1 Update `CHANGELOG.md` under `[Unreleased]` with API key authentication middleware
- [x] 4.2 Run `openspec validate` to verify specification and task consistency
