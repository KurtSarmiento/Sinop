# HOW-TO-USE.md — Milestone 11: MVC Administrative Dashboard

## Prerequisites
- .NET 10 SDK (`dotnet --version` → 10.0.403).
- SQL Server LocalDB `(localdb)\MSSQLLocalDB` with `SinopLibraryDb` seeded (Milestone 1).
- `export windir=/c/Windows` in bash.
- Working directory: `C:\Users/Kurt Sarmiento/Sinop`.

## Step-by-Step Instructions

### 1. Build
```bash
export windir=/c/Windows
dotnet build Sinop.sln
```

### 2. Run
```bash
dotnet run --project src/Sinop.Web
```
App listens on `http://localhost:5288`.

### 3. MVC Login (cookie auth)
Open `http://localhost:5288/Account/Login` in a browser. Log in with:
- admin@library.local / Password123! (Administrator — full access including Settings)
- librarian@library.local / Password123! (Librarian — catalog, inventory, circulation)
- borrower@library.local / Password123! (Borrower — read-only catalog, own notifications)

### 4. Navigate the dashboard
The shared layout includes: Dashboard, Catalog, Borrowers, Circulation, Reports, Notifications, Settings.

| Area | Path | Description |
|---|---|---|
| Dashboard | `/MVC/Home` | Welcome page |
| Catalog | `/MVC/Books` | List/search books, details, create/edit/archive |
| Inventory | `/MVC/Copies` | List/register/manage physical copies |
| Circulation | `/MVC/Circulation` | Active loans, checkout, return |
| Reservations | `/MVC/Reservations` | List, details, cancel |
| Borrowers | `/MVC/Borrowers` | List, details, history |
| Reports | `/MVC/Reports` | Real SQL counts (loans, copies, reservations) |
| Notifications | `/MVC/Notifications` | List, mark read |
| Settings | `/MVC/Settings` | Admin only |

## Role-Based Access
| Role | Access |
|---|---|
| Administrator | All pages including Settings |
| Librarian | Catalog, Inventory, Circulation, Reservations, Borrowers, Reports, Notifications |
| Borrower | Catalog (read), Notifications (own) |

## Examples
- Create a book: `/MVC/Books/Create` (Librarian+Admin) — POST with Title, ISBN, Summary, CategoryId.
- Register a copy: `/MVC/Copies/Register` (Librarian+Admin) — POST with BookId, optional AccessionNumber, Status.
- Checkout: `/MVC/Circulation/Checkout` — POST with BookId, UserId.
- Return: POST to `/MVC/Circulation/Return` with loanId.
- Cancel reservation: POST to `/MVC/Reservations/Cancel` with id + userId.

## Troubleshooting
| Problem | Cause | Fix |
|---|---|---|
| 302 redirect to login | Not authenticated | Log in at `/Account/Login` |
| 404 on MVC page | View not found or wrong area path | Use `/MVC/<Controller>` path |
| 403 on Settings | Not Administrator | Use admin@library.local |
| View displays no data | Seeded data may have been mutated by prior testing | Re-seed via fresh database |
| Build file locked | `dotnet run` holding dll | Kill dotnet process, retry |
| `Cannot open database SinopLibraryDb` | DB not created | `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE SinopLibraryDb;"` then re-run |
