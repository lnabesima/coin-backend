# Proposal

## Why
Deploying `coin-backend` to Microsoft Azure Container Apps with a managed serverless cloud database (Neon PostgreSQL) requires connection resilience against cold-start latency and transient network drops. In addition, developers and deployment pipelines require automated, standardized migration tasks to provision schema changes without compromising connection pooling constraints.

## What Changes
- Configure Npgsql / EF Core connection resilience in `Coin.Infrastructure` using `EnableRetryOnFailure` with configurable retry counts and delays.
- Add database migration and verification commands to `Taskfile.yml` (`task db:migrate`, `task db:status`) supporting both direct and local connection strings.
- Document cloud PostgreSQL configuration, SSL requirements, connection string differences (pooled vs direct), and environment variable templates in `README.md`.

## Capabilities

### New Capabilities

### Modified Capabilities
- `transactions`: Updates relational persistence requirements to mandate resilient connection retries on transient network and database restart failures.

## Impact
- Affected code: `src/Coin.Infrastructure/DependencyInjection.cs`, `Taskfile.yml`, `README.md`.
- Runtime behavior: EF Core operations will automatically retry on transient database errors instead of failing immediately.
- No breaking changes to existing domain models, API contracts, or unit/integration tests.
