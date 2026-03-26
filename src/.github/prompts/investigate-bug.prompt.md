---
description: "Investigate runtime issues with orchestrated root-cause analysis."
agent: Orchestrator
argument-hint: "**Specify:** error message/log line or file path | Optional: --seq (check Seq logs)"
---

# Investigate Bug Prompt

Perform root cause analysis for errors, unexpected behavior, or test failures with systematic debugging workflow.

## Task

1. Collect error context (logs, stack traces, test output)
2. Reproduce issue in development environment
3. Identify root cause using systematic debugging
4. Rule out environmental factors
5. Document findings and provide fix recommendation
6. Propose test case to prevent regression

## Context

**Reference**: [database.instructions.md](../instructions/database.instructions.md), [backend.instructions.md](../instructions/backend.instructions.md)

**Common Error Sources**:
- **Database**: Schema drift, unapplied migrations, connection string misconfiguration
- **Identity**: Wrong key type (`int` vs `string`), missing Identity registration
- **Configuration**: User Secrets not set, missing `DefaultConnection`
- **EF Core**: Navigation property misconfiguration, missing Fluent API config

## Example

```powershell
# Investigate error from console output
/investigate-bug "InvalidOperationException: No service for type 'IAuthService' has been registered"

# Investigate test failure
/investigate-bug "Assert.IsType() Failure: Expected RedirectToActionResult, got ViewResult"

# Investigate with Seq log analysis
/investigate-bug "NpgsqlException: relation does not exist" --seq
```

## Success Criteria

✅ Root cause identified with evidence
✅ Reproducible in development environment
✅ Fix recommendation provided
✅ Regression test proposed

