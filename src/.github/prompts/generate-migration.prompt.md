---
description: "Generate EF Core migration via the main orchestrator flow."
agent: Orchestrator
argument-hint: "**Specify:** migration name (e.g., 'AddRecipeImageUrl') | Optional: --apply"
---

# Generate Migration Prompt

Create and execute EF Core migration to add new entity or modify existing schema.

## Task

1. Validate entity/configuration changes in `LaconicAndIconic.DAL`
2. Generate new EF Core migration with UP/DOWN logic
3. Review generated migration code
4. Optionally apply migration to the development database
5. Verify schema consistency

## Context

**Reference**: [database.instructions.md](../instructions/database.instructions.md), [backend.instructions.md](../instructions/backend.instructions.md)

**Key Patterns**:
- Entities inherit from `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt` auto-managed)
- Fluent API configurations in `LaconicAndIconic.DAL/Data/Configurations/`
- Migrations always require `--project LaconicAndIconic.DAL --startup-project LaconicAndIconic.Web`
- Connection string is in User Secrets — `DefaultConnection` in `appsettings.json` is empty

## Example

```powershell
# Generate migration for entity changes
/generate-migration AddRecipeImageUrl

# Generate and apply immediately
/generate-migration AddRecipeImageUrl --apply
```

## Output Format

```
MIGRATION GENERATED
===================

File: LaconicAndIconic.DAL/Migrations/20260325_AddRecipeImageUrl.cs

Generated UP:
  - Add column: ImageUrl (string, nullable) to Recipes table

Generated DOWN:
  - Drop column: ImageUrl from Recipes table

MIGRATION STATUS
----------------
✓ Applied to development database (0.4s)
```

NEXT STEPS
----------
- Update serializer config for new fields
- Run tests to verify schema
```

## Success Criteria

✅ Migration generated with proper UP/DOWN
✅ Applied to development database successfully
✅ No schema conflicts or errors
✅ Serializer/schema config updated for new fields

