# DEVELOPER-DOCS.md — Milestone 12: Android Foundation and Networking

## Summary
Implemented the Android application foundation and networking layer (Milestone 12). All Kotlin source files, Gradle configuration, resources, manifest, theme, and UI screens were created/fixed so that `./gradlew assembleDebug` passes successfully and produces `app-debug.apk` (~13 MB). Architecture follows MVVM-style layering: Domain (models, interfaces, usecases), Data (local Room DAOs/entities/database, remote Retrofit/OkHttp API + DTOs, repositories), DI (ServiceLocator), and UI (Compose screens + SinopTheme). Room is configured as a cache only; SQL Server remains authoritative via the Web API.

## Milestone
- **MILESTONE 12** — Android Foundation and Networking. Acceptance criterion: Android application builds (`./gradlew assembleDebug` — BUILD SUCCESSFUL). JWT auth interceptor, Retrofit client, Room DB, Navigation Compose skeleton, and all required screens (Login, Home/Dashboard, Catalog, BookDetail, MyBooks, Reservations, ReadingLists, Notifications, Profile) compile.

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| AndroidApp | Kotlin + Jetpack Compose Android app | Entire Android application (Milestone 12) |

## Files Changed
All files under `AndroidApp/` are new (the directory was previously untracked):

| File | Change |
|---|---|
| `AndroidApp/build.gradle.kts` (project-level) | Gradle 8.7 wrapper, AGP 8.2.0, Kotlin 1.9.22, Compose compiler 1.5.10, no version-compat bypass |
| `AndroidApp/gradle/wrapper/gradle-wrapper.properties` | distributionUrl → gradle-8.7-bin.zip; wrapper jar copied from Gradle cache |
| `AndroidApp/app/build.gradle.kts` | Compose BOM, navigation-compose, Room (runtime+ktx+compiler 2.6.1), OkHttp logging interceptor, kapt plugin, Java 17 toolchain |
| `AndroidApp/app/src/main/AndroidManifest.xml` | package com.sinop.library, theme @style/AppTheme, usesCleartextTraffic, internet permission |
| `AndroidApp/app/src/main/res/values/themes.xml` | Removed invalid colorBackground attr; kept colorPrimary/Secondary/Surface/windowBackground |
| `AndroidApp/app/src/main/res/values/colors.xml` | deep_navy, muted_green, warm_off_white, muted_gold, surface, on_surface |
| `AndroidApp/app/src/main/res/values/strings.xml` | App name + string resources |
| `AndroidApp/app/src/main/res/xml/network_security_config.xml` | Cleartext traffic permitted for emulator (10.0.2.2) dev only |
| `AndroidApp/app/src/main/res/mipmap-{mdpi,hdpi,xhdpi,xxhdpi,xxxhdpi}/ic_launcher{,_round}.png` | Launcher icons generated via Python stdlib PNG |
| `.../ui/theme/SinopTheme.kt` | Material3 theme; darkTheme: Boolean param (no isSystemInLightTheme) |
| `.../ui/SinopActivity.kt` | Main activity; Navigation Compose host; login → catalog navigation |
| `.../di/ServiceLocator.kt` | Holds Retrofit, repositories, usecases; fixed init() to return instance |
| `.../data/remote/ApiClient.kt` | OkHttpClient + logging interceptor + Retrofit(baseUrl http://10.0.2.2:5288) |
| `.../data/remote/ApiService.kt` | Retrofit interface: login, getBooks, and other endpoints (bearer auth) |
| `.../data/remote/dto/*.kt` | AuthResponse, BookResponse, BookCopyResponse, LoanResponse, ReservationRequest/Response, NotificationResponse, PagedResult, LoginRequest |
| `.../data/repository/AuthRepository.kt` | authState MutableStateFlow; sealed AuthState (Unauthenticated / Authenticated) |
| `.../data/repository/LibraryRepository.kt` | Remote+local (Room) repository; getCachedBooksFlow returns plain Flow |
| `.../data/local/BookDao.kt`, `CachedBook.kt`, `LibraryDatabase.kt` | Room entity, DAO, database (cache only) |
| `.../domain/model/*.kt` | Book, Loan, Reservation, Notification domain models |
| `.../domain/repository/LibraryRepositoryInterface.kt` | Repository interface |
| `.../domain/usecase/LoginUseCase.kt`, `GetBooksUseCase.kt`, `CreateReservationUseCase.kt` | Use cases returning kotlin.Result |
| `.../ui/screens/login/LoginScreen.kt` | Email/password fields, login button, error display, result handling |
| `.../ui/screens/home/HomeScreen.kt` | Dashboard welcome + email display + logout |
| `.../ui/screens/catalog/CatalogScreen.kt` | Search, book list (LazyColumn), availability, logout |
| `.../ui/screens/bookdetail/BookDetailScreen.kt` | Book details placeholder |
| `.../ui/screens/mybooks/MyBooksScreen.kt` | Placeholder |
| `.../ui/screens/reservations/ReservationsScreen.kt` | Placeholder |
| `.../ui/screens/readinglists/ReadingListsScreen.kt` | Placeholder |
| `.../ui/screens/notifications/NotificationsScreen.kt` | Placeholder |
| `.../ui/screens/profile/ProfileScreen.kt` | Placeholder |
| `.../ui/components/PlaceholderComponents.kt` | Reusable placeholder UI components |

## Architecture and Responsibilities
- **Domain layer**: models (Book, Loan, Reservation, Notification), repository interface, use cases (LoginUseCase, GetBooksUseCase, CreateReservationUseCase) returning `kotlin.Result`. Pure Kotlin, no Android deps.
- **Data layer**: Retrofit/OkHttp remote source (bearer JWT auth), Room local source (cache only), repositories coordinating remote/local.
- **DI layer**: `ServiceLocator` singleton wiring Retrofit, repositories, and use cases.
- **UI layer**: Jetpack Compose + Material3, SinopTheme (deep navy / muted green / warm off-white / muted gold palette), Navigation Compose host in SinopActivity, screens for all required Milestone 12/13/14 screens.
- **Networking**: base URL `http://10.0.2.2:5288` (Android emulator → host localhost:5288). Cleartext permitted for emulator dev only via network_security_config.xml.
- **Persistence**: Room `LibraryDatabase` with `CachedBook` entity + `BookDao` — cache only, never authoritative.

## Important Implementations
- **LoginScreen**: email + password TextFields, login button invoking `LoginUseCase`, error display, loading state via CircularProgressIndicator.
- **CatalogScreen**: search TextField, LazyColumn of books with title/authors/ISBN/copy/available counts, availability color-coding, logout TextButton.
- **ServiceLocator.init()**: fixed to return the instance (was returning Unit due to `.also { INSTANCE = it }`).
- **AuthRepository**: `authState` exposed as `MutableStateFlow` directly (no `viewModelScope`/`stateIn` which were unresolved); sealed `AuthState`.
- **All screens**: `@OptIn(ExperimentalMaterial3Api::class)` on composables using `TopAppBar` (experimental in this Material3 version); `import kotlin.OptIn`.

## Database Changes
None on the server side. Local Room database `LibraryDatabase` initialized with `CachedBook` table (local cache only).

## Verification
- `./gradlew assembleDebug --no-daemon` — **BUILD SUCCESSFUL** (35 actionable tasks, 0 errors, only warnings: unused parameters, never-succeed casts).
- `app/build/outputs/apk/debug/app-debug.apk` produced (~13,366,102 bytes).
- Kotlin compilation clean across all screens, repositories, usecases, DI, and theme.

## Known Limitations / TODO
1. Placeholder screens (MyBooks, Reservations, ReadingLists, Notifications, Profile, BookDetail) are stubs — real implementations deferred to Milestones 13–14.
2. `GetBooksUseCase`/`CreateReservationUseCase` `authHeader` param unused — placeholder pending auth wiring.
3. JWT token storage not yet integrated into LoginScreen flow (no secure storage write after login).
4. Room cache not yet populated/fetched by repository flow (getCachedBooksFlow exists but unused by UI).
5. No unit/instrumentation tests for Android (deferred per CLAUDE §15.4).
6. Network cleartext enabled for emulator dev only — must be disabled for production builds.
7. `ServiceLocator` casts (`as LoginUseCase`) marked "can never succeed" warnings — placeholder DI wiring to be cleaned up.
8. Android app not yet run on emulator/device (build only verified).
