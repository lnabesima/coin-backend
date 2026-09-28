# Design

## Context
See [proposal.md](proposal.md) for motivation. `Coin.Infrastructure` registers `CoinDbContext` using `options.UseNpgsql(connectionString)` without configuring execution strategies or retry policies. In serverless cloud environments (Neon PostgreSQL in `aws-sa-east-1`), the database compute instance suspends after periods of inactivity (scale-to-zero) and resumes upon receiving inbound TCP connections. Network latency or transient delays during compute wake-up can result in temporary connection drops.

## Goals / Non-Goals

**Goals:**
- Enable Npgsql native retry policies (`EnableRetryOnFailure`) on `CoinDbContext` registration in `Coin.Infrastructure`.
- Implement `task db:migrate` and `task db:status` commands in `Taskfile.yml` for reliable migration deployment using the direct connection string.
- Provide comprehensive documentation in `README.md` for cloud configuration, SSL requirements, and difference between direct and pooled connection strings.

**Non-Goals:**
- External resilience libraries (e.g. Polly) for database operations: EF Core's built-in `NpgsqlRetryingExecutionStrategy` natively understands PostgreSQL error codes and safely resets `ChangeTracker` and transaction states.
- Running migrations automatically on API container startup in production: API instances connect to the PgBouncer pooler endpoint which rejects DDL migration locks, and multiple replicas would trigger concurrent migration conflicts.

## Decisions

### Decision 1: Native Npgsql Retrying Execution Strategy over Polly
- **Choice**: Use `npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)`.
- **Rationale**: EF Core transactions require the execution strategy to manage commit, rollback, and change tracking replay. Wrapping EF Core calls with generic Polly policies can corrupt change tracking or result in "transaction already completed" exceptions.
- **Alternatives Considered**: Polly pipeline wrapping repositories. Rejected due to state corruption risk in `DbContext`.

### Decision 2: CI/CD and CLI-driven Migrations over Application Startup
- **Choice**: Execute migrations via `Taskfile.yml` (`task db:migrate`) using the Direct Connection endpoint (`ep-xxx.sa-east-1.aws.neon.tech`).
- **Rationale**: The production API running in Azure Container Apps must use the Neon PgBouncer pooler endpoint (`ep-xxx-pooler...`) to optimize connection usage. PgBouncer in transaction mode does not support the DDL and advisory locks required by EF Core migrations.
- **Alternatives Considered**: `context.Database.MigrateAsync()` on startup. Rejected due to PgBouncer incompatibility and multi-replica race conditions.

### Decision 3: Local Dev Isolation
- **Choice**: Maintain local PostgreSQL via Docker Compose (`docker compose up -d`) and Testcontainers for integration tests.
- **Rationale**: Preserves Neon's free tier quota (100 CU-hours/month) and ensures offline developer experience without internet latency.

## Risks / Trade-offs

- **[Risk]** Developer accidentally targets pooled connection string for migrations.
  - **Mitigation:** Document clearly in `README.md` and script `task db:migrate` with verification of connection string parameters.
- **[Risk]** Neon cold start latency delays initial query.
  - **Mitigation:** `EnableRetryOnFailure` with 3 retries and 5s exponential delay transparently absorbs Neon's ~500ms-1s wake-up window.
