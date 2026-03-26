# GitHub Copilot Instructions — LaconicAndIconic

## Project Overview

LaconicAndIconic is a recipe-sharing web application built with ASP.NET Core MVC. It provides user authentication, recipe management (with categories, ratings, and comments), and a server-rendered UI for home cooks and food enthusiasts.

**Tech Stack**: C# / .NET 8.0 / ASP.NET Core MVC / PostgreSQL / Entity Framework Core 8 / ASP.NET Core Identity / Serilog / xUnit + Moq

## Architecture

Three-layer monolith with clear project boundaries:

- `LaconicAndIconic.Web` — ASP.NET Core MVC entry point (controllers, Razor views, view models, seeding)
- `LaconicAndIconic.BLL` — Business Logic Layer (services, interfaces, DTOs/models)
- `LaconicAndIconic.DAL` — Data Access Layer (EF Core DbContext, entities, Fluent API configurations, migrations)
- `LaconicAndIconic.Tests` — Unit tests (xUnit + Moq)

Dependencies flow **Web → BLL → DAL**. Each layer registers its own services via `ServiceCollectionExtensions`.

### Backend

- Entry point: `LaconicAndIconic.Web/Program.cs` (minimal hosting model)
- Controllers use constructor-injected services from BLL interfaces
- Authentication: ASP.NET Core Identity with cookie-based auth
- Logging: Serilog with Seq sink (`http://localhost:5341`)
- Static code analysis: Roslynator + .NET analyzers enforced via `Directory.Build.props` and `.editorconfig`

### Data Layer

- ORM: Entity Framework Core 8 with Npgsql (PostgreSQL)
- Code-first migrations in `LaconicAndIconic.DAL/Migrations/`
- Connection string: `DefaultConnection` in `appsettings.json` (empty by default, set via User Secrets)
- Fluent API configurations in `LaconicAndIconic.DAL/Data/Configurations/`
- Base entity pattern: `BaseEntity` with `Id`, `CreatedAt`, `UpdatedAt` (auto-set via `SaveChanges` override)
- Identity: `ApplicationUser : IdentityUser<int>` with `int` primary keys
- Central package management via `Directory.Packages.props`

## Conventions

### Naming

- **Files/Classes**: PascalCase (`AuthService.cs`, `RegisterRequest.cs`)
- **Interfaces**: `I` prefix (`IAuthService`)
- **Private fields**: `_camelCase` with underscore prefix (`_authService`)
- **Entities**: singular nouns (`Recipe`, `Category`, `Comment`)
- **ViewModels**: suffixed with `ViewModel` (`LoginViewModel`, `RegisterViewModel`)
- **Test classes**: suffixed with `Tests` (`AuthServiceTests`, `AccountControllerTests`)
- **Test methods**: `Method_Scenario_ExpectedResult` (`Login_ValidCredentials_RedirectsToHome`)
- **Namespaces**: match folder structure (`LaconicAndIconic.BLL.Services`)

### Code Style

- File-scoped namespaces (`namespace X;`)
- Nullable reference types enabled globally
- Implicit usings enabled
- 4-space indentation, CRLF line endings (enforced by `.editorconfig`)
- Roslynator analysis warnings enabled in `.editorconfig`

### Error Handling

- Controller-level `ModelState` validation for user input
- Identity `IdentityResult` pattern for auth operations (check `Succeeded`, iterate `Errors`)
- Structured logging via Serilog (never log sensitive data like passwords)
- `ArgumentNullException.ThrowIfNull()` for service-layer null guards

## Developer Workflow

```powershell
# Restore packages
dotnet restore

# Run development server
dotnet run --project LaconicAndIconic.Web

# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~AuthServiceTests"

# Add a new EF Core migration
dotnet ef migrations add <MigrationName> --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web

# Apply migrations
dotnet ef database update --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web

# Build for production
dotnet publish LaconicAndIconic.Web -c Release
```

## Critical Gotchas

1. **Connection string is in User Secrets** — `appsettings.json` has an empty `DefaultConnection`. Use `dotnet user-secrets` to set it locally.
2. **Migrations require `--startup-project LaconicAndIconic.Web`** — EF tools need the Web project for configuration resolution.
3. **Identity uses `int` keys** — `ApplicationUser : IdentityUser<int>` and `IdentityRole<int>`. All Identity-related code must use `int`, not `string`.
4. **`BaseEntity` timestamps are auto-managed** — `CreatedAt` and `UpdatedAt` are set in `ApplicationDbContext.SaveChanges`. Never set them manually in service code.
5. **No Docker** — the app runs directly on the host. PostgreSQL and Seq must be available locally.
6. **Central package versioning** — package versions are managed in `Directory.Packages.props`. Do not add version attributes to individual `PackageReference` entries in `.csproj` files.

