# Design

## Context

The domain model contains `Transaction`, `TransactionType`, and `TransactionCategory`. The application needs an infrastructure persistence layer using Entity Framework Core and PostgreSQL. A local PostgreSQL 16 container (`coin_db`) is already configured and running via Docker Compose. See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**
- Configure `CoinDbContext` with `Npgsql.EntityFrameworkCore.PostgreSQL`.
- Implement clean Fluent API mapping for `Transaction` via `IEntityTypeConfiguration<Transaction>`, keeping domain models free of persistence attributes.
- Enforce relational constraints: `Id` as primary key, `Amount` as `decimal(18,2)`, `Description` with max length 250, and composite index on `(UserId, Date)`.
- Store enums as compact numeric integers in the database.
- Provide `AddInfrastructure` extension method in `Coin.Infrastructure/DependencyInjection.cs`.
- Generate and apply the initial migration `InitialCreate`.

**Non-Goals:**
- Concrete repository implementations (Issue #5).
- Application service layer or CQRS handlers (Issue #4).
- Automatic migration execution at application startup in production (migrations should be controlled and explicit).

## Decisions

### 1. Fluent API Configuration via `IEntityTypeConfiguration<T>`
- **Decision**: Define entity mappings in `TransactionConfiguration : IEntityTypeConfiguration<Transaction>` inside `Coin.Infrastructure/Persistence/Configurations/`.
- **Rationale**: Clean Architecture strictly prohibits decorating domain entities with infrastructure/ORM attributes (`[Table]`, `[Column]`, etc.).
- **Alternatives Considered**: Inline configurations in `OnModelCreating` - rejected to prevent `CoinDbContext` from becoming cluttered as new entities are added.

### 2. Enum Storage Strategy (Numeric Integer Storage)
- **Decision**: Store `TransactionType` and `TransactionCategory` as numeric integer values in PostgreSQL (default EF Core enum mapping).
- **Rationale**: Integers minimize disk storage (4 bytes vs variable strings up to 15+ bytes per row), reduce memory pressure in the PostgreSQL buffer cache, and execute index/comparison operations faster. Direct database debugging is not a requirement, making compact integer storage the superior choice.
- **Alternatives Considered**: String conversion (`.HasConversion<string>()`) - rejected due to unnecessary storage and memory overhead.

### 3. Tenant Indexing on `(UserId, Date)`
- **Decision**: Create a single composite index on `(UserId, Date)`.
- **Rationale**: In PostgreSQL B-Tree indexes, queries filtering exclusively by the leading column (`UserId`) fully utilize the composite index. Adding a separate single-column index on `UserId` would be redundant. Furthermore, sorting and filtering by date range for a specific user (e.g. statement views) is natively accelerated. Additional indexes on low-cardinality columns like `(UserId, Type)` or `(UserId, Category)` are deliberately avoided to minimize write overhead and disk footprint.
- **Alternatives Considered**: Dual indexes (`UserId` + `(UserId, Date)`) or category indexes - rejected as redundant over-indexing.

### 4. Computed Property Handling
- **Decision**: Configure `builder.Ignore(t => t.SignedAmount)` in `TransactionConfiguration`.
- **Rationale**: `SignedAmount` is a domain-layer calculation and does not require physical storage in the database.

## Risks / Trade-offs

- **PostgreSQL DateTime Handling (timestamptz)**: Npgsql 6+ requires `DateTime` values to have `DateTimeKind.Utc` when writing to `timestamptz` columns.
  - *Mitigation*: The `Transaction` entity constructor already enforces UTC conversion, ensuring seamless compatibility with Npgsql.
- **Migration Startup Coupling**: Running migrations on app boot can cause race conditions in clustered deployments.
  - *Mitigation*: Migrations are managed via CLI (`dotnet ef database update`) rather than automatic `context.Database.Migrate()` on every web request.
