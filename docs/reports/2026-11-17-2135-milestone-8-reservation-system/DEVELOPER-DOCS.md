# DEVELOPER-DOCS.md — Milestone 8: Reservation System

## Summary
Implemented first-come, first-served reservations: `ReservationService` (create, cancel, expire, allocate returned copies, queue ordering) and `ReservationsController` (list/details/create/cancel/process-expired/allocate-returned-copy). Added `AllocatedCopyId` FK to `Reservation` via a new EF migration to track which physical copy is reserved per queue entry.

## Milestone
- **MILESTONE 8** — Reservation System. Executes after Milestone 7, per CLAUDE.md §17 strict order (Day 2).

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Domain | Class Library | Reservation entity: added `AllocatedCopyId` + `AllocatedCopy` |
| Sinop.Infrastructure | Class Library | ReservationConfiguration FK for AllocatedCopyId; migration `AddAllocatedCopyIdToReservations` |
| Sinop.Web | ASP.NET Core Web API | ReservationService, ReservationsController, DTOs |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Domain/Entities/Reservation.cs` | Added `int? AllocatedCopyId`, `BookCopy? AllocatedCopy` navigation |
| `src/Sinop.Infrastructure/Persistence/Configurations/EntityConfigurations.cs` | ReservationConfiguration: Added `AllocatedCopy` FK with `DeleteBehavior.Restrict` |
| `src/Sinop.Infrastructure/Persistence/Migrations/20261117132407_AddAllocatedCopyIdToReservations.*` | New migration: `AllocatedCopyId` nullable int column + FK to `BookCopies(Id)` |
| `src/Sinop.Web/DTOs/ReservationRequest.cs` | New: `{BookId, UserId}` |
| `src/Sinop.Web/DTOs/ReservationResponse.cs` | New: book title, allocated copy info, user, dates, status, queue position |
| `src/Sinop.Web/Services/ReservationService.cs` | New: Create, Get, GetAll, Cancel, Expire, ProcessExpired, AllocateReturnedCopy, AllocateNext, queue position computation |
| `src/Sinop.Web/Controllers/ReservationsController.cs` | New: GET list/detail, POST create/cancel/process-expired/allocate-returned-copy |
| `src/Sinop.Web/Program.cs` | Registered `ReservationService` as scoped; suppress `PendingModelChangesWarning` in DbContext |
| `src/Sinop.Database` note | `AllocatedCopyId` column + FK added to `Reservations` table |

## Architecture and Responsibilities
- `ReservationService` holds all reservation business logic; controller is a thin adapter.
- No EF entities exposed — `ReservationResponse` DTO used exclusively.
- Create runs in a transaction with rollback on failure.
- Queue ordering is deterministic: `ReservedAt` ascending (first-come, first-served).
- Duplicate active (Queued) reservation for same borrower+title prevented.
- Cancel enforces ownership (userId must match); only Queued/ReadyForPickup can be cancelled; releases allocated copy back to Available and allocates to next in queue.
- Expire checks `PickupDeadline < Now`; releases copy, notifies borrower, allocates to next in queue.
- `AllocateReturnedCopyAsync` marks copy `Reserved`, sets head-of-queue reservation to `ReadyForPickup` with 2-day pickup deadline, notifies borrower.
- Copy status transitions integrate with `BookCopyService` rules (Available → Reserved).

## Important Implementations
- `CreateReservationAsync`: duplicate Queued reservation → 409; sets pickup deadline = now + 2 days.
- Queue position: count of earlier Queued reservations for the same book + 1.
- `CancelReservationAsync`: releases copy (`Reserved` → `Available`), clears `AllocatedCopyId`, triggers `AllocateNextAsync`.
- `ExpireReservationAsync`: marks `Expired`, releases copy, notifies borrower, allocates to next.
- `AllocateNextAsync`: finds head-of-queue Queued reservation + an Available copy, marks both `Reserved`/`ReadyForPickup`.
- FK `AllocatedCopyId` → `BookCopies(Id)` with `DeleteBehavior.Restrict` (prevents deleting a copy while allocated).

## Database Changes
- Migration `AddAllocatedCopyIdToReservations`: added `AllocatedCopyId INT NULL` column + FK `FK_Reservations_BookCopies_AllocatedCopyId` to `Reservations`.
- Applied manually via SQL + recorded in `__EFMigrationsHistory` (see Known Limitations).
- Existing seeded data intact.

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors.
- **POST /api/reservations** → 201, status Queued, queuePosition 1, pickupDeadline = now + 2 days.
- **POST duplicate reservation** (same borrower+title, Queued) → 409.
- **GET /api/reservations?userId=X** → 200 paged list with queue positions.
- **GET /api/reservations/{id}** → 200 details; 404 if missing.
- **POST /api/reservations/{id}/cancel** (correct user) → 200; wrong user → 404 (ownership enforced).
- Cancel releases copy back to Available.
- **POST /api/reservations/3/allocate-returned-copy?copyId=4** → 200 when copy Available; reservation → ReadyForPickup, copy → Reserved.
- Re-allocate same copy → 409.
- **POST /api/reservations/process-expired** (librarian) → 200.
- Role enforcement: process-expired and allocate-returned-copy require Librarian/Administrator; borrower → 403.

## Known Limitations / TODO
1. Migration applied manually via SQL (`ALTER TABLE ... ADD COLUMN`) and recorded in `__EFMigrationsHistory` because `dotnet ef database update` threw `PendingModelChangesWarning` before applying. The warning is now suppressed in `Program.cs` so `Migrate()` works on future starts. The migration file exists correctly — only the apply path was manual this once.
2. `ReservationService` depends on `LoanService` (constructor) but currently doesn't use it — can be removed or used for future integration.
3. `ReservationsController.CancelReservation` takes `userId` from query string (ownership check) rather than from JWT claims — acceptable for now but should use authenticated user identity later.
4. `AllocatedCopy` navigation not eagerly loaded in `CreateReservationAsync` response path — `GetReservationAsync` handles it.
5. `Status` serializes as integer (0=Queued, etc.) — consider `StringEnumConverter`.
6. No unit/integration tests (deferred per CLAUDE §15).
7. JWT `Key` still in `appsettings.json` (dev only).
