# Proposal

## Why

External clients currently have no HTTP interface to interact with the transaction management features implemented in the application and infrastructure layers. Introducing a RESTful `TransactionsController` provides standardized, versioned HTTP endpoints for creating, retrieving, updating, and removing transactions with native RFC 7807 error handling and interactive Scalar API documentation.

## What Changes

- Add `TransactionsController` in `src/Coin.API/Controllers/TransactionsController.cs` with versioned route `api/v1/transactions`:
  - `POST /api/v1/transactions`: Creates a transaction for the authenticated user, returning `201 Created` with a `Location` header and the created transaction body.
  - `GET /api/v1/transactions`: Returns `200 OK` with an array of transactions for the authenticated user, supporting optional `startDate` and `endDate` query parameters.
  - `GET /api/v1/transactions/{id}`: Returns `200 OK` with the transaction if found and owned by the user; returns `404 Not Found` in RFC 7807 format if missing or owned by another user.
  - `PUT /api/v1/transactions/{id}`: Updates the specified transaction, returning `204 NoContent` on success, `404 Not Found` if missing, or `400 Bad Request` if payload validation fails.
  - `DELETE /api/v1/transactions/{id}`: Soft-deletes the transaction, returning `204 NoContent` on success or `404 Not Found` if missing.
- Register MVC controllers and native problem details support in `src/Coin.API/Program.cs` via `builder.Services.AddControllers()`, `builder.Services.AddProblemDetails()`, and `app.MapControllers()`.
- Configure OpenAPI with API Key security definition so all 5 endpoints can be tested interactively in Scalar UI at `/scalar/v1`.
- Add unit tests in `tests/Coin.API.UnitTests/Controllers/TransactionsControllerTests.cs` verifying each endpoint status code, response payload, and ProblemDetails generation.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `transactions`: Add RESTful HTTP API endpoints, status codes, and RFC 7807 error contracts for managing transactions.

## Impact

- **API Layer**: `src/Coin.API/Controllers/TransactionsController.cs`, `src/Coin.API/Program.cs`.
- **Testing**: Adds controller unit test suite in `tests/Coin.API.UnitTests/Controllers/TransactionsControllerTests.cs`.
- **API Surface**: Exposes 5 versioned HTTP endpoints under `/api/v1/transactions`.
