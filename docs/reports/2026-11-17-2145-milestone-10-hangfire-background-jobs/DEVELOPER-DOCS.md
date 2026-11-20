# DEVELOPER-DOCS.md — Milestone 10: Hangfire and Background Jobs

## Summary
Implemented Hangfire with SQL Server persistent storage, two recurring background jobs (`OverdueNotificationJob` daily at 8:00 AM, `ReservationExpirationJob` every 15 minutes), and an administrator-restricted Hangfire Dashboard. Jobs delegate to existing application services and are idempotent/logged.

## Milestone
- **MILESTONE 10** — Notifications and Hangfire (Sub-milestones 10A Notifications [done in Milestone 3], 10B Hangfire Setup, 10C Background Jobs). Executes after Milestone 9, per CLAUDE.md §17 strict order (Day 3).

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Web | ASP.NET Core Web API | Hangfire registration, jobs, dashboard authorization filter |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Web/Jobs/OverdueNotificationJob.cs` | New: finds active loans past due date, creates non-duplicate overdue notifications, logs execution |
| `src/Sinop.Web/Jobs/ReservationExpirationJob.cs` | New: calls `ReservationService.ProcessExpiredReservationsAsync()` to expire reservations past pickup deadline |
| `src/Sinop.Web/Jobs/HangfireDashboardAuthorizationFilter.cs` | New: restricts Hangfire Dashboard to authenticated `Administrator` users |
| `src/Sinop.Web/Program.cs` | Added `AddHangfire` (SQL Server storage), `AddHangfireServer`, `UseHangfireDashboard` (admin-only), registered recurring jobs, registered job classes as scoped, `PendingModelChangesWarning` already suppressed |
| `src/Sinop.Web/Services/ReservationService.cs` | Fixed `ProcessExpiredReservationsAsync` to query by `PickupDeadline < Now` (independent of enum status conversion) |
| `src/Sinop.Web/Sinop.Web.csproj` | Added `Hangfire` 1.8.13, `Hangfire.SqlServer` 1.8.13 |

## Architecture and Responsibilities
- Hangfire uses **SQL Server persistent storage** (`UseSqlServerStorage`) — jobs survive restarts.
- Jobs delegate to existing application services (`NotificationService`, `ReservationService`) — no duplicated business logic.
- Jobs registered as scoped services and instantiated by Hangfire's activator.
- **OverdueNotificationJob**: idempotent — skips loans that already have an overdue notification message for that loan ID; logs counts.
- **ReservationExpirationJob**: delegates to `ProcessExpiredReservationsAsync` which expires Queued reservations past `PickupDeadline`, releases copies, notifies borrowers, allocates to next in queue.
- **Hangfire Dashboard**: `/hangfire`, restricted to `Administrator` role via `IDashboardAuthorizationFilter`.
- Recurring jobs: `overdue-notification` (`Cron.Daily(8,0)`), `reservation-expiration` (`Cron.MinuteInterval(15)`).

## Important Implementations
- `OverdueNotificationJob.ExecuteAsync`: queries `Loans` where `Status == Active && DueDate < Now`, includes `BookCopy.Book`, creates one notification per overdue loan (deduped by message content).
- `ReservationExpirationJob.ExecuteAsync`: calls `ProcessExpiredReservationsAsync` (already implemented in Milestone 8).
- `ProcessExpiredReservationsAsync` fixed: queries by `PickupDeadline < Now` without filtering on `Status` enum (avoids `HasConversion<string>` LINQ translation issues).
- Hangfire tables created automatically in `HangFire` schema: `Job`, `JobParameter`, `JobQueue`, `Set`, `List`, `State`, `Counter`, `AggregatedCounter`, `Hash`.

## Database Changes
- Hangfire tables auto-created by `UseSqlServerStorage` in `HangFire` schema on first app start.
- `__EFMigrationsHistory` unchanged (Hangfire storage is separate from EF migrations).

## Verification
- `dotnet build Sinop.sln` — succeeded, 0 errors (pre-existing NU1902/NU1903 warnings only; one CS0618 obsolete warning for `Cron.MinuteInterval`).
- **Hangfire dashboard** (`/hangfire`) without auth → 401 (restricted).
- **Overdue job** (via temp endpoint): created notification for overdue loan 3 — message `Your overdue loan 3 for 'The Pragmatic Programmer' (copy LIB-0003) was due on 2026-11-16. Please return it.`
- **Expiration job** (via temp endpoint): expired reservation 3 (status → `Expired`), notification count incremented.
- Recurring jobs registered in `HangFire.[Set]` table: `overdue-notification`, `reservation-expiration`.

## Known Limitations / TODO
1. **Temp test controller removed** (`TempJobTestController`) — used only for verification; no permanent manual job-trigger endpoint exists yet (recurring jobs run on schedule).
2. `Cron.MinuteInterval(15)` is obsolete (CS0618) — should migrate to a CRON expression like `"0 */15 * * *"` before production.
3. `ReservationService` constructor takes `LoanService` but doesn't use it — can be removed or used later.
4. **No unit/integration tests** for jobs (deferred per CLAUDE §15).
5. **JWT `Key` still in `appsettings.json`** (dev only).
6. Overdue job does not change loan status (only notifies) — status change could be added if required.
7. Catalog sync job (`CatalogSyncJob`) not implemented — optional per CLAUDE §10.3 and marked secondary in §18 priorities.
