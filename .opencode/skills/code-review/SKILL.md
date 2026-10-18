---
name: Code review
description: Review Taskedin changes for bugs, security issues, architecture violations and missing validation, reporting findings by severity with file and line references.
---

Use this skill when asked to review code, a diff, or a pull-request-sized change. Report findings
only — do not edit files as part of a review.

## Procedure

1. Establish scope: `git status --short` and `git diff` for working-tree changes, or
   `git diff <base>...HEAD` for branch work. Include untracked files — this repository commonly
   holds uncommitted user work that must not be flagged as "stray".
2. Read `AGENTS.md` and review against its rules, not against personal taste.
3. Optionally verify compilation with `dotnet build Taskedin.sln`.

## What to check in this codebase

- **Correctness**: nullable dereferences (nullable is enabled), paging math in
  `DepartmentService.GetProfilesAsync`, race conditions around unique emails, transaction coverage
  in `AuthService.RegisterAsync`.
- **Layering**: dependency direction `Taskedin → Service → {Persistence | Infrastructure} → Domain`;
  no repositories or new layers; interfaces in `Interfaces/`, implementations in `Services/`.
- **API contract**: `ApiResponse<T>` on every response, FluentValidation for shape rules,
  `InvalidOperationException` → 401/403/409 mapping preserved.
- **Security**: no secrets or connection strings in files; `JwtClaimTypes` used for claims;
  `[Authorize]`/role checks present; legacy Identity tables untouched.
- **EF Core**: configurations in `Configurations/`, no hand-edited migrations, no N+1 queries.
- **Validation gaps**: endpoint accepts unvalidated input, or a DB rule that belongs in a service
  sits in a controller (or vice versa).

## Output format

Findings sorted by severity — **Critical → High → Medium → Low → Nit** — each with
`path/to/File.cs:line`, what is wrong, and a concrete suggestion. End with a one-line summary of
overall state. If a build fails because Visual Studio or a running app locked `bin`/`obj`
(MSB3021/MSB3027), state that rather than implying the code does not compile.
