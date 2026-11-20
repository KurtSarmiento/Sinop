# Sinop Digital Library and Reading Management System

A full-stack library management system built with .NET 10, ASP.NET Core (MVC + Web API), Entity Framework Core, SQL Server, Hangfire, and Kotlin + Jetpack Compose (Android).

## Architecture

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core Web API (.NET 10) |
| Admin UI | ASP.NET Core MVC (Razor) |
| Business Logic | Application Services (shared) |
| Data Access | Entity Framework Core + SQL Server |
| Background Jobs | Hangfire (SQL Server storage) |
| Mobile | Kotlin + Jetpack Compose (Android) |
| Local Cache | Room Database |

## Solution Structure

```
Sinop.sln
├── src/Sinop.Domain          # Domain entities, enums
├── src/Sinop.Infrastructure  # EF Core DbContext, migrations, external services, jobs
├── src/Sinop.Web             # MVC + Web API, DTOs, services, Program.cs
└── AndroidApp/               # Kotlin Android application
```

## Quick Start

### Backend

1. Install .NET 10 SDK and SQL Server (LocalDB or compatible).
2. From the solution root:
   ```bash
   export windir=/c/Windows
   dotnet build Sinop.sln
   dotnet run --project src/Sinop.Web --urls "http://localhost:5288"
   ```
3. The API listens on `http://localhost:5288`. Swagger is at `/swagger/index.html`.
4. Database migrations and seeding run automatically on startup.

### Seeded Demo Accounts

| Email | Password | Role |
|---|---|---|
| admin@library.local | Password123! | Administrator |
| librarian@library.local | Password123! | Librarian |
| borrower@library.local | Password123! | Borrower |

### Android

1. Install JDK 17 and Android SDK (compileSdk 34).
2. From `AndroidApp/`:
   ```bash
   export JAVA_HOME="/path/to/jdk-17"
   ./gradlew assembleDebug
   ```
3. Install `app/build/outputs/apk/debug/app-debug.apk` on an emulator or device.
4. The app connects to `http://10.0.2.2:5288` (emulator loopback to host).

## Core Features

- **Authentication**: JWT for Android, cookie auth for MVC; role-based authorization (Administrator, Librarian, Borrower).
- **Catalog**: Search, browse, book details, availability; Open Library import for librarians.
- **Inventory**: Physical copies with unique accession numbers, status management.
- **Circulation**: Librarian checkout/return, borrowing limits, overdue detection.
- **Reservations**: First-come-first-served queue, pickup deadlines, expiration, allocation.
- **Reading Lists**: Personal collections with add/remove items.
- **Notifications**: In-app, duplicate prevention, overdue and reservation alerts.
- **Background Jobs**: Overdue notifications (daily 8:00 AM), reservation expiration (every 15 min).
- **Android**: Login, catalog, book details, My Books, Reservations, Reading Lists, Notifications, Profile; Room offline cache.

## Documentation

Per-milestone reports are in `docs/reports/<date>-<milestone>/` with `DEVELOPER-DOCS.md`, `HOW-TO-USE.md`, and `FLOWCHARTS.md`.

## Development Standards

- Clean Architecture / separation of concerns; no business logic in controllers or views.
- EF Core Fluent API for all schema configuration; migrations only (no manual table changes).
- DTOs for all API contracts; no EF entities exposed over REST.
- Transactions for circulation operations; database constraints for key invariants.
- No hardcoded secrets; configuration via environment/user-secrets.
- Android: MVVM, Retrofit/OkHttp, Coroutines, StateFlow, Room, Navigation Compose, Coil.
