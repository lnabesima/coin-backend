# Tasks

## 1. DTOs and Interfaces

- [x] 1.1 Implement `CreateTransactionDto`, `UpdateTransactionDto`, and `TransactionResponseDto` in `src/Coin.Application/DTOs/Transactions/` and verify compilation
- [x] 1.2 Implement `ITransactionRepository` interface in `src/Coin.Application/Interfaces/ITransactionRepository.cs` with `AddAsync`, `GetByIdAsync`, `GetAllAsync`, `UpdateAsync`, and `DeleteAsync` requiring `userId`
- [x] 1.3 Implement `ITransactionService` interface in `src/Coin.Application/Interfaces/ITransactionService.cs`

## 2. Service Implementation & DI Registration

- [x] 2.1 Implement `TransactionService` in `src/Coin.Application/Services/TransactionService.cs` with domain invariant validation, user isolation, and DTO mappings
- [x] 2.2 Create `DependencyInjection.cs` in `src/Coin.Application/DependencyInjection.cs` registering `ITransactionService` and register `AddApplication` in `src/Coin.API/Program.cs`
- [x] 2.3 Verify solution builds cleanly with `dotnet build`

## 3. Unit Testing & Verification

- [x] 3.1 Setup `Coin.Application.UnitTests` test project in `tests/Coin.Application.UnitTests/` with `NSubstitute` and write comprehensive tests for `TransactionService`
- [x] 3.2 Run `dotnet test` confirming all application and domain unit tests pass with 0 failures
