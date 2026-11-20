# HOW-TO-USE.md — Milestone 9: Reading Lists

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

### 3. Obtain a JWT token (borrower)
```bash
curl -s -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"borrower@library.local","password":"Password123!"}'
```
Copy the `token`. UserId comes from the token's `nameid` claim automatically.

### 4. Create a reading list
```bash
curl -X POST http://localhost:5288/api/readinglists \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"My Favorites","description":"Books I love"}'
```

### 5. List your reading lists
```bash
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5288/api/readinglists"
```

### 6. Get list details
```bash
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5288/api/readinglists/<LIST_ID>"
```

### 7. Add a book to a list
```bash
curl -X POST "http://localhost:5288/api/readinglists/<LIST_ID>/items" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"bookId":2}'
```

### 8. Remove a book from a list
```bash
curl -X DELETE "http://localhost:5288/api/readinglists/<LIST_ID>/items/<ITEM_ID>" \
  -H "Authorization: Bearer $TOKEN"
```

### 9. Update a list
```bash
curl -X PUT "http://localhost:5288/api/readinglists/<LIST_ID>" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"New Name","description":"New desc"}'
```

### 10. Delete a list
```bash
curl -X DELETE "http://localhost:5288/api/readinglists/<LIST_ID>" \
  -H "Authorization: Bearer $TOKEN"
```

## Examples
- Add duplicate book → 409 `{"message":"Book 2 is already in reading list 1."}`.
- Add nonexistent book → 409 `{"message":"Book 999 not found."}`.
- Access another user's list → 404 `{"message":"Reading list 1 not found or access denied."}`.
- Unauthenticated → 401.

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| 401 | Missing/invalid JWT or missing nameid claim | Obtain token from `/api/auth/login` |
| 404 on list access | List not owned by token user | Ownership enforced — use your own lists |
| 409 duplicate add | Book already in list | Use a different book or remove first |
| 409 book not found | Invalid bookId | Use an existing book id from `/api/books` |
| Build file locked | `dotnet run` holding dll | Kill dotnet process, retry |
| `Cannot open database SinopLibraryDb` | DB not created | `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE SinopLibraryDb;"` then re-run |
