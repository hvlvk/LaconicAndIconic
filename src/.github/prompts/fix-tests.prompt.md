---
description: "Diagnose and fix failing tests through a single orchestrator workflow."
agent: Orchestrator
argument-hint: "**Specify:** test class name or namespace | Optional: --filter=MethodName"
---

# Fix Tests Prompt

Debug and fix failing test cases to restore test suite health.

## Task

1. Run the complete test suite or specified test class
2. Identify all failures and assertion errors
3. Analyze root causes (assertion mismatch, missing mock setup, arrange error)
4. Fix test code to match implementation (or vice versa)
5. Re-run tests to confirm all pass
6. Report changes made and test results

## Context

**Reference**: [testing.instructions.md](../instructions/testing.instructions.md)

**Key Points**:
- Tests run directly on the host via `dotnet test`
- Use orchestrated workflow: run → parse → fix → verify
- Mock Identity managers and BLL interfaces using Moq
- Tests must be deterministic and isolated

## Example

```powershell
# Run and fix all tests
/fix-tests

# Fix a specific test class
/fix-tests LaconicAndIconic.Tests/Controllers/AccountControllerTests.cs

# Fix a single failing test method
/fix-tests --filter="Login_ValidCredentials_RedirectsToHome"
```

## Success Criteria

✅ All specified tests pass
✅ Exit code 0 from `dotnet test`
✅ No skipped tests (unless intentional)
✅ No warnings in test output
✅ Code changes follow coding standards

