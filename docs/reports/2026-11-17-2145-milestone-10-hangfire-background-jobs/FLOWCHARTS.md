# FLOWCHARTS.md — Milestone 10: Hangfire and Background Jobs

## Overdue Notification Job Flow

```mermaid
flowchart TD
    A[RecurringJob: Daily 8:00 AM] --> B[OverdueNotificationJob.ExecuteAsync]
    B --> C[Query active loans where DueDate < Now]
    C --> D{Overdue loans found?}
    D -->|No| E[Log: 0 overdue, complete]
    D -->|Yes| F[For each overdue loan]
    F --> G{Already notified for this loan?}
    G -->|Yes| H[Skip - idempotent]
    G -->|No| I[CreateNotification: overdue message]
    H --> J[Next loan]
    I --> J
    J --> K[Log completion]
```

## Reservation Expiration Job Flow

```mermaid
flowchart TD
    A[RecurringJob: Every 15 min] --> B[ReservationExpirationJob.ExecuteAsync]
    B --> C[ReservationService.ProcessExpiredReservationsAsync]
    C --> D[Find reservations with PickupDeadline < Now]
    D --> E{Expired Queued reservations?}
    E -->|No| F[Complete]
    E -->|Yes| G[For each: Status=Expired]
    G --> H{AllocatedCopyId set?}
    H -->|Yes| I[copy.Status=Available; clear AllocatedCopyId]
    H -->|No| J[Skip]
    I --> J
    J --> K[Notify borrower: reservation expired]
    K --> L[AllocateNextAsync: head of queue gets copy]
    L --> M[SaveChanges]
    M --> F
```

## Hangfire Dashboard Authorization

```mermaid
flowchart TD
    A[Request /hangfire] --> B[User authenticated?]
    B -->|No| C[401 Unauthorized]
    B -->|Yes| D{User in Administrator role?}
    D -->|No| E[401 Unauthorized / access denied]
    D -->|Yes| F[Hangfire Dashboard displayed]