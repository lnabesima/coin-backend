# Tasks

## 1. EF Core Dependencies & Setup

- [x] 1.1 Add `Npgsql.EntityFrameworkCore.PostgreSQL` and `Microsoft.EntityFrameworkCore.Design` packages to `Coin.Infrastructure.csproj` and `Coin.API.csproj`, verifying restoration with `dotnet restore`

## 2. DbContext & Entity Configuration

- [x] 2.1 Implement `CoinDbContext` in `src/Coin.Infrastructure/Persistence/CoinDbContext.cs` exposing `DbSet<Transaction> Transactions`
- [x] 2.2 Implement `TransactionConfiguration` in `src/Coin.Infrastructure/Persistence/Configurations/TransactionConfiguration.cs` configuring primary key, column types, decimal precision (18,2), integer enum mappings, and `(UserId, Date)` composite index
- [x] 2.3 Create `DependencyInjection.cs` in `src/Coin.Infrastructure/DependencyInjection.cs` registering `CoinDbContext` with Npgsql, and register `AddInfrastructure` in `src/Coin.API/Program.cs`
- [x] 2.4 Verify solution builds cleanly with `dotnet build`

## 3. Database Migration & Verification

- [x] 3.1 Generate initial EF Core migration `InitialCreate` using `dotnet ef migrations add InitialCreate`
- [x] 3.2 Apply migration to local PostgreSQL container via `dotnet ef database update` and verify `Transactions` table creation
- [x] 3.3 Run `dotnet test` to confirm all domain unit tests continue to pass
