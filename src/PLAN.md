# Feature: Display Recipe Feed on Home/Index Page

## Summary
The Home/Index page currently shows a static welcome message. This feature adds a public recipe feed that fetches all recipes via `IRecipeService` and renders them as Bootstrap cards, ordered newest-first, with title, author, category, prep time, and optional image. The feed is visible to unauthenticated users. An empty-state message is shown when no recipes exist.

## Affected Areas
- **Models/Entities**: none
- **Controllers/Routes**: `LaconicAndIconic.Web/Controllers/HomeController.cs`
- **Services**: `LaconicAndIconic.BLL/Interfaces/IRecipeService.cs`, `LaconicAndIconic.BLL/Services/RecipeService.cs`
- **Config**: none
- **Frontend**: `LaconicAndIconic.Web/Views/Home/Index.cshtml`
- **Migrations**: no
- **Re-index needed**: no

## Risk Assessment
- Adding a method to `IRecipeService` is a breaking interface change: the implementation in `RecipeService` must be added in the same step to keep the build green.
- `GetAllAsync()` on `IRepository<T>` does not support eager-loading includes; `FindAsync(r => true, includes...)` must be used to load `Category` and `Author` navigation properties — consistent with the existing `GetRecipesByAuthorIdAsync` pattern.
- `HomeController.Index` must remain accessible to unauthenticated users (no `[Authorize]` attribute); the existing action already lacks it, so no authorization change is needed.
- `_ViewImports.cshtml` imports only `LaconicAndIconic.Web.Models`. Using `RecipeDto` (a BLL model) directly in the view requires either a `@using` directive in `Index.cshtml` or a new Web-layer ViewModel. Because no wrapper ViewModel is needed beyond the collection itself, a local `@using` in the view is the least-invasive approach.
- Making `HomeController.Index` `async` changes its return type from `IActionResult` to `Task<IActionResult>`; any existing unit tests for it must be updated accordingly (none exist today).

## Steps

### Step 1: Add `GetAllRecipesAsync` to `IRecipeService` and implement in `RecipeService`
- **Files**: `LaconicAndIconic.BLL/Interfaces/IRecipeService.cs`, `LaconicAndIconic.BLL/Services/RecipeService.cs`
- **Change**:
  - Add `Task<Result<IEnumerable<RecipeDto>>> GetAllRecipesAsync();` to `IRecipeService`.
  - In `RecipeService`, implement the method: call `_recipeRepository.FindAsync(r => true, r => r.Category, r => r.Author)`, project each `Recipe` to a `RecipeDto` (same field mapping as `GetRecipesByAuthorIdAsync`, including `CategoryName` and `AuthorName`), order results by `CreatedAt` descending, then return `Result<IEnumerable<RecipeDto>>.Success(dtos)`.
- **Test**: Build the solution (`dotnet build`) to verify no compile errors; the interface is satisfied and no other implementors exist.
- **Commit message**: `feat(bll): add GetAllRecipesAsync to IRecipeService and RecipeService`

---

### Step 2: Inject `IRecipeService` into `HomeController` and make `Index` async
- **Files**: `LaconicAndIconic.Web/Controllers/HomeController.cs`
- **Change**:
  - Add `IRecipeService _recipeService` as a constructor-injected field alongside the existing `ILogger<HomeController>`.
  - Change `Index()` to `async Task<IActionResult> Index()`.
  - Inside `Index`, call `await _recipeService.GetAllRecipesAsync()`. If the result is not successful or `Value` is null, pass an empty `IEnumerable<RecipeDto>` to the view. Otherwise pass `result.Value` ordered by `CreatedAt` descending (already ordered by the service; no secondary sort needed). Return `View(model)`.
  - `IRecipeService` is already registered in `LaconicAndIconic.BLL/ServiceCollectionExtensions.cs`; no DI registration change is needed.
- **Test**: Run the application and navigate to `/`; confirm the page loads without a 500 error (even with an empty database).
- **Commit message**: `feat(web): inject IRecipeService into HomeController and make Index async`

---

