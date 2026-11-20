# HOW-TO-USE.md — Milestone 16: Final Review, Documentation, and Demonstration Preparation

## Prerequisites
- .NET 10 SDK (`dotnet --version` → 10.0.x)
- SQL Server (LocalDB or compatible)
- JDK 17 and Android SDK (compileSdk 34)
- Working directory: `C:\Users/Kurt Sarmiento/Sinop`

## Step-by-Step Instructions

### 1. Build the entire solution
```bash
export windir=/c/Windows
dotnet build Sinop.sln
```
Expected: 0 errors (NuGet vulnerability warnings for pre-existing packages are informational only).

### 2. Build the Android APK
```bash
export JAVA_HOME="/c/Users/Kurt Sarmiento/.jdks/jbr-17.0.14"
export PATH="$JAVA_HOME/bin:$PATH"
export GRADLE_HOME="/c/Users/Kurt Sarmiento/.gradle/wrapper/dists/gradle-8.7-bin/bhs2wmbdwecv87pi65oeuq5iu/gradle-8.7"
cd AndroidApp
"$GRADLE_HOME/bin/gradle" assembleDebug --no-daemon
```
Expected: BUILD SUCCESSFUL. APK at `AndroidApp/app/build/outputs/apk/debug/app-debug.apk`.

### 3. Run the backend
```bash
export windir=/c/Windows
dotnet run --project src/Sinop.Web --urls "http://localhost:5288"
```
API listens on `http://localhost:5288`. Swagger at `/swagger/index.html`. Database migrations and seeding run automatically on startup.

### 4. Install and run the Android app
```bash
adb install AndroidApp/app/build/outputs/apk/debug/app-debug.apk
```
Launch "Sinop Library" on the emulator/device. Connects to `http://10.0.2.2:5288`.

### 5. Seeded demo accounts
| Email | Password | Role |
|---|---|---|
| admin@library.local | Password123! | Administrator |
| librarian@library.local | Password123! | Librarian |
| borrower@library.local | Password123! | Borrower |

### 6. Demonstration workflow
1. Backend running on port 5288.
2. Librarian logs in → views catalog, copies, loans, reservations via MVC or API.
3. Borrower logs in (Android) → views catalog, book details, My Books, Reservations, Reading Lists, Notifications, Profile.
4. Librarian checks out a book → borrower sees it in My Books.
5. Borrower reserves a book → librarian sees it in Reservations.
6. Librarian processes a return → inventory updates, reservation queue allocates copy.
7. Hangfire dashboard at `/hangfire` (Administrator only) → view recurring jobs.

### 7. Review documentation
- `README.md` — project overview, quick start, architecture, seeded accounts
- `docs/INDEX.md` — index of all 16 milestone reports
- `docs/reports/<date>-<milestone>/` — per-milestone DEVELOPER-DOCS.md, HOW-TO-USE.md, FLOWCHARTS.md

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| `dotnet build` file locked | Backend process running | Kill dotnet/Sinop.Web process, retry |
| 401 on API call | Missing/expired JWT | Re-authenticate via `/api/Auth/login` |
| Android build fails | JDK not 17 or JAVA_HOME unset | Set JAVA_HOME to JDK 17 |
| `adb install` fails | Emulator/device not connected | Start emulator, enable USB debugging |
| Open Library search fails | External API timeout | Sandbox network restriction; local catalog works |
