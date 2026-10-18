---
name: Debugging
description: Debug failures in the Taskedin API — build errors, unexpected 400/401/403/409/500 responses, JWT problems and EF Core/database connectivity issues.
---

Use this skill when something is failing and the cause is unknown. Diagnose first; change code
only after the user approves a fix.

## Triage order

1. **Build errors** — `dotnet build Taskedin.sln`. Distinguish real compile errors from
   MSB3021/MSB3027 file-lock failures (Visual Studio or a running `Taskedin` process holds
   `bin`/`obj`). Lock failures are environmental: report them; do not kill processes or change
   output paths.
2. **500 responses** — `ExceptionHandlingMiddleware` logs the exception with `ILogger` and returns a
   generic `ApiResponse` 500. Find the real error in the log, not in the response body.
3. **400 responses** — produced by `ValidationFilter` with per-property FluentValidation messages;
   the message list names the failing property. Check the matching validator in
   `Taskedin.Service/Validators/`.
4. **401 vs 403 vs 409**:
   - 401 = missing/invalid token or bad credentials (`AuthController.Login`).
   - 403 = valid identity but not allowed: deactivated account, role missing, or
     `IsSelfOrAdmin` failed.
   - 409 = `InvalidOperationException` business conflict (duplicate email, unknown department).
5. **Auth-specific** — confirm `Jwt:SigningKey` is set (user-secrets/env; startup fails fast if it
   is missing), `MapInboundClaims = false`, and `NameClaimType`/`RoleClaimType` match
   `JwtClaimTypes`. A token that decodes but fails role checks usually means claim-name drift.
6. **Database/connectivity** — `Program.cs` prints the `CanConnectAsync` result at startup;
   `SampleBackgroundService` logs connection health every 5 minutes. Check `DefaultConnection`
   points at `UserManagementDb` and that the expected tables exist (legacy Identity tables are
   present by design and are not used by this app).

## Rules while debugging

- Never "fix" a failure by weakening validation, removing authorization, or changing
  connection strings — diagnose and propose.
- No database-changing commands (`dotnet ef database update`, etc.) without explicit approval.
- Report the exact evidence (log line, stack frame, status code) that supports the conclusion.
