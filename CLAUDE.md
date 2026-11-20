# CLAUDE.md — Master Development Guide: Sinop Digital Library and Reading Management System
> Developer: Kurt Justine D. Sarmiento
> Project: Sinop Digital Library and Reading Management System
> Framework: .NET 10
> Database: Microsoft SQL Server
> Android: Kotlin + Jetpack Compose
> Project Type: Full-Stack Application
> Development Timeline: 3 Days (72 Hours)
> Date Started: 2026-11-17
> **IMPORTANT:** This file serves as the primary source of truth for the entire project. Claude Code must read and follow these instructions at the beginning of every session. Keep this file at the root of the solution directory. All architectural decisions, development standards, project boundaries, milestones, and documentation requirements must follow this document unless the developer explicitly approves a change.
# 1. Who You Are and How You Work
1. Designing a maintainable and properly structured software architecture.
2. Implementing features using established software engineering principles.
3. Maintaining strict separation of concerns across projects, layers, and classes.
4. Ensuring that the Web MVC, Web API, and Android applications work together as one integrated system.
5. Writing clean, readable, reusable, and maintainable code.
6. Testing every implementation before considering a task complete.
7. Maintaining accurate developer documentation.
8. Explaining important technical decisions in understandable language.
## 1.1 Development Philosophy
| Principle | Implementation Standard |
|---|---|
| Separation of Concerns | Every class and project must have a clearly defined responsibility. |
| Single Responsibility Principle | A class should have one primary reason to change. |
| DRY | Avoid unnecessary duplication of code and business logic. |
| KISS | Prefer straightforward solutions over unnecessary abstractions. |
| SOLID | Apply SOLID principles where they improve maintainability. |
| Dependency Injection | Use built-in dependency injection. |
| Clean Code | Use descriptive names, consistent formatting, and readable implementations. |
| Reusability | Extract genuinely reusable components and services. |
| Maintainability | Avoid tightly coupled implementations. |
| Security | Validate inputs, enforce authorization, and protect sensitive information. |
| Consistency | Use consistent naming, response structures, and coding conventions. |
| Testability | Keep business logic independently testable. |
## 1.2 Installed Skills and Plugins
| Skill | When to Use |
|---|---|
| `andrej-karpathy-skills:karpathy-guidelines` | Always. Think before coding, keep implementations simple, and make surgical changes. |
| `ponytail` | Always. Implement the smallest complete solution without unnecessary features. |
| `agent-skills` | Use `/spec` before new features, `/plan` to break down work, `/build` for implementation, `/test` for verification, and `/review` before completion. |
| `impeccable` | Required for UI development involving Razor views and Jetpack Compose screens. |
## 1.3 How to Handle Development Requests
1. Read the relevant project files before modifying anything.
2. Identify the affected projects, classes, and dependencies.
3. Review existing implementations to avoid duplication.
4. State assumptions and identify potential conflicts.
5. Use `/spec` for new features.
6. Use `/plan` to establish a manageable implementation plan.
7. Implement only the requested feature or milestone.
8. Run the appropriate build and tests.
9. Review the changes for architectural consistency.
10. Generate the required documentation and reports.
11. Summarize the implementation and any remaining limitations.
# 2. Project Overview
## 2.1 Project Description
1. ASP.NET Core MVC — Administrative and librarian-facing web application.
2. ASP.NET Core Web API — Central backend interface for Android and other clients.
3. ASP.NET Core Application Services — Shared business logic.
4. Entity Framework Core — Database access and persistence.
5. Microsoft SQL Server — Central relational database.
6. Hangfire — Background processing and scheduled automation.
7. Kotlin + Jetpack Compose — Borrower-facing Android application.
8. Room Database — Local Android catalog and reading-list cache.
9. Open Library API — External book metadata and cover information.
## 2.2 Core Architectural Principle
### Required architecture
```text
```
# 3. Technology Stack and Technical Standards
## 3.1 Backend
| Technology | Purpose |
|---|---|
| C# | Backend programming language |
| .NET 10 | Application framework |
| ASP.NET Core MVC | Administrative web interface |
| ASP.NET Core Web API | REST API |
| Entity Framework Core | ORM |
| ASP.NET Core Identity | Authentication and user management |
| JWT Bearer | Android API authentication |
| Hangfire | Background processing |
| Swagger / OpenAPI | API documentation and testing |
| HttpClientFactory | External API integration |
| Microsoft SQL Server | Database |
## 3.2 Android
| Technology | Purpose |
|---|---|
| Kotlin | Programming language |
| Jetpack Compose | UI development |
| MVVM | Presentation architecture |
| Retrofit | HTTP client |
| OkHttp | HTTP networking and interceptors |
| Kotlin Coroutines | Asynchronous operations |
| StateFlow | Reactive UI state |
| Room | Local persistence |
| Navigation Compose | Application navigation |
| Coil | Image loading |
| Jetpack Security or maintained secure storage solution | Token protection |
## 3.3 Database
- Microsoft SQL Server 2022 Express, LocalDB, or an existing compatible local SQL Server installation.
- Entity Framework Core migrations must be the only mechanism used to create or modify the database schema.
- Never manually alter production application tables.
- Use Fluent API configurations for entity relationships, constraints, indexes, and field requirements.
- Use appropriate database transactions for critical circulation operations.
## 3.4 External API
```text
```
# 4. Separation of Concerns and Project Responsibilities
## 4.1 Sinop.Domain
- ASP.NET Core MVC.
- ASP.NET Core Web API.
- Entity Framework Core.
- SQL Server.
- Android.
- Open Library implementation details.
- Hangfire implementation details.
```text
```
### Entity responsibilities
## 4.2 Sinop.Infrastructure
### Persistence
- LibraryDbContext.
- Entity Framework Core configurations.
- Database migrations.
- Database seeding.
- SQL Server integration.
### ExternalServices
```text
    ├── IOpenLibraryService.cs
    ├── OpenLibraryService.cs
    ├── OpenLibraryClient.cs
    └── Models/
```
- Search books.
- Retrieve metadata.
- Retrieve ISBN information.
- Map external responses to internal DTOs or domain-compatible models.
- Handle external API errors and timeouts.
- Avoid duplicate imports.
### BackgroundJobs
```text
```
### DependencyInjection
```csharp
```
## 4.3 Sinop.Web
### Controllers
1. Receiving HTTP requests.
2. Validating request models.
3. Calling the appropriate application service.
4. Returning the correct HTTP response or view.
```text
```
```text
```
### Application Services
```text
    ├── BookService.cs
    ├── BookCopyService.cs
    ├── BorrowerService.cs
    ├── LoanService.cs
    ├── ReservationService.cs
    ├── ReadingListService.cs
    └── NotificationService.cs
```
- Retrieve books.
- Search the catalog.
- Create and update book records.
- Delete or archive books according to circulation restrictions.
- Import selected external book metadata.
- Register physical copies.
- Generate accession numbers.
- Update copy status when permitted.
- Retrieve inventory.
- Validate borrowing eligibility.
- Process physical checkout.
- Assign available physical copies.
- Process returns.
- Calculate overdue conditions.
- Retrieve borrowing history.
- Create reservations.
- Enforce reservation eligibility.
- Maintain reservation queues.
- Allocate returned copies.
- Process reservation expiration.
- Create reading lists.
- Add and remove books.
- Retrieve personal collections.
- Enforce ownership.
- Generate in-app notifications.
- Retrieve notifications.
- Mark notifications as read.
- Prevent duplicate notification generation where appropriate.
### DTOs
```text
    ├── AuthResponse.cs
    ├── BookResponse.cs
    ├── BookDetailsResponse.cs
    ├── BookCopyResponse.cs
    ├── LoanResponse.cs
    ├── ReservationResponse.cs
    ├── ReadingListResponse.cs
    └── NotificationResponse.cs
```
### ViewModels
### Views
### Middleware and Extensions
- Exception handling.
- Authentication and authorization.
- Dependency registration.
- Swagger configuration.
- Hangfire dashboard authorization.
- API response handling.
# 5. Database Architecture and Business Rules
## 5.1 Database Name
```text
```
## 5.2 Database Entities
| Entity/Table | Responsibility |
|---|---|
| AspNetUsers | User accounts |
| AspNetRoles | Application roles |
| Books | Bibliographic records |
| BookCopies | Physical inventory |
| Authors | Author information |
| Categories | Book categories |
| BookAuthors | Book-author relationship |
| Loans | Borrowing transactions |
| Reservations | Reservation records |
| ReadingLists | Personal reading collections |
| ReadingListItems | Saved books |
| Notifications | In-app notifications |
| Hangfire tables | Persistent background-job storage |
## 5.3 Important Database Relationships
- One Book has many BookCopies.
- One Book can have many Authors.
- One Author can belong to many Books.
- One Book belongs to an optional Category.
- One Borrower can have many Loans.
- One Borrower can have many Reservations.
- One Book can have many Reservations.
- One Borrower can have many ReadingLists.
- One ReadingList can contain many ReadingListItems.
- One Book can appear in many ReadingListItems.
- One User can receive many Notifications.
## 5.4 Book and BookCopy Distinction
```text
- Title: Clean Code
- Author: Robert C. Martin
- ISBN: ...
- Category: Programming
- LIB-0001
- LIB-0002
- LIB-0003
```
## 5.5 Copy Statuses
```csharp
    Available,
    Borrowed,
    Reserved,
    Maintenance,
    Lost
```
## 5.6 Loan Statuses
```csharp
    Active,
    Returned,
    Cancelled
```
```text
```
## 5.7 Reservation Statuses
```csharp
    Queued,
    ReadyForPickup,
    Fulfilled,
    Expired,
    Cancelled
```
## 5.8 Initial Library Rules
| Rule | Default |
|---|---|
| Loan duration | 14 days |
| Maximum simultaneous active loans | 3 |
| Reservation pickup period | 2 days |
| Reservation ordering | First-come, first-served |
| Checkout confirmation | Librarian |
| Return confirmation | Librarian |
| Notifications | In-app |
## 5.9 Transaction and Concurrency Requirements
- A physical copy cannot be borrowed by two users simultaneously.
- A copy cannot be allocated to multiple reservations.
- A returned loan cannot be returned again.
- Duplicate active loans for the same borrower and title must be prevented.
- A borrower cannot have duplicate active reservations for the same title.
- Reservation queue ordering must be deterministic.
- Database constraints should support important business invariants where practical.
# 6. Authentication and Authorization
## 6.1 Roles
```text
```
## 6.2 Seeded Development Accounts
| Email | Role |
|---|---|
| admin@library.local | Administrator |
| librarian@library.local | Librarian |
| borrower@library.local | Borrower |
## 6.3 MVC Authentication
- Login.
- Logout.
- Session management.
- Role-based access.
- Unauthorized page handling.
- Antiforgery validation.
## 6.4 Android Authentication
- Login endpoint.
- Token issuance.
- Token validation.
- Expiration.
- Authorization middleware.
- Protected API endpoints.
## 6.5 Authorization Rules
- Full application access.
- Manage accounts and roles.
- Manage library settings.
- Access reports.
- Manage books and inventory.
- Manage circulation.
- Manage books.
- Import metadata.
- Manage physical copies.
- Manage borrowers.
- Process physical checkouts and returns.
- Manage reservations.
- Access circulation reports.
- Search books.
- View book details and availability.
- Reserve books.
- View borrowing history.
- View active loans.
- Maintain personal reading lists.
- View personal notifications.
# 7. Web API Standards
## 7.1 API Route Convention
### Authentication
### Books
### Open Library
### Physical Copies
### Borrowers
### Loans
### Reservations
### Reading Lists
### Notifications
## 7.2 HTTP Status Codes
## 7.3 API Response Standards
## 7.4 API Requirements
# 8. MVC Application Standards
## 8.1 Main Navigation
## 8.2 Dashboard
## 8.3 Book Catalog
## 8.4 Open Library Import
## 8.5 Physical Inventory
## 8.6 Borrower Management
## 8.7 Circulation
## 8.8 Reports
# 9. Kotlin Android Application Standards
## 9.1 Architecture
## 9.2 Layer Responsibilities
## 9.3 Required Screens
## 9.4 Android Functional Requirements
## 9.5 Offline Functionality
## 9.6 UI State Requirements
## 9.7 Android Networking
# 10. Hangfire and Background Processing
## 10.1 OverdueNotificationJob
## 10.2 ReservationExpirationJob
## 10.3 CatalogSyncJob
## 10.4 Background Job Requirements
# 11. Open Library Integration
## 11.1 Workflow
## 11.2 Requirements
# 12. UI and Design Standards
## 12.1 Design Direction
## 12.2 MVC UI
## 12.3 Android UI
## 12.4 UI Development Rules
# 13. Security and Data Integrity
## 13.1 Configuration
## 13.2 MVC Security
## 13.3 API Security
# 14. Coding Standards
## 14.1 C#
## 14.2 Kotlin
## 14.3 Naming
# 15. Testing and Verification Standards
## 15.1 Backend
## 15.2 Database
## 15.3 API
## 15.4 Android
## 15.5 End-to-End Integration
# 16. Automatic Development Reports — REQUIRED AFTER EVERY CODING PROMPT
## 16.1 Report Directory
## 16.2 DEVELOPER-DOCS.md
### Summary
### Projects Touched
### Files Changed
### Architecture and Responsibilities
### Important Implementations
### Database Changes
### Verification
### Known Limitations / TODO
## 16.3 HOW-TO-USE.md
### Prerequisites
### Step-by-Step Instructions
### Examples
### Troubleshooting
## 16.4 FLOWCHARTS.md
## 16.5 INDEX.md
## 16.6 Final Response Requirement
# 17. Development Milestones — STRICT EXECUTION ORDER
The project must be implemented incrementally.
**Do not attempt to complete multiple major milestones in a single prompt.**
Each milestone should be handled through a separate request or a clearly defined subtask.
Do not proceed to the next major milestone until the current milestone builds successfully and its requirements are verified.
If a milestone is too large, break it into smaller sub-milestones.
The following is the required implementation order.
## MILESTONE 0 — Project Initialization and Architecture
**Goal:** Establish the solution structure and development environment.
### Tasks
1. Inspect the development environment.
2. Verify installed .NET SDK.
3. Verify SQL Server availability.
4. Verify Android Studio and Android SDK.
5. Create `Sinop.sln`.
6. Create `Sinop.Domain`.
7. Create `Sinop.Infrastructure`.
8. Create `Sinop.Web`.
9. Create the Android application project.
10. Establish project references.
11. Configure nullable reference types.
12. Configure dependency injection foundations.
13. Establish configuration files.
14. Create initial documentation structure.
### Restrictions
- Do not implement actual business features yet.
- Do not generate every database entity.
- Do not build all MVC pages.
- Do not build Android screens.
### Acceptance Criteria
- Solution structure exists.
- All .NET projects compile.
- Project references are correct.
- Android project builds.
- No unnecessary dependencies are introduced.
## MILESTONE 1 — Domain Entities and Database Foundation
**Goal:** Establish the complete initial data model.
### Tasks
1. Create core domain entities.
2. Create enums.
3. Create entity relationships.
4. Implement `LibraryDbContext`.
5. Configure EF Core Fluent API.
6. Configure SQL Server.
7. Configure Identity integration.
8. Implement database seeding.
9. Seed administrator, librarian, and borrower accounts.
10. Seed sample books and physical copies.
11. Seed relevant categories and authors.
12. Create the initial EF Core migration.
13. Apply the migration.
### Acceptance Criteria
- Database is created successfully.
- All required tables exist.
- Relationships and foreign keys are correct.
- Unique accession number constraint works.
- User roles are seeded.
- Demo accounts can authenticate.
- Migrations apply successfully.
## MILESTONE 2 — Authentication and Authorization
**Goal:** Establish secure authentication for MVC and Android.
### Sub-milestone 2A: Identity and MVC Authentication
- Configure ASP.NET Core Identity.
- Implement login.
- Implement logout.
- Configure cookie authentication.
- Implement role-based MVC authorization.
### Sub-milestone 2B: JWT Authentication
- Implement JWT configuration.
- Implement login endpoint.
- Generate JWT tokens.
- Configure JWT Bearer authentication.
- Implement role-based API authorization.
- Configure Swagger JWT authentication.
### Acceptance Criteria
- Seeded users can authenticate.
- MVC uses cookie authentication.
- Android can obtain a JWT.
- Protected endpoints reject unauthenticated requests.
- Role restrictions work correctly.
## MILESTONE 3 — Core Application Services
**Goal:** Establish reusable business logic before implementing all interfaces.
### Sub-milestone 3A: BookService
- Retrieve books.
- Search books.
- Create books.
- Update books.
- Archive or delete eligible books.
- Detect duplicates.
### Sub-milestone 3B: BookCopyService
- Register physical copies.
- Generate accession numbers.
- Update copy status.
- Retrieve inventory.
### Sub-milestone 3C: BorrowerService
- Retrieve borrowers.
- Retrieve borrower details.
- Validate account status.
- Retrieve borrower history.
### Sub-milestone 3D: NotificationService
- Create notifications.
- Retrieve user notifications.
- Mark notifications as read.
- Prevent duplicate notifications where applicable.
### Acceptance Criteria
- Services have clear responsibilities.
- Business logic is not placed in controllers.
- Services are registered through dependency injection.
- Important operations are tested.
## MILESTONE 4 — Book Catalog Web API
**Goal:** Expose catalog functionality through REST endpoints.
### Tasks
1. Create book request DTOs.
2. Create book response DTOs.
3. Implement BooksController.
4. Implement GET endpoints.
5. Implement search.
6. Implement pagination.
7. Implement create.
8. Implement update.
9. Implement delete/archive.
10. Configure validation.
11. Configure error responses.
12. Document endpoints in Swagger.
### Acceptance Criteria
- Catalog endpoints work.
- DTOs are used.
- Database entities are not exposed.
- Validation works.
- Swagger documents the endpoints.
## MILESTONE 5 — Open Library Integration
**Goal:** Allow librarians to import external book metadata.
### Tasks
1. Implement Open Library client.
2. Implement OpenLibraryService.
3. Configure HttpClientFactory.
4. Implement title search.
5. Implement author search.
6. Implement ISBN search.
7. Map external metadata.
8. Handle missing fields.
9. Implement duplicate detection.
10. Implement metadata preview endpoint.
11. Implement import endpoint.
### Acceptance Criteria
- Librarian can search external books.
- Search results are mapped correctly.
- Imported metadata is stored locally.
- Duplicate records are handled.
- Physical copies are not automatically created.
- External API failure does not break the local catalog.
## MILESTONE 6 — Physical Inventory Management
**Goal:** Implement physical copy management.
### Tasks
1. Implement BookCopy DTOs.
2. Implement BookCopiesController.
3. Implement registration endpoint.
4. Implement accession number generation.
5. Support manual accession number override.
6. Implement copy status management.
7. Implement inventory retrieval.
8. Implement relevant validation.
### Acceptance Criteria
- Physical copies are associated with books.
- Accession numbers are unique.
- Invalid status transitions are rejected.
- Inventory can be retrieved through the API.
## MILESTONE 7 — Borrowing and Return Operations
**Goal:** Implement reliable physical circulation.
### Sub-milestone 7A: LoanService
- Validate borrower eligibility.
- Validate borrowing limit.
- Validate copy availability.
- Prevent duplicate active loans.
- Assign physical copies.
- Set due dates.
- Persist transactions safely.
### Sub-milestone 7B: Return Operations
- Retrieve active loans.
- Process physical returns.
- Record return dates.
- Update copy availability.
- Handle outstanding reservations.
- Prevent duplicate returns.
### Sub-milestone 7C: Loans API
- Implement LoansController.
- Implement active loan retrieval.
- Implement borrower loan history.
- Implement checkout endpoint.
- Implement return endpoint.
- Enforce librarian authorization for physical checkout and returns.
### Acceptance Criteria
- Only available copies can be borrowed.
- Borrowing limits are enforced.
- Concurrent borrowing cannot allocate one copy twice.
- Returns are processed correctly.
- Loan records are accurate.
- API authorization works.
## MILESTONE 8 — Reservation System
**Goal:** Implement first-come, first-served reservations.
### Tasks
1. Implement ReservationService.
2. Validate reservation eligibility.
3. Prevent duplicate active reservations.
4. Create reservation queue.
5. Implement queue ordering.
6. Implement reservation API.
7. Allocate returned copies.
8. Implement pickup deadlines.
9. Implement cancellation.
10. Implement expiration handling.
### Acceptance Criteria
- Reservations are ordered correctly.
- One copy cannot be allocated twice.
- Borrowers can view their reservations.
- Returned copies can be allocated to eligible reservations.
- Expired reservations release their allocated copies.
## MILESTONE 9 — Reading Lists
**Goal:** Implement personal reading management.
### Tasks
1. Implement ReadingListService.
2. Implement reading list CRUD.
3. Implement item addition.
4. Implement item removal.
5. Enforce ownership.
6. Implement API endpoints.
### Acceptance Criteria
- Borrowers can create lists.
- Borrowers can save books.
- Borrowers can remove books.
- Users cannot modify another user's lists.
- Reading lists persist in SQL Server.
## MILESTONE 10 — Notifications and Hangfire
**Goal:** Implement automated library operations.
### Sub-milestone 10A: Notifications
- Implement notification endpoints.
- Implement notification retrieval.
- Implement read/unread functionality.
- Connect notifications to relevant library events.
### Sub-milestone 10B: Hangfire Setup
- Configure SQL Server storage.
- Register Hangfire services.
- Configure Hangfire Dashboard authorization.
- Register recurring jobs.
### Sub-milestone 10C: Background Jobs
- Implement overdue notification job.
- Implement reservation expiration job.
- Implement optional catalog synchronization job.
- Verify idempotency.
- Verify execution and logging.
### Acceptance Criteria
- Hangfire persists jobs.
- Scheduled jobs execute.
- Duplicate notifications are prevented.
- Expired reservations are processed correctly.
- Dashboard is restricted to authorized administrators.
## MILESTONE 11 — MVC Administrative Dashboard
**Goal:** Implement the librarian-facing web interface.
### Sub-milestone 11A: Layout and Navigation
- Implement shared layout.
- Implement sidebar.
- Implement navigation.
- Implement reusable components.
- Establish visual design system.
### Sub-milestone 11B: Dashboard
- Display statistics.
- Display recent activity.
- Display overdue summaries.
- Display inventory information.
### Sub-milestone 11C: Catalog
- Book listing.
- Search and filters.
- Book details.
- Create and edit forms.
- Open Library import interface.
### Sub-milestone 11D: Inventory
- Physical copy listing.
- Copy registration.
- Accession number generation.
- Copy status management.
### Sub-milestone 11E: Borrowers
- Borrower listing.
- Borrower details.
- Borrower account management.
- Borrowing history.
### Sub-milestone 11F: Circulation
- Physical checkout.
- Return processing.
- Active loans.
- Reservation management.
### Sub-milestone 11G: Reports and Notifications
- Overdue reports.
- Inventory reports.
- Borrowing history.
- Notification center.
### Acceptance Criteria
- MVC consumes shared application services.
- Forms validate properly.
- Destructive operations require confirmation.
- Authorization works.
- All major pages display actual database information.
- UI is consistent and responsive.
## MILESTONE 12 — Android Foundation and Networking
**Goal:** Establish the Android application architecture.
### Tasks
1. Configure application dependencies.
2. Configure package structure.
3. Implement Retrofit.
4. Implement OkHttp.
5. Configure API base URL.
6. Implement authentication interceptor.
7. Implement API response handling.
8. Configure Room.
9. Implement local entities and DAOs.
10. Implement repositories.
11. Implement navigation.
12. Establish theme and reusable components.
### Acceptance Criteria
- Android application builds.
- Retrofit communicates with the API.
- JWT authentication works.
- Room database initializes.
- Navigation works.
## MILESTONE 13 — Android Authentication and Catalog
**Goal:** Implement the core borrower experience.
### Sub-milestone 13A: Authentication
- Login screen.
- Authentication ViewModel.
- Token storage.
- Logout.
- Unauthorized session handling.
### Sub-milestone 13B: Home and Catalog
- Home screen.
- Catalog screen.
- Search.
- Filters.
- Book details.
- Availability information.
### Sub-milestone 13C: Room Caching
- Cache catalog data.
- Cache book details.
- Retrieve cached books offline.
- Refresh cached catalog when online.
### Acceptance Criteria
- Borrowers can log in.
- Catalog loads from API.
- Book details display correctly.
- Catalog is cached.
- Offline browsing works.
## MILESTONE 14 — Android Reservations, Loans, and Reading Lists
**Goal:** Complete borrower-facing functionality.
### Tasks
1. Implement My Books.
2. Implement active loan display.
3. Implement due date display.
4. Implement reservation creation.
5. Implement reservation cancellation.
6. Implement reservation status display.
7. Implement reading lists.
8. Implement add/remove book operations.
9. Implement notification center.
10. Implement profile screen.
### Acceptance Criteria
- Borrowers can view their loans.
- Borrowers can reserve books.
- Borrowers can cancel eligible reservations.
- Borrowers can maintain reading lists.
- Borrowers can view notifications.
- UI reflects server-side changes.
## MILESTONE 15 — End-to-End Integration and Testing
**Goal:** Verify that all applications operate as one system.
### Required Test Scenarios
**Scenario A: Catalog Synchronization**
1. Librarian imports a book through MVC.
2. Librarian registers physical copies.
3. Android retrieves the book through the API.
4. Android displays the correct metadata and availability.
**Scenario B: Borrowing**
1. Borrower views a book in Android.
2. Librarian processes checkout through MVC.
3. Database records the loan.
4. Android displays the active loan.
**Scenario C: Reservation**
1. All physical copies are borrowed.
2. Borrower reserves a title through Android.
3. MVC displays the reservation.
4. A copy is returned.
5. Reservation queue is processed.
**Scenario D: Return and Notifications**
1. Librarian processes a return.
2. Inventory updates.
3. Reservation allocation occurs where applicable.
4. Notifications are generated.
5. Android displays the updated information.
**Scenario E: Offline Catalog**
1. Android retrieves catalog information.
2. Catalog is cached in Room.
3. Network connection is disabled.
4. Previously cached information remains accessible.
**Scenario F: Overdue Automation**
1. Create an overdue test loan.
2. Execute overdue processing.
3. Verify notification generation.
4. Verify duplicate notifications are prevented.
### Acceptance Criteria
All major workflows must work from beginning to end.
No critical synchronization issues may remain.
## MILESTONE 16 — Final Review, Documentation, and Demonstration Preparation
**Goal:** Prepare the complete project for presentation.
### Tasks
1. Run `/review` on the entire solution.
2. Review separation of concerns.
3. Review security and authorization.
4. Review database relationships.
5. Review API contracts.
6. Review Android networking.
7. Review Room caching.
8. Review Hangfire jobs.
9. Remove unused code.
10. Fix critical bugs.
11. Verify builds.
12. Update README.
13. Verify development reports.
14. Prepare seeded demonstration data.
15. Prepare a demonstration workflow.
### Final Acceptance Criteria
- Backend builds successfully.
- Android builds successfully.
- SQL Server database is operational.
- Seeded accounts work.
- MVC authentication works.
- Android authentication works.
- Swagger is functional.
- Catalog management works.
- Physical inventory works.
- Borrowing and returns work.
- Reservations work.
- Reading lists work.
- Notifications work.
- Hangfire jobs execute.
- Android caching works.
- All applications communicate correctly.
# 18. Three-Day Development Priorities
## DAY 1 — Backend and Database Foundation
- Milestone 0: Initialization.
- Milestone 1: Database.
- Milestone 2: Authentication.
- Milestone 3: Application Services.
- Milestone 4: Catalog API.
- Milestone 6: Physical Inventory API.
- Milestone 7: Borrowing and Returns.
## DAY 2 — MVC and Android
- Milestone 8: Reservations.
- Milestone 9: Reading Lists.
- Milestone 11: MVC.
- Milestone 12: Android Foundation.
- Milestone 13: Android Authentication and Catalog.
## DAY 3 — Automation, Integration, and Finalization
- Milestone 5: Open Library, if not completed earlier.
- Milestone 10: Hangfire.
- Milestone 14: Android Features.
- Milestone 15: Integration Testing.
- Milestone 16: Final Review.
### Priority Rules
1. Database integrity.
2. Authentication.
3. Book catalog.
4. Physical inventory.
5. Borrowing and returns.
6. MVC and Android integration.
7. Reservations.
8. Reading lists.
9. Hangfire overdue and expiration jobs.
10. Open Library enhancements and UI refinements.
# 19. Rules for Claude Code Sessions
## 19.1 One Major Task Per Prompt
## 19.2 Inspect Before Editing
- Inspect relevant files.
- Understand existing architecture.
- Identify dependencies.
- Check current implementation.
- Avoid recreating existing functionality.
## 19.3 Preserve Existing Working Code
## 19.4 Explain New Files
## 19.5 No Fake Implementations
- Hardcoded dashboard statistics.
- Fake API responses presented as real data.
- Placeholder authentication that bypasses security.
- Mock business logic in production services.
- Hardcoded book availability.
- Simulated synchronization presented as actual synchronization.
## 19.6 No Premature Completion
- Its implementation exists.
- Its dependencies work.
- The relevant build succeeds.
- Relevant tests or verification steps have been performed.
- Documentation has been generated.
## 19.7 Avoid Unnecessary Dependencies
- What it does.
- Why it is needed.
- Whether existing dependencies already provide the functionality.
## 19.8 Preserve Project Boundaries
- Put business logic inside controllers.
- Put database queries inside Razor views.
- Put API calls inside Compose UI functions.
- Put SQL Server-specific implementation inside Domain.
- Duplicate services between MVC and API.
- Expose EF Core entities directly through REST endpoints.
- Let Android bypass the Web API to access SQL Server.
# 20. Final Instructions
1. Read this CLAUDE.md before beginning work.
2. Respect the established architecture.
3. Maintain separation of concerns.
4. Implement one manageable task at a time.
5. Reuse existing services and components.
6. Keep the backend as the single source of truth.
7. Ensure MVC and Android remain synchronized through shared backend operations.
8. Test every meaningful implementation.
9. Generate the required reports after every coding prompt.
10. Never claim unverified functionality.

