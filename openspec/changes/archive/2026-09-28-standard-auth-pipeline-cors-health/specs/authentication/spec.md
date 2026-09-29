# Spec Delta

## MODIFIED Requirements

### Requirement: Tenant Identity Resolution
The system SHALL attach the authenticated tenant identity as a `ClaimsPrincipal` to the request context upon successful API key validation, setting `ClaimTypes.NameIdentifier` with the resolved tenant `UserId`.

#### Scenario: UserId attached to request items
- **WHEN** an HTTP request is authenticated successfully with a valid API key
- **THEN** the user identity is populated with an authenticated `ClaimsPrincipal` containing the tenant `UserId` in `ClaimTypes.NameIdentifier`

## ADDED Requirements

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
