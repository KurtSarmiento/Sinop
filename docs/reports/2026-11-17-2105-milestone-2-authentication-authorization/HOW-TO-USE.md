# HOW-TO-USE.md — Milestone 2: Authentication and Authorization

## Prerequisites
- .NET 10 SDK installed (`dotnet --version` → 10.0.403).
- SQL Server LocalDB `(localdb)\MSSQLLocalDB` running with `SinopLibraryDb` (from Milestone 1).
- Environment variable `windir` set: `export windir=/c/Windows`.
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
The app starts `Database.Migrate()` (no-op if already migrated) and listens on `http://localhost:5288`.

### 3. MVC (cookie) login
Open `http://localhost:5288/Account/Login` in a browser. Log in with:
- admin@library.local / Password123! (Administrator)
- librarian@library.local / Password123! (Librarian)
- borrower@library.local / Password123! (Borrower)

After login you reach the dashboard at `/`. Logout posts to `/Account/Logout`.

### 4. API (JWT) login
```bash
curl -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"borrower@library.local","password":"Password123!"}'
```
Response includes `token` (JWT), `email`, `roles`, `expiresAt`.

### 5. Call a protected API with JWT
```bash
TOKEN=$(curl -s -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"borrower@library.local","password":"Password123!"}' | \
  python -c "import sys,json; print(json.load(sys.stdin)['token'])")
curl -H "Authorization: Bearer $TOKEN" http://localhost:5288/swagger/v1/swagger.json
```

### 6. Swagger UI
Open `http://localhost:5288/swagger`. Click **Authorize**, enter `Bearer <token>`, then call endpoints.

## Examples
- Bad password:
```bash
curl -s -X POST http://localhost:5288/api/auth/login -H "Content-Type: application/json" -d '{"email":"borrower@library.local","password":"wrong"}'
# → 401 {"message":"Invalid email or password."}
```

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| `Cannot open database "SinopLibraryDb"` | Database not created | `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE SinopLibraryDb;"` then re-run app |
| `address already in use` | Previous `dotnet run` still bound | Kill the dotnet process (`ps \| grep dotnet`, then `kill <pid>`) |
| JWT login fails silently | LocalDB stopped | `sqllocaldb start MSSQLLocalDB` |
| `windir`/NuGet restore error | `windir` missing in bash | `export windir=/c/Windows` before dotnet commands |