#### API
Use `/api/{resource}`. Controllers only bind/validate, call services, and return HTTP responses. Use request/response DTOs; never expose EF entities/navigation properties. Require async I/O, cancellation tokens where useful, pagination, centralized errors, and no N+1 queries. Enable Development Swagger with JWT. Resources: auth login; books list/details/search/create/update/archive; Open Library search/ISBN/import; copies list/details/register/update; borrowers list/details; loans list/my/checkout/return; reservations list/my/create/cancel; reading-list CRUD/items; notifications list/read/read-all.

#### MVC
MVC is the librarian/admin UI and calls shared services; never duplicate business rules. Use Razor, layouts, partials, ViewModels, validation, and existing styling. No business logic/DB work in views. Navigation: Dashboard, Catalog, Borrowers, Circulation, Reports, Notifications, Settings. Dashboard counts must come from SQL: titles/copies/availability, active/overdue loans, pending reservations, recent activity. Catalog supports search/filter/page/details/create/edit/archive, copies and availability. Open Library import previews title/author/ISBN and covers, checks duplicates, requires confirmation, and creates no copies. Inventory manages accession numbers/status/history. Borrower pages show accounts and histories.

#### Android
Use Kotlin, Compose, MVVM, repositories, Retrofit/OkHttp, Coroutines, StateFlow, Room, Navigation Compose, Coil, secure token storage. UI observes state; ViewModels coordinate repositories; repositories coordinate remote/local sources and caching. Organize local DB/DAO/entities, remote API/DTO/interceptors, repositories, domain, navigation, UI components/screens/theme, and ViewModels. Screens: Login, Home, Catalog/Search/Filters, Book Details, My Books, Reservations, Reading Lists, Notifications, Profile. Borrowers browse, search, view availability, reserve/cancel, view loans/due dates, lists, notifications; checkout/return remain librarian actions. Cache catalog/details/covers/lists.

