# Design

## Context
`CoinDbContext` and `ITransactionRepository` have been established in previous changes. This design covers the concrete implementation of `TransactionRepository` in `Coin.Infrastructure` using EF Core.

## Goals / Non-Goals

**Goals:**
- Implement `TransactionRepository` satisfying `ITransactionRepository`.
- Ensure all queries strictly filter by `UserId` (mandatory multi-tenant isolation).
- Optimize read queries using `.AsNoTracking()` where appropriate and sort by `Date` descending.
- Leverage the `(UserId, Date)` composite index for queries.
- Register `ITransactionRepository` in `Coin.Infrastructure/DependencyInjection.cs`.
- Provide automated tests verifying all CRUD operations and tenant isolation.

**Non-Goals:**
- Modifying `CoinDbContext` or entity configurations.
- Adding pagination or complex aggregations (future cards).

## Decisions

### 1. Query Filtering and Ordering
- **Decision**: All queries begin with `.Where(t => t.UserId == userId)`. In `GetAllAsync`, apply optional `startDate` and `endDate` range filters, and order by `t.Date` descending.
- **Rationale**: Strict tenant isolation prevents cross-tenant data leaks. Ordering by `Date` descending matches user expectations for financial statements and leverages the `IX_Transactions_UserId_Date` index.

### 2. Change Tracking
- **Decision**: Use `.AsNoTracking()` for `GetAllAsync` since it is purely a read operation. `GetByIdAsync` will return tracked entities to facilitate updates, but `UpdateAsync` will also explicitly attach/update via `_context.Transactions.Update(transaction)` to support disconnected scenarios.

### 3. Repository Testing Strategy
- **Decision**: Test `TransactionRepository` using an in-memory SQLite / EF Core DbContext to verify query logic, tenant filtering, and persistence without external network dependencies.
- **Rationale**: Fast, isolated, reliable execution during CI and local development.

## Risks / Trade-offs

- **[Risk]** Missing `UserId` in a query causing data leakage.  
  **→ Mitigation**: `ITransactionRepository` interface mandates `Guid userId` in query signatures, and tests explicitly verify that records of user A are never returned when querying for user B.
