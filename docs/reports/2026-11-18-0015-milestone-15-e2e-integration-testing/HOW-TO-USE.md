# HOW-TO-USE.md — Milestone 15: End-to-End Integration and Testing

## Prerequisites
- JDK 17 (`JAVA_HOME` set).
- Gradle 8.7 wrapper (vendored).
- Backend Web API running on `http://localhost:5288`:
  ```bash
  export windir=/c/Windows
  dotnet run --project src/Sinop.Web --urls "http://localhost:5288"
  ```
- Seeded database with demo accounts (Milestone 1):
  - admin@library.local / Password123! (Administrator)
  - librarian@library.local / Password123! (Librarian)
  - borrower@library.local / Password123! (Borrower)

## E2E Test Scenarios (reproduce against live API)

All scenarios use `curl` with a real JWT bearer token. Get a token first:
```bash
TOKEN=$(curl -s http://localhost:5288/api/Auth/login -X POST -H "Content-Type: application/json" \
  -d '{"email":"librarian@library.local","password":"Password123!"}' | python3 -c "import sys,json; print(json.load(sys.stdin)['token'])")
```

### Scenario A: Catalog Synchronization
```bash
# 1. Librarian views books through API
curl -s "http://localhost:5288/api/books?page=1&pageSize=20" -H "Authorization: Bearer $TOKEN"

# 2. Librarian views physical copies
curl -s "http://localhost:5288/api/bookcopies?page=1&pageSize=20" -H "Authorization: Bearer $TOKEN"

# 3. (Android would retrieve the same data through the same endpoints)
```
Expected: 2 books (The Pragmatic Programmer, To Kill a Mockingbird), 3 copies.

### Scenario B: Borrowing
```bash
# 1. Librarian checks out book id=2 for borrower
curl -s -X POST "http://localhost:5288/api/loans/checkout" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"bookId":2,"UserId":"<borrower-userid>"}'

# 2. Verify copy status changed to Borrowed
curl -s "http://localhost:5288/api/bookcopies" -H "Authorization: Bearer $TOKEN"

# 3. Try duplicate active loan for same book+user → expect HTTP 409
curl -s -w "\nHTTP %{http_code}" -X POST "http://localhost:5288/api/loans/checkout" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"bookId":2,"UserId":"<borrower-userid>"}'

# 4. Try checkout unavailable book → expect HTTP 409
curl -s -w "\nHTTP %{http_code}" -X POST "http://localhost:5288/api/loans/checkout" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"bookId":3,"UserId":"<borrower-userid>"}'
```

### Scenario C: Reservation
```bash
# 1. Create reservation for book id=2
curl -s -X POST "http://localhost:5288/api/reservations" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"bookId":2,"userId":"<borrower-userid>"}'

# 2. View all reservations
curl -s "http://localhost:5288/api/reservations" -H "Authorization: Bearer $TOKEN"

# 3. Cancel reservation
curl -s -X POST "http://localhost:5288/api/reservations/<id>/cancel?userId=<borrower-userid>" \
  -H "Authorization: Bearer $TOKEN"
```

### Scenario D: Return and Notifications
```bash
# 1. Return an active loan
curl -s -X POST "http://localhost:5288/api/loans/return" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"loanId":<loan-id>}'

# 2. Verify copy status back to Available
curl -s "http://localhost:5288/api/bookcopies" -H "Authorization: Bearer $TOKEN"

# 3. View borrower notifications
curl -s "http://localhost:5288/api/notifications?userId=<borrower-userid>" \
  -H "Authorization: Bearer $TOKEN"
```

### Scenario E: Offline Catalog (Android)
Room cache implemented in CatalogScreen — cached books displayed when API unreachable. Build-verified; emulator test requires a device/emulator.

### Scenario F: Overdue Automation
```bash
# 1. View active loans — overdue loans have isOverdue=true
curl -s "http://localhost:5288/api/loans/active?userId=<borrower-userid>" -H "Authorization: Bearer $TOKEN"

# 2. Overdue notification generated for borrower
curl -s "http://localhost:5288/api/notifications?userId=<borrower-userid>" -H "Authorization: Bearer $TOKEN"
```

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| 401 on API call | Missing/expired JWT | Re-authenticate, get new token |
| 409 on checkout | Duplicate active loan or no copies | Use different book/user, or return existing loan first |
| 500 on Open Library search | External API timeout (sandbox network restriction) | Not a code defect; local catalog endpoints work |
| `dotnet build` file locked | Backend process running | Kill dotnet/Sinop.Web process, retry |