#### Hangfire and Open Library
Use persistent SQL Server Hangfire. Jobs delegate to services; make them idempotent, logged, failure-tolerant, observable, centrally registered; restrict Dashboard to admins. Schedule overdue notifications daily 8:00 AM and reservation expiration every 15 minutes. Overdue job finds active past-due loans and creates nonduplicate notifications. Expiration job expires pickup holds, releases copies, allocates to next queued borrower, and notifies. Catalog sync is secondary and never overwrites inventory/circulation. Open Library docs: https://openlibrary.org/developers/api. Use HttpClientFactory/timeouts/error handling for title, author, ISBN, and cover lookup.

#### UI and security
Identity: deep navy, muted forest green, warm off-white, muted gold, modern sans-serif, restrained borders/shadows, readable tables, consistent navigation. MVC needs sidebar, headers, real stats, accessible forms, search/filters, destructive confirmation, responsive layout, status indicators. Android uses Material 3, reusable components, accessible targets, covers, loading/empty/error states. Use `impeccable`; avoid decoration harming usability. Backend-enforced security: never commit secrets; use environment config/user-secrets, Identity hashing, secure Android tokens, role/ownership checks, validation, MVC antiforgery, protected Hangfire, parameterized EF, transactions, safe logs. Prevent mass assignment/data exposure.

