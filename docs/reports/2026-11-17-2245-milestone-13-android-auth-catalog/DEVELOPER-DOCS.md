# DEVELOPER-DOCS.md — Milestone 13: Android Authentication and Catalog

## Summary
Implemented the core borrower experience for the Sinop Android app (Milestone 13: Authentication and Catalog). Users can log in with JWT, view a real catalog of books from the API, see book details, and browse cached books offline via Room. Fixed two foundational bugs blocking the data layer: the `LibraryRepository`/`LibraryRepositoryInterface` mismatch (was throwing `ClassCastException` at DI time) and the missing `getBookDetails` API endpoint. Verified end-to-end against the live backend on `http://localhost:5288` — login, catalog, and book details all return camelCase JSON matching the Kotlin DTOs exactly (no `@SerializedName` needed since System.Text.Json uses `JsonNamingPolicy.CamelCase`).

## Milestone
- **MILESTONE 13** — Android Authentication and Catalog (Sub-milestones 13A Auth, 13B Home & Catalog, 13C Room Caching). DAY-2 milestone per CLAUDE §17.

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| AndroidApp | Kotlin + Jetpack Compose Android app | Authentication, catalog, book details, Room cache |

## Files Changed
| File | Change |
|---|---|
| `data/repository/LibraryRepository.kt` | Now implements `LibraryRepositoryInterface`; reads JWT token internally from SharedPreferences("sinop_prefs"); maps DTO→domain models (Book, Loan, Reservation, Notification); `getBooks` returns `Result<List<Book>>` |
| `domain/repository/LibraryRepositoryInterface.kt` | Adjusted signatures: removed `authHeader` param, added `getBookDetails(bookId)`, returns domain types |
| `domain/usecase/GetBooksUseCase.kt` | Simplified to `invoke(search: String?): Result<List<Book>>` (removed `authHeader`) |
| `domain/usecase/GetBookDetailsUseCase.kt` | NEW: `invoke(bookId: Int): Result<Book>` |
| `di/ServiceLocator.kt` | Removed bogus `as LibraryRepositoryInterface` cast; repo typed as interface; added `getBookDetailsUseCase` |
| `data/remote/ApiService.kt` | Added `@GET("api/books/{id}") getBookDetails(@Header authHeader, @Path id)` |
| `data/remote/dto/BookDetailsResponse.kt` | NEW DTO: BookDetailsResponse + BookCopySummary (status: Int matching C# CopyStatus enum) |
| `ui/screens/login/LoginScreen.kt` | Added `LaunchedEffect` auto-navigate if already authenticated (returning users skip login) |
| `ui/screens/catalog/CatalogScreen.kt` | Fixed usecase call (no authHeader); load cached books first (offline-first); cache on successful fetch; fall back to cache on API failure |
| `ui/screens/bookdetail/BookDetailScreen.kt` | Replaced placeholder with real implementation: fetches via GetBookDetailsUseCase, shows title/ISBN/summary/authors/category/copies/availability, loading + error states |

No files deleted. All pre-existing placeholder screens (MyBooks, Reservations, ReadingLists, Notifications, Profile) left as-is for Milestone 14.

## Architecture and Responsibilities
- **Domain**: models (Book, Loan, Reservation, Notification), repository interface, use cases returning `kotlin.Result`. Pure Kotlin, no Android deps.
- **Data**: Retrofit/OkHttp remote source (bearer JWT), Room local source (cache only), repositories coordinating remote/local + DTO→domain mapping.
- **DI**: `ServiceLocator` singleton wiring AuthRepository, LibraryRepositoryInterface, and all use cases.
- **UI**: Jetpack Compose + Material3, SinopTheme (deep navy / muted green / warm off-white / muted gold), Navigation Compose host in SinopActivity.
- **Networking**: base URL `http://10.0.2.2:5288` (emulator → host). Cleartext permitted for emulator dev only.
- **Persistence**: Room `LibraryDatabase` with `CachedBook` — cache only; SQL Server authoritative via API.

## Important Implementations
- **LoginScreen**: email/password fields, LoginUseCase, token stored in SharedPreferences via AuthRepository, auto-navigate if already authenticated, loading spinner, error display.
- **CatalogScreen**: search field, LazyColumn book cards with title/authors/ISBN/copies/availability (color-coded), offline-first via Room cache, cache-on-fetch, fallback to cache when API unreachable.
- **BookDetailScreen**: real book details (title, ISBN, summary, category, authors, copies, availability), loading + error states, back + logout.
- **ServiceLocator.init()**: fixed to return instance (was returning Unit); removed invalid cast.
- **LibraryRepository**: now implements `LibraryRepositoryInterface` with matching signatures; no more `ClassCastException` at DI time.

## Database Changes
None on server side. Local Room database already existed from Milestone 12 (`CachedBook` table); cacheBooks/cacheFlow now actively used by CatalogScreen.

## Verification
- `./gradlew assembleDebug --no-daemon` — **BUILD SUCCESSFUL** (0 errors; only pre-existing unused-parameter warnings).
- `app-debug.apk` produced (~13.4 MB).
- Backend `dotnet build Sinop.sln` — succeeded, 0 errors.
- **Live API verification** against `http://localhost:5288`:
  - POST `/api/Auth/login` with borrower@library.local / Password123! → returns `{"token","email","roles","expiresAt"}` camelCase ✓
  - GET `/api/books?page=1&pageSize=20` with Bearer token → returns 2 seeded books, camelCase fields matching BookResponse DTO ✓
  - GET `/api/books/2` with Bearer token → returns BookDetailsResponse with copies (status as int 0=Available) ✓
- Kotlin DTO field names match backend camelCase wire format exactly (System.Text.Json `JsonNamingPolicy.CamelCase` default) — no `@SerializedName` needed.

## Known Limitations / TODO
1. Placeholder screens deferred to Milestone 14: MyBooks (active loans), Reservations, ReadingLists, Notifications, Profile.
2. No auth interceptor auto-refresh on 401 beyond basic logout + navigation.
3. Search triggers fresh API fetch; no debounce/pull-to-refresh; pagination limited to first page.
4. Android app not yet installed on emulator/device (build verified only).
5. JWT expires after 120 minutes; no refresh-token flow implemented.
6. Network cleartext enabled for emulator dev only — must be disabled for production.
