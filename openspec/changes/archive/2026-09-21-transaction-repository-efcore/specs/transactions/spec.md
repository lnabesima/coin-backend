# Spec Delta

## ADDED Requirements

### Requirement: Relational transaction repository operations
The system SHALL provide a concrete relational repository implementation backed by Entity Framework Core that persists and queries transactions with mandatory user isolation.

#### Scenario: Strict user filtering on queries
- **WHEN** transactions are queried by identifier or list via the repository
- **THEN** only transactions matching the provided authenticated user identifier are returned, guaranteeing multi-tenant data isolation

#### Scenario: Transaction persistence via repository commands
- **WHEN** AddAsync, UpdateAsync, or DeleteAsync is executed on the repository
- **THEN** the corresponding database changes are committed to PostgreSQL via SaveChangesAsync
