# HOW-TO-USE.md — Milestone 14: Android Reservations, Loans, and Reading Lists

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

### 2. Start the backend API (required for all borrower features)
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
Launch "Sinop Library". Log in with:
- borrower@library.local / Password123! (Borrower)

### 5. My Books (active loans)
Tap "My Books" in the navigation drawer. Shows active loans with:
- Book title, copy accession number, due date
- Overdue indicator (red text if past due date)

### 6. Reservations
Tap "Reservations". Shows your reservations with status, queue position, and pickup deadline.
- **Create**: Tap the FAB (+), select a book from the searchable list, tap "Reserve".
- **Cancel**: Tap "Cancel" on any reservation card.

### 7. Reading Lists
Tap "Reading Lists". Shows your reading lists with item counts.
- **Create**: Tap "Create List", enter name and optional description.
- **Add item**: Open a list, select a book from your catalog, it gets added.
- **Remove item**: Tap "Remove" next to any item in a list.

### 8. Notifications
Tap "Notifications". Shows your in-app notifications with unread highlighting.

### 9. Profile
Tap "Profile". Shows your email and role from the JWT token. Tap "Logout" to sign out.

## Network Configuration
- API base URL: `http://10.0.2.2:5288` (Android emulator loopback to host).
- JWT bearer auth is sent on every borrower request via `Authorization: Bearer <token>`.
- Cleartext traffic permitted for emulator dev only via `network_security_config.xml`. Do NOT ship to production.

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| Login returns error | Wrong credentials or backend not running | Start `dotnet run --project src/Sinop.Web`; use seeded accounts |
| My Books empty | No active loans for user or backend unreachable | Verify backend on 5288; check user has loans in DB |
| Reservations create fails | Book not selected or backend conflict | Select a book first; check book isn't already reserved |
| Reading Lists empty | No lists created yet | Tap "Create List" to add one |
| Notifications empty | No notifications for user | Backend generates notifications on loan/reservation events |
| App crashes on DI (ClassCastException) | Old LibraryRepository not implementing interface | Rebuild — fixed in Milestone 14 |
