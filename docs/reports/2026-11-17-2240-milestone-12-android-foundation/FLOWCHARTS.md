# FLOWCHARTS.md — Milestone 12: Android Foundation and Networking

## App Launch → Login → Navigation

```mermaid
flowchart TD
    A[App launch: SinopActivity.onCreate] --> B[NavigationCompose Host]
    B --> C{Authenticated?}
    C -->|No| D[LoginScreen]
    C -->|Yes| E[CatalogScreen / HomeScreen]
    D -->|Login success| F[ServiceLocator.loginUseCase → JWT token]
    F --> G[Navigate to catalog/home]
    D -->|Login failure| H[Show error message]
    H --> D
```

## Login Flow

```mermaid
flowchart TD
    A[User enters email + password] --> B[Tap Login]
    B --> C[scope.launch { LoginUseCase.invoke(email, password) }]
    C --> D{result.isSuccess?}
    D -->|Yes| E[onLoginSuccess() → navigate to catalog]
    D -->|No| F[error = result.exceptionOrNull()?.message]
    F --> G[Show error Text in red]
```

## Catalog Data Flow

```mermaid
flowchart TD
    A[CatalogScreen composes] --> B[Tap Search]
    B --> C[GetBooksUseCase.invoke(authHeader, query)]
    C --> D[LibraryRepository → Retrofit API call]
    D --> E[Web API /api/books → JSON PagedResult]
    E --> F[Map DTO → domain Book]
    F --> G[LazyColumn renders book cards]
    G --> H[Tap book → BookDetailScreen]
```

## Room Cache Flow (planned, not yet wired)

```mermaid
flowchart TD
    A[LibraryRepository.getCachedBooksFlow] --> B[BookDao.getAllCachedBooks]
    B --> C[Room: LibraryDatabase → CachedBook table]
    C --> D[Flow< List<CachedBook> >]
    D --> E[UI observes cached books offline]
    F[Remote fetch succeeds] --> G[BookDao.insertAll → refresh cache]
```

## Navigation Flow

```mermaid
flowchart TD
    A[SinopActivity NavHost] --> B[route "login"]
    A --> C[route "catalog"]
    A --> D[route "home"]
    A --> E[route "bookdetail/{bookId}"]
    A --> F[route "mybooks"]
    A --> G[route "reservations"]
    A --> H[route "readinglists"]
    A --> I[route "notifications"]
    A --> J[route "profile"]
    B -->|onLoginSuccess| C
    C -->|onBookClick| D
    Any -->|onLogout| B
```
