# Spec: Milestone 0 — Project Initialization and Architecture

## Objective
Establish the solution structure and development environment for the Sinop Digital Library and Reading Management System. Create the .NET solution with Domain, Infrastructure, and Web projects, correct project references, nullable reference types, and DI/config foundations. No business features, no DB entities, no MVC pages, no Android screens yet.

## Tech Stack
- .NET 10, C#
- ASP.NET Core (Web API project hosting MVC + API controllers)
- Entity Framework Core, SQL Server (LocalDB for dev)
- Android (Kotlin + Jetpack Compose) — deferred, SDK not yet installed

## Commands
- Build: `dotnet build Sinop.sln`
- Create sln: `dotnet new sln -n Sinop`
- Add project: `dotnet sln add <csproj>`

## Project Structure
```
Sinop.sln
src/
  Sinop.Domain/          — class library, domain entities (later Milestone 1)
  Sinop.Infrastructure/  — class library, EF persistence (later Milestone 1)
  Sinop.Web/             — ASP.NET Core web app (API + MVC later)
```

## Code Style
- C# PascalCase public APIs, camelCase locals, nullable reference types enabled
- Constructor injection, no business logic in controllers (enforced in later milestones)

## Testing Strategy
- No tests in Milestone 0 (scaffolding only). Verification = `dotnet build` succeeds.

## Boundaries
- Always: `dotnet build` after changes; keep separation of concerns
- Ask first: adding NuGet packages, changing target framework
- Never: business logic, DB entities, MVC views, Android screens in this milestone

## Success Criteria
- Solution structure exists; all .NET projects compile
- Project references correct (Infra→Domain, Web→Infra+Domain)
- Nullable reference types enabled
- No unnecessary dependencies

## Open Questions
- Android SDK not installed; Android project creation deferred to when SDK is available (documented limitation)