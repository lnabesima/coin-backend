# Design: End-to-End Integration Tests

## Context
The API exposes 5 REST endpoints under `/api/v1/transactions` protected by `ApiKeyMiddleware`.
To verify full-pipeline integration (routing, middleware, dependency injection, serialization, and real PostgreSQL database persistence), integration tests will execute using ASP.NET Core's `WebApplicationFactory<Program>` combined with `Testcontainers.PostgreSql`.
See proposal.md for motivation.

## Goals / Non-Goals

**Goals:**
- Provide a reusable `CoinWebApplicationFactory` fixture that spins up an ephemeral PostgreSQL 16 container via `Testcontainers.PostgreSql`, replaces the application database connection string, and automatically applies EF Core migrations (`Database.MigrateAsync()`).
- Implement end-to-end integration tests verifying the full lifecycle of a transaction:
  - `POST /api/v1/transactions` (201 Created)
  - `GET /api/v1/transactions/{id}` (200 OK)
  - `PUT /api/v1/transactions/{id}` (204 NoContent)
  - `GET /api/v1/transactions` with query filters (200 OK)
  - `DELETE /api/v1/transactions/{id}` (204 NoContent)
  - Subsequent `GET` / `DELETE` returning 404 Not Found.
- Verify security and authentication pipeline:
  - Reject requests with missing or invalid `X-Api-Key` returning 401 Unauthorized in ProblemDetails format.
  - Verify validation errors return 400 Bad Request in ProblemDetails format.

**Non-Goals:**
- In-memory database mocks (Testcontainers provides 100% genuine PostgreSQL behavior, constraints, enums, and SQL dialect).
- Performance or load testing.

## Decisions

### 1. WebApplicationFactory with Testcontainers.PostgreSql
- **Decision**: Subclass `WebApplicationFactory<Program>`, implement `IAsyncLifetime`, and manage a `PostgreSqlContainer` instance using image `postgres:16-alpine`.
- **Rationale**: Completely isolates test runs, tests against real PostgreSQL 16, automatically applies migrations, and requires zero manual database setup.
- **Alternatives considered**:
  - EF Core InMemory / SQLite: Lacks real PostgreSQL constraints, enums, and migration validation.
  - Shared existing dev container: Risk of state collision and requires manual container lifecycle.

### 2. XUnit Class Fixture
- **Decision**: Implement `IClassFixture<CoinWebApplicationFactory>` across integration test classes.
- **Rationale**: Efficiently reuses the container lifecycle for test suites, spinning up the container once per test collection/class.

### 3. HTTP Client with Helper Headers
- **Decision**: Provide helper methods to create pre-authenticated `HttpClient` instances with default `X-Api-Key` headers, as well as unauthenticated clients to test auth failures.
- **Rationale**: Reduces test boilerplate while keeping authentication assertions explicit.

## Risks / Trade-offs

- [Risk] Docker daemon accessibility from Windows host.
  → Mitigation: Docker daemon in WSL is configured with TCP endpoint (`tcp://127.0.0.1:2375`) and `DOCKER_HOST` environment variable.
