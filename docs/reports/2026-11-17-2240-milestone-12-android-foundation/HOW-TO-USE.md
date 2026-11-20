# HOW-TO-USE.md — Milestone 12: Android Foundation and Networking

## Prerequisites
- JDK 17 (`JAVA_HOME` set to a JDK 17 installation, e.g. `/c/Users/Kurt Sarmiento/.jdks/jbr-17.0.14`).
- Gradle 8.7 wrapper (already vendored in `AndroidApp/gradle/wrapper`).
- Android SDK with compileSdk 34 / targetSdk 34 / minSdk 24.
- Backend Web API running on `http://localhost:5288` (reachable from emulator at `10.0.2.2:5288`).
- Working directory: `C:\Users/Kurt Sarmiento/Sinop/AndroidApp`.

## Step-by-Step Instructions

### 1. Build the debug APK
```bash
export JAVA_HOME="/c/Users/Kurt Sarmiento/.jdks/jbr-17.0.14"
export PATH="$JAVA_HOME/bin:$PATH"
export GRADLE_HOME="/c/Users/Kurt Sarmiento/.gradle/wrapper/dists/gradle-8.7-bin/bhs2wmbdwecv87pi65oeuq5iu/gradle-8.7"
"$GRADLE_HOME/bin/gradle" assembleDebug --no-daemon
```

### 2. Locate the APK
Output: `AndroidApp/app/build/outputs/apk/debug/app-debug.apk` (~13 MB).

### 3. Install on emulator/device
```bash
adb install app/build/outputs/apk/debug/app-debug.apk
```
Requires an emulator or device with USB debugging / emulator network access.

### 4. Run the app
Launch "Sinop Library" from the emulator launcher. The login screen appears first. On successful login, the app navigates to the catalog/home navigation host.

## Network Configuration
- API base URL: `http://10.0.2.2:5288` (Android emulator loopback to host machine port 5288).
- Cleartext traffic is permitted for the emulator dev build via `network_security_config.xml`. Do NOT ship this to production.
- JWT bearer auth is configured on `ApiClient`/`ApiService`; token storage flow is not yet wired (see Known Limitations).

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| `GradleWrapperMain` missing | Wrapper jar not present | Copy `gradle-wrapper.jar` from a Gradle 8.7 distribution into `gradle/wrapper/` |
| `compileDebugKotlin` fails on `topAppBarColors` | Experimental Material3 API | Add `@OptIn(ExperimentalMaterial3Api::class)` + `import kotlin.OptIn` to the composable |
| `visualTransformation = VisualTransformation.Password()` | Syntax error | Remove `visualTransformation`; use plain `singleLine = true` for password fields |
| `result.isSuccess` / `result.exceptionOrNull()` import fails | `kotlin.Result` members not importable | Access via `result.isSuccess` / `result.exceptionOrNull()` without importing `kotlin.Result` |
| Build hangs / daemon issues | Gradle daemon lock | `$GRADLE_HOME/bin/gradle --stop`, then retry |
| `Cannot open database` / connection refused | Backend not running | Start `dotnet run --project src/Sinop.Web` on port 5288 |
| APK installs but network calls fail | Emulator can't reach host | Verify `10.0.2.2:5288` is reachable from emulator; check Windows firewall |
