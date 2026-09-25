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
| `task test:unit` | Runs all unit tests (Domain, Application, Infrastructure, API) in seconds without requiring Docker |
| `task test:e2e` | Runs end-to-end integration tests against an isolated containerized PostgreSQL database using Testcontainers |
| `task test` | Runs the full test suite (unit tests followed by E2E tests) |
| `task build` | Compiles all projects in the solution |
| `task run` | Runs the ASP.NET Core API project |

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
