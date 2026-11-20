# FLOWCHARTS.md — Milestone 16: Final Review, Documentation, and Demonstration Preparation

## Overall System Architecture

```mermaid
flowchart TD
    subgraph Android["Android App (Kotlin + Compose)"]
        A[Login Screen] --> B[Catalog Screen]
        B --> C[Book Detail Screen]
        B --> D[My Books Screen]
        B --> E[Reservations Screen]
        B --> F[Reading Lists Screen]
        B --> G[Notifications Screen]
        B --> H[Profile Screen]
        A -->|JWT token| I[SharedPreferences secure storage]
        B -->|Room cache| J[(Local Room DB - cache only)]
    end

    subgraph WebAPI["ASP.NET Core Web API (.NET 10)"]
        K[AuthController - login/JWT]
        L[BooksController - catalog]
        M[BookCopiesController - inventory]
        N[LoansController - checkout/return]
        O[ReservationsController - FIFO queue]
        P[ReadingListsController - personal lists]
        Q[NotificationsController - in-app notifications]
        R[OpenLibraryController - external metadata]
    end

    subgraph MVC["ASP.NET Core MVC (Admin/Librarian UI)"]
        S[Dashboard - real SQL stats]
        T[Catalog - books/copies]
        U[Circulation - checkout/return]
        V[Reservations - queue management]
        W[Reports - SQL counts]
        X[Hangfire Dashboard - Admin only]
    end

    subgraph Services["Application Services (shared)"]
        Y[BookService]
        Z[BookCopyService]
        AA[LoanService - transactions]
        BB[ReservationService - FIFO + allocation]
        CC[NotificationService]
        DD[ReadingListService]
    end

    subgraph DB["SQL Server - authoritative"]
        EE[(AspNetUsers/Roles)]
        FF[(Books/BookCopies)]
        GG[(Loans/Reservations)]
        HH[(ReadingLists/Items)]
        II[(Notifications)]
        JJ[(Hangfire tables)]
    end

    Android -->|Retrofit/OkHttp JWT| WebAPI
    WebAPI --> Services
    Services --> EF[Entity Framework Core] --> DB
    MVC --> Services --> EF --> DB
    Hangfire[Hangfire recurring jobs] --> Services --> DB
```

## Authentication & Authorization Flow

```mermaid
flowchart TD
    A[Android login: email + password] --> B[POST /api/Auth/login]
    B --> C[Validate credentials via Identity]
    C -->|Valid| D[Generate JWT (120 min, roles)]
    D -->|Invalid| E[401 Unauthorized]
    C -->|Valid| D
    D --> F[Store JWT in SharedPreferences]
    F --> G[All API calls: Authorization: Bearer <token>]
    G --> H[JWT Bearer validation on server]
    H -->|Role check| I[Authorize attribute per endpoint]
    H -->|Valid| J[Return data]
    H -->|Invalid| K[401/403]

    subgraph MVC["MVC Cookie Auth"]
        L[Login form + antiforgery] --> M[Identity sign-in]
        M --> N[Cookie session]
        N --> O[Role-based [Authorize] on MVC controllers]
    end
```

## Circulation Transaction Flow (Checkout)

```mermaid
flowchart TD
    A[POST /api/loans/checkout] --> B[DbTransaction begins]
    B --> C[Validate user exists]
    C --> D[Validate book exists]
    D --> E[Check duplicate active loan same title]
    E -->|Duplicate| F[409 Conflict - rollback]
    E -->|OK| G[Check active loan count < 3]
    G -->|Limit reached| H[409 Conflict - rollback]
    G -->|OK| I[Find available copy (order by accession)]
    I -->|None| J[409 Conflict - rollback]
    I -->|Found| K[Re-fetch copy with update lock, re-check status]
    K -->|Not available| L[409 Conflict - rollback]
    K -->|Available| M[copy.Status = Borrowed, create Loan]
    M --> N[SaveChanges + Commit]
    N --> O[Return loan response]
```

## Reservation Queue Flow (First-Come, First-Served)

