# Proposal

## Why
The personal finance manager requires a clean domain model to represent financial inflows and outflows with strict business validations and data integrity. Establishing the `Transaction` entity and associated enums in the domain layer provides the foundational building block for all future transaction management, categorization, and account/goal extensions without leaking persistence or framework concerns.

## What Changes
- Introduce `Transaction` entity in `Coin.Domain` with properties `Id`, `UserId`, `Description`, `Amount`, `Type`, `Category`, `Date`, and `CreatedAt`.
- Introduce `TransactionType` enum (`Income`, `Expense`).
- Introduce `TransactionCategory` enum (`Food`, `Housing`, `Transportation`, `Salary`, `Health`, `Leisure`, `Education`, `Other`).
- Enforce domain validation rules: `Amount` must be strictly greater than zero, `Description` must not be null or whitespace, and `UserId` must be non-empty.
- Add comprehensive unit tests in `Coin.Domain.UnitTests`.

## Capabilities

### New Capabilities
- `transactions`: Core domain models, invariants, and categorizations for personal financial transactions.

### Modified Capabilities

## Impact
- Affected code: `src/Coin.Domain/Entities/Transaction.cs`, `src/Coin.Domain/Enums/TransactionType.cs`, `src/Coin.Domain/Enums/TransactionCategory.cs`, and `tests/Coin.Domain.UnitTests/TransactionTests.cs`.
- No breaking changes.
- Zero external package dependencies introduced in `Coin.Domain`.
