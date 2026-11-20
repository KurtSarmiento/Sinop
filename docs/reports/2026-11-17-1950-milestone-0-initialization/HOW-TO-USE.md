# HOW-TO-USE.md — Milestone 0: Project Initialization

## Prerequisites
- .NET 10 SDK installed (verified: 10.0.403 at `C:\Program Files\dotnet`)
- SQL Server available for later milestones (LocalDB `MSSQLLocalDB` verified reachable)
- A working `ProgramData`/`ALLUSERSPROFILE`/`windir` environment in the shell (see Troubleshooting #1 — this is a machine issue, not a project issue)

## Step-by-Step Instructions

### 1. Build the solution
```powershell
dotnet build Sinop.sln
```

### 2. Restore packages
```powershell
dotnet restore Sinop.sln
```

### 3. Verify project references
- `Sinop.Infrastructure` → `Sinop.Domain`
- `Sinop.Web` → `Sinop.Domain`, `Sinop.Infrastructure`

### 4. Confirm target framework
All three projects target `net10.0` with nullable reference types enabled.

## Examples
Open `Sinop.sln` in Visual Studio or run from the solution root:
```powershell
dotnet build Sinop.sln
```

## Troubleshooting

**1. `Value cannot be null. (Parameter 'path1')` during restore/build**
- Cause: this shell lacks `ProgramData`/`ALLUSERSPROFILE`/`windir` env vars; NuGet crashes reading machine-wide config.
- Affects even new console projects outside this solution — it is an environment issue.
- Fix: set `ProgramData=C:\ProgramData`, `ALLUSERSPROFILE=C:\ProgramData`, `windir=C:\windows` in your shell, then rerun `dotnet build Sinop.sln`.

**2. `NETSDK1045: The current .NET SDK does not support targeting .NET 10.0`**
- Cause: .NET SDK 9 is active instead of SDK 10.
- Fix: ensure SDK 10.0.403 is the active SDK (no `global.json` pinning to 9.0).

**3. Build still fails after setting env vars**
- A local `NuGet.Config` exists at repo root pointing to nuget.org; confirm network access to `https://api.nuget.org/v3/index.json`.
