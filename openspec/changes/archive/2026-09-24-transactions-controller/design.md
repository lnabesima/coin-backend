# Design

## Context

See `proposal.md` for motivation.

The domain, application, and infrastructure layers are complete with:
- Domain model (`Transaction`) with validation invariants, soft-delete, and categorization.
- Concrete repository (`TransactionRepository`) backed by EF Core with strict tenant filtering.
- Application service (`TransactionService`) implementing `ITransactionService`.
- API Key authentication middleware (`ApiKeyMiddleware`) injecting `UserId` into `HttpContext.Items`.
- `HttpContextExtensions.GetUserId(HttpContext)` for safe tenant ID resolution.

## Goals / Non-Goals

**Goals:**
- Implement `TransactionsController` inheriting from `ControllerBase` annotated with `[ApiController]` and route `api/v1/transactions`.
- Support 5 standard HTTP operations:
  - `POST /api/v1/transactions` -> `201 Created` with `Location` header and payload.
  - `GET /api/v1/transactions` -> `200 OK` with transaction array and optional date range filtering (`startDate`, `endDate`).
  - `GET /api/v1/transactions/{id:guid}` -> `200 OK` or `404 Not Found` (ProblemDetails).
  - `PUT /api/v1/transactions/{id:guid}` -> `204 NoContent`, `404 Not Found`, or `400 Bad Request` (ProblemDetails).
  - `DELETE /api/v1/transactions/{id:guid}` -> `204 NoContent` or `404 Not Found` (ProblemDetails).
- Extract `UserId` securely from the authenticated context via `HttpContext.GetUserId()`.
- Register controllers and native RFC 7807 ProblemDetails in `Program.cs`.
- Configure OpenAPI with `X-Api-Key` security definition so that Scalar UI provides an authorization field for testing.

**Non-Goals:**
- Pagination and cursor navigation (deferred to future backlog issues).
- Bulk export/import endpoints.

## Decisions

### 1. ControllerBase vs Minimal APIs
- **Decision**: Implement `TransactionsController : ControllerBase` with `[ApiController]`.
- **Rationale**: Directly aligns with Issue #7 technical scope (`src/Coin.API/Controllers/TransactionsController.cs`), provides automatic model validation, and structures standard CRUD actions cleanly.

### 2. Versioned Route Prefix
- **Decision**: Route attribute `[Route("api/v1/transactions")]`.
- **Rationale**: Adheres to RESTful conventions, supports forward-compatibility for future breaking API versions, and satisfies acceptance criteria.

### 3. Native RFC 7807 ProblemDetails
- **Decision**: Return ProblemDetails using `Problem(statusCode: ..., title: ..., detail: ..., type: ...)` for 400 and 404 responses. Register `builder.Services.AddProblemDetails()` in `Program.cs`.
- **Rationale**: Native ASP.NET Core support, complies with RFC 7807 standards without third-party libraries.

### 4. Domain Validation Error Translation
- **Decision**: In `PUT` (and `POST` if applicable), catch `ArgumentException` and `ArgumentOutOfRangeException` thrown by domain entity invariants and translate them into `400 Bad Request` ProblemDetails with specific error descriptions.
- **Rationale**: Prevents unhandled 500 errors when client sends invalid business input (e.g. negative amount or invalid category enum).

### 5. OpenAPI Security Scheme for Scalar UI
- **Decision**: Configure `builder.Services.AddOpenApi(options => ...)` to add an ApiKey security scheme (`apiKey` in `header` with name `X-Api-Key`).
- **Rationale**: Allows developers opening `/scalar/v1` to enter their API key in the UI and test all 5 endpoints interactively.

## Risks / Trade-offs

- **[Scalar UI Authorization]** → Without configuring the security scheme in OpenAPI, Scalar UI requests will omit `X-Api-Key` and fail with 401. Adding the security scheme enables interactive testing directly from the browser.
