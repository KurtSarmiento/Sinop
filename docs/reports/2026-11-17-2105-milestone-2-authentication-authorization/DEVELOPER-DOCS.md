# DEVELOPER-DOCS.md — Milestone 2: Authentication and Authorization

## Summary
Implemented secure authentication for MVC (cookie) and Android (JWT Bearer) per CLAUDE.md MILESTONE 2. Users can log in via MVC cookie flow and via JWT API endpoint. Role-based authorization policies are registered. Swagger documents API endpoints with JWT Bearer support.

## Milestone
- **MILESTONE 2** — Authentication and Authorization (executes after Milestone 1, per CLAUDE.md §17 strict order).

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Web | ASP.NET Core Web API + MVC | Auth controllers, services, views, DI/auth registration |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Web/Program.cs` | Added cookie auth (`AddCookie`), JWT Bearer auth (`AddJwtBearer`), authorization policies (`RequireAdministrator`, `RequireLibrarian`, `RequireBorrower`, `RequireLibrarianOrAdmin`), `AddControllersWithViews`, `AddSwaggerGen` with JWT security definition, `UseAuthentication`/`UseAuthorization`, `MapControllers`, MVC default route, `TokenService` scoped registration |
| `src/Sinop.Web/appsettings.json` | Added `Jwt` section: `Key`, `Issuer`, `Audience`, `ExpireMinutes` |
| `src/Sinop.Web/DTOs/Auth/LoginRequest.cs` | New: `Email`, `Password` request DTO |
| `src/Sinop.Web/DTOs/Auth/AuthResponse.cs` | New: `Token`, `Email`, `Roles[]`, `ExpiresAt` response DTO |
| `src/Sinop.Web/Services/TokenService.cs` | New: creates JWT with nameid, email, name, role claims; signs with HMAC-SHA256 |
| `src/Sinop.Web/Controllers/AccountController.cs` | New: MVC login (GET/POST with antiforgery), logout (POST), access denied |
| `src/Sinop.Web/Controllers/AuthController.cs` | New: API `POST api/auth/login` returning JWT token |
| `src/Sinop.Web/Controllers/HomeController.cs` | New: `[Authorize]` dashboard index |
| `src/Sinop.Web/Views/Shared/_Layout.cshtml` | New: shared layout with nav (Dashboard, Catalog, Borrowers, Circulation, Reports, Notifications, Settings), login/logout |
| `src/Sinop.Web/Views/_ViewStart.cshtml` | New: sets layout |
| `src/Sinop.Web/Views/_ViewImports.cshtml` | New: tag helpers import |
| `src/Sinop.Web/Views/Account/Login.cshtml` | New: login form |
| `src/Sinop.Web/Views/Account/AccessDenied.cshtml` | New: access denied page |
| `src/Sinop.Web/Views/Home/Index.cshtml` | New: dashboard view |
| `src/Sinop.Web/Sinop.Web.csproj` | Removed `Microsoft.AspNetCore.OpenApi` (conflicts with Swashbuckle); added `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore` |

## Architecture and Responsibilities
- **MVC (2A)**: cookie authentication as default scheme; `AccountController` handles login/logout/access denied with antiforgery validation; role-based `[Authorize]` on controllers; `HomeController.Index` is the protected dashboard.
- **API (2B)**: JWT Bearer as a second scheme; `AuthController` (`api/auth/login`) validates credentials via `SignInManager.CheckPasswordSignInAsync`, issues a signed JWT via `TokenService`; roles included as claims.
- **Authorization policies**: `RequireAdministrator`, `RequireLibrarian`, `RequireBorrower`, `RequireLibrarianOrAdmin` — ready to apply to controllers/actions in later milestones.
- **Swagger**: `AddSwaggerGen` with `Bearer` security definition/requirement so the Authorize button works in Swagger UI.
- **Identity** (carried from Milestone 1): `ApplicationUser`, `LibraryDbContext`, seeded roles + 3 accounts remain the user store.
- Secrets: JWT `Key` is in `appsettings.json` for development; production must override via environment/user-secrets (CLAUDE.md §13.1).

## Important Implementations
- JWT claims: `nameid` (user Id), `email`, `name` (userName), `role` (one per role).
- Token expiration driven by `Jwt:ExpireMinutes` config (default 120).
- MVC antiforgery on login POST (`[ValidateAntiForgeryToken]`).
- `CheckPasswordSignInAsync` used for API (no lockout side effects) vs `PasswordSignInAsync` for MVC cookie sign-in.
- Protected dashboard returns 302 to `/Account/Login` when unauthenticated.
- Navigation shell per CLAUDE §8.1: Dashboard, Catalog, Borrowers, Circulation, Reports, Notifications, Settings.

## Database Changes
None. Reuses `SinopLibraryDb`, Identity tables, and seeded users from Milestone 1.

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors.
- **JWT login valid** (`POST /api/auth/login` borrower/Password123!) → 200 with JWT token, email, roles: ["Borrower"], expiresAt.
- **JWT login bad password** → 401 `{"message":"Invalid email or password."}`.
- **JWT login unknown user** → 401 `{"message":"Invalid email or password."}`.
- **MVC login page** (`GET /Account/Login`) → 200.
- **Protected dashboard without auth** (`GET /`) → 302 redirect to login.
- **Swagger JSON** (`/swagger/v1/swagger.json`) → 200.
- `dotnet test` — no tests exist yet (tests added in later milestones per CLAUDE §15).

## Known Limitations / TODO
1. JWT `Key` hardcoded in `appsettings.json` for dev — must move to user-secrets/env before production (CLAUDE §13.1).
2. MVC login POST currently returns the same view with model error; no "remember me" or external providers (not required by spec).
3. No `[Authorize]` / role policies applied to catalog/circulation controllers yet — those controllers don't exist until Milestones 3–7; policies are registered and ready.
4. Role-based API authorization not yet enforced on any API controller (no API controllers with `[Authorize]` exist yet beyond auth).
5. No unit/integration tests for auth flows yet.
6. TokenService reads `Jwt:ExpireMinutes` directly; fallback "120" hardcoded if missing.
