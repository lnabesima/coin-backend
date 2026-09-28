# Spec Delta

## ADDED Requirements

### Requirement: Relational transaction persistence
The system SHALL persist transaction records in a relational PostgreSQL database adhering to schema constraints, column precision, indexing, and audit timestamps.

#### Scenario: Relational schema constraints
- **WHEN** the database schema is generated and migrated
- **THEN** a Transactions table exists with primary key Id, required UserId, max length Description (250), decimal precision Amount (18,2), DateTime Date, enum Type, enum Category, and DateTime CreatedAt, with a composite index on (UserId, Date)

#### Scenario: Transaction persistence roundtrip
- **WHEN** a valid transaction entity is saved and reloaded via the database context
- **THEN** all persisted property values match the original entity state exactly
