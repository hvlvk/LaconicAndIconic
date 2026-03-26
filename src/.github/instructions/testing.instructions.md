---
description: 'xUnit + Moq testing conventions'
applyTo: '**/LaconicAndIconic.Tests/**'
---

# Testing Standards — xUnit + Moq

## Scope Rules
- Unit tests for BLL services (mock Identity managers, loggers, repositories).
- Controller tests mock BLL interfaces — test action results and ModelState behavior.
- Use Arrange/Act/Assert pattern with clear section comments (`// Arrange`, `// Act`, `// Assert`).

## Naming & Structure
- Test classes: `{ClassName}Tests` (e.g., `AuthServiceTests`, `AccountControllerTests`).
- Test methods: `Method_Scenario_ExpectedResult` (e.g., `Login_ValidCredentials_RedirectsToHome`).
- Mirror source structure: controller tests in `Controllers/`, service tests at project root.
- Test project references `LaconicAndIconic.Web` (which transitively includes BLL and DAL).

## Mocking Patterns
- Use `Mock<T>` from Moq. Set up with `.Setup()` and verify with `.Verify()`.
- For `UserManager<ApplicationUser>`: create via helper that provides `Mock<IUserStore<ApplicationUser>>`.
- For `SignInManager<ApplicationUser>`: create via helper that wraps UserManager mock.
- Use `NullLogger<T>.Instance` for logger dependencies in service tests.
- For controller tests: mock `IUrlHelper`, set `ControllerContext` with `DefaultHttpContext`.

## Execution Rules
- Tests run directly on the host via `dotnet test` (no Docker).
- Use `--filter "FullyQualifiedName~ClassName"` for targeted test runs.
- All tests must be deterministic and independent — no shared mutable state.

## Quality Rules
- Assert specific types (`Assert.IsType<RedirectToActionResult>`) not just null checks.
- Verify mock interactions to ensure service methods were called with expected parameters.
- Use realistic test data — valid emails, passwords meeting Identity requirements.
