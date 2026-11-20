# DEVELOPER-DOCS.md — Milestone 6: Physical Inventory Management

## Summary
Implemented the Physical Inventory Management API (`BookCopiesController`) for registering physical copies, accession number generation (auto or manual), copy status management with valid transitions, and inventory retrieval. `BookCopyRequest`/`BookCopyResponse` DTOs and `BookCopyService` were already created in Milestone 3; added status-transition validation and a `GetCopyAsync` method.

## Milestone
- **MILESTONE 6** — Physical Inventory Management (per CLAUDE.md §17 strict order; Milestone 5 Open Library deferred per 3-day priorities).

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Web | ASP.NET Core Web API | BookCopiesController, service additions |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Web/Controllers/BookCopiesController.cs` | New: GET copies (paged/filter), GET copy/{id}, POST register, PATCH status update, GET accession-number/{bookId}; role-based `[Authorize(Roles="Librarian,Administrator")]` on mutating endpoints |
| `src/Sinop.Web/Services/BookCopyService.cs` | Added: `GetCopyAsync(id)`, `UpdateCopyStatusAsync` with valid-transition validation (`IsValidTransition`), `GenerateAccessionNumberAsync` already existed |

## Architecture and Responsibilities
- Controller only binds/validates, calls `BookCopyService`, returns HTTP responses.
- No EF entities exposed — `BookCopyResponse` DTO used exclusively.
- `BookCopyService` holds business logic (accession uniqueness, transition rules, book existence).
- Role enforcement: only `Librarian`/`Administrator` may register copies or change status; `Borrower` gets 403.
- JWT Bearer auth scheme for all API endpoints.
- Errors: `InvalidOperationException` → 409 Conflict; missing resource → 404; invalid model → 400; unhandled → 500 via `ExceptionMiddleware` (Milestone 4).
- Async I/O with `CancellationToken` throughout.

## Important Implementations
- **Accession number uniqueness**: duplicate `AccessionNumber` → 409.
- **Manual accession override**: caller may supply `AccessionNumber`; if blank, `GenerateAccessionNumberAsync` produces `LIB-####-NN` unique value.
- **Status transition validation** (`IsValidTransition`):
  - Available → Borrowed/Reserved/Maintenance/Lost
  - Borrowed → Available/Maintenance/Lost (NOT Reserved)
  - Reserved → Available/Borrowed/Maintenance/Lost
  - Maintenance → Available/Lost
  - Lost → Available/Maintenance
  - Same status → allowed (no-op)
- Book existence validated before register.

## Database Changes
None (schema from Milestone 1). Verification created test copy rows (LIB-MANUAL-01 etc.) — test data added to `BookCopies` table.

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors.
- **GET /api/bookcopies** → 200 paged result (2 remaining seeded copies 3, 4 after Milestone 4 archive of book 1).
- **GET /api/bookcopies/1** → 200 with copy details.
- **GET /api/bookcopies/999** → 404.
- **POST /api/bookcopies** new copy → 201.
- **POST duplicate accession** → 409.
- **GET /api/bookcopies/accession-number/3** → `{"accessionNumber":"LIB-0003-01"}`.
- **PATCH status Available→Borrowed** → 200.
- **PATCH status Borrowed→Reserved (invalid)** → 409.
- **PATCH status Borrowed→Available** → 200.
- **Borrower POST /api/bookcopies** → 403 (role enforcement).
- **GET without JWT** → 401.

## Known Limitations / TODO
1. `Status` serializes as integer (0=Available, etc.) because of `HasConversion<string>` in EF but JSON outputs underlying enum value — clients see `0`/`1`. Consider `StringEnumConverter` or DTO string mapping.
2. `GetCopiesAsync` used by `GetCopy` was replaced by dedicated `GetCopyAsync` — verified.
3. No unit/integration tests (deferred per CLAUDE §15).
4. JWT `Key` still in `appsettings.json` (dev only).
5. Physical inventory seeding: books 1 was archived in Milestone 4 testing; its copies (LIB-0001, LIB-0002) were cascade-deleted. Remaining seeded copies: LIB-0003 (book 2), LIB-0004 (book 3).
