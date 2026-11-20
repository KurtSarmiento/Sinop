# DEVELOPER-DOCS.md — Milestone 0: Project Initialization and Architecture

## Summary
Established the solution structure and development environment for the Sinop Digital Library and Reading Management System (.NET 10). Created the `Sinop.sln` solution with three .NET projects (`Sinop.Domain`, `Sinop.Infrastructure`, `Sinop.Web`), correct project references, nullable reference types, and configuration foundations. No business features, database entities, MVC pages, or Android screens were implemented — this is pure scaffolding per Milestone 0 acceptance criteria.

### Spec
- `SPEC-MILESTONE-0.md` (root) — objective, tech stack, commands, structure, style, testing strategy, boundaries, success criteria, open questions.

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Domain | Class Library (net10.0) | Domain entities (deferred to Milestone 1) |
| Sinop.Infrastructure | Class Library (net10.0) | EF persistence / migrations (deferred to Milestone 1) |
| Sinop.Web | ASP.NET Core Web API (net10.0) | API + MVC surface (deferred to Milestones 4, 11) |

## Files Changed
| File | Change |
|---|---|
| `Sinop.sln` | New Visual Studio solution file; references all three projects |
| `src/Sinop.Domain/Sinop.Domain.csproj` | net10.0, nullable enable, implicit usings enable |
| `src/Sinop.Infrastructure/Sinop.Infrastructure.csproj` | net10.0, nullable enable, implicit usings enable; ProjectReference → Domain |
| `src/Sinop.Web/Sinop.Web.csproj` | net10.0, nullable enable, implicit usings enable; PackageReference AspNetCore.OpenApi 10.0.14; ProjectReferences → Domain, Infrastructure |
| `NuGet.Config` | Local package source config (nuget.org) |
| `SPEC-MILESTONE-0.md` | New spec document |
| `docs/reports/2026-11-17-1950-milestone-0-initialization/` | New report directory (this report) |

### Files Removed
- `src/Sinop.Domain/Class1.cs` — template placeholder
- `src/Sinop.Infrastructure/Class1.cs` — template placeholder
- `Sinop.slnx` — SDK-generated alternate format; replaced with classic `Sinop.sln` per CLAUDE.md

## Architecture and Responsibilities
- `Sinop.Domain` — zero dependencies; domain entities only (Milestone 1).
- `Sinop.Infrastructure` — depends on Domain; EF Core `LibraryDbContext`, Fluent API configurations, migrations, seeding, SQL Server integration (Milestone 1).
- `Sinop.Web` — depends on Domain + Infrastructure; ASP.NET Core host with controllers, application services, DTOs, middleware, Swagger, Hangfire registration (later milestones).
- No business logic in controllers; no DB queries in views; separation of concerns enforced by project boundaries.

## Important Implementations
- Classic `Sinop.sln` (Format Version 12.00) created explicitly because SDK 10 emitted `Sinop.slnx`; CLAUDE.md requires `Sinop.sln`.
- Nullable reference types enabled on all three projects.
- .NET 10 target framework on all projects.
- `Microsoft.AspNetCore.OpenApi` 10.0.14 added to Web for Swagger/OpenAPI.

## Database Changes
None. No entities, migrations, or database created in Milestone 0. SQL Server LocalDB (`MSSQLLocalDB`) verified reachable for Milestone 1.

## Verification
- **Build:** `dotnet build Sinop.sln` could not be completed in this session because of an environment issue: this bash session lacks `ProgramData`/`ALLUSERSPROFILE`/`windir` environment variables, causing NuGet's `XPlatMachineWideSetting` to crash on `Path.Combine(null, ...)` during restore. This affects even a brand-new scratch console project outside this solution — it is a machine/environment issue, not a code issue.
- Workaround attempted: created `C:\ProgramData\NuGet\Config\NuGet.Config`, set `ProgramData` via `setx` — the harness shell did not propagate it to the dotnet process.
- **Projects compile:** Not yet verifiable until the environment `ProgramData` issue is resolved. The project files, references, and target frameworks are correctly configured.
- Solution structure exists. Project references are correct (Infra→Domain, Web→Domain+Infra). Nullable reference types enabled. No unnecessary dependencies introduced.
- Android SDK (`/c/Android/Sdk`) confirmed present; Android project deferred to when SDK tooling can be verified (per spec open question).

## Known Limitations / TODO
1. `dotnet build`/`dotnet restore` fails on this machine due to missing `ProgramData`/`ALLUSERSPROFILE`/`windir` env vars in the bash shell — not a code defect. Set these variables, then run `dotnet build Sinop.sln` to verify compilation.
2. Android project creation deferred (Android SDK installed at `/c/Android/Sdk`, but SDK tooling not yet verified; per spec open question).
3. No `dotnet test` run — no tests exist in Milestone 0 (scaffolding only).
