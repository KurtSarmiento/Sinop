# FLOWCHARTS.md — Milestone 6: Physical Inventory Management

## Register Copy Flow

```mermaid
flowchart TD
    A[POST /api/bookcopies] --> B[ModelState valid?]
    B -->|No| C[400 ValidationProblem]
    B -->|Yes| D[Role Librarian/Admin?]
    D -->|No| E[403 Forbidden]
    D -->|Yes| F[BookCopyService.RegisterCopyAsync]
    F --> G[Book exists?]
    G -->|No| H[409 Conflict: book not found]
    G -->|Yes| I[Accession provided?]
    I -->|No| J[GenerateAccessionNumberAsync]
    I -->|Yes| K[Use provided accession]
    J --> L[Accession unique?]
    K --> L
    L -->|No| M[409 Conflict: accession exists]
    L -->|Yes| N[Create BookCopy]
    N --> O[SaveChangesAsync]
    O --> P[201 Created + Location header]
```

## Status Update Flow

```mermaid
flowchart TD
    A[PATCH /api/bookcopies/{id}/status] --> B[ModelState valid?]
    B -->|No| C[400 ValidationProblem]
    B -->|Yes| D[Role Librarian/Admin?]
    D -->|No| E[403 Forbidden]
    D -->|Yes| F[BookCopyService.UpdateCopyStatusAsync]
    F --> G[Copy exists?]
    G -->|No| H[404 Not Found]
    G -->|Yes| I{Valid transition?}
    I -->|No| J[409 Conflict]
    I -->|Yes| K[Update Status]
    K --> L[SaveChangesAsync]
    L --> M[200 OK]
```

## Accession Generation Flow

```mermaid
flowchart TD
    A[GET /api/bookcopies/accession-number/{bookId}] --> B[Book exists?]
    B -->|No| C[409 Conflict]
    B -->|Yes| D[Prefix = LIB-{bookId:D4}]
    D --> E[Find existing accessions with prefix]
    E --> F[Next available NN suffix]
    F --> G[Return LIB-####-NN]
```
