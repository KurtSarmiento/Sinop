# FLOWCHARTS.md — Milestone 5: Open Library Integration

## Search by Title Flow

```mermaid
flowchart TD
    A[GET /api/openlibrary/search?title=X] --> B[Role Librarian/Admin?]
    B -->|No| C[403 Forbidden]
    B -->|Yes| D[OpenLibraryService.SearchByTitleAsync]
    D --> E[OpenLibraryClient calls openlibrary.org/search.json]
    E --> F{External API reachable?}
    F -->|No| G[500 after 10s timeout]
    F -->|Yes| H[Map OpenLibraryWork[] → OpenLibraryBookPreview[]]
    H --> I{For each preview with ISBN}
    I -->|Yes| J[Check IsDuplicateIsbnAsync vs local catalog]
    I -->|No| K[IsDuplicate = false]
    J --> K
    K --> L[200 {total, results}]
```

## Import Flow

```mermaid
flowchart TD
    A[POST /api/openlibrary/import + {title, isbn, ...}] --> B[Role Librarian/Admin?]
    B -->|No| C[403 Forbidden]
    B -->|Yes| D[ModelState valid?]
    D -->|No| E[400 ValidationProblem]
    D -->|Yes| F[IsDuplicateIsbnAsync(isbn)?]
    F -->|Yes| G[409: ISBN already exists]
    F -->|No| H[BookService.CreateBookAsync(request)]
    H --> I[Create Book + BookAuthor links]
    I --> J[SaveChangesAsync]
    J --> K[201 Created + OpenLibraryImportResponse]
```

## Cover URL Flow

```mermaid
flowchart TD
    A[GET /api/openlibrary/cover/{coverEditionKey}] --> B[OpenLibraryService.GetCoverUrlAsync]
    B --> C[Return https://covers.openlibrary.org/b/id/{key}-M.jpg]
    C --> D[200 {coverUrl}]
```

## Error Handling

```mermaid
flowchart TD
    A[External API call fails/times out] --> B[HttpRequestException / TaskCanceledException]
    B --> C[ExceptionMiddleware catches]
    C --> D[500 application/problem+json]
    D --> E[Local catalog unaffected]