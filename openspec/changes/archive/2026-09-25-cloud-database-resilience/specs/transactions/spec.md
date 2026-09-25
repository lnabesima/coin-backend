# Spec Delta

## MODIFIED Requirements

### Requirement: Relational transaction persistence
The system SHALL persist transaction records in a relational PostgreSQL database adhering to schema constraints, column precision, indexing, audit timestamps, and connection resilience against transient network disruptions or database cold starts.

#### Scenario: Relational schema constraints
- **WHEN** the database schema is generated and migrated
- **THEN** a Transactions table exists with primary key Id, required UserId, max length Description (250), decimal precision Amount (18,2), DateTime Date, enum Type, enum Category, and DateTime CreatedAt, with a composite index on (UserId, Date)

#### Scenario: Transaction persistence roundtrip
- **WHEN** a valid transaction entity is saved and reloaded via the database context
- **THEN** all persisted property values match the original entity state exactly

#### Scenario: Resilient retry on transient database connectivity failures
- **WHEN** a database operation encounters a transient connection error or scale-to-zero wake-up delay
- **THEN** the system automatically retries the operation according to configured retry counts and exponential delays before surfacing a failure
