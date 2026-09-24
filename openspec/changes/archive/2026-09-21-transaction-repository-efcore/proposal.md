# Proposal

## Why
While `ITransactionRepository` was defined in `Coin.Application`, no concrete implementation exists to persist and retrieve transactions from the database. We need to implement `TransactionRepository` using EF Core (`CoinDbContext`) in `Coin.Infrastructure` to enable real database operations with mandatory user isolation.

## What Changes
- Implement `TransactionRepository` in `src/Coin.Infrastructure/Repositories/TransactionRepository.cs`:
  - `AddAsync(Transaction transaction, CancellationToken cancellationToken)`: Persists a transaction via `CoinDbContext.Transactions.AddAsync` and calls `SaveChangesAsync`.
  - `GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)`: Retrieves a transaction filtering strictly by `UserId` and `Id`.
  - `GetAllAsync(Guid userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)`: Retrieves transactions for the tenant, applying optional date range filters, ordered by `Date` descending.
  - `UpdateAsync(Transaction transaction, CancellationToken cancellationToken)`: Updates the entity via `CoinDbContext.Transactions.Update` and calls `SaveChangesAsync`.
  - `DeleteAsync(Transaction transaction, CancellationToken cancellationToken)`: Removes the entity via `CoinDbContext.Transactions.Remove` and calls `SaveChangesAsync`.
- Register `ITransactionRepository` as `Scoped` in `src/Coin.Infrastructure/DependencyInjection.cs`.
- Add comprehensive repository tests verifying CRUD operations and strict tenant isolation.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `transactions`: Add relational repository operations and tenant isolation requirements.

## Impact
- **Affected code**: `src/Coin.Infrastructure/Repositories/TransactionRepository.cs`, `src/Coin.Infrastructure/DependencyInjection.cs`
- **Dependencies**: Uses existing `Microsoft.EntityFrameworkCore` and `CoinDbContext`.
- **APIs / Systems**: Completes the persistence pipeline for transactions, enabling end-to-end execution from application services down to PostgreSQL.
