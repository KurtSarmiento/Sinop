# HOW-TO-USE.md — Milestone 4: Book Catalog Web API

## Prerequisites
- .NET 10 SDK (`dotnet --version` → 10.0.403).
- SQL Server LocalDB `(localdb)\MSSQLLocalDB` with `SinopLibraryDb` seeded (Milestone 1).
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
Copy the `token` value.

### 4. Call catalog endpoints
```bash
TOKEN="<paste token>"

# List books (paged)
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5288/api/books?page=1&pageSize=10"

# Search
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5288/api/books?search=clean"

# Book details
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5288/api/books/1"

# Create (librarian only)
curl -X POST http://localhost:5288/api/books \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"New Book","isbn":"978-1234567890","categoryId":2,"authorIds":[1]}'

# Update
curl -X PUT http://localhost:5288/api/books/1 -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{"title":"Updated","isbn":"978-1234567890","categoryId":2,"authorIds":[1]}'

# Archive (delete)
curl -X DELETE http://localhost:5288/api/books/1 -H "Authorization: Bearer $TOKEN"
```

## Examples
- Unauthenticated request → 401.
- Borrower calling POST → 403.
- Duplicate ISBN → 409 `{"message":"A book with ISBN ... already exists."}`.
- Missing book → 404 `{"message":"Book 999 not found."}`.

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| 401 on API calls | Missing/invalid JWT | Obtain token from `/api/auth/login`; send `Authorization: Bearer <token>` |
| 403 on POST/PUT/DELETE | Borrower token used | Use librarian@library.local or admin@library.local token |
| 409 on create | Duplicate ISBN | Use a unique ISBN |
| `address already in use` | Previous `dotnet run` still bound | Kill dotnet process, retry |
| Build file locked | `dotnet run` process holding dll | Kill dotnet process before `dotnet build` |
