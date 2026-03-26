# Skill: Test Workflow — xUnit + Moq

Use this skill for command-level test execution in the LaconicAndIconic project.

## Typical Tasks
- Run full test suite
- Run filtered class/method tests
- Run with verbose or detailed output

## Common Commands
```powershell
# Run full suite
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~AuthServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~Login_ValidCredentials_RedirectsToHome"

# Verbose output
dotnet test --verbosity detailed

# Run with logger output
dotnet test --logger "console;verbosity=detailed"
```

## Notes
- Tests run directly on the host (no Docker).
- Prefer narrow test scope first (`--filter`), then expand to broader regression.
- Test project is `LaconicAndIconic.Tests` — it references `LaconicAndIconic.Web`.
- Mock Identity managers (`UserManager`, `SignInManager`) via helper methods in test classes.

