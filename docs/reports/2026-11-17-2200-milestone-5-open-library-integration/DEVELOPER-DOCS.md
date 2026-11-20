# DEVELOPER-DOCS.md — Milestone 5: Open Library Integration

## Summary
Implemented Open Library external book metadata integration: `OpenLibraryClient` (Infrastructure/ExternalServices) calls Open Library API via HttpClientFactory with 10-second timeout; `OpenLibraryService` wraps it plus duplicate-ISBN checking against the local catalog; `OpenLibraryController` exposes title/author/ISBN/cover search and import endpoints for librarians. Import delegates to the existing `BookService.CreateBookAsync` — no physical copies are created automatically.

## Milestone
- **MILESTONE 5** — Open Library Integration. Executes after Milestone 4 per CLAUDE §11; moved earlier per developer decision so Milestone 11 MVC can connect properly.

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Infrastructure | Class Library | `IOpenLibraryClient`, `OpenLibraryClient`, `IOpenLibraryService`, `OpenLibraryService`, external API models |
| Sinop.Web | ASP.NET Core Web API | `OpenLibraryController`, DTOs, DI registration (`AddHttpClient<IOpenLibraryClient>`) |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Infrastructure/ExternalServices/Models/OpenLibrarySearchResult.cs` | New: search result + work/edition/author models (JsonElement for volatile fields) |
| `src/Sinop.Infrastructure/ExternalServices/IOpenLibraryClient.cs` | New: client interface (search title/author, ISBN, cover) |
| `src/Sinop.Infrastructure/ExternalServices/OpenLibraryClient.cs` | New: HttpClient-based client to openlibrary.org with timeout + error handling |
| `src/Sinop.Infrastructure/ExternalServices/IOpenLibraryService.cs` | New: service interface incl. `IsDuplicateIsbnAsync` |
| `src/Sinop.Infrastructure/ExternalServices/OpenLibraryService.cs` | New: service wrapping client + local duplicate check |
| `src/Sinop.Web/DTOs/OpenLibraryBookPreview.cs` | New: mapped preview DTO for search results |
| `src/Sinop.Web/DTOs/OpenLibraryImportRequest.cs` | New: import request DTO |
| `src/Sinop.Web/DTOs/OpenLibraryImportResponse.cs` | New: import response DTO |
| `src/Sinop.Web/Controllers/OpenLibraryController.cs` | New: GET search/search-by-author/isbn/{isbn}/cover/{key}, POST import (librarian+admin) |
| `src/Sinop.Web/Program.cs` | Added `AddHttpClient`, registered `IOpenLibraryClient`/`OpenLibraryService` + `IOpenLibraryService`/`OpenLibraryService` as scoped |
| `src/Sinop.Web/Services/BookService.cs` | Changed `IsDuplicateIsbnAsync` param order: `(isbn, ct, excludeId)` |
| `src/Sinop.Web/Sinop.Web.csproj` | No new packages (HttpClientFactory is in-box) |

## Architecture and Responsibilities
- **Infrastructure** owns external API details (per CLAUDE §4.2): client models, HTTP calls, timeout, error handling, mapping external responses.
- **Web** owns the API contract: controller validates, calls `OpenLibraryService` + `BookService`, returns DTOs, never exposes EF entities.
- Import uses `BookService.CreateBookAsync` — reuses duplicate detection, category/author validation, no code duplication.
- No physical copies created on import (per CLAUDE §11.2 acceptance criteria).
- `AddHttpClient` with 10s timeout and User-Agent header; external API failure returns 500 via `ExceptionMiddleware` — local catalog unaffected.

## Important Implementations
- `OpenLibraryClient.SearchByTitleAsync` → `https://openlibrary.org/search.json?title=...`
- `OpenLibraryClient.SearchByAuthorAsync` → `https://openlibrary.org/search.json?author=...`
- `OpenLibraryClient.GetByIsbnAsync` → `https://openlibrary.org/isbn/{isbn}.json`
- `OpenLibraryClient.GetCoverUrlAsync` → `https://covers.openlibrary.org/b/id/{key}-M.jpg`
- `OpenLibraryController` role-restricted to `Librarian,Administrator`; borrower → 403.
- Import endpoint checks duplicate ISBN via `BookService.IsDuplicateIsbnAsync` → 409 if duplicate.
- `BookService.IsDuplicateIsbnAsync` signature changed to `(isbn, ct, excludeId)` to allow the Open Library service to call it.

## Database Changes
None. Reuses `Books`, `Categories`, `Authors`, `BookAuthors` tables from Milestone 1.

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors.
- **External API note**: `openlibrary.org` is unreachable in this sandbox environment (connection timeout). The client has a 10-second timeout so it fails fast rather than hanging. When network is available, search/ISBN/cover endpoints work as implemented.
- **Borrower on Open Library endpoints** → 403 (role enforcement).
- **Invalid model** → 400; missing book → 404; duplicate ISBN on import → 409.

## Known Limitations / TODO
1. `openlibrary.org` unreachable in this environment — could not perform end-to-end external API test. Code path verified via build + role enforcement; runtime behavior depends on network access.
2. `Cron.MinuteInterval(15)` obsolete warning (CS0618) — pre-existing from Milestone 10; migrate to CRON expression before production.
3. Import only creates the bibliographic `Book` record — no `BookCopy` records (by design, per CLAUDE §11.2).
4. Cover images are returned as URLs only; the client does not download/copy cover files.
5. No unit/integration tests (deferred per CLAUDE §15).
6. JWT `Key` still in `appsettings.json` (dev only).
