# Design

## Context
The domain model (`Transaction`, `TransactionType`, `TransactionCategory`) and persistence layer (`CoinDbContext`) are established. We now need the application layer contracts and services to coordinate use cases, isolate tenants by `userId`, and provide data contracts for incoming and outgoing data. See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**
- Provide strongly-typed DTO records for transaction creation, updates, and responses.
- Define `ITransactionRepository` with mandatory tenant (`userId`) isolation across all queries and commands.
- Implement `TransactionService` (`ITransactionService`) orchestrating domain validation, repository persistence, and DTO mappings.
- Provide comprehensive unit tests for `TransactionService` using a mocked repository.
- Register application services in `Coin.Application/DependencyInjection.cs`.

**Non-Goals:**
- Implementing the concrete EF Core `TransactionRepository` (deferred to Issue #5).
- Implementing the authentication middleware (`X-Api-Key` / `UserId` resolution) (deferred to Issue #6).
- Creating ASP.NET Core controllers or HTTP endpoints (deferred to Issue #7).

## Decisions

### 1. DTOs as Immutable C# Records
- **Decision**: Define `CreateTransactionDto`, `UpdateTransactionDto`, and `TransactionResponseDto` as positional C# `record` types.
- **Rationale**: Records provide immutability, concise syntax, value equality, and clean serialization semantics.
- **Alternatives considered**: Mutable classes with `{ get; set; }` properties — rejected to prevent unexpected mutation across layer boundaries.

### 2. Decoupling Tenant Context from HttpContext
- **Decision**: Pass `Guid userId` explicitly as a method argument to all `ITransactionService` and `ITransactionRepository` methods.
- **Rationale**: Keeps `Coin.Application` completely agnostic of web hosting concerns (`IHttpContextAccessor` / ASP.NET Core). This allows the application service to be invoked by API controllers, background workers, or CLI tools without web framework coupling.
- **Alternatives considered**: Injected `ICurrentUserService` — rejected for now as premature abstraction; explicit method arguments provide compile-time safety and transparency.

### 3. Not Found and Validation Handling
- **Decision**: Return nullable `TransactionResponseDto?` for `GetByIdAsync` and `UpdateAsync`, and `bool` for `DeleteAsync` when a transaction is not found (or belongs to another user). Invariant violations continue to throw domain validation exceptions (`ArgumentException`).
- **Rationale**: Not-found is an expected, non-exceptional operational outcome that controllers easily map to HTTP 404. Invariant violations represent invalid input that maps to HTTP 400.
- **Alternatives considered**: Custom `Result<T>` monad — rejected to keep application interfaces simple and idiomatic without external functional libraries.

### 4. Date Range Filtering in `GetAllAsync`
- **Decision**: Include optional `DateTime? startDate = null, DateTime? endDate = null` in `GetAllAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)`.
- **Rationale**: Aligns with statement and history query patterns specified in Issue #4 and upcoming endpoint requirements.

## Risks / Trade-offs

- **[Risk]** Cross-tenant data leakage if a query ignores `userId`.  
  **→ Mitigation**: Every query method on `ITransactionRepository` requires `Guid userId`, and `TransactionService` unit tests explicitly verify that entities belonging to other users return `null`/`false`.
- **[Risk]** Entity-to-DTO mapping boilerplate.  
  **→ Mitigation**: Implement clean static mapping helper methods (e.g. `TransactionResponseDto.FromDomain(Transaction)`) without introducing heavy reflection-based mapping libraries (like AutoMapper).
