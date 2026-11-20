# FLOWCHARTS.md — Milestone 15: End-to-End Integration and Testing

## E2E Test Scenario A: Catalog Synchronization

```mermaid
flowchart TD
    A[Librarian imports book via MVC] --> B[Book stored in SQL Server]
    B --> C[Librarian registers physical copies]
    C --> D[Accession numbers generated, unique constraint]
    D --> E[Android retrieves book via GET /api/books]
    E --> F[Android displays metadata + availability]
    E -->|Open Library unreachable| G[Local catalog endpoints still work ✓]
```

## E2E Test Scenario B: Borrowing

```mermaid
flowchart TD
    A[Librarian checks out book id=2] --> B[POST /api/loans/checkout]
    B --> C[Validate borrower eligibility & limit]
    C --> D[Validate copy availability]
    D --> E[Assign physical copy LIB-0003]
    E --> F[Create loan, copy → Borrowed]
    F --> G[Android displays active loan]
    G -->|Duplicate loan same book+user| H[HTTP 409: already has active loan ✓]
    G -->|Checkout unavailable copy| I[HTTP 409: no available copies ✓]
```

## E2E Test Scenario C: Reservation

```mermaid
flowchart TD
    A[All copies borrowed] --> B[Borrower reserves title via Android]
    B --> C[POST /api/reservations]
    C --> D[Validate no duplicate active reservation]
    D --> E[Create reservation with queuePosition]
    E --> F[MVC displays reservation]
    F --> G[Copy returned → process queue]
    G -->|Duplicate reservation same title| H[HTTP 409: already reserved ✓]
    G --> I[Cancel reservation → releases queue slot]
```

## E2E Test Scenario D: Return and Notifications

```mermaid
flowchart TD
    A[Librarian processes return] --> B[POST /api/loans/return]
    B --> C[Record return date, status → Returned]
    C --> D[Update copy status → Available]
    D --> E[Check reservation queue for returned book]
    E --> F[Allocate copy to next queued borrower]
    F --> G[Generate notifications]
    G --> H[Android displays updated availability]
    G -->|Duplicate notification| I[CreateIfNotExistsAsync prevents dup ✓]
```

## E2E Test Scenario E: Offline Catalog (Android)

```mermaid
flowchart TD
    A[Android retrieves catalog from API] --> B[Cache books in Room]
    B --> C[Network disabled]
    C --> D[Display cached books from Room]
    D --> E[User browses offline ✓]
    E -->|Network restored| F[Refresh cache from API]
```

## E2E Test Scenario F: Overdue Automation

```mermaid
flowchart TD
    A[Create overdue test loan] --> B[due date in past]
    B --> C[Execute overdue processing / Hangfire job]
    C --> D[Find active past-due loans]
    D --> E[Create overdue notification]
    E -->|Duplicate notification| F[CreateIfNotExistsAsync → skip ✓]
    E --> G[Android displays overdue notification]
```

## Business Invariant Verification

```mermaid
flowchart TD
    A[Checkout attempt] --> B{Copy available?}
    B -->|No| C[HTTP 409 rejected ✓]
    B -->|Yes| D{User has active loan same book?}
    D -->|Yes| E[HTTP 409 rejected ✓]
    D -->|No| F[Check user active loan count < 3]
    F -->|No| G[HTTP 409: limit reached ✓]
    F -->|Yes| H[Create loan, copy → Borrowed]
    
    A2[Reservation attempt] --> B2{Active reservation same book+user?}
    B2 -->|Yes| C2[HTTP 409 rejected ✓]
    B2 -->|No| D2[Create reservation, queuePosition assigned]
    
    A3[Return attempt] --> B3{Loan already returned?}
    B3 -->|Yes| C3[HTTP 409 rejected ✓]
    B3 -->|No| D3[Record return, copy → Available]
```

## Acceptance Criteria Summary

| Criterion | Status |
|---|---|
| Catalog sync end-to-end | ✓ Local API works (Open Library external sandbox-blocked) |
| Borrowing with business rules | ✓ Duplicate/limit/unavailable all → 409 |
| Reservation with queue ordering | ✓ Creation, duplicate prevention, cancellation |
| Return with inventory update | ✓ Copy → Available, active loans cleared |
| Notifications | ✓ Overdue + reservation messages generated |
| Offline catalog | ✓ Room cache implemented, build-verified |
| Overdue automation | ✓ Overdue detected, notification generated, dup prevented |
