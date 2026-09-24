# Spec Delta

## ADDED Requirements

### Requirement: RESTful transaction management endpoints
The system SHALL expose RESTful HTTP endpoints under `/api/v1/transactions` to create, retrieve, update, and soft-delete financial transactions with authenticated tenant isolation.

#### Scenario: Create transaction (POST /api/v1/transactions)
- **WHEN** a valid creation payload is posted by an authenticated user
- **THEN** the system returns HTTP 201 Created with a Location header pointing to the new resource and the created transaction body

#### Scenario: List transactions (GET /api/v1/transactions)
- **WHEN** a GET request is received for the authenticated user
- **THEN** the system returns HTTP 200 OK with the array of transactions, filtered by optional startDate and endDate query parameters

#### Scenario: Get transaction by ID (GET /api/v1/transactions/{id})
- **WHEN** a GET request is received for a transaction identifier
- **THEN** the system returns HTTP 200 OK if found and owned by the authenticated user, or HTTP 404 Not Found in RFC 7807 format if missing or owned by another user

#### Scenario: Update transaction (PUT /api/v1/transactions/{id})
- **WHEN** a PUT request is received with an update payload for an existing transaction owned by the user
- **THEN** the system returns HTTP 204 NoContent on success, HTTP 404 Not Found in RFC 7807 format if missing, or HTTP 400 Bad Request in RFC 7807 format if domain validation fails

#### Scenario: Delete transaction (DELETE /api/v1/transactions/{id})
- **WHEN** a DELETE request is received for a transaction identifier owned by the user
- **THEN** the system returns HTTP 204 NoContent on success or HTTP 404 Not Found in RFC 7807 format if missing
