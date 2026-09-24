# transactions Specification

## Purpose
Provides the core domain specifications, business rules, and categorization taxonomy for personal financial transactions.

## Requirements

### Requirement: Transaction entity invariants
The system SHALL represent financial transactions with a unique identifier, user identity, description, positive monetary amount, transaction date, transaction type, category, and audit creation timestamp, ensuring all domain validation rules are satisfied.

#### Scenario: Valid transaction instantiation
- **WHEN** a transaction is instantiated with valid details (positive amount, non-empty description, non-empty userId, valid date, type, and category)
- **THEN** the transaction is successfully initialized with the provided values and a generated or specified identifier

#### Scenario: Reject non-positive transaction amount
- **WHEN** a transaction is instantiated with an amount less than or equal to zero
- **THEN** the system rejects the transaction with an ArgumentException or domain validation exception

#### Scenario: Reject empty transaction description
- **WHEN** a transaction is instantiated with a null, empty, or whitespace-only description
- **THEN** the system rejects the transaction with an ArgumentException or domain validation exception

#### Scenario: Reject empty user identifier
- **WHEN** a transaction is instantiated with an empty GUID for user identifier
- **THEN** the system rejects the transaction with an ArgumentException or domain validation exception

### Requirement: Transaction classification
The system SHALL classify transactions into distinct directional types and standardized expense/income categories.

#### Scenario: Transaction type classification
- **WHEN** a transaction directional flow is defined
- **THEN** the transaction type must be either Income or Expense

#### Scenario: Transaction category classification
- **WHEN** a transaction category is assigned
- **THEN** the category must belong to the predefined categories: Food, Housing, Transportation, Salary, Health, Leisure, Education, or Other

### Requirement: Relational transaction persistence
The system SHALL persist transaction records in a relational PostgreSQL database adhering to schema constraints, column precision, indexing, and audit timestamps.

#### Scenario: Relational schema constraints
- **WHEN** the database schema is generated and migrated
- **THEN** a Transactions table exists with primary key Id, required UserId, max length Description (250), decimal precision Amount (18,2), DateTime Date, enum Type, enum Category, and DateTime CreatedAt, with a composite index on (UserId, Date)

#### Scenario: Transaction persistence roundtrip
- **WHEN** a valid transaction entity is saved and reloaded via the database context
- **THEN** all persisted property values match the original entity state exactly

### Requirement: Transaction application service orchestration
The system SHALL provide an application service to orchestrate transaction operations, enforce user isolation, map between domain entities and data transfer objects, and return appropriate response contracts.

#### Scenario: Create transaction via application service
- **WHEN** a valid creation contract with a valid authenticated user identifier is processed
- **THEN** the transaction is persisted via the repository and a transaction response containing the generated identifier, user identifier, description, amount, date, type, category, signed amount, and creation timestamp is returned

#### Scenario: User-isolated retrieval of all transactions
- **WHEN** transactions are queried for a specific authenticated user identifier
- **THEN** only transactions belonging to that specific user identifier are returned, optionally filtered by date range

#### Scenario: User-isolated retrieval by transaction identifier
- **WHEN** a transaction is queried by its identifier and the requesting authenticated user identifier
- **THEN** the matching transaction response is returned if it exists and belongs to the user, or a not-found result is returned if it does not exist or belongs to another user

#### Scenario: User-isolated transaction update
- **WHEN** an update contract is submitted for an existing transaction belonging to the authenticated user identifier
- **THEN** the transaction properties are updated and persisted, returning the updated transaction response, or a not-found result if the transaction does not exist or belongs to another user

#### Scenario: User-isolated transaction deletion
- **WHEN** a deletion is requested for a transaction identifier and the authenticated user identifier
- **THEN** the transaction is removed from persistence if it exists and belongs to the user, or a not-found result is returned if it does not exist or belongs to another user

### Requirement: Relational transaction repository operations
The system SHALL provide a concrete relational repository implementation backed by Entity Framework Core that persists and queries transactions with mandatory user isolation.

#### Scenario: Strict user filtering on queries
- **WHEN** transactions are queried by identifier or list via the repository
- **THEN** only transactions matching the provided authenticated user identifier are returned, guaranteeing multi-tenant data isolation

#### Scenario: Transaction persistence via repository commands
- **WHEN** AddAsync, UpdateAsync, or DeleteAsync is executed on the repository
- **THEN** the corresponding database changes are committed to PostgreSQL via SaveChangesAsync

#### Scenario: Soft deletion of transactions
- **WHEN** DeleteAsync is executed on the repository for a transaction
- **THEN** the transaction is marked as deleted with a deletion timestamp and excluded from standard queries via global query filters

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