#### Coding and verification
C#: PascalCase public APIs, camelCase locals, nullable refs, constructor injection, async I/O, CancellationToken, focused methods, descriptive exceptions; no empty catches, mutable static state, or magic numbers. Kotlin: immutable by default, Coroutines, StateFlow, focused composables, reusable UI, explicit errors, no business logic in composables. Run `dotnet build`/`dotnet test` after .NET changes; `./gradlew assembleDebug`/`./gradlew test` after Android changes. Verify migrations, FKs, unique constraints, seeds, transactions, relationships, Swagger, validation, auth/roles, missing resources, conflicts, concurrency.

#### Integration and reports
E2E: MVC import/copies appear in Android; MVC checkout appears in Android loans; Android reservation appears in MVC; MVC return updates Android availability; Hangfire notices appear in Android; Android lists persist after restart. Report actual commands/results; disclose untested work. After every code-changing prompt create `docs/reports/<yyyy-MM-dd-HHmm>-<task-slug>/` with `DEVELOPER-DOCS.md` (summary, projects, changed-file table, responsibilities, methods/endpoints/decisions, DB changes or “None”, actual verification, TODOs); `HOW-TO-USE.md` (prerequisites, steps, examples, 2–3 troubleshooting cases); `FLOWCHARTS.md` (Mermaid flowchart with success/error, sequence, ER if schema changed).

#### Session discipline
Read this guide and inspect relevant files each session. Identify affected projects/dependencies, review existing code, state assumptions, and ask one focused question if ambiguity changes architecture. For features use `/spec`, `/plan`, `/build`, `/test`, `/review`; apply Karpathy and ponytail skills for simple surgical work. One manageable task per prompt; do not combine milestones. Explain new files/decisions; preserve working code/conventions. No fake implementations, placeholder success, or unverified claims. Before adding packages, explain purpose/need/alternatives. Avoid MediatR, CQRS, AutoMapper, generic repositories, microservices, or frameworks without justification/approval.
#### Additional operational invariants
SQL Server is authoritative for users, catalog, copies, loans, reservations, and notifications; Room is only a cache. A title is not a copy: checkout targets one physical copy. Keep queues ordered; never allocate one copy twice. Re-check eligibility and copy status in the circulation transaction. Make retries safe, preserve history; report conflicts, never silently change records. Keep configuration environment-specific; never weaken production security needlessly.
