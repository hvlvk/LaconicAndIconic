# Feature: Enforce Architectural Standards Across All Layers (Issue #28)

## Summary

Refactor the codebase to strictly enforce architectural best practices: enable `TreatWarningsAsErrors` globally, introduce the Result pattern for service returns, add global exception handling middleware, abstract data access behind repository interfaces, ensure thin controllers, inject `ILogger<T>` into all services, and update unit tests for full coverage of refactored code.

## Affected Areas

- **Project Config**: `Directory.Build.props`
- **Models (BLL)**: `LaconicAndIconic.BLL/Models/Result.cs` (new), `LaconicAndIconic.BLL/Models/LoginResult.cs`
- **Interfaces (BLL)**: `LaconicAndIconic.BLL/Interfaces/IAuthService.cs`, `LaconicAndIconic.BLL/Interfaces/IExampleService.cs` (remove or replace)
- **Services (BLL)**: `LaconicAndIconic.BLL/Services/AuthService.cs`, `LaconicAndIconic.BLL/Services/ExampleService.cs` (remove or replace)
- **DI Registration (BLL)**: `LaconicAndIconic.BLL/ServiceCollectionExtensions.cs`
- **Repository Interfaces (DAL)**: `LaconicAndIconic.DAL/Interfaces/IRepository.cs` (new)
- **Repository Implementations (DAL)**: `LaconicAndIconic.DAL/Repositories/Repository.cs` (new)
- **DI Registration (DAL)**: `LaconicAndIconic.DAL/ServiceCollectionExtensions.cs`
- **Middleware (Web)**: `LaconicAndIconic.Web/Middleware/GlobalExceptionHandlerMiddleware.cs` (new)
- **Controllers (Web)**: `LaconicAndIconic.Web/Controllers/AccountController.cs`, `LaconicAndIconic.Web/Controllers/HomeController.cs`
- **Entry Point (Web)**: `LaconicAndIconic.Web/Program.cs`
- **Tests**: `LaconicAndIconic.Tests/AuthServiceTests.cs`, `LaconicAndIconic.Tests/Controllers/AccountControllerTests.cs`
- **Migrations**: No — no schema changes required.

## Current State Analysis

### Issues Found

1. **No `TreatWarningsAsErrors`** — `Directory.Build.props` has analyzers enabled but does not treat warnings as errors.
2. **`ExampleService` accesses `ApplicationDbContext` directly** — violates the layering rule (BLL depends directly on EF Core DbContext, no repository abstraction).
3. **`ExampleService` has no `ILogger<T>`** — only `AuthService` has logging.
4. **`AuthService.RegisterAsync` returns `IdentityResult`** — leaks an Identity-layer type through the BLL interface. Should return `Result<T>` instead.
5. **`AuthService.LoginAsync` returns `LoginResult` enum** — acceptable, but the issue requires all service methods to return `Result` or `Result<T>`.
6. **`AuthService` uses `.ConfigureAwait(false)` in `RegisterAsync`** — violates the backend instructions (ASP.NET Core app, not a library).
7. **No global exception handling middleware** — `Program.cs` only uses the built-in `UseExceptionHandler` for non-dev environments.
8. **No try-catch in services currently** — good, but the global middleware should still be added as a safety net.
9. **`AccountController` contains logging logic** — the controller logs user actions directly. Per thin-controller rule, logging for business events should live in services.
10. **`AccountController.Register` calls `_authService.LoginAsync` after register** — this is a two-step business operation done in the controller. Should be a single service call or the auto-login should be handled in the service.

## Risk Assessment

- **Breaking change to `IAuthService` interface** — `RegisterAsync` signature changes from `IdentityResult` to `Result<T>`. All consumers (controller, tests) must be updated in the same step or the build breaks. Mitigated by doing interface + service + controller in one atomic step.
- **`ExampleService` removal/replacement** — `ExampleService` is a generic CRUD wrapper over `DbContext`. It will be replaced by proper repository interfaces. Any code referencing `IExampleService` must be updated. Currently nothing in controllers uses it, only registered in DI.
- **`TreatWarningsAsErrors` may surface hidden warnings** — must fix all warnings before or in the same step as enabling the flag, or the build fails.
- **Test changes are required** — `AuthServiceTests` directly tests `AuthService` which will change signatures. `AccountControllerTests` mocks `IAuthService` which will change. Both must be updated.
- **No database migration needed** — all changes are code-level refactors; entity schema is unchanged.

