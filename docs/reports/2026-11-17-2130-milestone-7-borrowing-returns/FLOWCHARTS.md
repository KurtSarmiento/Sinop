# FLOWCHARTS.md — Milestone 7: Borrowing and Return Operations

## Checkout Flow (Transactional)

```mermaid
flowchart TD
    A[POST /api/loans/checkout] --> B[Validate model + role Librarian/Admin]
    B --> C[LoanService.CheckoutAsync: BeginTransaction]
    C --> D[User exists?]
    D -->|No| E[409: user not found]
    D -->|Yes| F[Book exists?]
    F -->|No| G[409: book not found]
    F -->|Yes| H{Duplicate active loan for borrower+title?}
    H -->|Yes| I[409: duplicate loan]
    H -->|No| J{Active loan count < 3?}
    J -->|No| K[409: borrowing limit reached]
    J -->|Yes| L[Find available copy Status=Available]
    L -->|None| M[409: no available copies]
    L -->|Found| N[Re-fetch copy inside transaction, re-check Status=Available]
    N -->|Not available| O[409: copy no longer available]
    N -->|Available| P[copy.Status=Borrowed, create Loan Active, DueDate=+14d]
    P --> Q[SaveChangesAsync]
    Q --> R[CommitTransaction]
    R --> S[201 LoanResponse]
```

## Return Flow (Transactional)

```mermaid
flowchart TD
    A[POST /api/loans/return] --> B[Validate model + role Librarian/Admin]
    B --> C[LoanService.ReturnAsync: BeginTransaction]
    C --> D[Loan exists?]
    D -->|No| E[409: loan not found]
    D -->|Yes| F{Loan Status == Active?}
    F -->|No| G[409: duplicate return / already returned]
    F -->|Yes| H[loan.ReturnDate=Now, loan.Status=Returned]
    H --> I[copy.Status=Available]
    I --> J[SaveChangesAsync]
    J --> K[CommitTransaction]
    K --> L[200 LoanResponse]
```

## Eligibility Flow

```mermaid
flowchart TD
    A[GET /api/loans/eligibility/{userId}] --> B[ValidateBorrowerEligibilityAsync]
    B --> C[User exists?]
    C -->|No| D[eligible: false]
    C -->|Yes| E[Active loan count < 3?]
    E -->|Yes| F[eligible: true]
    E -->|No| G[eligible: false]
    F --> H[200 {userId, eligible, maxActiveLoans:3}]
    G --> H