# FLOWCHARTS.md — Milestone 4: Book Catalog Web API

## GET /api/books Flow

```mermaid
flowchart TD
    A[Request GET /api/books?page&pageSize&search] --> B[Clamp page/pageSize]
    B --> C[BookService.GetBooksAsync]
    C --> D[Query Books: include Category, BookAuthors, Copies]
    D --> E{Search term present?}
    E -->|Yes| F[Filter title/ISBN contains search]
    E -->|No| G[All books]
    F --> H[Order by title, skip/take]
    G --> H
    H --> I[Project to BookResponse DTOs]
    I --> J[Return PagedResult 200]
```

## POST /api/books Flow

```mermaid
flowchart TD
    A[Request POST /api/books + body] --> B[ModelState valid?]
    B -->|No| C[400 ValidationProblem]
    B -->|Yes| D[Role Librarian/Admin?]
    D -->|No| E[403 Forbidden]
    D -->|Yes| F[BookService.CreateBookAsync]
    F --> G{ISBN duplicate?}
    G -->|Yes| H[409 Conflict]
    G -->|No| I[Create Book + BookAuthor links]
    I --> J[SaveChangesAsync]
    J --> K[201 Created + Location header]
```

## DELETE /api/books/{id} Flow

```mermaid
flowchart TD
    A[Request DELETE /api/books/{id}] --> B[Role Librarian/Admin?]
    B -->|No| C[403 Forbidden]
    B -->|Yes| D[BookService.ArchiveBookAsync]
    D --> E[Load book + copies + loans + reservations]
    E --> F{Active loan or queued reservation?}
    F -->|Yes| G[409 Conflict]
    F -->|No| H[Remove book (copies cascade-delete)]
    H --> I[SaveChangesAsync]
    I --> J[204 No Content]
```

## Error Handling Flow

```mermaid
flowchart TD
    A[Controller throws exception] --> B{Exception type?}
    B -->|InvalidOperationException| C[ExceptionMiddleware / controller catch → 409]
    B -->|NotFound (null result)| D[Controller → 404]
    B -->|Unhandled| E[ExceptionMiddleware → 500 problem+json]
    B -->|Invalid model| F[Controller → 400 ValidationProblem]