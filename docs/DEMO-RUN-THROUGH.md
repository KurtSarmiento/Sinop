# Sinop Library — Full Demonstration Run-Through

Use this guide for presentations and end-to-end testing of the MVC application, Hangfire, REST API/Swagger, and Android application.

## 1. Prerequisites

Before demonstrating or testing:

- SQL Server LocalDB is available.
- The Sinop web/API project is running.
- Visual Studio or a PowerShell terminal is available.
- Android Studio is available for the emulator or physical device test.
- At least two physical book copies are registered.
- The database has been initialized and seeded.

Open a PowerShell terminal in the solution source directory:

```powershell
cd "C:\Users\Kurt Sarmiento\Sinop\src"
```

Start the web/API project:

```powershell
dotnet run --project .\Sinop.Web\Sinop.Web.csproj --launch-profile http
```

The web and API run at:

```text
http://localhost:5288
```

The Android emulator is configured for:

```text
http://10.0.2.2:5288/
```

For a physical Android device, replace `10.0.2.2` with the PC's LAN IP address and ensure Windows Firewall allows inbound traffic on port `5288`.

## 2. Seeded Demo Accounts

| Role | Email | Password |
|---|---|---|
| Administrator | `admin@library.local` | `Password123!` |
| Staff/Librarian | `librarian@library.local` | `Password123!` |
| Borrower | `borrower@library.local` | `Password123!` |

## 3. Key URLs

| Feature | URL |
|---|---|
| MVC login | http://localhost:5288/Account/Login |
| Landing page | http://localhost:5288/ |
| Catalog | http://localhost:5288/MVC/Books |
| Account management | http://localhost:5288/MVC/Accounts |
| Borrowers | http://localhost:5288/MVC/Borrowers |
| Copies | http://localhost:5288/MVC/Copies |
| Circulation | http://localhost:5288/MVC/Circulation |
| Reservations | http://localhost:5288/MVC/Reservations |
| Reports | http://localhost:5288/MVC/Reports |
| Notifications | http://localhost:5288/MVC/Notifications |
| Hangfire | http://localhost:5288/hangfire |
| Swagger | http://localhost:5288/swagger |

## 4. Administrator MVC Demonstration

### 4.1 Login

1. Open http://localhost:5288/Account/Login.
2. Enter `admin@library.local`.
3. Enter `Password123!`.
4. Submit the login form.
5. Confirm that the user is redirected to the landing page.

### 4.2 Landing Page

Demonstrate:

- Sinop branding and official logo.
- Informational library overview.
- Hero and placeholder/final imagery.
- Operational quick links.
- Sidebar navigation.
- Responsive layout.

### 4.3 Account Management

Open **Accounts**.

Demonstrate account creation:

1. Select **Create account**.
2. Enter a full name.
3. Enter a unique email address.
4. Enter and confirm a valid password.
5. Select a role:
   - Borrower
   - Staff/Librarian
   - Administrator
6. Submit the form.

Also demonstrate validation:

- Duplicate email address.
- Missing full name or email.
- Invalid email format.
- Password shorter than the configured minimum.
- Password confirmation mismatch.

Confirm that the account list is grouped by:

- Administrators
- Staff
- Borrowers

### 4.4 Catalog

Open **Catalog**.

Demonstrate:

- Local catalog cards and cover images.
- Title or ISBN search.
- Author filtering.
- Category filtering.
- Publication-year filtering.
- Available-copies-only filtering.
- Clear filters.
- Previous and next pagination.
- Catalog details.
- Authors and publication year.
- Copy and availability totals.

### 4.5 Open Library Integration

From the catalog, select **Search Open Library**.

Demonstrate:

1. Search by title.
2. Optionally filter by author or publication year.
3. Review covers and metadata.
4. Import a result that is not yet in the local catalog.
5. Confirm the book appears in the local catalog.
6. Attempt to import the same ISBN again.
7. Confirm that duplicate ISBN validation prevents the duplicate import.

### 4.6 Adding or Editing a Local Book

Demonstrate direct local catalog management:

1. Open **Catalog**.
2. Select **Add** or **Create**, if available.
3. Enter the title, ISBN, summary, category, and authors.
4. Save the book.
5. Open the book details.
6. Edit the book and save changes.

