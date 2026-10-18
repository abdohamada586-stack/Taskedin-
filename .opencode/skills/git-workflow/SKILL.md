---
name: Git workflow and pre-push checks
description: Run safe Git workflows in this repository — inspect changes, stage work, run pre-push checks, and follow the rules on commits, pushes and secrets.
---

Use this skill for any Git operation in Taskedin.

## Standing rules

- **Never push** unless the user explicitly asked in this session. `git push` is denied by
  project permissions.
- **Never commit** unless asked; when asked, commit only the files belonging to the task — the
  repository carries unrelated uncommitted user work (services, DTOs, migrations, `SampleBackgroundService`)
  that must stay uncommitted unless the user says otherwise.
- Never `git reset --hard`, `git clean`, force-checkout files, or discard changes you did not make.

## Inspecting

```bash
git status --short          # see everything, including untracked work
git diff                    # unstaged changes
git diff --stat             # compact overview
git log --oneline -10       # recent history
```

## Pre-push checklist (run before the user pushes)

1. `dotnet build Taskedin.sln` — must succeed; report MSB3021/MSB3027 lock failures honestly.
2. `git status --short` — confirm only intended files are staged; flag unrelated user work.
3. `git diff` review — verify no secrets: connection strings, `Jwt:SigningKey` values, passwords,
   API keys, or `appsettings.json` changes with credentials. Secrets belong in user-secrets or
   environment variables (`.config/dotnet-tools.json` and `appsettings.json` placeholders are fine).
4. Verify no generated-file hand edits: `Taskedin.Persistence/Migrations/*.cs` diffs must match
   what `dotnet ef migrations add` produces.
5. Verify AI config files only (`AGENTS.md`, `opencode.jsonc`, `.opencode/**`, `CLAUDE.md`,
   `.editorconfig`, `.github/copilot-instructions.md`) if the task was tooling-related.
6. Produce a short list of files to be committed and let the user run the push themselves.
