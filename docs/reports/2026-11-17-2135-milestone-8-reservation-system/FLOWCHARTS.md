# FLOWCHARTS.md — Milestone 8: Reservation System

## Create Reservation Flow

```mermaid
flowchart TD
    A[POST /api/reservations] --> B[Validate model]
    B --> C[CreateReservationAsync: BeginTransaction]
    C --> D[User exists?]
    D -->|No| E[409: user not found]
    D -->|Yes| F[Book exists?]
    F -->|No| G[409: book not found]
    F -->|Yes| H{Duplicate Queued reservation for borrower+title?}
    H -->|Yes| I[409: duplicate reservation]
    H -->|No| J[Create Reservation: Queued, pickupDeadline=+2d]
    J --> K[SaveChangesAsync]
    K --> L[CommitTransaction]
    L --> M[Compute queue position: earlier Queued for same book + 1]
    M --> N[201 ReservationResponse]
```

## Cancel Reservation Flow

```mermaid
flowchart TD
    A[POST /api/reservations/{id}/cancel?userId=X] --> B[Reservation exists?]
    B -->|No| C[404]
    B -->|Yes| D{userId matches reservation owner?}
    D -->|No| E[404: not found / not owned]
    D -->|Yes| F{Status Queued or ReadyForPickup?}
    F -->|No| G[404: cannot cancel]
    F -->|Yes| H[Status=Cancelled]
    H --> I{AllocatedCopyId set?}
    I -->|Yes| J[copy.Status=Available if Reserved; clear AllocatedCopyId]
    I -->|No| K[Skip]
    J --> K
    K --> L[SaveChangesAsync]
    L --> M[AllocateNextAsync(bookId): head of queue gets next Available copy]
    M --> N[200 Cancelled]
```

## Expire Reservation Flow

```mermaid
flowchart TD
    A[POST /api/reservations/process-expired] --> B[Find Queued reservations with PickupDeadline < Now]
    B --> C[For each: ExpireReservationAsync]
    C --> D[Status=Expired]
    D --> E{AllocatedCopyId set?}
    E -->|Yes| F[copy.Status=Available; clear AllocatedCopyId]
    E -->|No| G[Skip]
    F --> G
    G --> H[Notify borrower: reservation expired]
    H --> I[AllocateNextAsync: head of queue gets copy]
    I --> J[200 Processed]
```

## Allocate Returned Copy Flow

```mermaid
flowchart TD
    A[POST /api/reservations/{bookId}/allocate-returned-copy?copyId=X] --> B[copy exists AND copy.Status==Available?]
    B -->|No| C[409: copy not available]
    B -->|Yes| D[Find head-of-queue Queued reservation for bookId (earliest ReservedAt)]
    D -->|None| E[false: no queue; copy stays Available]
    D -->|Found| F[copy.Status=Reserved]
    F --> G[reservation.Status=ReadyForPickup, AllocatedCopyId=copy.Id, pickupDeadline=+2d]
    G --> H[SaveChangesAsync]
    H --> I[Notify borrower: ready for pickup]
    I --> J[200 Allocated]
```

## Queue Ordering (First-Come, First-Served)

```mermaid
flowchart TD
    A[Multiple Queued reservations for same book] --> B[Order by ReservedAt ascending]
    B --> C[Earliest = head of queue (position 1)]
    C --> D[AllocateNextAsync picks head when copy Available]
    D --> E[Copy allocated to first borrower who reserved]