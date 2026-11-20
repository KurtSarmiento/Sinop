# DEVELOPER-DOCS.md — Milestone 9: Reading Lists

## Summary
Implemented personal reading lists: `ReadingListService` (CRUD, add/remove items, ownership enforcement) and `ReadingListsController` (list/details/create/update/delete/add-item/remove-item). Uses entities created in Milestone 1 (`ReadingList`, `ReadingListItem`). Ownership is enforced via the JWT `nameid` claim — users cannot modify another user's lists.

## Milestone
- **MILESTONE 9** — Reading Lists. Executes after Milestone 8, per CLAUDE.md §17 strict order (Day 2).

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Web | ASP.NET Core Web API | ReadingListService, ReadingListsController, DTOs, DI registration |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Web/DTOs/ReadingListRequest.cs` | New: `{Name, Description}` |
| `src/Sinop.Web/DTOs/ReadingListResponse.cs` | New: list summary + items |
| `src/Sinop.Web/DTOs/ReadingListItemResponse.cs` | New: `{Id, BookId, BookTitle}` |
| `src/Sinop.Web/DTOs/AddReadingListItemRequest.cs` | New: `{BookId}` |
| `src/Sinop.Web/Services/ReadingListService.cs` | New: GetReadingListsAsync (paged, user-scoped), GetReadingListAsync, CreateReadingListAsync, UpdateReadingListAsync, DeleteReadingListAsync, AddItemAsync (duplicate check, book-exists check), RemoveItemAsync, ItemExistsAsync |
| `src/Sinop.Web/Controllers/ReadingListsController.cs` | New: GET list/detail, POST create, PUT update, DELETE delete, POST add-item, DELETE remove-item; userId from JWT `nameid` claim |
| `src/Sinop.Web/Program.cs` | Registered `ReadingListService` as scoped |

## Architecture and Responsibilities
- `ReadingListService` holds all reading-list business logic; controller is a thin adapter.
- No EF entities exposed — DTOs used exclusively.
- Ownership enforced at every access point: service queries filter by `UserId`, so users cannot read/update/delete another user's lists or items.
- Create runs in context (SaveChanges); add-item validates book exists and prevents duplicates.
- Async I/O with `CancellationToken` throughout.
- Role: any authenticated user (Borrower, Librarian, Administrator) may manage their own lists.

## Important Implementations
- `GetReadingListsAsync` is user-scoped — only returns lists belonging to the authenticated user.
- `AddItemAsync` rejects duplicates (book already in list) and missing books (409).
- `RemoveItemAsync` enforces ownership via join through `ReadingList.UserId`.
- `ItemExistsAsync` helper for duplicate checks.
- `ReadingListItem` → book title populated via `Include(rl => rl.Items).ThenInclude(i => i.Book)`.

## Database Changes
None. Reuses `ReadingLists` and `ReadingListItems` tables created in Milestone 1.

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors.
- **POST /api/readinglists** → 201, list created with userId from JWT.
- **GET /api/readinglists** → 200 paged, user's lists only.
- **POST /api/readinglists/1/items** add book → 200, item added with book title.
- **POST duplicate add** → 409.
- **GET /api/readinglists/1** → 200 with items.
- **PUT /api/readinglists/1** (owner) → 204.
- **DELETE /api/readinglists/1** (owner) → 204; after delete GET → 404.
- **Another user (admin) GET/DELETE list 1** → 404 (ownership enforced).
- **Unauthenticated** → 401.

## Known Limitations / TODO
1. `UserId` read from JWT `nameid` claim in controller; if token lacks claim → 401 Unauthorized.
2. No list-level duplicate name validation (two lists with same name allowed) — not required by spec.
3. `Status` not applicable here (no enums).
4. No unit/integration tests (deferred per CLAUDE §15).
5. JWT `Key` still in `appsettings.json` (dev only).
6. No "share list" or "public list" feature — not required by spec.
