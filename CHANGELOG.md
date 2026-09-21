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
