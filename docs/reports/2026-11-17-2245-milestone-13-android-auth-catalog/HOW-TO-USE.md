# HOW-TO-USE.md — Milestone 13: Android Authentication and Catalog

## Prerequisites
- JDK 17 (`JAVA_HOME` set to a JDK 17 installation).
- Gradle 8.7 wrapper (vendored in `AndroidApp/gradle/wrapper`).
- Android SDK with compileSdk 34 / targetSdk 34 / minSdk 24.
- Backend Web API running on `http://localhost:5288` (reachable from emulator at `10.0.2.2:5288`).
- Seeded database with demo accounts (Milestone 1): borrower@library.local / Password123!.
- Working directory: `C:\Users/Kurt Sarmiento/Sinop/AndroidApp`.

## Step-by-Step Instructions

### 1. Build the debug APK
```bash
export JAVA_HOME="/c/Users/Kurt Sarmiento/.jdks/jbr-17.0.14"
export PATH="$JAVA_HOME/bin:$PATH"
export GRADLE_HOME="/c/Users/Kurt Sarmiento/.gradle/wrapper/dists/gradle-8.7-bin/bhs2wmbdwecv87pi65oeuq5iu/gradle-8.7"
"$GRADLE_HOME/bin/gradle" assembleDebug --no-daemon
```

### 2. Start the backend API (required for catalog/login)
```bash
export windir=/c/Windows
dotnet run --project src/Sinop.Web --urls "http://localhost:5288"
```
API listens on `http://localhost:5288`. Swagger at `/swagger/index.html`.

### 3. Install on emulator/device
```bash
adb install app/build/outputs/apk/debug/app-debug.apk
```

### 4. Run the app
Launch "Sinop Library". If you have no saved token, the login screen appears. Log in with:
- borrower@library.local / Password123! (Borrower)
- admin@library.local / Password123! (Administrator)
- librarian@library.local / Password123! (Librarian)

On success, the app navigates to the catalog. Returning users with a saved token skip the login screen automatically.

### 5. Browse the catalog
- Type a search term (or leave empty) and tap **Search**.
- Books display title, authors, ISBN, copy count, and availability (green if copies available, red if none).
- Tap a book to view its details (title, ISBN, category, authors, summary, copies available).
- Tap **Logout** in any top bar to clear the token and return to login.

### 6. Offline browsing
- After at least one successful catalog search, books are cached in Room.
- With network disabled, reopening the catalog shows the previously cached books (offline-first).

## Network Configuration
- API base URL: `http://10.0.2.2:5288` (Android emulator loopback to host).
- JWT bearer auth is sent on every catalog/books request via `Authorization: Bearer <token>`.
- Cleartext traffic permitted for emulator dev only via `network_security_config.xml`. Do NOT ship to production.

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| Login returns "Invalid credentials" | Wrong email/password or backend not running | Verify backend on 5288; use seeded accounts |
| Catalog shows no data / error | Backend unreachable or no cached data | Start `dotnet run --project src/Sinop.Web`; ensure emulator reaches 10.0.2.2:5288 |
| `GradleWrapperMain` missing | Wrapper jar not present | Copy from Gradle 8.7 distribution into `gradle/wrapper/` |
| Build hangs / daemon lock | Gradle daemon stuck | `$GRADLE_HOME/bin/gradle --stop`, then retry |
| Offline shows nothing | No prior successful cache write | Search once while online to populate Room cache |
| App crashes on DI (ClassCastException) | Old LibraryRepository not implementing interface | Rebuild — fixed in this milestone |
