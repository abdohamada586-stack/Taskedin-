# AGENTS.md — Taskedin

Instructions for AI coding agents working in this repository. This file is the single source of
truth for project rules; other tool-specific files only point back to it.

## Project overview

.NET 8 ASP.NET Core Web API (SQL Server + EF Core 8). Nullable reference types and implicit
usings are enabled in every project.

| Project | Purpose | Project references |
|---|---|---|
| `Taskedin` | Web API host: `Program.cs`, controllers, `appsettings.json`, Swagger | Domain, Infrastructure, Service |
| `Taskedin.Domain` | Entities and shared types (e.g. `Common/ApiResponse<T>`) | — |
| `Taskedin.Persistence` | `TaskedinDbContext`, EF configurations, migrations | Domain |
| `Taskedin.Service` | Application services, interfaces, hosted/background services | Domain, Persistence, Infrastructure |
| `Taskedin.Infrastructure` | Cross-cutting items: middleware, action filters, request models | Domain |

Dependency direction: `Taskedin → Taskedin.Service → { Taskedin.Persistence | Taskedin.Infrastructure } → Taskedin.Domain`.
`Taskedin.Service` and `Taskedin.Infrastructure` also carry an `Microsoft.AspNetCore.App` framework reference.

Never add a reference that points the wrong way (e.g. Domain referencing Persistence) or creates a
cycle. Verify the current state with `dotnet sln Taskedin.sln list` plus the `ProjectReference`
lines in each `.csproj` (`dotnet list <project>.csproj reference`; the solution form
`dotnet list <solution> reference` is not supported by this SDK).

## Code organization (follow existing patterns)

- **Service interfaces** → `Taskedin.Service/Interfaces/IXService.cs` (XML `<summary>` on each member).
- **Service implementations** → `Taskedin.Service/Services/XService.cs`
  - Constructor-inject `TaskedinDbContext`; async CRUD methods suffixed `Async`; use EF Core directly (no repository layer).
- **DTOs / request models** → two locations exist today: auth/user DTOs in `Taskedin.Service/DTOs/`
  and shared request models in `Taskedin.Infrastructure/Models/` (see `GetProfilesRequest.cs`).
  Place new DTOs beside the DTOs of the same feature. Do not create, rename, or move DTO folders
  without explicit approval.
- **Entity configurations** → `Taskedin.Persistence/Configurations/` as `IEntityTypeConfiguration<T>`.
  They are auto-discovered via `ApplyConfigurationsFromAssembly` in `TaskedinDbContext`.
- **Dependency injection** → register services in the existing `DependencyInjection.cs` files:
  - `Taskedin.Service/DependencyInjection.cs` → `AddServiceDependencies()`
  - `Taskedin.Persistence/DependencyInjection.cs` → `AddPersistence(IConfiguration)`
  - The host wires both in `Program.cs`.
- **Controllers** → `[ApiController]`, `[Route("api/[controller]")]`, constructor-injected service
  interface. Wrap responses in `ApiResponse<T>` (`Success`, `Message`, `Data`) as done in
  `DepartmentsController`.
- **Background services** → follow the `Taskedin.Service/Services/SampleBackgroundService.cs` pattern
  (`PeriodicTimer`, structured `ILogger`, per-tick try/catch, graceful cancellation) and register with
  `AddHostedService<T>()` in `Taskedin.Service/DependencyInjection.cs`.

## C# and .NET 8 conventions

- Target framework is `net8.0` everywhere; do not change target frameworks or package versions
  without approval.
- Newer files use file-scoped namespaces (`namespace Taskedin.X;`); older files use block-scoped
  namespaces. Match the style of the file you are editing.
- Constructor injection; private fields in `_camelCase`; `var` where the type is obvious.
- Async methods are suffixed `Async` and use EF Core async APIs (`ToListAsync`, `AnyAsync`, …).
- Keep comments short and purposeful: a one-line `//` explaining intent, or a brief XML `<summary>`
  where the surrounding file already uses them. Do not narrate what the code obviously does.
- Services signal business-rule violations by throwing `InvalidOperationException` with a
  user-safe message; controllers translate that into an HTTP status code.

## Database and Entity Framework Core conventions

- `TaskedinDbContext` maps only `Departments`, `Profiles`, `Users`, `Roles`, `UserRoles`.
  Never map, modify, or delete the legacy ASP.NET Identity tables (`AspNetUsers`, `AspNetRoles`, …)
  that already exist in the `UserManagementDb` database; new tables must not collide with them.
