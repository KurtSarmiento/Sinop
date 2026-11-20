# FLOWCHARTS.md — Milestone 9: Reading Lists

## Create Reading List Flow

```mermaid
flowchart TD
    A[POST /api/readinglists + {name, description}] --> B[Extract userId from JWT nameid claim]
    B --> C[ReadingListService.CreateReadingListAsync]
    C --> D[Create ReadingList entity with UserId]
    D --> E[SaveChangesAsync]
    E --> F[201 Created + ReadingListResponse]
```

## Add Item Flow

```mermaid
flowchart TD
    A[POST /api/readinglists/{listId}/items + {bookId}] --> B[Extract userId from JWT]
    B --> C[ReadingListService.AddItemAsync]
    C --> D[List exists AND owned by userId?]
    D -->|No| E[404 / 409]
    D -->|Yes| F[Book exists?]
    F -->|No| G[409: book not found]
    F -->|Yes| H{Book already in list?}
    H -->|Yes| I[409: duplicate item]
    H -->|No| J[Create ReadingListItem]
    J --> K[SaveChangesAsync]
    K --> L[200 Updated ReadingListResponse]
```

## Remove Item Flow

```mermaid
flowchart TD
    A[DELETE /api/readinglists/{listId}/items/{itemId}] --> B[Extract userId from JWT]
    B --> C[ReadingListService.RemoveItemAsync]
    C --> D[Item exists AND its ReadingList owned by userId?]
    D -->|No| E[404: not found or access denied]
    D -->|Yes| F[Remove ReadingListItem]
    F --> G[SaveChangesAsync]
    G --> H[204 No Content]
```

## Ownership Enforcement

```mermaid
flowchart TD
    A[Any list/item operation] --> B[Query filters by UserId == JWT nameid]
    B --> C{Record found?}
    C -->|No| D[404: not found or access denied]
    C -->|Yes| E[Proceed with operation]
    D --> F[User cannot access another user's lists]