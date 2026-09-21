# Proposal

## Why
Currently, the system has a rich domain model and an EF Core persistence context, but lacks an application service layer to orchestrate transaction workflows. To enable API endpoints and client interactions, we need strongly typed data transfer objects (DTOs), repository abstractions, and an application service enforcing user isolation and business validation.

## What Changes
- Create transaction DTOs in `src/Coin.Application/DTOs/Transactions/`:
  - `CreateTransactionDto`: input contract for creating a transaction
  - `UpdateTransactionDto`: input contract for updating an existing transaction
  - `TransactionResponseDto`: output contract returning transaction details (including calculated `SignedAmount`)
- Define repository abstraction `ITransactionRepository` in `src/Coin.Application/Interfaces/` with tenant-aware methods (`AddAsync`, `GetByIdAsync`, `GetAllAsync`, `UpdateAsync`, `DeleteAsync`).
- Define application service abstraction `ITransactionService` and implementation `TransactionService` in `src/Coin.Application/`:
  - Orchestrates creation, retrieval, updates, and deletion.
  - Enforces mandatory user isolation across all operations.
  - Maps between Domain entities and Application DTOs.
- Register application layer services via dependency injection in `src/Coin.Application/DependencyInjection.cs`.
- Add comprehensive unit tests in `tests/Coin.Application.UnitTests/` covering all `TransactionService` scenarios with a mocked repository.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `transactions`: Add application orchestration and user-isolated transaction service requirements.

## Impact
- **Affected code**: `src/Coin.Application/`, `tests/Coin.Application.UnitTests/`
- **Dependencies**: No new external dependencies required; uses `Microsoft.Extensions.DependencyInjection.Abstractions` and domain types.
- **APIs / Systems**: Establishes the core contracts and services consumed by the upcoming API controllers and endpoints.
