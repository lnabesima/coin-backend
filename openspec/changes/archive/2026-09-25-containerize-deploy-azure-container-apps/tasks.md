# Tasks

## 1. Containerization

- [x] 1.1 Create `.dockerignore` excluding local build artifacts, test projects, version control, and temporary directories
- [x] 1.2 Create multi-stage production `Dockerfile` for .NET 10 with non-root runtime listening on port 8080
- [x] 1.3 Add `docker:build` and `docker:run` commands to `Taskfile.yml`
- [x] 1.4 Verify `docker build -t coin-api .` succeeds locally without warnings or errors

## 2. CI/CD Pipeline & Deployment Automation

- [x] 2.1 Create `.github/workflows/deploy.yml` defining the complete deployment pipeline: test execution, database migration via `task db:migrate`, container build & push to GitHub Container Registry (`ghcr.io`), and Azure Container Apps deployment
- [x] 2.2 Update `README.md` with step-by-step Azure CLI provisioning commands, ACA secret mappings, and GitHub Secrets configuration

## 3. Verification & Documentation

- [x] 3.1 Verify full test suite (`task test`) passes cleanly
- [x] 3.2 Update `CHANGELOG.md` under `[Unreleased]` with containerization, taskfile, and CI/CD workflow additions
