# HOW-TO-USE.md — Milestone 10: Hangfire and Background Jobs

## Prerequisites
- .NET 10 SDK (`dotnet --version` → 10.0.403).
- SQL Server LocalDB `(localdb)\MSSQLLocalDB` with `SinopLibraryDb` seeded (Milestone 1).
- `export windir=/c/Windows` in bash.
- Working directory: `C:\Users\Kurt Sarmiento\Sinop`.

## Step-by-Step Instructions

### 1. Build
```bash
export windir=/c/Windows
dotnet build Sinop.sln
```

### 2. Run
```bash
dotnet run --project src/Sinop.Web
```
On startup, Hangfire creates its SQL Server storage tables (in `HangFire` schema) and registers recurring jobs. The app listens on `http://localhost:5288`.

### 3. Hangfire Dashboard (administrators only)
Open `http://localhost:5288/hangfire` in a browser. Log in as `admin@library.local` / `Password123!` first (or send a valid JWT). Only users in the `Administrator` role can access the dashboard.

### 4. Recurring jobs
| Job | Cron | Purpose |
|---|---|---|
| `overdue-notification` | Daily 8:00 AM | Finds active loans past due date, creates non-duplicate overdue notifications |
| `reservation-expiration` | Every 15 minutes | Expires Queued reservations past pickup deadline, releases copies, allocates to next in queue |

Jobs run automatically via the Hangfire server; no manual trigger needed. To test manually, invoke the job logic through the service layer or add a temporary admin endpoint.

### 5. Example: trigger overdue notifications (manual)
```bash
TOKEN=$(curl -s -X POST http://localhost:5288/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@library.local","password":"Password123!"}' | \
  python3 -c "import sys,json; print(json.load(sys.stdin)['token'])")
```
Then call whatever admin job-trigger endpoint exists, or wait for the recurring schedule.

### 6. Verify jobs ran
Check the `HangFire.[Set]` table for recurring job entries, and `HangFire.[Job]`/`[State]` for processed jobs:
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -d SinopLibraryDb -Q "SELECT * FROM [HangFire].[Set];"
```

## Background Job Details
| Job | Trigger | Idempotency | Action |
|---|---|---|---|
| OverdueNotificationJob | Daily 8:00 AM | Skips if notification for that loan already exists | Creates overdue notification per past-due active loan |
| ReservationExpirationJob | Every 15 min | Expires each reservation once (status → Expired) | Releases copy, notifies borrower, allocates to next queued borrower |

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| Hangfire tables not created | App hasn't started with Hangfire registered | Run `dotnet run --project src/Sinop.Web` once |
| Dashboard 401 | Not authenticated or not Administrator | Log in as admin@library.local |
| Job not executing | Hangfire server not running | Ensure `UseHangfireServer()` is called (in Program.cs) |
| Overdue notification duplicate | Idempotency check | By design — only one notification per overdue loan |
| Build file locked | `dotnet run` holding dll | Kill dotnet process, retry |
| `Cannot open database SinopLibraryDb` | DB not created | `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE SinopLibraryDb;"` then re-run |
