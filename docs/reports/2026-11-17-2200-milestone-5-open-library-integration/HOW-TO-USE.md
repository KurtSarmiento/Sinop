# HOW-TO-USE.md — Milestone 5: Open Library Integration

## Prerequisites
- .NET 10 SDK (`dotnet --version` → 10.0.403).
- SQL Server LocalDB `(localdb)\MSSQLLocalDB` with `SinopLibraryDb` seeded (Milestone 1).
- Outbound network access to `openlibrary.org` (required for search/import; blocked in some sandboxes).
- `export windir=/c/Windows` in bash.
- Working directory: `C:\Users/Kurt Sarmiento/Sinop`.

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

### 3. Obtain a librarian JWT token
```bash
curl -s -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"librarian@library.local","password":"Password123!"}'
```

### 4. Search Open Library by title
```bash
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5288/api/openlibrary/search?title=clean%20code&limit=5"
```

### 5. Search by author
```bash
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5288/api/openlibrary/search-by-author?author=martin%20fowler&limit=5"
```

### 6. Look up by ISBN
```bash
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5288/api/openlibrary/isbn/978-0132350884"
```

### 7. Get cover URL
```bash
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5288/api/openlibrary/cover/<COVER_EDITION_KEY>"
```

### 8. Import a book (creates bibliographic record only, no copies)
```bash
curl -X POST http://localhost:5288/api/openlibrary/import \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Clean Code","isbn":"978-0132350884","summary":"...","categoryId":2,"authorIds":[1]}'
```

## Responses
| Endpoint | Success | Errors |
|---|---|---|
| GET search | 200 `{total, results[]}` | 400 bad title, 404 no results, 500 external API down |
| GET search-by-author | 200 `{total, results[]}` | 400 bad author, 404 no results |
| GET isbn/{isbn} | 200 `{OpenLibraryBookPreview}` | 404 not found |
| GET cover/{key} | 200 `{coverUrl}` | 404 not found |
| POST import | 201 `{bookId, title, isbn, created:true}` | 400 invalid model, 409 duplicate ISBN |

## Role Enforcement
- `Librarian` and `Administrator`: full access.
- `Borrower`: 403 Forbidden on all Open Library endpoints.

## Examples
- Duplicate ISBN import → 409 `{"message":"A book with ISBN ... already exists."}`.
- External API unreachable → 500 (fast fail after 10s timeout; local catalog unaffected).
- Borrower calling search → 403.

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| 500 on search/ISBN | `openlibrary.org` unreachable or slow | Check network; client times out after 10s |
| 403 | Borrower token used | Use librarian@library.local token |
| 409 on import | ISBN already in local catalog | Use a different ISBN or import updates existing book |
| Build file locked | `dotnet run` holding dll | Kill dotnet process, retry |
| `Cannot open database SinopLibraryDb` | DB not created | `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE SinopLibraryDb;"` then re-run |
