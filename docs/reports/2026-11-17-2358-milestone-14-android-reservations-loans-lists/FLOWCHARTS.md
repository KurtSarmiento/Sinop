# FLOWCHARTS.md — Milestone 14: Android Reservations, Loans, and Reading Lists

## My Books (Active Loans) Flow

```mermaid
flowchart TD
    A[MyBooksScreen composes] --> B[LaunchedEffect: get userId from LoginUseCase]
    B --> C{userId non-empty?}
    C -->|No| D[Show empty state]
    C -->|Yes| E[GetActiveLoansUseCase.invoke(userId)]
    E --> F[LibraryRepository.getActiveLoans → Retrofit GET /api/loans/active?userId=...]
    F --> G[Map LoanResponse → domain Loan]
    G --> H[LazyColumn renders loan cards]
    H --> I[Show book title, accession number, due date, overdue status]
```

## Reservations Flow

```mermaid
flowchart TD
    A[ReservationsScreen composes] --> B[Load reservations via GetReservationsUseCase]
    B --> C[Load books via GetBooksUseCase for picker]
    C --> D[LazyColumn: list reservations with status, queue position, pickup deadline]
    D --> E[Tap Cancel → CancelReservationUseCase.invoke(id, userId)]
    E --> F[Refresh reservations list]
    D --> G[Tap FAB → show create dialog]
    G --> H[Select book from searchable list]
    H --> I[Tap Reserve → CreateReservationUseCase.invoke(bookId, userId)]
    I --> J[Refresh reservations list]
```

## Reading Lists Flow

```mermaid
flowchart TD
    A[ReadingListsScreen composes] --> B[Load reading lists via GetReadingListsUseCase]
    B --> C[LazyColumn: list lists with name, itemCount]
    C --> D[Tap list → show items with remove buttons]
    D --> E[Tap Remove → RemoveReadingListItemUseCase.invoke(listId, itemId)]
    E --> F[Refresh lists]
    C --> G[Tap Create List → dialog with name/description]
    G --> H[Tap Create → CreateReadingListUseCase.invoke(name, description)]
    H --> I[Refresh lists]
    D --> J[Open list detail → tap Add Item]
    J --> K[Select book → AddReadingListItemUseCase.invoke(listId, bookId)]
    K --> L[Refresh list items]
```

## Notifications Flow

```mermaid
flowchart TD
    A[NotificationsScreen composes] --> B[Get userId from LoginUseCase]
    B --> C[GetNotificationsUseCase.invoke(userId)]
    C --> D[LibraryRepository.getNotifications → Retrofit GET /api/notifications?userId=...]
    D --> E[Map NotificationResponse → domain Notification]
    E --> F[LazyColumn renders notifications]
    F --> G[Unread notifications highlighted with primary container background]
```

## Profile Flow

```mermaid
flowchart TD
    A[ProfileScreen composes] --> B[Get email from LoginUseCase.getEmail()]
    B --> C[Get role from AuthRepository.authState.value]
    C --> D[Display email and role]
    D --> E[Tap Logout → LoginUseCase.logout() → navigate to login]
```

## JWT UserId Parsing Flow

```mermaid
flowchart TD
    A[Login success → token stored in SharedPreferences] --> B[getUserId() called]
    B --> C[Split JWT on '.', take payload part]
    C --> D[Base64 URL-decode payload]
    D --> E[Parse JSON for nameidentifier claim]
    E --> F[Return userId GUID]
    F --> G[Use userId for loans/reservations/notifications endpoints]
```

## Navigation Flow

```mermaid
flowchart TD
    A[SinopActivity NavHost] --> B[route "login"]
    A --> C[route "home"]
    A --> D[route "catalog"]
    A --> E[route "bookdetail/{bookId}"]
    A --> F[route "mybooks"]
    A --> G[route "reservations"]
    A --> H[route "readinglists"]
    A --> I[route "notifications"]
    A --> J[route "profile"]
    F -->|onLogout| B
    G -->|onLogout| B
    H -->|onLogout| B
    I -->|onLogout| B
    J -->|onLogout| B
```
