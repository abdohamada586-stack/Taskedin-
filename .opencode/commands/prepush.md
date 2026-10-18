---
description: Run pre-push checks and produce a commit/push readiness report
---

Run the pre-push checks for: $ARGUMENTS

!`git status --short`
!`git diff --stat`

Checklist — report PASS/FAIL per item:

1. **Scope** — only intended files are staged/modified; unrelated uncommitted user work
   (`SampleBackgroundService.cs` and its DI registration, services, DTOs, migrations, JWT setup)
   is listed separately and left alone.
2. **Build** — run `dotnet build Taskedin.sln` (or cite the most recent result). MSB3021/MSB3027
   file-lock errors from Visual Studio or a running app are reported as environmental, not as code
   success or failure.
3. **No secrets** — scan the diff for connection strings, passwords, JWT signing keys, and API
   keys. `Jwt:SigningKey` must stay empty in `appsettings.json`; real values belong in
   user-secrets or environment variables. `UserSecretsId` and `.config/dotnet-tools.json` are fine
   to commit.
4. **Migrations** — any `Taskedin.Persistence/Migrations/*.cs` changes were generated, not
   hand-written, and their content was reviewed with the migrations command.
5. **AI/tooling files only** — if the task was configuration-only, confirm no application code,
   DI registration, database configuration, or endpoint changed.
6. **Instructions followed** — `AGENTS.md` hard rules respected (no legacy Identity tables, no new
   layers, no behavior changes for tooling).

Finish with the exact list of files that would be committed. Do not commit and do not push — the
user performs those steps after reviewing the report.
