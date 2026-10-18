---
description: Review database migration changes in Taskedin.Persistence before they are applied
---

Review the migration changes for: $ARGUMENTS

Use this scope: with no argument, review the most recent migration (plus its `.Designer.cs` and
the `TaskedinDbContextModelSnapshot.cs` diff); with an argument, review the named migration or
path.

!`git status --short -- Taskedin.Persistence/Migrations Taskedin.Persistence/Configurations Taskedin.Domain/Entities`

Verify and report:

1. **Source of truth** — the migration matches the entity classes and
   `Taskedin.Persistence/Configurations/*Configuration.cs` (columns, types, lengths, keys,
   indexes, delete behavior).
2. **Snapshot consistency** — `TaskedinDbContextModelSnapshot.cs` reflects exactly the same model
   as the migration; no unrelated model changes slipped in.
3. **Table-name safety** — no table collides with the legacy ASP.NET Identity tables
   (`AspNetUsers`, `AspNetRoles`, `AspNetUserClaims`, …) in `UserManagementDb`, and none of those
   tables is touched.
4. **Destructive changes** — dropped columns/tables, changed nullability, or cascades that could
   lose data are called out explicitly.
5. **Hand edits** — generated `.cs` files must not contain manual edits; they should be exactly
   what `dotnet ef migrations add` produces.

Never run `dotnet ef database update` or any command that applies or removes migrations — that
requires explicit user approval. This command reviews changes only; do not modify files.