## Steps

### Step 1: Add the `Result<T>` pattern class to BLL

- **Files**: `LaconicAndIconic.BLL/Models/Result.cs` (new)
- **Change**: Create a `Result` class and a generic `Result<T>` class in `LaconicAndIconic.BLL/Models/`. `Result` has `bool IsSuccess`, `string? ErrorMessage`, and static factory methods `Success()` and `Failure(string errorMessage)`. `Result<T>` extends this concept with a `T? Value` property and static factory methods `Success(T value)` and `Failure(string errorMessage)`. Both classes should be sealed. Keep `LoginResult` enum as-is for now (it will be wrapped inside `Result<LoginResult>` in the next step).
- **Test**: Build succeeds. No behavioral changes yet — this is a new file with no consumers.
- **Commit message**: `feat(bll): add Result and Result<T> pattern classes`

### Step 2: Refactor `IAuthService` and `AuthService` to return `Result<T>`

- **Files**: `LaconicAndIconic.BLL/Interfaces/IAuthService.cs`, `LaconicAndIconic.BLL/Services/AuthService.cs`
- **Change**:
  - `LoginAsync` returns `Result<LoginResult>` instead of bare `LoginResult`. On success, return `Result<LoginResult>.Success(LoginResult.Success)`. On failure, return `Result<LoginResult>.Success(LoginResult.InvalidCredentials)` or `Result<LoginResult>.Success(LoginResult.LockedOut)` (these are expected business outcomes, not errors). Keep the enum values as data inside the Result.
  - `RegisterAsync` returns `Result` instead of `IdentityResult`. On `IdentityResult.Succeeded`, return `Result.Success()`. On failure, concatenate error descriptions into a single message and return `Result.Failure(errorMessage)`.
  - `LogoutAsync` returns `Result` instead of `Task`. Return `Result.Success()` after sign-out.
  - Remove `.ConfigureAwait(false)` from `RegisterAsync` (violates backend instructions).
  - Move the logging that currently lives in `AccountController` for login success/lockout into `AuthService.LoginAsync` (it already has some — verify no duplication).
- **Test**: Build succeeds. Existing tests will break — they are updated in a later step.
- **Commit message**: `refactor(bll): return Result<T> from AuthService methods`

### Step 3: Update `AccountController` to consume `Result<T>` from `AuthService`

- **Files**: `LaconicAndIconic.Web/Controllers/AccountController.cs`
- **Change**:
  - `Login` POST: call `_authService.LoginAsync(...)`, check `result.IsSuccess` and then inspect `result.Value` for the `LoginResult` enum. Remove the `_logger.LogInformation` and `_logger.LogWarning` calls for login events (this logging now lives in the service).
  - `Register` POST: call `_authService.RegisterAsync(request)`, check `result.IsSuccess`. On failure, add `result.ErrorMessage` to `ModelState`. Remove the `foreach (var error in result.Errors)` loop (no longer `IdentityResult`).
  - `Register` POST: the post-registration auto-login (`_authService.LoginAsync`) stays in the controller since it's a UI workflow decision (login after register), but the controller should just fire-and-forget the result (it's a convenience login, not critical).
  - `Logout` POST: call `_authService.LogoutAsync()`, no change needed beyond accepting `Result` return.
  - Ensure the controller contains ZERO business logic — only calls services, inspects results, and returns views/redirects.
- **Test**: Build succeeds. App functions identically to before. Manual smoke-test: login, register, logout flows work.
- **Commit message**: `refactor(web): update AccountController to consume Result pattern`

### Step 4: Add generic repository interface and implementation in DAL

