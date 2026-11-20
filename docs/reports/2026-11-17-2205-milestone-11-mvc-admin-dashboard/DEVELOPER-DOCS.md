# DEVELOPER-DOCS.md — Milestone 11: MVC Administrative Dashboard

## Summary
Implemented the librarian/admin-facing MVC dashboard with Areas/MVC routing. All pages display real SQL Server data via existing application services (no duplicated business logic). MVC uses cookie authentication; mutating endpoints use antiforgery validation; role-based authorization enforced per CLAUDE §6.3/§8.

## Milestone
- **MILESTONE 11** — MVC Administrative Dashboard (Sub-milestones 11A Layout & Navigation, 11B Dashboard, 11C Catalog, 11D Inventory, 11E Borrowers, 11F Circulation, 11G Reports & Notifications). Executes after Milestone 5 per developer decision; Milestone 5 (Open Library) was intentionally moved earlier so MVC connects properly.

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Web | ASP.NET Core MVC | MVC controllers (Areas/MVC), Razor views (Areas/MVC/Views), area routing |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Web/Controllers/MVC/BooksController.cs` | New: MVC catalog list/details/create/edit/archive (Librarian+Admin) |
| `src/Sinop.Web/Controllers/MVC/BorrowersController.cs` | New: borrower list/details/history |
| `src/Sinop.Web/Controllers/MVC/CirculationController.cs` | New: active loans, checkout, return |
| `src/Sinop.Web/Controllers/MVC/ReservationsController.cs` | New: reservation list/details/cancel |
| `src/Sinop.Web/Controllers/MVC/CopiesController.cs` | New: inventory list/details/register/status |
| `src/Sinop.Web/Controllers/MVC/ReportsController.cs` | New: inventory/circulation stats |
| `src/Sinop.Web/Controllers/MVC/NotificationsController.cs` | New: notification list/mark-read/mark-all-read |
| `src/Sinop.Web/Controllers/MVC/SettingsController.cs` | New: settings (Admin only) |
| `src/Sinop.Web/Controllers/HomeController.cs` | Moved to Controllers/MVC, `[Area("MVC")]`, dashboard index |
| `src/Sinop.Web/Program.cs` | Added area route `{area:exists}/...` before default route |
| `src/Sinop.Web/Areas/MVC/Views/{Books,Borrowers,Circulation,Reservations,Copies,Reports,Notifications,Settings,Home}/*.cshtml` | New: Razor views for all MVC pages |
| `src/Sinop.Web/Areas/MVC/Views/_ViewImports.cshtml` | New: DTOs + enums namespace imports |

## Architecture and Responsibilities
- MVC controllers live in `Areas/MVC` with `[Area("MVC")]` — no route conflicts with API controllers.
- Controllers call existing application services (BookService, LoanService, etc.) — no business logic in controllers (per CLAUDE §7 / MVC controller standard).
- Views are Razor using the shared `_Layout.cshtml` with consistent navigation (Dashboard, Catalog, Borrowers, Circulation, Reports, Notifications, Settings).
- Views display real SQL data (seeded books/copies/users + runtime data).
- Antiforgery validation on all POST forms (`[ValidateAntiForgeryToken]`).
- Destructive operations (archive, delete, return, cancel) use POST with confirmation.
- Role-based authorization: mutating endpoints require Librarian/Administrator; Settings requires Administrator.

## Important Implementations
- **Dashboard** (`/MVC/Home`): placeholder welcome page (stats via Reports page).
- **Catalog** (`/MVC/Books`): list with search, details, create/edit/archive.
- **Inventory** (`/MVC/Copies`): list with bookId/status filters, details, register (manual or auto accession), status change.
- **Circulation** (`/MVC/Circulation`): active loans, checkout (librarian), return (librarian).
- **Reservations** (`/MVC/Reservations`): list, details, cancel (ownership enforced).
- **Borrowers** (`/MVC/Borrowers`): list, details, history.
- **Reports** (`/MVC/Reports`): real counts from SQL (active loans, overdue, copies by status, pending reservations).
- **Notifications** (`/MVC/Notifications`): list, mark read, mark all read.

## Database Changes
None. Reuses `SinopLibraryDb` tables from Milestones 1–10.

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors.
- **All MVC views return 200** with real database data (seeded books, copies, users, loans, reservations).
- **Cookie login** works via `/Account/Login` with antiforgery token.
- **Role enforcement**: Settings (Admin-only) → 200 for admin, 404/redirect for librarian.
- **Catalog content** shows seeded books (Clean Code, Pragmatic Programmer, To Kill a Mockingbird).
- API controllers (non-area) unaffected — still work via JWT.

## Known Limitations / TODO
1. Dashboard stats are a placeholder welcome page — real stat tiles should query SQL (Reports page already does this).
2. Book edit/create doesn't include author selection (authors not editable in MVC yet — placeholder).
3. No category dropdown populated in create/edit forms (CategoryId select is empty).
4. MVC delete/archive uses TempData error messages rather than inline validation.
5. No unit/integration tests for MVC (deferred per CLAUDE §15).
6. JWT `Key` still in `appsettings.json` (dev only).
