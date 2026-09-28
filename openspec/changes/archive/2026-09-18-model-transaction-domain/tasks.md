# Tasks

## 1. Domain Enums

- [x] 1.1 Create `TransactionType` enum (`Income`, `Expense`) in `Coin.Domain/Enums/TransactionType.cs` and verify compilation with `dotnet build`
- [x] 1.2 Create `TransactionCategory` enum (`Food`, `Housing`, `Transportation`, `Salary`, `Health`, `Leisure`, `Education`, `Other`) in `Coin.Domain/Enums/TransactionCategory.cs` and verify compilation with `dotnet build`

## 2. Transaction Entity & Invariants

- [x] 2.1 Implement `Transaction` entity in `Coin.Domain/Entities/Transaction.cs` with properties (`Id`, `UserId`, `Description`, `Amount`, `Date`, `Type`, `Category`, `CreatedAt`) and guard clauses enforcing domain invariants (amount > 0, non-empty description, non-empty user id)
- [x] 2.2 Verify solution builds cleanly with `dotnet build`

## 3. Unit Testing & Verification

- [x] 3.1 Implement unit tests in `tests/Coin.Domain.UnitTests/TransactionTests.cs` verifying valid entity creation and edge cases/guard clauses (negative/zero amount, null/empty/whitespace description, empty user GUID)
- [x] 3.2 Execute test suite with `dotnet test` and confirm all tests pass
