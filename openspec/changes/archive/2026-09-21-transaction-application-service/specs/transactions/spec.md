# Spec Delta

## ADDED Requirements

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
