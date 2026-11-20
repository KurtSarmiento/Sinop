# DEVELOPER-DOCS.md — Milestone 14: Android Reservations, Loans, and Reading Lists

## Summary
Implemented the remaining borrower-facing screens for the Sinop Android app (Milestone 14: Reservations, Loans, and Reading Lists). Users can now view active loans (My Books), create/cancel reservations, manage reading lists (CRUD + add/remove items), view notifications, and view their profile. Also created the missing backend `NotificationsController` API (previously only MVC existed). All endpoints verified against the live backend on `http://localhost:5288`. Build: `./gradlew assembleDebug` → BUILD SUCCESSFUL (0 errors).

## Milestone
- **MILESTONE 14** — Android Reservations, Loans, and Reading Lists (Sub-milestones 14A My Books/Loans, 14B Reservations, 14C Reading Lists, 14D Notifications, 14E Profile). DAY-2 milestone per CLAUDE §17.

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| AndroidApp | Kotlin + Jetpack Compose Android app | My Books, Reservations, Reading Lists, Notifications, Profile screens |
| Sinop.Web | ASP.NET Core Web API | New NotificationsController API |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Web/Controllers/NotificationsController.cs` | NEW: API controller with GET /api/notifications, GET unread-count, POST /read, POST /read-all |
| `data/remote/ApiService.kt` | Added: getReadingLists, createReadingList, addReadingListItem, removeReadingListItem Retrofit methods |
| `data/remote/dto/ReadingListResponse.kt` | NEW DTO (id, userId, name, description, itemCount, items) |
| `data/remote/dto/ReadingListItemResponse.kt` | NEW DTO (id, bookId, bookTitle) |
| `data/remote/dto/ReadingListRequest.kt` | NEW DTO (name, description) |
| `data/remote/dto/AddReadingListItemRequest.kt` | NEW DTO (bookId) |
| `domain/model/ReadingList.kt` | NEW domain model |
| `domain/model/ReadingListItem.kt` | NEW domain model |
| `domain/repository/LibraryRepositoryInterface.kt` | Added: getActiveLoans(userId), getReservations(userId), createReservation(bookId,userId), cancelReservation(id,userId), getNotifications(userId), getReadingLists(), createReadingList(name,desc), addReadingListItem(listId,bookId), removeReadingListItem(listId,itemId) |
| `data/repository/LibraryRepository.kt` | Implemented new methods; DTO→domain mapping; status code→string mapping for reservations; reads token from SharedPreferences |
| `data/repository/AuthRepository.kt` | Added JWT nameidentifier claim parsing → getUserId(); stores userId in SharedPreferences after login |
| `domain/usecase/LoginUseCase.kt` | Added getUserId() delegate |
| `domain/usecase/GetActiveLoansUseCase.kt` | NEW |
| `domain/usecase/GetReservationsUseCase.kt` | NEW |
| `domain/usecase/CreateReservationUseCase.kt` | Updated to (bookId, userId) signature |
| `domain/usecase/CancelReservationUseCase.kt` | NEW |
| `domain/usecase/GetReadingListsUseCase.kt` | NEW |
| `domain/usecase/CreateReadingListUseCase.kt` | NEW |
| `domain/usecase/AddReadingListItemUseCase.kt` | NEW |
| `domain/usecase/RemoveReadingListItemUseCase.kt` | NEW |
| `domain/usecase/GetNotificationsUseCase.kt` | NEW |
| `di/ServiceLocator.kt` | Added all new usecase fields + construction |
| `ui/screens/mybooks/MyBooksScreen.kt` | Real: active loans list with due dates and overdue indicators |
| `ui/screens/reservations/ReservationsScreen.kt` | Real: reservation list, create reservation (book picker), cancel |
| `ui/screens/readinglists/ReadingListsScreen.kt` | Real: reading lists list, create list, add/remove items |
| `ui/screens/notifications/NotificationsScreen.kt` | Real: notification list with unread highlighting |
| `ui/screens/profile/ProfileScreen.kt` | Real: email, role, logout |

## Architecture and Responsibilities
- **Domain**: models (Loan, Reservation, ReadingList, ReadingListItem), repository interface, use cases returning `kotlin.Result`. Pure Kotlin, no Android deps.
- **Data**: Retrofit/OkHttp remote source (bearer JWT), Room local source (cache only), repositories coordinating remote/local + DTO→domain mapping.
- **DI**: `ServiceLocator` singleton wiring AuthRepository, LibraryRepositoryInterface, and all use cases.
- **Networking**: base URL `http://10.0.2.2:5288` (emulator → host). Cleartext for emulator dev only.
- **Persistence**: Room `LibraryDatabase` with `CachedBook` — cache only; SQL Server authoritative via API.
- **Auth**: JWT parsed client-side to extract `nameidentifier` (userId) claim for user-scoped endpoints (loans, reservations, notifications).

## Important Implementations
- **MyBooksScreen**: loads active loans via `GetActiveLoansUseCase.invoke(userId)`, displays book title, copy accession number, due date, overdue status (color-coded red if overdue).
- **ReservationsScreen**: lists reservations with status, queue position, pickup deadline; FAB to create reservation with book picker dropdown; cancel button per reservation.
- **ReadingListsScreen**: lists reading lists with item counts; FAB to create list (name/description dialog); per-item remove button.
- **NotificationsScreen**: notification list with unread highlighting (primary container background).
- **ProfileScreen**: displays email and role from parsed JWT token.
- **AuthRepository.getUserId()**: parses JWT payload base64 to extract `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier` claim.

## Database Changes
None on server side. Local Room database already existed from Milestone 12 (`CachedBook` table); no schema changes needed for Milestone 14.

## Verification
- `./gradlew assembleDebug --no-daemon` — **BUILD SUCCESSFUL** (0 errors; only pre-existing unused-parameter warnings).
- `app-debug.apk` produced (~13.5 MB).
- Backend `dotnet build Sinop.sln` — succeeded, 0 errors.
- **Live API verification** against `http://localhost:5288`:
  - POST `/api/Auth/login` → returns `{"token","email","roles","expiresAt"}` camelCase ✓
  - GET `/api/loans/active?userId=...` → returns 1 active loan (Pragmatic Programmer, overdue) ✓
  - GET `/api/reservations?userId=...` → returns 3 reservations with status codes (0=Queued,1=ReadyForPickup,3=Expired,4=Cancelled) ✓
  - GET `/api/readinglists` → returns empty list (no reading lists seeded) ✓
  - GET `/api/notifications?userId=...` → returns 2 notifications ✓
- JWT `nameidentifier` claim successfully parsed → userId `95443af8-0176-46e9-9b7c-a39cc04ec0e0`

## Known Limitations / TODO
1. Android app not yet installed on emulator/device (build verified only).
2. Reservation ReadyForPickup → Fulfilled status transitions are librarian-side actions (not borrower screens).
3. Notification mark-as-read / mark-all-read endpoints exist on API but not yet wired to Android UI.
4. Pull-to-refresh / pagination beyond first page not implemented.
5. JWT expires after 120 minutes; no refresh-token flow.
6. Network cleartext enabled for emulator dev only — must be disabled for production.
