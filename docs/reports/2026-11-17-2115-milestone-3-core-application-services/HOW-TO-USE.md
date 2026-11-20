# HOW-TO-USE.md — Milestone 3: Core Application Services

## Prerequisites
- .NET 10 SDK (`dotnet --version` → 10.0.403).
- SQL Server LocalDB `(localdb)\MSSQLLocalDB` with `SinopLibraryDb` (Milestone 1).
- `export windir=/c/Windows` in bash.
- Working directory: `C:\Users\Kurt Sarmiento\Sinop`.

## Step-by-Step Instructions

### 1. Build
```bash
export windir=/c/Windows
dotnet build Sinop.sln
```

### 2. Run (services resolve via DI at startup)
```bash
dotnet run --project src/Sinop.Web
```
The app registers `BookService`, `BookCopyService`, `BorrowerService`, `NotificationService` as scoped. If DI resolution failed, the app would not start.

### 3. Verify endpoints still responding
```bash
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5288/swagger/v1/swagger.json
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5288/Account/Login
```

## Examples
No public API endpoints consume these services yet (endpoints come in Milestones 4–7). Services are used by controllers via constructor injection:
```csharp
public class BooksController : ControllerBase
{
    private readonly BookService _bookService;
    public BooksController(BookService bookService) => _bookService = bookService;
}
```

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| Build error: `CopyStatus` not found | Missing `using Sinop.Domain.Enums;` in DTO | Already fixed in `BookCopyResponse.cs`/`BookCopyRequest.cs` |
| App fails to start | DI cannot resolve a service | Ensure all four services registered as scoped in `Program.cs` |
| `address already in use` | Previous `dotnet run` still bound | Kill the dotnet process and retry |
| `Cannot open database SinopLibraryDb` | DB not created | `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE SinopLibraryDb;"` then re-run |
