# AGENTS.md – PassKee Codebase Guide

## Architecture Overview

**PassKee** is a zero-knowledge password and secrets management platform built on ASP.NET Core (.NET 10, C# 13).

### Key Projects & Roles

| Project | Role | Dependencies & Constraints |
|---|---|---|
| `PassKee.Api` | ASP.NET Core REST API host | Depends on `PassKee.Business`, `PassKee.Api.Shared`, `PassKee.Orm` |
| `PassKee.Api.Shared` | API contracts, DTOs, request/response records, `ApiUrl` constants | Zero business logic; shared only with API, Web, and Server |
| `PassKee.Business` | Core domain services (`IAuthService`), DAOs, business rules, Autofac DI | **CRITICAL: Must NEVER depend on `PassKee.Api.Shared`** |
| `PassKee.Business.Common` | Reusable utilities (`CryptoUtils`, `StringUtils`), exceptions, constants | Shared across all layers (Domain, API, Web, Tests) |
| `PassKee.Orm` | NHibernate entity definitions (`UserEntity`), Fluent mappings | Extends `AEntity`, uses UUID v7 |
| `PassKee.Migrations` | FluentMigrator console application (PostgreSQL) | Manages database schema migrations |
| `PassKee.Web` | Blazor WebAssembly client application | Interactive client SPA; uses Fluxor, `ApiService`, code-behind pattern |
| `PassKee.Web.Core` | Reusable UI components, modals, toasts, forms, layouts, client services | Razor Class Library; shared between `PassKee.Web` and `PassKee.Web.Server` |
| `PassKee.Web.Server` | ASP.NET Core host for Blazor application | Hosts static SSR landing pages and boots Interactive WebAssembly SPA |
| `PassKee.WorkerServices` | Background queue processors and hosted services | Background processing |
| `PassKee.Tests.Unit` | Fast unit tests for utilities, cryptography, extensions | Uses xUnit |
| `PassKee.Tests.Integration.Api` | End-to-end API integration tests | Uses `ApiCustomWebApplicationFactory` & PostgreSQL test database |

---

## Strict Architectural Rules

### 1. Layer Dependency Isolation
- `PassKee.Business` **must never** reference `PassKee.Api.Shared`.
- Only `PassKee.Api`, `PassKee.Web`, and `PassKee.Web.Server` are allowed to reference `PassKee.Api.Shared`.
- Domain services (`IAuthService`, etc.) and DAOs must accept domain models or primitive parameters (`string email`, `byte[] authHash`, `byte[] authSalt`, etc.), **never** API/frontend DTOs (e.g. `RegisterRequest`, `LoginRequest`).

### 2. Request/Handler Pattern (CQRS, NOT MediatR)
All API endpoints use a custom `IAsyncRequestHandler<TRequest, TResponse>` pattern.

**Controller layout:**
Controllers must be organized into feature folders with thin endpoints that delegate to action handlers:
```
PassKee.Api/Controllers/<Feature>/
  <Feature>Controller.cs         ← Thin controller, routes and status codes only
  Actions/
    <Action>RequestHandler.cs    ← Action business flow execution
```

**Controller dispatching pattern:**
Controllers extend `MainApiControllerBase(ILifetimeScope scope)`:
```csharp
// Endpoint with response:
[HttpPost("login")]
public Task<IActionResult> Login([FromBody] LoginRequest request)
    => this.RequestAsync()
        .For<AuthResponse>()
        .With(request);

// Endpoint with no content (void):
[HttpPost("action")]
public Task<IActionResult> DoAction([FromBody] ActionRequest request)
    => this.RequestAsync(request);
```

Handlers implement `IAsyncRequestHandler<TRequest, TResponse>` and are automatically registered by Autofac in `ApiModule.cs` via `builder.RegisterAssemblyTypes(assembly).AsClosedTypesOf(typeof(IAsyncRequestHandler<,>))`.

---

## Cryptography Guidelines

Zero-knowledge cryptography is foundational to PassKee.

### 1. Library
- Always use **`BouncyCastle.Cryptography`** on both client and server.

### 2. Storage & Database Formats
- **Raw Binary Only**: All cryptographic keys, salts, and hashes in `UserEntity` and PostgreSQL tables must be stored as raw byte arrays (`byte[]`, PostgreSQL `bytea`).
- **No Base64 in Database or Entities**: Entities and database columns must never store Base64 strings for crypto data.
- **Base64 Conversion Boundary**: Base64 encoding/decoding is strictly restricted to API DTO boundaries and Web responses via AutoMapper profiles (`PassKee.Api/Profiles/UserProfile.cs`).
- **Entity Documentation**: All cryptographic properties on `UserEntity` must contain XML documentation comments clarifying their algorithm, role, and storage format.

### 3. Shared Cryptographic Workflows (`CryptoUtils`)
Common cryptographic workflows live in `PassKee.Business.Common/Utils/CryptoUtils.cs` to eliminate duplicated boilerplate between client, server, and test suites:
- `DeriveMasterKey(...)`: Argon2id KDF on `(Password + SecretKey)` with `AuthSalt`.
- `ComputeAuthHash(...)`: HMAC-SHA256 of `MasterKey` with context `"auth_login"`.
- `GenerateUserKeyEnvelope(...)`: Generates RSA keypair + random 32-byte Vault Key, encrypted with `MasterKey` via AES-GCM.
- `PrepareClientRegistration(...)`: Prepares complete registration package for Web and test flows.
- `AesGcmEncrypt(...)` / `AesGcmDecrypt(...)`: AES-256-GCM AEAD encryption with 12-byte random nonce prepended.
- `GenerateArgon2idHash(...)`: Argon2id hashing with configurable memory, iterations, and parallelism.

### 4. JWT Authentication
- All JWT token generation and validation must use `IJwtAuthService`. Never create ad-hoc JWT generation helpers in controllers or services.

---

## ORM & Database (NHibernate)

- **Database**: PostgreSQL (default port `5433` in development/testing `appsettings.json`).
- **Base Entity**: All entities inherit from `AEntity` (`Id: Guid`, `CreatedAt`, `UpdatedAt`, `DeletedAt`, `IsDeleted`, `IsNew`).
- **ID Generation**: All IDs are **UUID v7** generated via `GuidV7Generator` on the application side.
- **Mappings**: FluentNHibernate mappings live in `PassKee.Orm/Mapping/Entities/` and extend `BaseGuidMappings<T>`.
- **Naming Conventions**: `SnakeCaseConvention` automatically converts `PascalCase` properties and class names to `snake_case` in PostgreSQL.
- **Transactions**: Every request is wrapped in an ambient transaction via `CommitPerformerMiddleware` (commits on success, rolls back on exception). Do not commit transactions manually inside handlers.
- **DAOs**:
  - DAOs implement `IDomainService` and are resolved through Autofac.
  - **No Dummy DAO Wrappers**: Do not write redundant wrapper methods like `SaveAsync(entity)` that merely call `Session.SaveOrUpdateAsync(entity)`.
- **Migrations**: FluentMigrator migrations are located in `PassKee.Migrations`. Run via:
  ```bash
  dotnet run --project PassKee.Migrations
  ```

---

## Blazor Web Client (`PassKee.Web`)

### 1. Code-Behind Separation
- Always separate Blazor markup and C# logic:
  - `Feature.razor`: HTML markup, layout, UI binding.
  - `Feature.razor.cs`: Partial code-behind class containing properties, event handlers, and injected dependencies.
- Use `[Inject]` attributes in code-behind rather than `@inject` directives in `.razor` markup.
- Naming conventions: route-level components use the `Page` suffix, reusable UI fragments use the `Block` suffix, and layout components use the `Layout` suffix (for example, `VaultPage.razor`, `VaultWelcomeBlock.razor`, `MainLayout.razor`). Keep the Razor filename and partial class name aligned.
- When a page or component grows beyond roughly 200 lines, split cohesive UI sections into block components with their own markup and code-behind where logic is needed.
- Place child components in a `Parts/` subdirectory of the parent component's directory. Keep their `.razor` and `.razor.cs` files together, align namespaces with the directory structure, and import the `Parts` namespace in the parent where needed.
- For Blazor component invocations with multiple parameters, put each attribute on a separate line for readable diffs.

### 2. API Communication
- All HTTP requests to the backend must go through `ApiService` (e.g. `PassKee.Web/Services/Http/ApiService.<Feature>.cs`).
- **Never** make direct `HttpClient` calls inside Blazor components.

### 3. State Management
- Use **Fluxor** for client-side state management:
  - Store actions in `Store/<Feature>/<Feature>Actions.cs`.
  - State records in `Store/<Feature>/<Feature>State.cs`.
  - Reducers in `Store/<Feature>/<Feature>Reducers.cs`.

### 4. Mandatory Use of Shared UI Components (`PassKee.Web.Core/Ui/Shared/Components/`)
- **Always Reuse Existing Shared Components**: Never hand-craft raw HTML inputs, buttons, tables, dropdowns, popovers, or modals in pages. Always use standard components from `PassKee.Web.Core.Ui.Shared.Components`:
  - **Inputs & Text Fields**: `AppInputText`, `SecretInput`, `InputTextField`, `InputTextareaField`, `InputNumericField`, `InputDateField` (with `AppCalendar`), `InlineTextEdit`.
  - **Select & Pickers**: `AppSelect`, `AppSelectItem`, `BooleanSelect`, `EnumSelect`.
  - **Switches & Toggles**: `AppCheckbox`, `AppSwitch`.
  - **Buttons & Spinners**: `AppButton`, `AppSpinner`, `AppSkeleton`.
  - **Badges & Chips**: `AppBadge`, `AppChip`.
  - **Containers & Layout**: `AppCard` (`AppCardHeader`, `AppCardBody`, `AppCardFooter`), `AppTabs`, `AppTab`, `PaginatedItemsListBlock`.
  - **Overlays & Dialogs**: `AppDropdown`, `AppDropdownItem`, `AppPopover`, `AppModalWindow`, `AppConfirmationModal`, `AppToastContainer`, `AppToastItem`.
  - **Data Display**: `AppTable` (wraps QuickGrid with Tailwind styling, sorting, and pagination), `UserAvatarBlock`.
  - **Validation**: `CustomValidationSummary`, `CustomValidationMessage`.
- **Consistent Styling**: All shared components follow standard Tailwind CSS classes. Do not create auxiliary CSS/LESS stylesheets when Tailwind utilities can achieve the design.
- **Code-Behind Rule**: Every new or modified shared component must strictly separate markup (`.razor`) and C# logic (`.razor.cs`). Inline `@code` blocks in `.razor` files are forbidden.

---

## Dependency Injection (Autofac)

- PassKee uses **Autofac modules** located in `PassKee.Business/Di/Autofac/Modules/` and `PassKee.Api/Di/Autofac/Modules/`.
- Lifetimes:
  - `IDomainService` → `InstancePerDependency`
  - `IScopedDomainService` → `InstancePerLifetimeScope`
- Handlers and DAOs are auto-discovered and registered by assembly scanning.

---

## Exception Handling

- Domain exceptions implement `IDomainException` and are caught by `ExceptionHandlerActionFilter`.
- Handled domain errors return HTTP 400 with a standard payload:
  ```json
  { "status": "fail", "errorCode": "ExceptionClassName", "message": "..." }
  ```
- Unhandled non-domain exceptions return HTTP 500 `"Server error"` and are logged.
- Always verify `_apiRequestService.IsAuthorized()` before retrieving the current user ID to avoid unneeded `UserNotFoundException` on anonymous endpoints.

---

## Testing Conventions

The test suite consists of:

| Test Project | Scope | Details |
|---|---|---|
| `PassKee.Tests.Unit` | Unit testing | Tests utility classes (`CryptoUtilsTest`, `Base64UtilsTest`, `StringUtilsTest`, etc.) and extensions. |
| `PassKee.Tests.Integration.Api` | Full API integration | Boots test server using `ApiCustomWebApplicationFactory`, auto-cleans DB before each test via `IDbCleanUpService`. |

### Testing Guidelines:
- **No Boilerplate in Tests**: Common setup routines and crypto helpers belong in `CryptoUtils` or `PassKee.Business.Testing`.
- Verify database modifications by querying the database in assertions.
- Run the full test suite before committing:
  ```bash
  dotnet test PassKee.sln
  ```

---

## Code Quality & Style Rules

1. **Language**: Write all code comments, docstrings, commit messages, and documentation exclusively in **English**.
2. **No Redundant Comments**: Do not write superficial action comments (e.g., `// Added: Invalid` or `// Set name`). Comments should explain *why*, not restate *what*.
3. **C# Modern Features**: Use file-scoped namespaces, nullable reference types, records for immutable DTOs, and primary constructors where appropriate.

