---
name: EF Core and database migrations
description: Work with EF Core entities, configurations and migrations in Taskedin — inspect schema changes, add migrations safely, and follow the approval rules for database changes.
---

Use this skill when touching entities, `TaskedinDbContext`, entity configurations, or migrations.
`AGENTS.md` holds the hard rules; this skill is the working procedure.

## Layout

- Context: `Taskedin.Persistence/Context/TaskedinDbContext.cs` — maps `Departments`, `Profiles`,
  `Users`, `Roles`, `UserRoles` only. Configurations are discovered automatically via
  `ApplyConfigurationsFromAssembly`.
- Configurations: `Taskedin.Persistence/Configurations/*Configuration.cs`
  (`IEntityTypeConfiguration<T>`). Never inline mapping in the context.
- Migrations: `Taskedin.Persistence/Migrations/` — generated artifacts; never hand-edit.
- Legacy ASP.NET Identity tables (`AspNetUsers`, `AspNetRoles`, …) already exist in
  `UserManagementDb`. Never map, modify, or delete them, and do not create tables whose names
  collide with them.

## Changing the schema

1. Inspect existing migrations first (`git log -- Taskedin.Persistence/Migrations` and the
   `Migrations/` files) so you know the current schema.
2. Make the entity + configuration change in code.
3. Generate the migration only after the user approves it:
   ```bash
   dotnet tool restore
   dotnet ef migrations add <Name> --project Taskedin.Persistence --startup-project Taskedin
   ```
4. Review the generated `.cs` and `TaskedinDbContextModelSnapshot.cs` diff for unintended changes
   (table renames, dropped columns, cascades).
5. Never run `dotnet ef database update`, `migrations remove/delete`, or any other
   database-changing command without explicit user approval.

## Query review

- Prefer projected queries (`.Select(...)`) over materializing whole entities.
- Use async EF APIs (`ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`) — never `.Result`/`.Wait()`.
- Check `Include` usage for accidental cartesian loading and N+1 patterns in loops.
- Save only once per unit of work unless a transaction is intentional (see
  `AuthService.RegisterAsync` for the transaction pattern used in this codebase).
