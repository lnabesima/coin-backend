# Tasks

## 1. Controller Setup & Pipeline Configuration

- [x] 1.1 Register controllers, native ProblemDetails, and OpenAPI security scheme in `src/Coin.API/Program.cs` (`AddControllers()`, `AddProblemDetails()`, `MapControllers()`)
- [x] 1.2 Verify compilation and pipeline registration with `dotnet build`

## 2. TransactionsController Implementation

- [x] 2.1 Implement `TransactionsController` in `src/Coin.API/Controllers/TransactionsController.cs` with `POST /api/v1/transactions` returning `201 Created` with Location header and created resource
- [x] 2.2 Implement `GET /api/v1/transactions` with optional `startDate` and `endDate` query parameters returning `200 OK`
- [x] 2.3 Implement `GET /api/v1/transactions/{id}` returning `200 OK` when found or `404 Not Found` ProblemDetails
- [x] 2.4 Implement `PUT /api/v1/transactions/{id}` returning `204 NoContent` on success, `404 Not Found` when missing, or `400 Bad Request` ProblemDetails on domain validation failure
- [x] 2.5 Implement `DELETE /api/v1/transactions/{id}` returning `204 NoContent` on success or `404 Not Found` ProblemDetails when missing

## 3. Automated Testing

- [x] 3.1 Implement unit tests in `tests/Coin.API.UnitTests/Controllers/TransactionsControllerTests.cs` covering all 5 endpoints, status codes, user isolation, and ProblemDetails error responses
- [x] 3.2 Run `dotnet test` and verify that all solution test suites pass with 0 failures

## 4. Documentation & Verification

- [x] 4.1 Update `CHANGELOG.md` under `[Unreleased]` with TransactionsController and OpenAPI configuration
- [x] 4.2 Run `openspec validate` to verify specification and task consistency