### Step 3: Update `Home/Index.cshtml` with Bootstrap card feed
- **Files**: `LaconicAndIconic.Web/Views/Home/Index.cshtml`
- **Change**:
  - Replace the current static content entirely.
  - Add `@using LaconicAndIconic.BLL.Models` at the top of the file.
  - Set the model directive to `@model IEnumerable<RecipeDto>`.
  - Set `ViewData["Title"]` to `"Recipes"`.
  - Render a page heading (e.g., `<h1>Latest Recipes</h1>`).
  - If `Model` is empty, render a friendly empty-state `<p>` (e.g., "No recipes yet. Be the first to share one!").
  - Otherwise, render a `row` with `col-md-4` Bootstrap grid columns, one per recipe. Each card displays:
    - Recipe image in `card-img-top` (use `<img>` only when `ImagePath` is not null/empty; otherwise show a placeholder `div` with a muted background).
    - `card-body` with `card-title` for `Title`, `card-text` with `Description`, and a small `text-muted` line showing category, author, and prep time (e.g., `{CategoryName} · {AuthorName} · {PrepTimeMin} min`).
    - A "View Recipe" anchor styled as `btn btn-outline-primary btn-sm` linking to the recipe detail page (the route is not yet implemented; link can point to `#` for now or be omitted until a detail route exists).
- **Test**: Run the application, seed or insert a test recipe, and verify the card renders with correct data. Verify empty state appears when the database has no recipes.
- **Commit message**: `feat(views): render recipe feed cards on Home/Index`

---

### Step 4: Add unit tests for `GetAllRecipesAsync` in `RecipeServiceTests`
- **Files**: `LaconicAndIconic.Tests/Services/RecipeServiceTests.cs`
- **Change**: Add the following test cases to the existing `RecipeServiceTests` class (reusing the existing mock and `_service` setup):
  1. `GetAllRecipesAsync_RecipesExist_ReturnsMappedDtosOrderedByDateDescending` — mock `_recipeRepositoryMock.Setup(r => r.FindAsync(It.IsAny<...>(), ...)).ReturnsAsync(listOfTwoRecipes)` where the list contains two `Recipe` objects with differing `CreatedAt` values; assert the result is successful, contains two items, and the first item has the later `CreatedAt`.
  2. `GetAllRecipesAsync_NoRecipes_ReturnsSuccessWithEmptyCollection` — mock `FindAsync` to return an empty list; assert the result is successful and the collection is empty.
  3. `GetAllRecipesAsync_RecipesExist_MapsCategoryNameAndAuthorName` — mock one `Recipe` with a populated `Category` and `Author` navigation property; assert `CategoryName` and `AuthorName` on the returned `RecipeDto` match those navigation property values.
- **Test**: Run `dotnet test --filter "FullyQualifiedName~RecipeServiceTests"` and confirm all new tests pass.
- **Commit message**: `test(bll): add unit tests for GetAllRecipesAsync in RecipeServiceTests`

---

### Step 5: Add `HomeControllerTests` for the updated `Index` action
- **Files**: `LaconicAndIconic.Tests/Controllers/HomeControllerTests.cs` (new file)
- **Change**: Create a new `HomeControllerTests` class in `namespace LaconicAndIconic.Tests.Controllers` following the same sealed-class + `IDisposable` pattern used in `AccountControllerTests`. Inject a `Mock<IRecipeService>` and a `Mock<ILogger<HomeController>>`. Add the following test cases:
  1. `Index_RecipesExist_ReturnsViewWithRecipeDtoCollection` — mock `GetAllRecipesAsync` to return `Result<IEnumerable<RecipeDto>>.Success(new[] { recipeDtoStub })`; assert the action result is a `ViewResult` whose `Model` is an `IEnumerable<RecipeDto>` containing the stub.
  2. `Index_NoRecipes_ReturnsViewWithEmptyCollection` — mock `GetAllRecipesAsync` to return `Result<IEnumerable<RecipeDto>>.Success(Enumerable.Empty<RecipeDto>())`; assert the `ViewResult` model is an empty `IEnumerable<RecipeDto>`.
  3. `Index_ServiceReturnsFailure_ReturnsViewWithEmptyCollection` — mock `GetAllRecipesAsync` to return `Result<IEnumerable<RecipeDto>>.Failure("error")`; assert the action still returns a `ViewResult` (no exception thrown) with an empty collection as the model.
- **Test**: Run `dotnet test --filter "FullyQualifiedName~HomeControllerTests"` and confirm all tests pass.
- **Commit message**: `test(web): add HomeControllerTests for Index action`