### 4.7 Physical Copies

Open **Copies**.

Demonstrate:

1. Register a copy for a book.
2. Assign an accession number.
3. Confirm the copy status.
4. Change the status when appropriate.
5. Show available and unavailable copy counts in the catalog.

Prepare at least two copies for a complete checkout and reservation demonstration.

### 4.8 Circulation

Open **Circulation**.

Demonstrate:

1. Select checkout.
2. Select a borrower.
3. Select a book.
4. Choose or confirm an available copy.
5. Submit the checkout.
6. Show the loan in the active-loan list.
7. Confirm the copy is no longer available.
8. Return the loan.
9. Confirm the copy becomes available again.

If desired, demonstrate an overdue loan:

1. Create or prepare a loan.
2. Set or identify a due date in the past.
3. Confirm the loan is overdue.
4. Show that it is visible in reports or overdue notification processing.

### 4.9 Reservations

Open **Reservations**.

Demonstrate:

1. Create or process a reservation.
2. Show a queued reservation.
3. Confirm allocation behavior when a copy is returned.
4. Cancel a reservation.
5. Demonstrate expired reservation processing.

### 4.10 Borrowers

Open **Borrowers**.

Demonstrate:

- Borrower account listing.
- Borrower details.
- Borrowing history.
- Active loans.
- Borrower notifications or reservations.

### 4.11 Reports

Open **Reports**.

Demonstrate:

- Catalog/book totals.
- Physical copy totals.
- Available copy totals.
- Active-loan totals.
- Overdue-loan totals.
- Reservation totals.
- Borrower totals.

Explain how report values come from the live SQL database and API services.

### 4.12 Notifications

Open **Notifications**.

Demonstrate:

- Unread notification filtering.
- Individual notification details.
- Mark one notification as read.
- Mark all notifications as read.
- Confirm unread counts change.

### 4.13 Settings

Open **Settings**.

Demonstrate Administrator-only access:

- Settings page is visible only to Administrators.
- Staff/Librarians cannot access it.

## 5. Staff/Librarian MVC Demonstration

Log out of the Administrator account and log in as:

```text
librarian@library.local
Password123!
```

Demonstrate:

- Catalog access.
- Inventory/copy registration.
- Borrower management.
- Circulation checkout and return.
- Reservation management.
- Open Library search and import.
- Account management access.

### Staff Account Creation Rules

Open **Accounts** and create a test account.

Verify:

- The Staff role selector is not available.
- The role is displayed as Borrower.
- Any attempted or submitted role value is overridden to Borrower.
- The created account can access borrower functionality.

A Librarian must not be able to:

- Create an Administrator account.
- Create a Librarian account through the role selector.
- Access Administrator Settings.

## 6. Borrower MVC Demonstration

Log out and log in as:

```text
borrower@library.local
Password123!
```

Demonstrate restrictions and borrower-facing information:

- Notifications.
- Reserved books.
- Borrowing history or active loans.
- No account creation access.
- No circulation administration access.
- No physical inventory management access.
- No Administrator Settings access.

## 7. Hangfire Demonstration

### 7.1 Access the Dashboard

Log in as an Administrator, then open:

```text
http://localhost:5288/hangfire
```

The dashboard is restricted to authenticated users with the Administrator role.

Demonstrate:

- Hangfire server status.
- Recurring jobs.
- Scheduled jobs.
- Enqueued jobs.
- Processing jobs.
- Succeeded jobs.
- Failed jobs.
- Retries and job history.

### 7.2 Recurring Overdue-Notification Job

Job name:

```text
overdue-notification
```

Schedule:

```text
Daily at 8:00 AM
```

Show that the job:

1. Finds active loans whose due date is before the current time.
2. Loads the related book and copy.
3. Creates a notification for the borrower.
4. Checks for an existing identical overdue notification.
5. Does not create duplicates for the same overdue loan.

To demonstrate:

1. Create an active loan.
2. Set or use a due date in the past.
3. Trigger or wait for the recurring job.
4. Open **Notifications**.
5. Confirm the overdue notification exists.

### 7.3 Recurring Reservation-Expiration Job

Job name:

```text
reservation-expiration
```