- **Files**: `LaconicAndIconic.DAL/Interfaces/IRepository.cs` (new), `LaconicAndIconic.DAL/Repositories/Repository.cs` (new)
- **Change**:
  - Create `IRepository<T>` interface in `LaconicAndIconic.DAL/Interfaces/` with methods: `Task<IEnumerable<T>> GetAllAsync()`, `Task<T?> GetByIdAsync(int id)`, `Task AddAsync(T entity)`, `void Update(T entity)`, `void Remove(T entity)`, `Task<bool> ExistsAsync(int id)`, `Task SaveChangesAsync()`. Constrain `T` to `class`.
  - Create `Repository<T> : IRepository<T>` in `LaconicAndIconic.DAL/Repositories/`. It takes `ApplicationDbContext` via constructor injection and delegates to `_context.Set<T>()`.
  - Register `IRepository<T>` → `Repository<T>` as scoped in `LaconicAndIconic.DAL/ServiceCollectionExtensions.cs` using `services.AddScoped(typeof(IRepository<>), typeof(Repository<>))`.
- **Test**: Build succeeds. No behavioral changes — new files with no consumers yet.
- **Commit message**: `feat(dal): add generic IRepository<T> interface and EF Core implementation`

### Step 5: Remove `ExampleService` and `IExampleService`

- **Files**: `LaconicAndIconic.BLL/Interfaces/IExampleService.cs` (delete), `LaconicAndIconic.BLL/Services/ExampleService.cs` (delete), `LaconicAndIconic.BLL/ServiceCollectionExtensions.cs`
- **Change**:
  - Delete `IExampleService.cs` and `ExampleService.cs`. These are replaced by the generic repository pattern. The `ExampleService` was a thin pass-through to `DbContext` — that responsibility now belongs to `IRepository<T>` in DAL.
  - Remove the `services.AddScoped<IExampleService, ExampleService>()` line from `ServiceCollectionExtensions.cs` in BLL.
  - Verify no controller or other code references `IExampleService`. Currently none do (confirmed by codebase read).
- **Test**: Build succeeds. `dotnet test` passes (no tests reference `ExampleService`).
- **Commit message**: `refactor(bll): remove ExampleService in favor of repository pattern`

### Step 6: Add global exception handling middleware

- **Files**: `LaconicAndIconic.Web/Middleware/GlobalExceptionHandlerMiddleware.cs` (new), `LaconicAndIconic.Web/Program.cs`
- **Change**:
  - Create `GlobalExceptionHandlerMiddleware` class. It wraps `RequestDelegate _next` in a try-catch. On unhandled exception: log the exception via `ILogger<GlobalExceptionHandlerMiddleware>`, and redirect to `/Home/Error` (for MVC, set response status 500 and re-execute to error path). Use `IHostEnvironment` to decide whether to include details.
  - In `Program.cs`, register the middleware early in the pipeline: `app.UseMiddleware<GlobalExceptionHandlerMiddleware>()` — place it before `UseRouting`. Keep the existing `UseExceptionHandler("/Home/Error")` for non-dev as a fallback, or replace it with the new middleware entirely (the new middleware handles all environments).
- **Test**: Build succeeds. Manually verify: throw a test exception in a controller action and confirm the middleware catches it, logs it, and redirects to the error page.
- **Commit message**: `feat(web): add global exception handling middleware`

### Step 7: Enable `TreatWarningsAsErrors` in `Directory.Build.props`

- **Files**: `Directory.Build.props`
- **Change**:
  - Add `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` inside the existing `<PropertyGroup>`.
  - Run `dotnet build` and fix any warnings that are now errors. Likely candidates:
    - Unused variables, unused `using` directives.
    - Nullable reference type warnings.
    - Any Roslynator warnings not already suppressed.
  - If new suppressions are needed, add them to the `<NoWarn>` list in `Directory.Build.props` with a comment explaining why.
- **Test**: `dotnet build` succeeds with zero warnings/errors across all four projects.
- **Commit message**: `build: enable TreatWarningsAsErrors globally`

### Step 8: Inject `ILogger<T>` into all services that lack it

- **Files**: `LaconicAndIconic.BLL/Services/AuthService.cs` (already has it — verify only)
- **Change**:
  - `AuthService` already injects `ILogger<AuthService>` — no change needed.
  - `ExampleService` was deleted in Step 5 — no action.
  - If any new services are introduced in future steps, they must follow the pattern: `private readonly ILogger<T> _logger;` injected via constructor.
  - This step is essentially a verification pass. If everything is clean, it's a no-op commit (skip it). Included here for completeness per the acceptance criteria.
