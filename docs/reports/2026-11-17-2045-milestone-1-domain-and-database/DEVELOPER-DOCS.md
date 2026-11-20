# DEVELOPER-DOCS.md — Milestone 1: Domain Entities and Database Foundation

## Summary
Implemented the complete initial data model for the Sinop Digital Library and Reading Management System. Created domain entities, enums, EF Core `LibraryDbContext`, Fluent API configurations, reference-data seeding, Identity integration, the initial migration, and applied it to SQL Server LocalDB. Seeded categories, authors, books, physical copies, and the three development accounts.

## Milestone
- **MILESTONE 1** — Domain Entities and Database Foundation (per CLAUDE.md §17, MILESTONES must execute in order; Milestone 0 was already complete).

## Projects Touched
| Project | Type | Responsibility |
|---|---|---|
| Sinop.Domain | Class Library (net10.0) | Domain entities + enums |
| Sinop.Infrastructure | Class Library (net10.0) | EF persistence, `LibraryDbContext`, Fluent API configs, migrations, seeding |
| Sinop.Web | ASP.NET Core Web API (net10.0) | Connection string, DbContext + Identity registration, startup seeding |

## Files Changed
| File | Change |
|---|---|
| `src/Sinop.Domain/Enums/CopyStatus.cs` | New: `Available, Borrowed, Reserved, Maintenance, Lost` |
| `src/Sinop.Domain/Enums/LoanStatus.cs` | New: `Active, Returned, Cancelled` |
| `src/Sinop.Domain/Enums/ReservationStatus.cs` | New: `Queued, ReadyForPickup, Fulfilled, Expired, Cancelled` |
| `src/Sinop.Domain/Entities/Book.cs` | New: bibliographic record entity |
| `src/Sinop.Domain/Entities/BookCopy.cs` | New: physical inventory entity |
| `src/Sinop.Domain/Entities/Author.cs` | New: author entity |
| `src/Sinop.Domain/Entities/Category.cs` | New: category entity |
| `src/Sinop.Domain/Entities/BookAuthor.cs` | New: book-author join entity |
| `src/Sinop.Domain/Entities/Loan.cs` | New: borrowing transaction entity |
| `src/Sinop.Domain/Entities/Reservation.cs` | New: reservation record entity |
| `src/Sinop.Domain/Entities/ReadingList.cs` | New: personal reading list entity |
| `src/Sinop.Domain/Entities/ReadingListItem.cs` | New: saved book entity |
| `src/Sinop.Domain/Entities/Notification.cs` | New: in-app notification entity |
| `src/Sinop.Domain/Sinop.Domain.csproj` | Unchanged (net10.0, nullable, implicit usings) |
| `src/Sinop.Infrastructure/Persistence/ApplicationUser.cs` | New: `ApplicationUser : IdentityUser` with `FullName` |
| `src/Sinop.Infrastructure/Persistence/LibraryDbContext.cs` | New: `IdentityDbContext<ApplicationUser>`, DbSets, `ApplyConfigurationsFromAssembly` |
| `src/Sinop.Infrastructure/Persistence/Configurations/EntityConfigurations.cs` | New: Fluent API configs + reference-data seeding (HasData) |
| `src/Sinop.Infrastructure/Persistence/Migrations/20261117...InitialCreate.cs` | New: initial EF Core migration |
| `src/Sinop.Infrastructure/Sinop.Infrastructure.csproj` | Added: `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore` |
| `src/Sinop.Web/Program.cs` | Added: DbContext + Identity registration, `Database.Migrate()`, role/user seeding |
| `src/Sinop.Web/appsettings.json` | Added: `DefaultConnection` SQL Server LocalDB connection string |
| `src/Sinop.Web/Sinop.Web.csproj` | Added: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design` |

## Architecture and Responsibilities
- **Domain** holds only entities + enums (zero external dependencies). Primitive FKs (`string UserId`, `int BookId`) keep Identity/EF out of Domain.
- **Infrastructure** owns `LibraryDbContext : IdentityDbContext<ApplicationUser>`, all Fluent API configuration (via `IEntityTypeConfiguration` classes applied through `ApplyConfigurationsFromAssembly`), EF migrations, and reference-data seeding via `HasData`.
- **Web** composes everything: registers DbContext + Identity, applies migrations at startup, seeds roles and the three demo accounts.
- Enums mapped via EF `HasConversion<string>()` for readable storage.
- Separate configuration classes per entity replace inline `OnModelCreating` blocks — no duplication, single responsibility.

## Important Implementations
- Enums: `CopyStatus`, `LoanStatus`, `ReservationStatus` per CLAUDE.md §5.5–5.7.
- Unique indexes: `Books.ISBN`, `BookCopies.AccessionNumber` (CLAUDE.md §5.9 uniqueness invariants).
- Cascade/restrict FK behavior: Book→Copies cascade, Loan→Copy restrict (prevents orphaned loans on copy deletion).
- Seeding: 3 categories, 3 authors, 3 books (with category links), 4 copies (LIB-0001…LIB-0004), 3 book-author links.
- Identity: roles Administrator/Librarian/Borrower; accounts admin@library.local, librarian@library.local, borrower@library.local (Password123!).
- Migration `InitialCreate` created all 18 tables including `__EFMigrationsHistory` and all AspNetIdentity tables.
- Startup calls `context.Database.Migrate()` then seeds roles/users idempotently.

## Database Changes
- Created `SinopLibraryDb` on `(localdb)\MSSQLLocalDB`.
- Tables: `__EFMigrationsHistory`, `AspNetRoleClaims`, `AspNetRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserRoles`, `AspNetUsers`, `AspNetUserTokens`, `Authors`, `BookAuthors`, `BookCopies`, `Books`, `Categories`, `Loans`, `Notifications`, `ReadingListItems`, `ReadingLists`, `Reservations`.
- FKs, unique indexes (ISBN, AccessionNumber), and enum string conversions all applied.
- Seed data verified present via sqlcmd (categories, authors, books, copies, users).

## Verification
- `dotnet build Sinop.sln` — **succeeded**, 0 errors (8 pre-existing NU1902/NU1903 package-vulnerability warnings for System.Security.Cryptography.Pkcs, unrelated to this change).
- `dotnet ef migrations add InitialCreate` — succeeded.
- App startup applied migration + seeded users/roles; listening on http://localhost:5288.
- sqlcmd verified all 18 tables and seed rows (3 categories, 3 authors, 3 books, 4 copies, 3 users).
- `dotnet test` — no tests exist yet (Milestone 1 is scaffolding; tests added in later milestones per CLAUDE.md §15).
- Android not yet created (deferred per SPEC-MILESTONE-0 open question; SDK present at /c/Android/Sdk).

## Known Limitations / TODO
1. `System.Security.Cryptography.Pkcs` NU1903 warnings — transitive dependency; not a code defect. Address only if required later.
2. No unit/integration tests yet — to be added in Milestones 2–4 per CLAUDE.md §15.
3. JWT authentication (Milestone 2B) not yet configured; Identity cookie auth not yet wired in MVC.
4. Application services (BookService, etc.) deferred to Milestone 3.
