# Design

## Context
See [proposal.md](proposal.md) for motivation. `coin-backend` is an ASP.NET Core 10 Web API configured with PostgreSQL persistence, EF Core migrations, API Key authentication, and Scalar OpenAPI documentation. To serve production traffic with zero-idle cost, the API will be containerized and hosted in Microsoft Azure Container Apps (ACA) in `brazilsouth`.

## Goals / Non-Goals

**Goals:**
- Provide a minimal, secure, multi-stage `Dockerfile` adhering to .NET 10 container best practices (running as non-root on port 8080).
- Provide `.dockerignore` to keep image builds clean, fast, and cache-efficient.
- Structure Azure Container Apps infrastructure provisioning in `brazilsouth` with serverless consumption pricing (`minReplicas: 0`).
- Configure secrets in ACA directly for the Neon pooled connection string and API authentication key without incurring Azure Key Vault overhead.
- Provide a GitHub Actions deployment workflow (`.github/workflows/deploy.yml`) automating test execution, migration application, image push to Azure Container Registry (ACR), and ACA deployment.
- Expose `docker:build` and `docker:run` commands in `Taskfile.yml`.

**Non-Goals:**
- Azure Key Vault: ACA native secrets provide secure environment variable mapping without additional cloud resource cost.
- Continuous multi-region clustering: A single ACA environment in `brazilsouth` fulfills the latency and cost requirements.

## Decisions

### Decision 1: Multi-stage Dockerfile with non-root runtime
- **Choice**: Multi-stage build starting from `mcr.microsoft.com/dotnet/sdk:10.0` for compilation and publish, copying published output to `mcr.microsoft.com/dotnet/aspnet:10.0`.
- **Port**: Default ASP.NET Core port 8080 (`ASPNETCORE_HTTP_PORTS=8080`).
- **Rationale**: Keeps production container image minimal (< 220MB) and eliminates build dependencies from the runtime image.

### Decision 2: Azure Container Apps Consumption Tier with Scale-to-Zero
- **Choice**: `minReplicas = 0`, `maxReplicas = 2`, `cpu = 0.25`, `memory = 0.5Gi`.
- **Region**: `brazilsouth` (São Paulo) to maintain ~5-15ms network latency with the Neon PostgreSQL database (`aws-sa-east-1`).
- **Rationale**: Stays within Azure's free monthly consumption grants (180,000 vCPU-seconds and 2 million requests) when idle.

### Decision 3: Separation of Secrets between CI/CD and ACA
- **Choice**:
  - GitHub Actions Secrets: `AZURE_CREDENTIALS`, `REGISTRY_USERNAME`, `REGISTRY_PASSWORD`, `NEON_DIRECT_CONNECTION_STRING`.
  - ACA Secrets: `postgres-connection-string` (pointing to the Neon `-pooler` endpoint) and `api-key`.
- **Rationale**: Isolates migration privileges (direct endpoint) inside GitHub Actions runners during deployment while granting the runtime API access only via the connection pooler.

### Decision 4: GitHub Container Registry (GHCR) over Azure Container Registry (ACR)
- **Choice**: Push production images to `ghcr.io/lnabesima/coin-api`.
- **Rationale**: Azure Container Registry (Basic SKU) costs ~US$ 5/month with no permanent free tier. GHCR (`ghcr.io`) is free for public repositories and included in GitHub's free tier for personal projects. Azure Container Apps natively supports pulling from `ghcr.io` with authentication.
- **Alternatives Considered**: ACR Basic (rejected due to US$ 5/month recurring charge), Docker Hub (rejected due to strict rate limits on free accounts).

## Risks / Trade-offs

- **[Risk]** Cold start delay on initial request after idle period.
  - **Mitigation:** Documented and expected behavior for personal finance applications. The combined cold boot is approximately 3-5 seconds, after which instances remain warm while active.
- **[Risk]** Large Docker build context.
  - **Mitigation:** Comprehensive `.dockerignore` excluding `.git`, `.agents`, `bin/`, `obj/`, `tests/`, and documentation.
