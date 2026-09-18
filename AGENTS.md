# Development Guidelines

## Git & Workflow Rules
- **No Automatic Commits**: Never make a git commit or push automatically. Always present the implemented changes for user review and wait for explicit confirmation before committing.
- **Security & Secrets**: Never commit real production connection strings, passwords, or secrets. Local development credentials pointing to disposable local Docker containers (e.g. `postgres:postgres` in `appsettings.Development.json`) are permitted for developer portability across multiple machines.
- **Language**: All code, commit messages, PR descriptions, issue titles, user stories, and documentation must be written in English.
- **Style**: Clean text only (no emojis in cards or commit messages).
