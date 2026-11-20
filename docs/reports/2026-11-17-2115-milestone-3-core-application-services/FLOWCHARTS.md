# FLOWCHARTS.md — Milestone 3: Core Application Services

## Book Create Flow

```mermaid
flowchart TD
    A[CreateBookAsync(request)] --> B[ISBN duplicate?]
    B -->|Yes| C[Throw: ISBN already exists]
    B -->|No| D[Validate CategoryId exists]
    D -->|Missing| E[Throw: category not found]
    D -->|OK| F[Create Book entity + BookAuthor links]
    F --> G[SaveChangesAsync]
    G --> H[Return BookResponse]
```

## Book Archive Flow

```mermaid
flowchart TD
    A[ArchiveBookAsync(id)] --> B[Load book + copies + loans + reservations]
    B --> C{Active loan or queued reservation?}
    C -->|Yes| D[Throw: cannot archive]
    C -->|No| E[Remove book]
    E --> F[SaveChangesAsync]
    F --> G[Return true]
```

## Copy Registration Flow

```mermaid
flowchart TD
    A[RegisterCopyAsync(request)] --> B[Book exists?]
    B -->|No| C[Throw: book not found]
    B -->|Yes| D[AccessionNumber provided?]
    D -->|No| E[GenerateAccessionNumberAsync]
    D -->|Yes| F[Use provided accession]
    E --> G[Accession unique?]
    F --> G
    G -->|No| H[Throw: accession exists]
    G -->|Yes| I[Create BookCopy]
    I --> J[SaveChangesAsync]
    J --> K[Return BookCopyResponse]
```

## Notification Create-if-not-exists Flow

```mermaid
flowchart TD
    A[CreateIfNotExistsAsync(userId, message)] --> B{Unread identical message exists?}
    B -->|Yes| C[Return false (no-op)]
    B -->|No| D[CreateNotificationAsync]
    D --> E[SaveChangesAsync]
    E --> F[Return true]
```

## DI Registration

```mermaid
flowchart TD
    A[Program.cs builder.Services] --> B[AddScoped<BookService>]
    A --> C[AddScoped<BookCopyService>]
    A --> D[AddScoped<BorrowerService>]
    A --> E[AddScoped<NotificationService>]
    B --> F[Controllers inject services via constructor]