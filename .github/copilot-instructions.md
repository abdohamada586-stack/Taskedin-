# GitHub Copilot instructions — Taskedin

Read `AGENTS.md` in the repository root and follow it in full. Key points:

- .NET 8 layered solution: `Taskedin → Taskedin.Service → { Taskedin.Persistence | Taskedin.Infrastructure } → Taskedin.Domain`. Never add backwards or cyclic references; no repositories or new layers.
- Controllers return the `ApiResponse<T>` envelope; request-shape rules go in FluentValidation validators (`Taskedin.Service/Validators/`), database-backed rules in services (thrown as `InvalidOperationException`).
- `TaskedinDbContext` maps only `Departments`, `Profiles`, `Users`, `Roles`, `UserRoles`. Never touch the legacy ASP.NET Identity tables.
- Never write secrets (connection strings, JWT keys, passwords) into files; `Jwt:SigningKey` stays empty in `appsettings.json`.
- Never hand-edit files in `Taskedin.Persistence/Migrations/`; never run database-changing commands.
- Build with `dotnet build Taskedin.sln` and keep changes scoped to the approved task.
