# Development Guidelines

## Git & Workflow Rules
- **No Automatic Commits**: Never make a git commit or push automatically. Always present the implemented changes for user review and wait for explicit confirmation before committing.
- **Security & Secrets**: Never commit real production connection strings, passwords, or secrets. Local development credentials pointing to disposable local Docker containers (e.g. `postgres:postgres` in `appsettings.Development.json`) are permitted for developer portability across multiple machines.
- **Language**: All code, commit messages, PR descriptions, issue titles, user stories, and documentation must be written in English.
- **Style**: Clean text only (no emojis in cards or commit messages).
- **Pull Request Titles**: Write PR titles as clean imperative sentences in English (e.g. `Model transaction entity and enums` or `[Domain] Model transaction entity and enums`). Do not use Conventional Commits prefixes (such as `feat:` or `fix:`) in PR titles.

## Cloud & Infrastructure
- **Database**: Neon PostgreSQL. Note: `aws-sa-east-1` (São Paulo) is supported on the free tier and must be selected at project creation time.
- **Compute / Hosting**: Microsoft Azure Container Apps (ACA) in `brazilsouth` to maintain low latency with São Paulo Neon instance.