- **Test**: Build succeeds. All services have `ILogger<T>` injected.
- **Commit message**: `refactor(bll): verify ILogger<T> injection in all services`

### Step 9: Update `AuthServiceTests` for Result pattern

- **Files**: `LaconicAndIconic.Tests/AuthServiceTests.cs`
- **Change**:
  - `RegisterAsync_ValidRequest_ReturnsSucceeded` — assert `result.IsSuccess` is `true` instead of `result.Succeeded`.
  - `RegisterAsync_DuplicateEmail_ReturnsFailureWithError` — assert `result.IsSuccess` is `false` and `result.ErrorMessage` contains the expected duplicate email text.
  - `RegisterAsync_PasswordTooShort_ReturnsFailureWithError` — assert `result.IsSuccess` is `false` and `result.ErrorMessage` contains password-related text.
  - `RegisterAsync_NullRequest_ThrowsArgumentNullException` — unchanged (still throws).
  - Add new test: `LoginAsync_ValidCredentials_ReturnsSuccessWithLoginResultSuccess` — assert `result.IsSuccess` and `result.Value == LoginResult.Success`.
  - Add new test: `LoginAsync_NonExistentUser_ReturnsSuccessWithInvalidCredentials` — assert `result.Value == LoginResult.InvalidCredentials`.
  - Add new test: `LoginAsync_LockedOutUser_ReturnsSuccessWithLockedOut` — assert `result.Value == LoginResult.LockedOut`.
  - Add new test: `LogoutAsync_ReturnsSuccess` — assert `result.IsSuccess`.
- **Test**: `dotnet test --filter "FullyQualifiedName~AuthServiceTests"` — all tests pass.
- **Commit message**: `test: update AuthServiceTests for Result pattern`

### Step 10: Update `AccountControllerTests` for Result pattern

- **Files**: `LaconicAndIconic.Tests/Controllers/AccountControllerTests.cs`
- **Change**:
  - Update mock setups: `_authServiceMock.Setup(s => s.LoginAsync(...)).ReturnsAsync(Result<LoginResult>.Success(LoginResult.Success))` instead of bare `LoginResult.Success`.
  - `Login_ValidCredentials_RedirectsToHome` — mock returns `Result<LoginResult>.Success(LoginResult.Success)`. Assert redirect unchanged.
  - `Login_InvalidCredentials_ReturnsViewWithError` — mock returns `Result<LoginResult>.Success(LoginResult.InvalidCredentials)`. Assert view with model error unchanged.
  - `Login_InvalidModelState_ReturnsView` — unchanged (doesn't reach service).
  - Add new test: `Register_ValidRequest_RedirectsToHome` — mock `RegisterAsync` returning `Result.Success()` and `LoginAsync` returning success. Assert `RedirectToActionResult` to Home/Index.
  - Add new test: `Register_ServiceFailure_ReturnsViewWithErrors` — mock `RegisterAsync` returning `Result.Failure("Email is already taken.")`. Assert `ViewResult` with model error.
  - Add new test: `Register_InvalidModelState_ReturnsView` — add model error, assert view returned, service never called.
  - Add new test: `Logout_RedirectsToHome` — mock `LogoutAsync` returning `Result.Success()`. Assert redirect to Home/Index.
- **Test**: `dotnet test --filter "FullyQualifiedName~AccountControllerTests"` — all tests pass.
- **Commit message**: `test: update AccountControllerTests for Result pattern`

### Step 11: Final verification and cleanup

- **Files**: All project files
- **Change**:
  - Run `dotnet build` across the entire solution — zero errors, zero warnings.
  - Run `dotnet test` — all tests pass.
  - Verify no controller contains business logic (only service calls + view/redirect returns).
  - Verify no service references `ApplicationDbContext` directly (only through repository interfaces or Identity managers).
  - Verify global exception handling middleware is registered and functional.
  - Remove any dead `using` directives or unused code introduced during refactoring.
- **Test**: Full solution build and test pass. Manual smoke-test of login, register, logout, and error page.
- **Commit message**: `chore: final verification and cleanup after architectural refactor`
