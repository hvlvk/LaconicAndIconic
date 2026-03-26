---
description: 'C# / .NET 8 backend coding standards'
applyTo: '**/*.cs'
---

# Backend Standards — C# / .NET 8 / ASP.NET Core MVC

## Core Architecture
- Three-layer structure: Web → BLL → DAL. Never bypass layers (e.g., controllers must not reference DAL directly).
- Services live in `LaconicAndIconic.BLL/Services/` and implement interfaces in `LaconicAndIconic.BLL/Interfaces/`.
- Register new services in the appropriate `ServiceCollectionExtensions.cs` (`AddBusinessLogicLayer` or `AddDataAccessLayer`).
- Use constructor injection for all dependencies. Never use service locator patterns.

## Language & Framework Rules
- Use file-scoped namespaces (`namespace X;`).
- Nullable reference types are enabled — handle nullability explicitly (`string?`, `null!` for nav props).
- Use `async`/`await` throughout. Do not use `.Result` or `.Wait()`.
- Do not add `ConfigureAwait(false)` — this is an ASP.NET Core app, not a reusable library.
- Use `ArgumentNullException.ThrowIfNull()` for service-method parameter validation.
- Prefer collection expressions (`[]`) over `new List<T>()` for initializers.

## Naming & Style
- PascalCase for public members, `_camelCase` for private fields.
- `I` prefix for interfaces. `ViewModel` suffix for view models. `Tests` suffix for test classes.
- Follow `.editorconfig` rules — Roslynator and .NET analyzers are enforced at build time.

## Domain Modeling Rules
- Entities inherit from `BaseEntity` (provides `Id`, `CreatedAt`, `UpdatedAt`).
- `ApplicationUser : IdentityUser<int>` — Identity uses `int` keys throughout.
- Navigation properties use `null!` default: `public Category Category { get; set; } = null!;`
- Collection navigation properties use `[]`: `public ICollection<Rating> Ratings { get; } = [];`
- Fluent API configurations go in `LaconicAndIconic.DAL/Data/Configurations/`.

## Logging
- Use Serilog structured logging via `ILogger<T>`.
- Use message templates with named parameters: `_logger.LogInformation("User {Email} logged in", email);`
- Never log passwords, tokens, or other credentials.
