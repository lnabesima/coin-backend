# Design

## Context

The solution follows Clean Architecture principles. The `Coin.Domain` project is the innermost layer and must contain no external dependencies, database frameworks, or UI/API concerns. See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**
- Provide a robust, encapsulated `Transaction` entity ensuring that instances can never be created in an invalid state.
- Model `TransactionType` and `TransactionCategory` as clear, type-safe enums.
- Ensure 100% test coverage for domain invariants in `Coin.Domain.UnitTests`.

**Non-Goals:**
- Persistence and EF Core configuration (addressed in Issue #3).
- Application service layer, commands/queries, or API endpoints (addressed in subsequent issues).
- Multi-currency conversions or custom user-defined category entities (future scope).

## Decisions

### 1. Rich Domain Entity with Guard Clauses
- **Decision**: Validate invariants directly within the `Transaction` entity constructor and expose getters with private setters.
- **Rationale**: Prevents invalid transactions (such as negative or zero amounts, empty descriptions, or missing user IDs) from ever being constructed in memory.
- **Alternatives Considered**: Anemic model with external validator - rejected because it violates Clean Architecture principles by scattering business invariants outside the domain model.

### 2. Monetary Precision with `decimal`
- **Decision**: Use `decimal` for `Amount`.
- **Rationale**: `decimal` prevents floating-point inaccuracies inherent in `double` or `float` when handling monetary currency calculations.

### 3. Enum-based Categorization
- **Decision**: Use an enum (`TransactionCategory`) with standard categories (`Food`, `Housing`, `Transportation`, `Salary`, `Health`, `Leisure`, `Education`, `Other`).
- **Rationale**: Simple, highly performant, and perfectly fits current requirements without unnecessary table joins or complexity. Future requirements for user-customizable categories can introduce a dedicated entity when needed.

### 4. Direct `UserId` Assignment
- **Decision**: Include `UserId` (`Guid`) directly on `Transaction`.
- **Rationale**: Ensures every transaction is natively multi-tenant and tied to a user from day one.

### 5. Positive Absolute Amounts and Balance Calculation Strategy
- **Decision**: Store `Amount` as a strictly positive magnitude (`Amount > 0`), with `Type` (`Income` vs `Expense`) determining directional flow. Expose a domain helper property `SignedAmount => Type == TransactionType.Income ? Amount : -Amount;` for in-memory domain evaluations.
- **Rationale**: Storing negative amounts creates ambiguity (e.g. distinguishing an expense from a refund or correction, or risk of inconsistent sign inputs).
- **Query & Scaling Strategy**:
  - *Read-Time Aggregation (MVP / Phase 1)*: Balance calculations are delegated to the database via native SQL aggregation (`SUM(Amount) ... GROUP BY Type` or `SUM(CASE ...)`), avoiding loading records into application memory.
  - *Write-Time Snapshots (Future Account Entity)*: When the `Account` entity is introduced, the account balance will be maintained at write-time (`account.Balance += transaction.SignedAmount`), providing O(1) instant balance queries without scanning historical records.

## Risks / Trade-offs

- **Fixed Category Set**: Users cannot create custom categories.
  - *Mitigation*: The `Other` category acts as a catch-all; a dynamic category management feature can be introduced in a later milestone.
- **Timezone Inconsistencies**: Transactions created with local times might cause query anomalies across time zones.
  - *Mitigation*: Ensure `Date` and `CreatedAt` use UTC timestamps.
- **Read-Time Balance Recalculation**: Summing transaction histories on every dashboard load scales with the number of transactions per user.
  - *Mitigation*: In the initial phase, database index scans over `(UserId, Date)` execute in single-digit milliseconds. As transaction volume scales, the future `Account` entity will maintain pre-calculated snapshot balances updated at write-time.
