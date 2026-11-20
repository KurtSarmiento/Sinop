# HOW-TO-USE.md — Milestone 1: Domain Entities and Database Foundation

## Prerequisites
- .NET 10 SDK installed (`dotnet --version` → 10.0.403).
- SQL Server LocalDB available: `(localdb)\MSSQLLocalDB`.
- Environment variable `windir` set in the bash shell (NuGet requires it): `export windir=/c/Windows`.
- Working directory: `C:\Users\Kurt Sarmiento\Sinop`.

## Step-by-Step Instructions

### 1. Build the solution
```bash
export windir=/c/Windows
dotnet build Sinop.sln
```
All three projects (`Sinop.Domain`, `Sinop.Infrastructure`, `Sinop.Web`) compile with 0 errors.

### 2. Create / apply the database migration
Migration `InitialCreate` is already added under `src/Sinop.Infrastructure/Persistence/Migrations/`.
To add a new migration after model changes:
```bash
dotnet ef migrations add <Name> --project src/Sinop.Infrastructure --startup-project src/Sinop.Web --output-dir Persistence/Migrations
```
The database is created/migrated automatically when the web app starts (`Program.cs` calls `context.Database.Migrate()`).

### 3. Run the web application
```bash
export windir=/c/Windows
dotnet run --project src/Sinop.Web
```
The app listens on `http://localhost:5288` (see `launchSettings.json`). On first run it:
1. Applies the `InitialCreate` migration to `(localdb)\MSSQLLocalDB` → database `SinopLibraryDb`.
2. Seeds roles: Administrator, Librarian, Borrower.
3. Seeds accounts: admin@library.local, librarian@library.local, borrower@library.local (password `Password123!`).

### 4. Inspect the database
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d SinopLibraryDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME;"
```

## Examples
- Verify seeded books:
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d SinopLibraryDb -Q "SELECT Id, Title, ISBN FROM Books;"
```
- Verify seeded copies:
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d SinopLibraryDb -Q "SELECT AccessionNumber, Status FROM BookCopies;"
```

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| `dotnet build`/`dotnet ef` fails: NuGet `Path.Combine(null)` crash | `windir` / `ProgramData` / `ALLUSERSPROFILE` missing in bash shell | `export windir=/c/Windows` before running dotnet |
| `dotnet ef` says startup project lacks `Microsoft.EntityFrameworkCore.Design` | Design-time EF package missing | Already added to `Sinop.Web`; ensure restored |
| `SqlException` on startup: LocalDB not running | SQL Server LocalDB instance stopped | Start LocalDB or verify `(localdb)\MSSQLLocalDB` exists |
| Duplicate user/role seed errors on re-run | Seeding is idempotent (`RoleExistsAsync`, `FindByEmailAsync` checks) | No action needed — safe to re-run |
