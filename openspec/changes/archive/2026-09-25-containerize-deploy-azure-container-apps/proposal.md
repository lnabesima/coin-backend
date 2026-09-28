# Proposal

## Why
To run `Coin.API` in production with zero-idle compute cost, high availability, and HTTPS ingress, the application needs a multi-stage production Dockerfile and automated deployment to Microsoft Azure Container Apps (ACA) in `brazilsouth`. This provides seamless execution paired with the Neon cloud PostgreSQL database in `aws-sa-east-1`.

## What Changes
- Create an optimized, multi-stage production `Dockerfile` for .NET 10 (`mcr.microsoft.com/dotnet/aspnet:10.0` runtime base and `mcr.microsoft.com/dotnet/sdk:10.0` build base) exposing port 8080.
- Create a `.dockerignore` file excluding non-essential files, source control, artifacts, and local binaries.
- Define infrastructure provisioning scripts and steps for Azure Container Apps (Resource Group, Azure Container Registry, Log Analytics Workspace, Container Apps Environment, Container App with `minReplicas = 0`).
- Configure secure secret management in ACA for database connection string (`ConnectionStrings__DefaultConnection` using the pooled endpoint) and authentication (`Authentication__ApiKey`).
- Add GitHub Actions CI/CD deployment workflow (`.github/workflows/deploy.yml`) to automatically build, push image to ACR, execute database migrations via `task db:migrate`, and deploy to Azure Container Apps.
- Update `Taskfile.yml` and `README.md` with docker build/run tasks and deployment documentation.

## Capabilities

### New Capabilities

### Modified Capabilities

## Impact
- Affected code: `Dockerfile`, `.dockerignore`, `.github/workflows/deploy.yml`, `Taskfile.yml`, `README.md`.
- No breaking changes to existing domain entities, application contracts, or test suites.