- Entity mappings belong in `Taskedin.Persistence/Configurations/`, never inline in the context.
- Files in `Taskedin.Persistence/Migrations/` are generated artifacts — never hand-edit them.
- Migrations are generated only after approval: `dotnet tool restore` first (the `dotnet-ef` local
  tool is declared in `.config/dotnet-tools.json`), then
  `dotnet ef migrations add <Name> --project Taskedin.Persistence --startup-project Taskedin`.
- `dotnet ef database update` and any other database-changing command require explicit user approval.

## API, validation, authentication, authorization

- Every endpoint returns the `ApiResponse<T>` envelope with a consistent `Message`.
- Request shape validation runs in the global `ValidationFilter`
  (`Taskedin.Infrastructure/Filters/ValidationFilter.cs`), which executes FluentValidation
  validators registered by `AddValidatorsFromAssemblyContaining` and returns 400 with a list of
  errors. Database-backed rules stay in the service layer.
- Authentication is JWT Bearer (`Program.cs`). Tokens are issued by
  `Taskedin.Service/Services/TokenService.cs` using the short claim names in
  `Taskedin.Service/Security/JwtClaimTypes.cs`; inbound claim mapping is disabled
  (`MapInboundClaims = false`) and the same claim constants are used for validation.
- Passwords are hashed with `IPasswordHasher<User>` (ASP.NET Identity hasher only — the legacy
  Identity tables are not used).
- `[Authorize]` protects user endpoints, `[Authorize(Roles = "Admin")]` protects activation and
  deactivation, and `UsersController` allows self-or-admin access based on the `sub` claim.

## Error handling and logging

- `ExceptionHandlingMiddleware` (`Taskedin.Infrastructure/Middlewares/`) is registered first in the
  pipeline and converts unhandled exceptions into a 500 `ApiResponse<string>`.
- Use `ILogger<T>` with structured messages in services and middleware; avoid `Console.WriteLine`
  outside of `Program.cs` startup checks.

## Build and migration instructions

```bash
dotnet build Taskedin.sln          # verify after every approved change
dotnet tool restore                # restores dotnet-ef (see .config/dotnet-tools.json)
dotnet ef migrations add <Name> --project Taskedin.Persistence --startup-project Taskedin   # requires approval
```

- If the build fails with MSB3021/MSB3027 because Visual Studio or a running application has locked
  files under `bin`/`obj`, report that accurately. Do not terminate running processes and do not
  change output configuration to hide the error.

## Hard rules

- **Do not introduce new layers, repositories, or architectural patterns** unless explicitly approved.
- **Never use, modify, delete, or map the legacy ASP.NET Identity tables** (`AspNetUsers`,
  `AspNetRoles`, etc.) in `UserManagementDb`. Keep `TaskedinDbContext` limited to the entities listed
  above unless new entities are approved.
- **Never expose or commit secrets**: passwords, connection-string credentials, JWT signing keys,
  API keys. Keep secrets in user-secrets or environment variables, never in `appsettings.json`
  committed to git.
- **Database changes**: inspect existing code and migrations first
  (`Taskedin.Persistence/Migrations/`). Never run `dotnet ef database update` or any
  database-changing command without explicit user approval. Migrations are generated only after approval.
- **Do not commit or push** unless the user explicitly asks for it.
- **Do not change business logic, API behavior, authentication behavior, database configuration,
  connection strings, migrations, or DI registrations** just to accommodate AI tooling.

## Workflow

1. Before implementing a new task, inspect the relevant files and present a concise plan. Wait for approval before changing application code.
2. After approved changes, build the solution (`dotnet build Taskedin.sln`) and report exactly which files changed and whether the build succeeded.
3. Preserve unrelated existing work in progress (e.g. uncommitted `SampleBackgroundService` and its DI registration). Do not revert, delete, or "clean up" files you did not create.
4. Do not modify files outside the scope of the approved task.

## AI tooling files

| File / directory | Used by | Purpose |
|---|---|---|
| `AGENTS.md` | OpenCode (native), Claude Code, Copilot, other agents | Project rules — source of truth |
| `opencode.jsonc`, `.opencode/` (agents, skills, commands, plugins) | OpenCode only | OpenCode project configuration |
| `CLAUDE.md` | Claude Code only | Pointer to `AGENTS.md` |
| `.github/copilot-instructions.md` | GitHub Copilot only | Short pointer to `AGENTS.md` |
| `.editorconfig` | Editors/IDEs and coding agents | Formatting conventions only (no analyzer severities) |

Do not duplicate project rules in tool-specific files; add rules here only.
