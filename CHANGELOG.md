# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Clean Architecture solution structure (.NET 10) with Domain, Application, Infrastructure, and API layers.
- Native OpenAPI generation via `Microsoft.AspNetCore.OpenApi` (10.0.12).
- Interactive API documentation using `Scalar.AspNetCore`.
- Docker Compose configuration with local PostgreSQL 16 container and healthcheck.
- `Transaction` domain entity, `TransactionType`, and `TransactionCategory` enums with unit tests in `Coin.Domain`.
- Entity Framework Core setup with PostgreSQL provider (`Npgsql`), `CoinDbContext`, `Transaction` entity configuration, and initial database migration (`InitialCreate`).
- Application layer contracts (`CreateTransactionDto`, `UpdateTransactionDto`, `TransactionResponseDto`), `ITransactionRepository`, `ITransactionService`, `TransactionService` implementation with user isolation, and unit tests in `Coin.Application.UnitTests`.
- Concrete `TransactionRepository` implementation with EF Core in `Coin.Infrastructure` with strict `UserId` tenant isolation and comprehensive unit tests in `Coin.Infrastructure.UnitTests`.
- Soft-delete support for transactions (`IsDeleted`, `DeletedAt`, EF Core Global Query Filter, and `AddSoftDeleteToTransactions` database migration).
- API Key authentication middleware (`ApiKeyMiddleware`) validating `X-Api-Key` headers via constant-time comparison, attaching `UserId` to `HttpContext.Items`, returning RFC 7807 `ProblemDetails` on 401 Unauthorized, and bypassing exploratory documentation routes in development.
- `Coin.API.UnitTests` test project with unit tests covering `ApiKeyMiddleware` validation, route bypass, and `HttpContextExtensions`.
- RESTful `TransactionsController` exposing 5 CRUD endpoints under `/api/v1/transactions` (`POST`, `GET`, `GET {id}`, `PUT {id}`, `DELETE {id}`) with strict tenant isolation, date range filtering, and native RFC 7807 `ProblemDetails` error responses.
- OpenAPI security scheme configuration for `X-Api-Key` enabling interactive authorized testing in Scalar UI (`/scalar/v1`).
- Controller unit test suite in `tests/Coin.API.UnitTests/Controllers/TransactionsControllerTests.cs` bringing total test suite to 67 unit tests.

