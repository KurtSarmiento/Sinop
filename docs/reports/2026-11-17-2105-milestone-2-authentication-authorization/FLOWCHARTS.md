# FLOWCHARTS.md — Milestone 2: Authentication and Authorization

## MVC Login Flow (Cookie)

```mermaid
flowchart TD
    A[User opens /Account/Login] --> B[Login page rendered]
    B --> C[User submits email/password + antiforgery token]
    C --> D[AccountController.Login POST]
    D --> E[FindUserByEmail]
    E --> F{User found?}
    F -->|No| G[ModelError: Invalid credentials, re-render view]
    F -->|Yes| H[PasswordSignInAsync]
    H --> I{Succeeded?}
    I -->|No| J[ModelError: Invalid credentials]
    I -->|Yes| K[Cookie issued, redirect to dashboard /]
    G --> B
    J --> B
    K --> L[Dashboard accessible, role-based [Authorize]]
```

## JWT API Login Flow

```mermaid
flowchart TD
    A[Android client POST /api/auth/login] --> B[AuthController.Login]
    B --> C[FindUserByEmail]
    C --> D{User found?}
    D -->|No| E[401 Invalid credentials]
    D -->|Yes| F[CheckPasswordSignInAsync]
    F --> G{Password valid?}
    G -->|No| E
    G -->|Yes| H[GetRolesAsync]
    H --> I[TokenService.CreateToken: nameid, email, name, role claims]
    I --> J[200 {token, email, roles, expiresAt}]
    J --> K[Client stores JWT, sends Authorization: Bearer on protected calls]
```

## Authorization Policy Flow

```mermaid
flowchart TD
    A[Request reaches controller/action] --> B{[Authorize] or policy present?}
    B -->|No| C[Request allowed]
    B -->|Yes| D[Authentication scheme checked: Cookie or JWT Bearer]
    D --> E{User authenticated?}
    E -->|No| F[302 to /Account/Login (MVC) or 401 (API)]
    E -->|Yes| G[Policy role requirement evaluated]
    G --> H{User in required role?}
    H -->|No| I[403 Forbidden / AccessDenied]
    H -->|Yes| J[Action executes]