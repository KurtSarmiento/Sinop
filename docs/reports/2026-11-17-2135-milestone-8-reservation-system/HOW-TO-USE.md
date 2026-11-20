# HOW-TO-USE.md — Milestone 8: Reservation System

## Prerequisites
- .NET 10 SDK (`dotnet --version` → 10.0.403).
- SQL Server LocalDB `(localdb)\MSSQLLocalDB` with `SinopLibraryDb` including migration `AddAllocatedCopyIdToReservations` (Milestone 8).
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
App listens on `http://localhost:5288`. `Database.Migrate()` applies migrations at startup.

### 3. Obtain a JWT token
```bash
# Borrower (creates/cancels reservations)
curl -s -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"borrower@library.local","password":"Password123!"}'

# Librarian (process-expired, allocate-returned-copy)
curl -s -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"librarian@library.local","password":"Password123!"}'
```

### 4. Create a reservation (borrower)
```bash
curl -X POST http://localhost:5288/api/reservations \
  -H "Authorization: Bearer $BT" \
  -H "Content-Type: application/json" \
  -d '{"bookId":2,"userId":"<USER_ID>"}'
```
Sets status Queued, pickupDeadline = now + 2 days, queue position by `ReservedAt`.

### 5. List reservations
```bash
# My reservations
curl -H "Authorization: Bearer $BT" "http://localhost:5288/api/reservations?userId=<USER_ID>"

# All reservations for a book (librarian)
curl -H "Authorization: Bearer $TOKEN" "http://localhost:5288/api/reservations?bookId=2"
```

### 6. Get reservation details
```bash
curl -H "Authorization: Bearer $BT" "http://localhost:5288/api/reservations/<ID>"
```

### 7. Cancel a reservation (borrower, ownership enforced)
```bash
curl -X POST "http://localhost:5288/api/reservations/<ID>/cancel?userId=<USER_ID>" \
  -H "Authorization: Bearer $BT"
```
Releases allocated copy back to Available and allocates to next in queue.

### 8. Allocate a returned copy to the queue (librarian)
After a copy is returned (Available), allocate it to the head of a book's reservation queue:
```bash
curl -X POST "http://localhost:5288/api/reservations/<BOOK_ID>/allocate-returned-copy?copyId=<COPY_ID>" \
  -H "Authorization: Bearer $TOKEN"
```
Sets head reservation to ReadyForPickup (2-day pickup deadline), copy to Reserved, notifies borrower.

### 9. Process expired reservations (librarian)
```bash
curl -X POST http://localhost:5288/api/reservations/process-expired \
  -H "Authorization: Bearer $TOKEN"
```
Expires Queued reservations past their pickup deadline, releases copies, notifies borrowers, reallocates.

## Reservation Statuses
| Value | Meaning |
|---|---|
| 0 | Queued — waiting in line |
| 1 | ReadyForPickup — copy allocated, 2-day pickup window |
| 2 | Fulfilled — checked out |
| 3 | Expired — pickup deadline passed |
| 4 | Cancelled — borrower or librarian cancelled |

## Business Rules
| Rule | Default |
|---|---|
| Reservation ordering | First-come, first-served (by ReservedAt) |
| Pickup period | 2 days |
| Duplicate active reservation (borrower+title) | Prevented |
| One copy allocated to at most one reservation | Enforced via AllocatedCopyId |
| Cancel releases copy to next in queue | Yes |
| Expired releases copy to next in queue | Yes |

## Examples
- Duplicate reservation → 409 `{"message":"User already has an active reservation for '...'."}`.
- Cancel with wrong userId → 404.
- Allocate unavailable copy → 409 `{"message":"Copy N is not available or there is no queued reservation..."}`.
- Borrower on process-expired/allocate → 403.

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| 401 | Missing/invalid JWT | Obtain token from `/api/auth/login` |
| 403 | Borrower on librarian-only endpoint | Use librarian@library.local token |
| 409 duplicate reservation | Borrower already has Queued reservation for title | Cancel existing first |
| 409 allocate copy unavailable | Copy not Available or no queue | Return copy first / check queue |
| Invalid column 'AllocatedCopyId' | Migration not applied | `dotnet ef database update` or add column manually + record in `__EFMigrationsHistory` |
| Build file locked | `dotnet run` holding dll | Kill dotnet process, retry |
