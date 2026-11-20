# DEVELOPER-DOCS.md — Milestone 4: Book Catalog Web API

## Summary
Implemented the Book Catalog Web API (`BooksController`) with GET list/details/search/pagination, POST create, PUT update, DELETE/archive, role-based authorization, and centralized exception handling. DTOs were already created in Milestone 3.

## Milestone
- **MILESTONE 4** — Book Catalog Web API. Executes after Milestone 3, per CLAUDE.md §17 strict order.

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Web | ASP.NET Core Web API + MVC | BooksController, ExceptionMiddleware, DI/pipeline wiring |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Web/Controllers/BooksController.cs` | New: GET books (paged/search), GET book/{id}, POST create, PUT update, DELETE/archive; role-based `[Authorize(Roles="Librarian,Administrator")]` on mutating endpoints; JWT Bearer scheme on all endpoints |
| `src/Sinop.Web/Middleware/ExceptionMiddleware.cs` | New: centralized exception handler returning `application/problem+json` (500) |
| `src/Sinop.Web/Program.cs` | Added `UseMiddleware<ExceptionMiddleware>()`; `using Sinop.Web.Middleware;` |

## Architecture and Responsibilities
- Controllers only bind/validate, call `BookService`, return HTTP responses (per CLAUDE §7 / MVC controller standard).
- No EF entities exposed — all responses use `BookResponse`/`BookDetailsResponse` DTOs from Milestone 3.
- `BookService` (Milestone 3) holds all business logic; controller is a thin adapter.
- Role enforcement: only `Librarian`/`Administrator` may create/update/archive; `Borrower` gets 403.
- JWT Bearer is the auth scheme for API endpoints (Cookie remains for MVC).
- Centralized errors: `InvalidOperationException` → 409 Conflict; missing resource → 404; invalid model → 400; unhandled → 500 via middleware.
- Pagination clamped: page ≥ 1, pageSize 1–100.
- Async I/O with `CancellationToken` throughout.

## Important Implementations
- `POST /api/books` returns `201 Created` with `Location` header (`CreatedAtAction`).
- Duplicate ISBN → `409 Conflict` with message.
- Archive refuses (409) when active loans or queued reservations exist (§5.9 invariant).
- Archive of a book with no active loans/queued reservations succeeds (204) — copies cascade-delete per Fluent API config.
- Swagger (Swashbuckle) documents all endpoints with `Bearer` security requirement.

## Database Changes
None (schema already created in Milestone 1). Verification writes deleted book id 4 (test-created) and seeded book id 1 + its copies (LIB-0001, LIB-0002) via archive testing — seeded reference data was mutated during verification; re-seeding requires restoring those rows if a clean demo state is needed.

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors.
- **GET /api/books** (authenticated) → 200 paged result with 3 seeded books.
- **GET /api/books?search=clean** → 1 item (search works).
- **GET /api/books/1** → 200 with details + copies (LIB-0001, LIB-0002).
- **GET /api/books/999** → 404.
- **POST /api/books** new book → 201 with body.
- **POST /api/books** duplicate ISBN → 409.
- **PUT /api/books/4** → 200 updated.
- **POST /api/books** with Borrower token → 403 (role enforcement).
- **DELETE /api/books/4** → 204.
- **DELETE /api/books/1** → 204 (mutated seed data).
- **GET /api/books** without JWT → 401.
- Swagger JSON → 200.

## Known Limitations / TODO
1. Archive test deleted seeded book id 1 and copies LIB-0001/LIB-0002 — demo seed data mutated; consider re-seeding or making archive safer (prevent archive when copies exist).
2. `Status` serializes as integer (0 = Available) because of `HasConversion<string>` but JSON outputs the enum's underlying value — clients see `0`/`1`/etc. rather than names. Consider `StringEnumConverter` or DTO mapping to string.
3. No `GetBooks` filter by category/author yet (search only by title/ISBN).
4. JWT `Key` still in `appsettings.json` (dev only).
5. No unit/integration tests yet (deferred per CLAUDE §15).