Schedule:

```text
Every 15 minutes
```

Show that the job:

1. Finds expired reservations.
2. Changes expired reservation statuses.
3. Processes the next queued reservation when appropriate.
4. Allocates available copies when possible.

To demonstrate:

1. Create a reservation.
2. Set or wait until the pickup deadline expires.
3. Wait for or trigger the recurring job.
4. Open **Reservations**.
5. Confirm the reservation status changed or a copy was allocated.

### 7.4 Immediate Swagger Demonstration

Open http://localhost:5288/swagger if immediate execution is more convenient.

Authenticate with a JWT and demonstrate reservation-expiration processing or other background-service endpoints if available.

## 8. Swagger/API Demonstration

Open:

```text
http://localhost:5288/swagger
```

### 8.1 Get an API Token

Send:

```text
POST /api/auth/login
```

Use a Librarian account:

```json
{
  "email": "librarian@library.local",
  "password": "Password123!"
}
```

Copy the returned token.

In Swagger:

1. Select **Authorize**.
2. Enter:

```text
Bearer YOUR_TOKEN
```

3. Confirm protected endpoints become available.

### 8.2 Suggested API Demonstration Order

1. Authenticate.
2. Get the local catalog.
3. Search books.
4. Get book details.
5. Search Open Library.
6. Import a book.
7. Get the local catalog again.
8. Register a physical copy.
9. Check loan eligibility.
10. Checkout a loan.
11. View active loans.
12. Return the loan.
13. Create or list reservations.
14. Cancel or expire reservations.
15. View notifications.
16. Mark notifications as read.

### 8.3 Main API Feature Groups

- Authentication
- Books
- Book copies
- Loans
- Reservations
- Reading lists
- Notifications
- Open Library search and import

## 9. Android Application Demonstration

### 9.1 Run Configuration

Open the Android project in Android Studio:

```text
C:\Users\Kurt Sarmiento\Sinop\AndroidApp
```

Use an emulator or physical device.

For the emulator, the API URL is configured as:

```text
http://10.0.2.2:5288
```

For a physical device, configure the API URL to the PC's LAN IP, for example:

```text
http://192.168.1.100:5288
```

Confirm HTTP cleartext networking is allowed for the emulator or LAN address.

### 9.2 Login

Use:

```text
borrower@library.local
Password123!
```

Demonstrate:

- Successful login.
- JWT storage.
- Invalid-login validation.
- Protected navigation after login.
- Logout and forced return to login.

### 9.3 Home

Demonstrate:

- Branding and logo.
- User-facing landing content.
- Navigation to catalog or borrower features.

### 9.4 Catalog

Demonstrate:

- Browse book cards.
- Search for a title.
- Open a book detail page.
- Show cover, title, author, ISBN, year, and availability.
- Confirm catalog data comes from the web API.

### 9.5 Reservations

Demonstrate:

1. Open the reservations screen.
2. Create a reservation for a book.
3. Show queued or active reservation status.
4. Cancel a reservation.
5. Confirm the change through the API.

### 9.6 Reading Lists

Demonstrate:

1. Create a reading list.
2. Add a book to the list.
3. Show the list contents.
4. Remove a list item.
5. Delete or manage the list if supported.

### 9.7 My Books

Demonstrate:

- Active loans.
- Borrowed-book details.
- Loan due dates.
- Loan history.
- Overdue or active status.

### 9.8 Notifications

Demonstrate:

- Notifications retrieved from the API.
- Unread notification count or state.
- Notification details.
- Mark-read behavior.

### 9.9 Profile

Demonstrate:

- Authenticated user's email or name.
- Reader role or account information.
- Logout.
- Confirmation that protected actions require authentication again.

## 10. Complete End-to-End Demonstration Script

Use this sequence for the strongest final presentation:

1. Start SQL Server LocalDB and run the web/API project.
2. Log in to MVC as Administrator.
3. Open Account Management.
4. Create a new Borrower account.
5. Search Open Library and import a book.
6. Register at least two physical copies of the imported book.
7. Logout and log in to Android using the new Borrower account.
8. Search for the imported book.
9. Open book details.
10. Create a reservation or reading-list entry.
11. Logout and log in to MVC as Staff/Librarian.
12. Process or allocate the reservation.
13. Check out the book.
14. Confirm the copy changes to unavailable or borrowed.
15. Return the book.
16. Confirm the copy becomes available.
17. Open Notifications and show associated notifications.
18. Open Hangfire and show recurring job configuration and job history.
19. Open Swagger and demonstrate authentication plus the REST API operations.
20. Logout and confirm unauthenticated users cannot access protected MVC or API pages.

