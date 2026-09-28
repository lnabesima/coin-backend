# Proposal

## Why
Financial transactions modeled in `Coin.Domain` must be persisted reliably in a relational database with strict data constraints, column mappings, and version-controlled schema migrations. Configuring Entity Framework Core with PostgreSQL enables reliable persistence, type-safe queries, and auditability while keeping infrastructure details decoupled from domain models.

## What Changes
- Add `Npgsql.EntityFrameworkCore.PostgreSQL` and `Microsoft.EntityFrameworkCore.Design` packages to `Coin.Infrastructure` and `Coin.API`.
- Create `CoinDbContext` in `src/Coin.Infrastructure/Persistence/CoinDbContext.cs` with `DbSet<Transaction> Transactions`.
- Create `TransactionConfiguration` in `src/Coin.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs` configuring Fluent API:
  - Table name `"Transactions"`.
  - Primary key `Id`.
  - Non-nullable `UserId` with a composite database index on `(UserId, Date)` for tenant-scoped query and statement performance.
  - Non-nullable `Description` with max length constraint (e.g. 250 characters).
  - Column precision for monetary `Amount` (`decimal(18,2)`).
  - UTC timestamps for `Date` and `CreatedAt`.
  - Store `Type` and `Category` enums as numeric integer values for minimal storage footprint and high performance.
  - Ignore the domain helper property `SignedAmount`.
- Create `AddInfrastructure` service extension in `src/Coin.Infrastructure/DependencyInjection.cs` to register `CoinDbContext` with Npgsql provider.
- Register infrastructure services in `src/Coin.API/Program.cs`.
- Generate and apply the initial EF Core migration (`InitialCreate`) to the local PostgreSQL database.

## Capabilities

### New Capabilities

### Modified Capabilities
- `transactions`: Adds relational persistence requirements, database schema constraints, and index definitions for PostgreSQL.

## Impact
- Affected projects: `Coin.Infrastructure` and `Coin.API`.
- Database: Creates `__EFMigrationsHistory` and `Transactions` table with indexes in PostgreSQL database `coin_db`.
- No breaking changes to existing domain models or unit tests.
