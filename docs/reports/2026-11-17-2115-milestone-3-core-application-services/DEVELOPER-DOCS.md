# DEVELOPER-DOCS.md — Milestone 3: Core Application Services

## Summary
Implemented reusable core application services (`BookService`, `BookCopyService`, `BorrowerService`, `NotificationService`) plus request/response DTOs and DI registration. Business logic lives in services, not controllers, per CLAUDE.md separation of concerns.

## Milestone
- **MILESTONE 3** — Core Application Services (sub-milestones 3A BookService, 3B BookCopyService, 3C BorrowerService, 3D NotificationService). Executes after Milestone 2, per CLAUDE.md §17 strict order.

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Web | ASP.NET Core Web API + MVC | DTOs, application services, DI registration |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Web/DTOs/BookRequest.cs` | New: create/update book DTO (Title, ISBN, Summary, CategoryId, AuthorIds) |
| `src/Sinop.Web/DTOs/BookResponse.cs` | New: book list item response (incl. copy counts, authors, category) |
| `src/Sinop.Web/DTOs/BookDetailsResponse.cs` | New: book details with copies |
| `src/Sinop.Web/DTOs/BookCopyRequest.cs` | New: register copy DTO (BookId, AccessionNumber, Status) |
| `src/Sinop.Web/DTOs/BookCopyResponse.cs` | New: book copy response |
| `src/Sinop.Web/DTOs/BorrowerResponse.cs` | New: borrower summary response |
| `src/Sinop.Web/DTOs/NotificationRequest.cs` | New: create notification DTO |
| `src/Sinop.Web/DTOs/NotificationResponse.cs` | New: notification response |
| `src/Sinop.Web/DTOs/PagedResult.cs` | New: generic paginated result |
| `src/Sinop.Web/Services/BookService.cs` | New: get books (paged/search), details, create (duplicate ISBN check), update, archive (guards active loans/reservations), ISBN duplicate check |
| `src/Sinop.Web/Services/BookCopyService.cs` | New: get copies (paged/filter), register copy (accession gen/manual override, unique check), update status, generate accession number, available count |
| `src/Sinop.Web/Services/BorrowerService.cs` | New: get borrowers (paged), get borrower, validate account, borrower history |
| `src/Sinop.Web/Services/NotificationService.cs` | New: create notification, create-if-not-exists (dedupe), get notifications (paged/unread filter), mark read, mark all read |
| `src/Sinop.Web/Program.cs` | Registered all four services as scoped in DI |

## Architecture and Responsibilities
- Services depend only on `LibraryDbContext` (+ `UserManager` for BorrowerService) via constructor injection.
- No business logic in controllers; controllers (Milestone 4+) will call these services.
- Domain entities never exposed through API; DTOs used exclusively.
- Pagination via `PagedResult<T>`; search/filter supported on books and copies.
- Async I/O throughout with `CancellationToken`.
- Book archive enforces CLAUDE §5.9 invariants: no archive while active loans or queued reservations exist.
- Copy accession numbers unique; auto-generation when not provided.
- Notification dedupe via `CreateIfNotExistsAsync` (prevents duplicate unread notifications).

## Important Implementations
- `BookService.CreateBookAsync` rejects duplicate ISBN.
- `BookService.ArchiveBookAsync` throws if active loans or active reservations exist.
- `BookCopyService.RegisterCopyAsync` validates book exists and accession number unique.
- `BookCopyService.GenerateAccessionNumberAsync` produces `LIB-####-NN` unique accession numbers.
- `NotificationService.CreateIfNotExistsAsync` checks for existing unread identical message before inserting.
- `BorrowerService.ValidateAccountAsync` checks user exists and not locked out.

## Database Changes
None. Reuses `SinopLibraryDb` tables from Milestone 1 (Books, BookCopies, Users, Loans, Reservations, Notifications).

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors (pre-existing NU1902/NU1903 warnings only).
- App startup resolves all four scoped services through DI (app boots and listens on :5288).
- Endpoints from Milestones 1–2 still respond (JWT login, Swagger, MVC login).
- Service-level unit tests not yet written — deferred per CLAUDE §15 (tests added in later milestones); important operations listed in Known Limitations.

## Known Limitations / TODO
1. No controllers consume these services yet — controllers/endpoints implemented in Milestones 4–7.
2. No unit/integration tests for service logic (duplicate ISBN rejection, archive guards, accession uniqueness, notification dedupe).
3. `BorrowerService.GetBorrowerHistoryAsync` currently returns the borrower summary (loan-based counts) rather than a full loan list — full history endpoint comes in Milestone 7.
4. JWT `Key` still in `appsettings.json` (dev only; move to user-secrets before production).