## 11. Pre-Demonstration Testing Checklist

### Core Application

- [ ] Web/API application starts successfully.
- [ ] SQL database connects.
- [ ] Database migrations complete.
- [ ] Seeded users exist.
- [ ] Seeded roles exist.
- [ ] Login works for Administrator, Staff, and Borrower.
- [ ] Logout works.
- [ ] No public registration link is available.

### MVC

- [ ] Landing page renders correctly.
- [ ] Logo and brand colors appear.
- [ ] Navigation appears for the correct roles.
- [ ] Account creation works for Administrator.
- [ ] Staff account creation forces Borrower role.
- [ ] Account validation works.
- [ ] Catalog search and filters work.
- [ ] Catalog pagination works.
- [ ] Book covers and details appear.
- [ ] Open Library search and import work.
- [ ] Duplicate ISBN import is prevented.
- [ ] Copy registration works.
- [ ] Checkout and return work.
- [ ] Reservation and cancellation work.
- [ ] Notifications work.
- [ ] Reports contain expected values.

### Hangfire

- [ ] Dashboard opens for Administrator only.
- [ ] Dashboard rejects non-Administrator users.
- [ ] Hangfire server is running.
- [ ] Overdue notification job is registered.
- [ ] Reservation expiration job is registered.
- [ ] Job history or execution appears.
- [ ] Duplicate overdue notifications are prevented.

### API/Swagger

- [ ] Login endpoint returns a JWT.
- [ ] Swagger Bearer authentication works.
- [ ] Protected endpoints reject unauthenticated requests.
- [ ] Books endpoints work.
- [ ] Copy endpoints work.
- [ ] Loan endpoints work.
- [ ] Reservation endpoints work.
- [ ] Reading-list endpoints work.
- [ ] Notification endpoints work.
- [ ] Open Library endpoints work.

### Android

- [ ] Android project builds successfully.
- [ ] Emulator can reach `10.0.2.2:5288`.
- [ ] Physical-device API URL is configured if used.
- [ ] Login works.
- [ ] Catalog loads.
- [ ] Book details open.
- [ ] Reservations work.
- [ ] Reading lists work.
- [ ] Active loans display.
- [ ] Notifications display.
- [ ] Logout works.

## 12. Troubleshooting

### Web/API will not start

- Confirm port `5288` is not already occupied.
- Stop duplicate `Sinop.Web` processes.
- Rebuild the solution.

```powershell
Get-NetTCPConnection -LocalPort 5288 -State Listen
```

### Android cannot connect

For an emulator:

```text
http://10.0.2.2:5288
```

For a physical device:

```text
http://YOUR_PC_LAN_IP:5288
```

Confirm:

- The web server is running.
- The emulator or device is on the same network.
- Windows Firewall allows the port.
- HTTP cleartext networking is allowed.

### Catalog has no books

- Import or create books first.
- Confirm SQL Server LocalDB contains `Books` records.
- Search a title or ISBN that exists locally.
- Clear active filters.

### Open Library returns no metadata

- Verify internet connectivity.
- Search with a known title such as `Harry Potter`.
- Confirm Open Library endpoints are reachable.
- Confirm ISBN data exists for the selected result.

### Hangfire dashboard is denied

- Log in as `admin@library.local`.
- Confirm the logged-in user has the `Administrator` role.
- Confirm `HangfireDashboardAuthorizationFilter` is active.

### API request returns 401

- Obtain a fresh JWT from `/api/auth/login`.
- Add `Bearer ` before the token.
- Confirm the account role is authorized for the endpoint.

### Notification or overdue process does not appear

- Confirm an active overdue loan exists.
- Confirm the loan's due date is in the past.
- Trigger or wait for the Hangfire schedule.
- Refresh Notifications.
- Review Hangfire job history for failures.
