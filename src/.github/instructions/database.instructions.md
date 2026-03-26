---
description: 'EF Core / PostgreSQL database and migration conventions'
applyTo: '**/LaconicAndIconic.DAL/**, **/Migrations/**'
---

# Database & Migration Standards — EF Core / PostgreSQL

## Entity Rules
- Entities inherit from `BaseEntity` (provides `Id`, `CreatedAt`, `UpdatedAt` — auto-set in `SaveChanges` override).
- `ApplicationUser : IdentityUser<int>` — Identity uses `int` primary keys.
- Entity classes use singular nouns (`Recipe`, `Category`, `Comment`).
- Navigation properties: `= null!;` for references, `= [];` for collections.
- Put entity classes in `LaconicAndIconic.DAL/Entities/`.

## Configuration Rules
- Use Fluent API for all entity configurations — no data annotations on entities.
- Configuration classes go in `LaconicAndIconic.DAL/Data/Configurations/` and implement `IEntityTypeConfiguration<T>`.
- Configurations are auto-discovered via `builder.ApplyConfigurationsFromAssembly()`.

## Migration Rules
- Migrations live in `LaconicAndIconic.DAL/Migrations/`.
- Always use `--project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web` flags with EF tools.
- Keep migrations reversible. Never hardcode environment-specific values.
- Never modify existing migration files — create new migrations for schema changes.
- Timestamps (`CreatedAt`, `UpdatedAt`) are managed by `ApplicationDbContext.SaveChanges()` — never set them manually.

## Connection & Registration
- Connection string `DefaultConnection` is stored in User Secrets (empty in `appsettings.json`).
- DAL services are registered via `AddDataAccessLayer()` in `ServiceCollectionExtensions.cs`.
- DbContext uses `UseNpgsql()` for PostgreSQL.
