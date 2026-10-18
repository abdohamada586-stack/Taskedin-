---
name: JWT authentication and authorization
description: Implement or review authentication, JWT token issuance, claims, roles and authorization in Taskedin — TokenService, AuthService, JwtClaimTypes and the JwtBearer setup.
---

Use this skill for anything touching login, registration, tokens, claims, roles, or
`[Authorize]` in Taskedin.

## Pieces and where they live

| Concern | File |
|---|---|
| Token issuance | `Taskedin.Service/Services/TokenService.cs` (reads the `Jwt` config section) |
| Registration / login | `Taskedin.Service/Services/AuthService.cs` (transactional user + default role) |
| Claim name constants | `Taskedin.Service/Security/JwtClaimTypes.cs` (`sub`, `name`, `email`, `role`, `jti`) |
| Bearer validation | `Taskedin/Program.cs` (`MapInboundClaims = false`, `NameClaimType`/`RoleClaimType` set to the constants) |
| Password hashing | `IPasswordHasher<User>` — Identity hasher only; the legacy Identity tables are never used |
| Endpoint protection | `[Authorize]` on `UsersController`, `[Authorize(Roles = "Admin")]` on activate/deactivate, `[AllowAnonymous]` on `AuthController` |

## Rules

- Use `JwtClaimTypes` everywhere — never hardcode `"sub"` or `"role"` strings, and never change
  `MapInboundClaims` without re-checking both issuance and validation sides.
- The signing key comes from `Jwt:SigningKey` via user-secrets or an environment variable. It is
  empty in `appsettings.json` on purpose: never write a real key into any committed file.
- `InvalidOperationException` from `AuthService` maps to 409 (registration conflicts) or 403
  (deactivated account) in `AuthController`; invalid credentials map to 401. Keep those mappings
  when changing auth flows.
- `UsersController.IsSelfOrAdmin` implements self-or-admin access from the `sub` claim — preserve
  it on any new user endpoint.
- Login must not reveal whether an email exists: return the same 401 for unknown email and wrong
  password, as `LoginAsync` does today.
