# HOW-TO-USE.md — Milestone 6: Physical Inventory Management

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

### 2. Run
```bash
dotnet run --project src/Sinop.Web
```
App listens on `http://localhost:5288`.

### 3. Obtain a JWT token (librarian)
```bash
curl -s -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"librarian@library.local","password":"Password123!"}'
```

### 4. Call inventory endpoints
```bash
TOKEN="<paste token>"

# List copies (paged, optional bookId/status filters)
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5288/api/bookcopies?page=1&pageSize=10&bookId=2&status=0"

# Copy details
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5288/api/bookcopies/3"

# Register a copy (manual accession)
curl -X POST http://localhost:5288/api/bookcopies \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"bookId":2,"accessionNumber":"LIB-MANUAL-01","status":0}'

# Register with auto-generated accession (omit accessionNumber)
curl -X POST http://localhost:5288/api/bookcopies \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"bookId":3,"status":0}'

# Generate a suggested accession number for a book
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5288/api/bookcopies/accession-number/3"

# Update copy status
curl -X PATCH http://localhost:5288/api/bookcopies/3/status \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"status":1}'
```

## Valid Status Transitions
| From → To allowed | Notes |
|---|---|
| Available → Borrowed, Reserved, Maintenance, Lost | |
| Borrowed → Available, Maintenance, Lost | NOT Reserved |
| Reserved → Available, Borrowed, Maintenance, Lost | |
| Maintenance → Available, Lost | |
| Lost → Available, Maintenance | |

Status values: 0=Available, 1=Borrowed, 2=Reserved, 3=Maintenance, 4=Lost.

## Examples
- Duplicate accession → 409 `{"message":"Accession number ... already exists."}`.
- Invalid transition → 409 `{"message":"Invalid copy status transition from ... to ..."}`.
- Borrower on mutating endpoints → 403.
- Missing copy → 404.

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| 401 | Missing/invalid JWT | Obtain token from `/api/auth/login` |
| 403 | Borrower token on mutating endpoint | Use librarian@library.local token |
| 409 duplicate | Accession number exists | Use unique accession or omit for auto-generation |
| Build file locked | `dotnet run` holding dll | Kill dotnet process, retry |
| `Cannot open database SinopLibraryDb` | DB not created | `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE SinopLibraryDb;"` then re-run |
