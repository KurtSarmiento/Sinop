# DEVELOPER-DOCS.md — Milestone 7: Borrowing and Return Operations

## Summary
Implemented reliable physical circulation: `LoanService` with transactional checkout/return, concurrency-safe copy allocation, borrowing-limit enforcement, duplicate-loan/duplicate-return prevention, overdue detection; `LoansController` exposes active loans, history, eligibility, overdue count, checkout, and return endpoints. `LibraryRules` holds circulation constants (14-day loan, 3 max active loans, 2-day pickup period).

## Milestone
- **MILESTONE 7** — Borrowing and Return Operations (sub-milestones 7A LoanService, 7B Return Operations, 7C Loans API). Executes after Milestone 6, per CLAUDE.md §17 strict order.

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Web | ASP.NET Core Web API | LoanService, LibraryRules, LoansController, DTOs |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Web/DTOs/LoanRequest.cs` | New: `{BookId, UserId}` checkout request |
| `src/Sinop.Web/DTOs/LoanResponse.cs` | New: loan details incl. accession, book title, user email, due date, isOverdue |
| `src/Sinop.Web/DTOs/ReturnRequest.cs` | New: `{LoanId}` return request |
| `src/Sinop.Web/Services/LibraryRules.cs` | New: `LoanDurationDays=14`, `MaxSimultaneousActiveLoans=3`, `ReservationPickupPeriodDays=2` |
| `src/Sinop.Web/Services/LoanService.cs` | New: CheckoutAsync (transactional), ReturnAsync (transactional), GetActiveLoansAsync, GetLoanHistoryAsync, ValidateBorrowerEligibilityAsync, GetOverdueCountAsync |
| `src/Sinop.Web/Controllers/LoansController.cs` | New: GET active/history/eligibility/overdue-count, POST checkout/return |
| `src/Sinop.Web/Program.cs` | Registered `LoanService` as scoped |

## Architecture and Responsibilities
- `LoanService` holds all circulation business logic; controller is a thin adapter (DTOs in/out, HTTP responses).
- No EF entities exposed — `LoanResponse` DTO used exclusively.
- Checkout and return run inside `DbContext` transactions with rollback on failure.
- **Concurrency safety**: checkout finds an available copy (`Status == Available`), then re-fetches it inside the transaction and re-checks status before allocating — prevents two concurrent checkouts allocating the same copy.
- Duplicate active loan for same borrower+title prevented.
- Borrowing limit (max 3 active loans) enforced.
- Duplicate return prevented (loan must be `Active`).
- On return: loan marked `Returned` with `ReturnDate`, copy set back to `Available`.
- Async I/O with `CancellationToken` throughout.
- Role enforcement: checkout/return are librarian actions (`Librarian,Administrator`); borrowers can view their own active loans/history/eligibility.

## Important Implementations
- `CheckoutAsync` validates: user exists, book exists, no duplicate active loan for title, under limit, an available copy exists.
- Copy allocation: `copy.Status = Borrowed` + loan `Status = Active`, `DueDate = Now + 14 days`.
- `ReturnAsync`: validates loan is `Active`, sets `ReturnDate`, `Status = Returned`, copy `Status = Available`.
- `IsOverdue` computed on `LoanResponse`: `Status == Active && DateTime.UtcNow > DueDate`.
- `GetOverdueCountAsync` counts active loans past due date.

## Database Changes
None (schema from Milestone 1). Verification created loan rows (Loans table) and toggled BookCopy statuses.

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors.
- **GET /api/loans/eligibility/{userId}** → 200 `{eligible:true, maxActiveLoans:3}`.
- **POST /api/loans/checkout** (librarian) → 201, loan created, copy allocated (LIB-0003), status Borrowed.
- **POST duplicate checkout** (same borrower+title) → 409.
- **POST checkout second book** → 201 (2 active loans).
- **GET /api/loans/active?userId=X** → 200, 2 active loans.
- **POST /api/loans/return** → 200, loan Returned with ReturnDate, copy back to Available.
- **POST duplicate return** → 409.
- **GET /api/loans/overdue-count** → 200.
- Role enforcement: borrower token on checkout/return → 403.
- Missing copy available → 409 when no copies available.

## Known Limitations / TODO
1. **No concurrency test** — transactional checkout with re-check under lock not tested under concurrent load (would need a load test).
2. `Status` serializes as integer (0=Active,1=Returned,2=Cancelled) — clients see numbers not names. Consider `StringEnumConverter`.
3. `LoanResponse.UserEmail` populated via extra query; could be optimized.
4. No overdue automation yet (Milestone 10 Hangfire job).
5. No unit/integration tests (deferred per CLAUDE §15).
6. JWT `Key` still in `appsettings.json` (dev only).
7. Return does not yet allocate copy to waiting reservations (Milestone 8 handles reservation queue allocation).
