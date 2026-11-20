# HOW-TO-USE.md — Milestone 7: Borrowing and Return Operations

## Prerequisites
- .NET 10 SDK (`dotnet --version` → 10.0.403).
- SQL Server LocalDB `(localdb)\MSSQLLocalDB` with `SinopLibraryDb` seeded (Milestone 1).
- At least one book with an available copy (Physical Inventory, Milestone 6).
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

### 3. Obtain a JWT token (librarian for checkout/return)
```bash
curl -s -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"librarian@library.local","password":"Password123!"}'
```

### 4. Check borrower eligibility
```bash
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5288/api/loans/eligibility/<USER_ID>"
```

### 5. Checkout a book (librarian only)
```bash
curl -X POST http://localhost:5288/api/loans/checkout \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"bookId":2,"userId":"<USER_ID>"}'
```
Allocates an available copy, sets due date = now + 14 days.

### 6. View active loans
```bash
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5288/api/loans/active?userId=<USER_ID>"
```

### 7. View loan history
```bash
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5288/api/loans/history?userId=<USER_ID>"
```

### 8. Return a book (librarian only)
```bash
curl -X POST http://localhost:5288/api/loans/return \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"loanId":<LOAN_ID>}'
```

### 9. Overdue count
```bash
curl -H "Authorization: Bearer $TOKEN" \
  "http://localhost:5288/api/loans/overdue-count"
```

## Business Rules Enforced
| Rule | Default | Enforcement |
|---|---|---|
| Loan duration | 14 days | DueDate = LoanDate + 14 days |
| Max simultaneous active loans | 3 | 409 if limit reached |
| Duplicate active loan (borrower+title) | prevented | 409 |
| Copy cannot be borrowed twice | prevented | transactional re-check under lock |
| Duplicate return | prevented | 409 if loan not Active |

Status values: 0=Available, 1=Borrowed, 2=Reserved, 3=Maintenance, 4=Lost. Loan status: 0=Active, 1=Returned, 2=Cancelled.

## Examples
- Checkout when no copies available → 409 `{"message":"No available copies of '...'."}`.
- Duplicate checkout → 409 `{"message":"User already has an active loan for '...'."}`.
- Limit reached → 409 `{"message":"Borrowing limit reached: maximum 3 simultaneous active loans."}`.
- Duplicate return → 409 `{"message":"Loan N is already Returned and cannot be returned again."}`.

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| 401 | Missing/invalid JWT | Obtain token from `/api/auth/login` |
| 403 | Borrower token on checkout/return | Use librarian@library.local token |
| 409 no copies | All copies borrowed/maintenance | Return a copy first, or use a different book |
| 409 duplicate loan | Borrower already has active loan for title | Return the existing loan first |
| Build file locked | `dotnet run` holding dll | Kill dotnet process, retry |
| `Cannot open database SinopLibraryDb` | DB not created | `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE SinopLibraryDb;"` then re-run |
