# Skill: EF Core Migration Runbook

Use this skill for schema change operations and validation in the LaconicAndIconic project.

## Typical Tasks
- Generate migration from entity/configuration changes
- Apply migrations to the development database
- Validate schema/mapping consistency
- Check migration status

## Common Commands
```powershell
# Generate migration
dotnet ef migrations add <MigrationName> --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web

# Check migration status
dotnet ef migrations list --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web

# Apply migrations
dotnet ef database update --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web

# Generate SQL script (for review)
dotnet ef migrations script --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web

# Remove last unapplied migration
dotnet ef migrations remove --project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web
```

## Guardrails
- Review generated migration code before applying.
- Never modify existing migration files — create new migrations instead.
- Connection string is in User Secrets — ensure it's configured before running.
- `BaseEntity` timestamps (`CreatedAt`, `UpdatedAt`) are auto-managed in `SaveChanges` — don't configure defaults in migrations.
- Always use both `--project` and `--startup-project` flags.