```mermaid
flowchart TD
    A[POST /api/reservations] --> B[DbTransaction begins]
    B --> C[Check duplicate active reservation same book+user]
    C -->|Duplicate| D[409 Conflict - rollback]
    C -->|OK| E[Create reservation Status=Queued, PickupDeadline=+2 days]
    E --> F[Calculate queuePosition = earlier queued + 1]
    F --> G[SaveChanges + Commit]
    
    G --> H[Copy returned → POST /api/loans/return]
    H --> I[copy.Status = Available]
    I --> J[AllocateReturnedCopyAsync(bookId, copyId)]
    J --> K[Find next Queued reservation ordered by ReservedAt]
    K -->|None| L[Copy stays Available]
    K -->|Found| M[copy.Status = Reserved, reservation → ReadyForPickup]
    M --> N[Create notification: ready for pickup]
```

## Overdue Notification Job (Hangfire)

```mermaid
flowchart TD
    A[Cron: daily 8:00 AM] --> B[OverdueNotificationJob.ExecuteAsync]
    B --> C[Find active loans with DueDate < now]
    C -->|None| D[Log: no overdue loans, complete]
    C -->|Found| E[For each loan: check notification exists]
    E -->|Exists| F[Skip - prevent duplicate]
    E -->|Not exists| G[CreateNotificationAsync: overdue loan message]
    G --> H[Log completion]
```

## Reservation Expiration Job (Hangfire)

```mermaid
flowchart TD
    A[Cron: every 15 minutes] --> B[ReservationExpirationJob.ExecuteAsync]
    B --> C[ReservationService.ProcessExpiredReservationsAsync]
    C --> D[Find reservations with PickupDeadline < now]
    D -->|None| E[Complete]
    D -->|Found| F[ExpireReservationAsync per reservation]
    F --> G[Status → Expired, release allocated copy to Available]
    G --> H[Create notification: reservation expired]
    H --> I[AllocateNextAsync: give copy to next queued borrower]
    I --> J[Complete]
```

## Demonstration Workflow

```mermaid
flowchart TD
    A[Start: dotnet run --project src/Sinop.Web] --> B[Android app installed on emulator]
    B --> C[Librarian logs in (web or API)]
    C --> D[View catalog: The Pragmatic Programmer, To Kill a Mockingbird]
    D --> E[Librarian checks out copy LIB-0003 to borrower]
    E --> F[Borrower logs in Android]
    F --> G[Borrower sees active loan in My Books]
    G --> H[Borrower reserves book id=2]
    H --> I[Reservation created, queuePosition=1]
    I --> J[Librarian processes return]
    J --> K[Copy → Available, reservation queue allocates]
    K --> L[Borrower notified: ready for pickup]
    L --> M[Hangfire dashboard: Administrator views recurring jobs]
```

## Acceptance Criteria — Final

| Criterion | Status |
|---|---|
| Backend builds successfully | ✓ 0 errors |
| Android builds successfully | ✓ BUILD SUCCESSFUL, APK produced |
| SQL Server database operational | ✓ Migrations + seeding on startup |
| Seeded accounts work | ✓ All 3 roles verified |
| MVC authentication works | ✓ Cookie auth + antiforgery + roles |
| Android authentication works | ✓ JWT login + bearer on all API calls |
| Swagger functional | ✓ `/swagger/index.html` with JWT |
| Catalog management works | ✓ List/search/details/create/update/archive |
| Physical inventory works | ✓ Accession numbers, status, unique constraint |
| Borrowing and returns work | ✓ Transactions, limits, duplicate prevention |
| Reservations work | ✓ FIFO queue, expiration, allocation |
| Reading lists work | ✓ CRUD, add/remove, ownership |
| Notifications work | ✓ In-app, duplicate prevention |
| Hangfire jobs execute | ✓ Registered: overdue daily 8AM, expiration every 15 min |
| Android caching works | ✓ Room cache, offline-first |
| All apps communicate | ✓ E2E scenarios A-F verified |
