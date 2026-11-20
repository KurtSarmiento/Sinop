# Sinop Project Documentation Index

## Milestone Reports

| Milestone | Report Directory | Focus |
|---|---|---|
| M0 — Project Initialization | `reports/2026-11-17-1950-milestone-0-initialization/` | Solution structure, project references, DI foundations |
| M1 — Domain & Database | `reports/2026-11-17-2045-milestone-1-domain-and-database/` | Entities, EF Core, migrations, seeding |
| M2 — Authentication | `reports/2026-11-17-2105-milestone-2-authentication-authorization/` | Identity, JWT, MVC cookie auth |
| M3 — Application Services | `reports/2026-11-17-2115-milestone-3-core-application-services/` | BookService, BookCopyService, BorrowerService, NotificationService |
| M4 — Catalog API | `reports/2026-11-17-2120-milestone-4-book-catalog-api/` | BooksController, DTOs, pagination, Swagger |
| M5 — Open Library | `reports/2026-11-17-2200-milestone-5-open-library-integration/` | External book metadata import |
| M6 — Physical Inventory | `reports/2026-11-17-2125-milestone-6-physical-inventory/` | BookCopies, accession numbers, copy status |
| M7 — Borrowing & Returns | `reports/2026-11-17-2130-milestone-7-borrowing-returns/` | LoanService, checkout, return |
| M8 — Reservation System | `reports/2026-11-17-2135-milestone-8-reservation-system/` | ReservationService, FIFO queue, expiration |
| M9 — Reading Lists | `reports/2026-11-17-2140-milestone-9-reading-lists/` | ReadingListService, CRUD, ownership |
| M10 — Hangfire Jobs | `reports/2026-11-17-2145-milestone-10-hangfire-background-jobs/` | Overdue + expiration jobs, SQL Server storage |
| M11 — MVC Dashboard | `reports/2026-11-17-2205-milestone-11-mvc-admin-dashboard/` | Librarian/admin Razor UI |
| M12 — Android Foundation | `reports/2026-11-17-2240-milestone-12-android-foundation/` | Gradle, Retrofit, Room, Compose, navigation |
| M13 — Android Auth & Catalog | `reports/2026-11-17-2245-milestone-13-android-auth-catalog/` | Login, catalog, book details, offline cache |
| M14 — Android Reservations/Lists | `reports/2026-11-17-2358-milestone-14-android-reservations-loans-lists/` | My Books, Reservations, Reading Lists, Notifications, Profile |
| M15 — E2E Integration | `reports/2026-11-18-0015-milestone-15-e2e-integration-testing/` | All 6 E2E scenarios verified against live API |

## Key Documents

- **`README.md`** — Project overview, quick start, architecture, seeded accounts
- **`CLAUDE.md`** — Master development guide (project standards, milestones, rules)

## Verification Summary

- Backend (`dotnet build Sinop.sln`): **0 errors**
- Android (`./gradlew assembleDebug`): **BUILD SUCCESSFUL**, APK produced
- All 6 E2E scenarios verified against live API on `http://localhost:5288`
