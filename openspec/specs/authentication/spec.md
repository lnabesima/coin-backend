# authentication Specification

## Purpose

Provides incoming HTTP request authentication via an API Key header, rejecting unauthorized requests with standard RFC 7807 responses and establishing tenant identity for downstream application services.

## Requirements

### Requirement: API Key Header Validation
The system SHALL validate the `X-Api-Key` HTTP header on incoming requests against the configured secret key using constant-time comparison.

#### Scenario: Request with valid API key
- **WHEN** an HTTP request is received with an `X-Api-Key` header matching the configured key
- **THEN** the system allows the request to proceed down the middleware pipeline

#### Scenario: Request missing API key header
- **WHEN** an HTTP request is received without an `X-Api-Key` header or with an empty value
- **THEN** the system short-circuits the pipeline and returns HTTP 401 Unauthorized in RFC 7807 ProblemDetails format

#### Scenario: Request with invalid API key
- **WHEN** an HTTP request is received with an `X-Api-Key` header that does not match the configured key
- **THEN** the system short-circuits the pipeline and returns HTTP 401 Unauthorized in RFC 7807 ProblemDetails format

### Requirement: Tenant Identity Resolution
The system SHALL attach the authenticated tenant identity as a `ClaimsPrincipal` to the request context upon successful API key validation, setting `ClaimTypes.NameIdentifier` with the resolved tenant `UserId`.

#### Scenario: UserId attached to request items
- **WHEN** an HTTP request is authenticated successfully with a valid API key
- **THEN** the user identity is populated with an authenticated `ClaimsPrincipal` containing the tenant `UserId` in `ClaimTypes.NameIdentifier`

### Requirement: Documentation Route Bypass
The system SHALL allow unauthenticated access to designated API documentation and exploration endpoints in development.

#### Scenario: Accessing OpenAPI specification or Scalar UI
- **WHEN** an HTTP request is received for paths starting with `/openapi` or `/scalar`
- **THEN** the system bypasses API key authentication and forwards the request to downstream handlers without requiring `X-Api-Key`

### Requirement: Unauthenticated Preflight Request Bypass
The system SHALL allow browser CORS preflight `OPTIONS` requests to proceed through the HTTP security pipeline without requiring authentication credentials.

#### Scenario: Preflight OPTIONS request
- **WHEN** an HTTP `OPTIONS` request is received
- **THEN** the authentication handler does not challenge or reject the request for missing API keys, allowing CORS middleware to complete the handshake

### Requirement: Endpoint Authorization Enforcement
The system SHALL enforce authentication on protected endpoints and controllers via standard authorization attributes (`[Authorize]`), returning standard RFC 7807 responses when unauthenticated.

#### Scenario: Accessing protected endpoint without credentials
- **WHEN** an unauthenticated request attempts to access an endpoint decorated with `[Authorize]`
- **THEN** the system challenges the request and returns HTTP 401 Unauthorized in RFC 7807 ProblemDetails format
