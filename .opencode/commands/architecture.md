---
description: Check the Taskedin solution architecture and project dependency direction
---

Audit $ARGUMENTS (default: the whole solution) for architecture and dependency-direction
violations.

Check the references in every `.csproj` (`ProjectReference` items; verify a single project with
`dotnet list <project>.csproj reference` — note the solution form of `dotnet list ... reference`
is not supported by this SDK):

!`dotnet sln Taskedin.sln list`

Check that:
1. The references match `Taskedin → Taskedin.Service → { Taskedin.Persistence | Taskedin.Infrastructure } → Taskedin.Domain`,
   with no backwards, sideways, or cyclic references (`Taskedin.Domain` must reference nothing).
2. No new layers, repositories, or architectural patterns were introduced without approval.
3. Controllers stay thin, services use `TaskedinDbContext` directly, interfaces live in
   `Taskedin.Service/Interfaces/`, implementations in `Taskedin.Service/Services/`.
4. Entity mappings are all in `Taskedin.Persistence/Configurations/`; `TaskedinDbContext` maps only
   `Departments`, `Profiles`, `Users`, `Roles`, `UserRoles` and never the legacy ASP.NET Identity
   tables.
5. DI registrations remain in the two existing `DependencyInjection.cs` files.

Report each violation with the exact project/file, or state clearly that the architecture is
compliant. Do not modify any files.
