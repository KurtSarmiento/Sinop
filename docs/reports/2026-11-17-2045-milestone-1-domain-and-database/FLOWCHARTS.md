# FLOWCHARTS.md — Milestone 1: Domain Entities and Database Foundation

## Entity Relationship Diagram (Mermaid)

```mermaid
erDiagram
    Book ||--o{ BookCopy : "has many physical copies"
    Book }o--|| Category : "belongs to optional"
    Book ||--o{ BookAuthor : "book-author links"
    Author ||--o{ BookAuthor : "writes many"
    Book ||--o{ Reservation : "has many reservations"
    BookCopy ||--o{ Loan : "has many loans"
    ApplicationUser ||--o{ Loan : "borrows"
    ApplicationUser ||--o{ Reservation : "reserves"
    ApplicationUser ||--o{ ReadingList : "owns"
    ReadingList ||--o{ ReadingListItem : "contains"
    Book ||--o{ ReadingListItem : "saved in"
    ApplicationUser ||--o{ Notification : "receives"
```

## Database Initialization Flow

```mermaid
flowchart TD
    A[App starts] --> B[Program.cs: AddDbContext + AddIdentity]
    B --> C[Create scope]
    C --> D[context.Database.Migrate()]
    D --> E[Create roles if missing: Administrator, Librarian, Borrower]
    E --> F[Create users if missing: admin, librarian, borrower]
    F --> G[App listens on http://localhost:5288]
    G --> H[Ready]
```

## Migration Flow

```mermaid
flowchart TD
    A[dotnet ef migrations add InitialCreate]
    B[Migration snapshot in Persistence/Migrations/]
    C[App start → Database.Migrate()]
    D[SQL Server LocalDB: SinopLibraryDb created]
    E[HasData seed rows inserted: categories, authors, books, copies]
    A --> B --> C --> D --> E