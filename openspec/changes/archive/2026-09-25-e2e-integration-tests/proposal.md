# Proposal: End-to-End Integration Tests

## Why
While unit tests with mocked dependencies cover isolated logic across the Domain, Application, Infrastructure, and API layers, automated end-to-end integration tests using `WebApplicationFactory<Program>` are required to verify the entire ASP.NET Core request pipeline (routing, middleware, authentication, dependency injection, model binding, and HTTP serialization) against the complete transaction lifecycle.

## What Changes
- Implement a test server fixture based on `WebApplicationFactory<Program>` in `Coin.IntegrationTests`.
- Add comprehensive end-to-end integration tests covering the complete CRUD lifecycle of transactions (`POST`, `GET`, `GET by ID`, `PUT`, `DELETE`).
- Verify API Key authentication enforcement (`X-Api-Key` missing/invalid vs valid) in an integrated pipeline.
- Verify RFC 7807 ProblemDetails responses in an integrated HTTP environment.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None.

## Impact
- Affected code: `tests/Coin.IntegrationTests/`.
- Dependencies: `Microsoft.AspNetCore.Mvc.Testing` (already referenced).
- No production code breaking changes.
