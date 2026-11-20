# FLOWCHARTS.md — Milestone 11: MVC Administrative Dashboard

## MVC Request Flow

```mermaid
flowchart TD
    A[Browser request /MVC/Books] --> B[Area route matches {area:exists}]
    B --> C[BooksController.Index (Area MVC)]
    C --> D[Authorize: user authenticated?]
    D -->|No| E[302 to /Account/Login]
    D -->|Yes| F[BookService.GetBooksAsync]
    F --> G[Query SQL Server: Books + copies + authors]
    G --> H[Render Index.cshtml with PagedResult]
    H --> I[200 HTML response]
```

## Checkout Flow (Circulation)

```mermaid
flowchart TD
    A[POST /MVC/Circulation/Checkout] --> B[Authorize: Librarian/Admin?]
    B -->|No| C[403 Forbidden]
    B -->|Yes| D[Validate antiforgery token]
    D -->|Invalid| E[400]
    D -->|Valid| F[LoanService.CheckoutAsync]
    F --> G{Validation passed?}
    G -->|No| H[ModelError, re-render view]
    G -->|Yes| I[Create loan, copy → Borrowed]
    I --> J[Redirect to active loans]
```

## Antiforgery Protection

```mermaid
flowchart TD
    A[POST form submission] --> B[__RequestVerificationToken present?]
    B -->|No| C[400 Bad Request]
    B -->|Yes| D[Validate token cookie match]
    D -->|Invalid| E[400 Bad Request]
    D -->|Valid| F[Controller action executes]
```

## Role Authorization

```mermaid
flowchart TD
    A[Request to role-restricted page] --> B[User authenticated?]
    B -->|No| C[302 to login]
    B -->|Yes| D{User in required role?}
    D -->|No| E[403 Forbidden / 404]
    D -->|Yes| F[Page renders]