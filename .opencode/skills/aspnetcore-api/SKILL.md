---
name: ASP.NET Core API development
description: Create or modify controllers, services, DTOs and validation in the Taskedin API, following the repository's ApiResponse envelope, layering and dependency-injection conventions.
---

Use this skill when adding or changing an endpoint, a service, a DTO, or request validation in
Taskedin. Read `AGENTS.md` for the full rules; this skill gives the working procedure.

## Where code goes

| Kind of code | Project / folder |
|---|---|
| Controllers | `Taskedin/Controllers/` (route `api/[controller]`, `[ApiController]`) |
| Service interface / implementation | `Taskedin.Service/Interfaces/IXService.cs` / `Taskedin.Service/Services/XService.cs` |
| Auth/user DTOs | `Taskedin.Service/DTOs/` |
| Shared request models | `Taskedin.Infrastructure/Models/` (see `GetProfilesRequest.cs`) |
| Request validation rules | `Taskedin.Service/Validators/` (FluentValidation) |
| DI registration | `Taskedin.Service/DependencyInjection.cs` (`AddServiceDependencies()`) |

Dependency direction: `Taskedin → Service → {Persistence | Infrastructure} → Domain`. Do not add
references in any other direction.

## Endpoint checklist

1. Controller action returns the `ApiResponse<T>` envelope with a consistent `Message`
   (copy the style of `DepartmentsController`).
2. Input shape rules (required, length, format) go into a FluentValidation validator; the global
   `ValidationFilter` runs it automatically and returns 400. Do not hand-roll `ModelState` checks.
3. Rules that need the database (unique email, existing department) belong in the service, which
   throws `InvalidOperationException` with a user-safe message; the controller maps that to 409 or
   403 as the existing actions do.
4. Services take `TaskedinDbContext` by constructor injection, use async EF APIs, and are
   registered scoped in `AddServiceDependencies()`.
5. Finish with `dotnet build Taskedin.sln` and report the files you touched.

## Do not

- Introduce repositories, MediatR, or any new layer without approval.
- Move or rename existing DTO folders.
- Return raw objects or error strings that bypass `ApiResponse<T>`.
