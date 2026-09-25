# Tasks

## 1. Connection Resilience Implementation

- [x] 1.1 Configure `EnableRetryOnFailure` with 3 retries and 5s exponential delay in `src/Coin.Infrastructure/DependencyInjection.cs` and verify solution compiles cleanly via `task build`
- [x] 1.2 Verify existing test suites pass with retry policy configured by executing `task test:unit`

## 2. Tooling and Migration Tasks

- [x] 2.1 Add `db:migrate` and `db:status` tasks to `Taskfile.yml` executing `dotnet ef database update` with optional `CONNECTION_STRING` override
- [x] 2.2 Verify `task --list` displays the new database management tasks and runs cleanly

## 3. Documentation and Verification

- [x] 3.1 Update `README.md` documenting cloud PostgreSQL configuration, direct vs pooled connection strings, SSL parameters, and migration execution
- [x] 3.2 Verify full test suite and build pass without regressions via `task test`
