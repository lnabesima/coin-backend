# Coin Backend

A personal finance management and expense tracking backend built with .NET 10, ASP.NET Core, and PostgreSQL following Clean Architecture principles.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/) (Docker Engine or Docker Desktop)
- [Task](https://taskfile.dev/) (Task runner)

### Installing Task

Install Task using your preferred package manager:

- **Windows (Scoop):**
  ```powershell
  scoop install task
  ```

- **Windows (WinGet):**
  ```powershell
  winget install Task.Task
  ```

- **Linux / macOS:**
  ```bash
  sh -c "$(curl --location https://taskfile.dev/install.sh) -- -d -b ~/.local/bin"
  ```

## Quick Start

### 1. Build the solution

```bash
task build
```

### 2. Run the application

```bash
task run
```

Interactive API documentation (Scalar UI) will be available at:
`http://localhost:5000/scalar/v1`

### 3. Local Database (Optional for exploratory manual testing)

A local PostgreSQL database is provided via Docker Compose:

```bash
docker compose up -d
```

Connection details default to:
`Host=localhost;Port=5432;Database=coin_db;Username=postgres;Password=postgres`

## Running Tests

The test suite is organized into fast unit tests and end-to-end integration tests using Testcontainers.

### Available Task Commands

To see all available project commands, run:

```bash
task
```

| Command | Description |
|---|---|
| `task db:migrate` | Applies pending EF Core migrations locally or to a cloud DB via `CONNECTION_STRING="..."` |
| `task db:status` | Lists applied and pending EF Core database migrations |
| `task test:unit` | Runs all unit tests (Domain, Application, Infrastructure, API) in seconds without requiring Docker |
| `task test:e2e` | Runs end-to-end integration tests against an isolated containerized PostgreSQL database using Testcontainers |
| `task test` | Runs the full test suite (unit tests followed by E2E tests) |
| `task build` | Compiles all projects in the solution |
| `task run` | Runs the ASP.NET Core API project |

## Database Configuration & Migrations

### Local Database
For local development, start the PostgreSQL container:

```bash
docker compose up -d
```

Connection details default to:
`Host=localhost;Port=5432;Database=coin_db;Username=postgres;Password=postgres`

### Cloud Database (Managed PostgreSQL)
The backend is cloud-agnostic and compatible with any standard PostgreSQL 16+ provider (such as Neon, Supabase, Azure Database for PostgreSQL, AWS RDS, or self-hosted instances) over TLS/SSL.

#### Connection String Configurations
When deploying to cloud environments that utilize external connection poolers (e.g., PgBouncer, Supavisor, or AWS RDS Proxy):

1. **Pooled Connection String:**
   - Injected into the container runtime (e.g. Azure Container Apps Secrets or environment variables) as `ConnectionStrings__DefaultConnection`.
   - Uses transaction pooling to support high concurrency with minimal memory overhead:
     ```
     Host=<pooler-host>;Port=5432;Database=<database>;Username=<user>;Password=<password>;SslMode=Require;Trust Server Certificate=true;
     ```
2. **Direct Connection String:**
   - Provided to deployment automation (CI/CD pipelines) or developer CLI tools via secrets.
   - Dedicated connection to the PostgreSQL engine required for executing EF Core DDL migrations and acquiring migration locks:
     ```
     Host=<direct-host>;Port=5432;Database=<database>;Username=<user>;Password=<password>;SslMode=Require;Trust Server Certificate=true;
     ```
*(Note: If your provider does not use a separate transaction pooler, both runtime and migrations can use the same direct connection string).*

#### Connection Resilience
`Coin.Infrastructure` configures Npgsql's native retrying execution strategy (`EnableRetryOnFailure`) with up to 3 automatic retries and 5-second exponential backoff. This absorbs transient cloud network latency, failovers, and serverless scale-to-zero wake-up delays without failing HTTP requests.

#### Running Migrations
To apply migrations against your target database:

- **Local database:**
  ```bash
  task db:migrate
  ```

- **Cloud database (any PostgreSQL provider):**
  ```bash
  task db:migrate CONNECTION_STRING="Host=<host>;Port=5432;Database=<database>;Username=<user>;Password=<password>;SslMode=Require;Trust Server Certificate=true;"
  ```

- **Check migration status:**
  ```bash
  task db:status
  ```

### Running Tests with the .NET CLI Directly

If you prefer not using the task runner:

- **Unit tests only:**
  ```bash
  dotnet test --filter "FullyQualifiedName!~IntegrationTests"
  ```

- **Full test suite:**
  ```bash
  dotnet test
  ```

*(Note: On Windows machines where Docker runs inside WSL2 without Docker Desktop, run integration tests inside WSL or use `task test:e2e` which routes the command to the WSL Docker daemon).*

## Project Architecture

- **`src/Coin.Domain`**: Core business entities, value objects, domain invariants, and repository interfaces.
- **`src/Coin.Application`**: Application services, DTOs, interfaces, and orchestration logic.
- **`src/Coin.Infrastructure`**: EF Core persistence, PostgreSQL mapping, migrations, and repository implementations.
- **`src/Coin.API`**: RESTful HTTP controllers, authentication middleware, OpenAPI documentation, and DI wiring.
- **`tests/`**: Unit test suites per layer and `Coin.IntegrationTests` for end-to-end verification.
