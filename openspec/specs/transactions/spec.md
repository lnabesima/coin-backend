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
