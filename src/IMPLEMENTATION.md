# Implementation: Display Recipe Feed on Home/Index Page

Implements every step from `PLAN.md` in dependency order. All file contents are complete — no placeholders.

---

## Step 1 — Add `GetAllRecipesAsync` to `IRecipeService` and implement in `RecipeService`

### `LaconicAndIconic.BLL/Interfaces/IRecipeService.cs` — **modify**

```csharp
using LaconicAndIconic.BLL.Models;

namespace LaconicAndIconic.BLL.Interfaces;

public interface IRecipeService
{
    Task<Result<RecipeDto>> CreateRecipeAsync(int authorId, CreateRecipeDto dto);
    Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId);
    Task<Result<IEnumerable<RecipeDto>>> GetAllRecipesAsync();
    Task<Result> DeleteRecipeAsync(int recipeId, int authorId);
}
```

### `LaconicAndIconic.BLL/Services/RecipeService.cs` — **modify**

Add `GetAllRecipesAsync` using the same `FindAsync` + projection pattern as `GetRecipesByAuthorIdAsync`.

```csharp
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;

namespace LaconicAndIconic.BLL.Services;

public class RecipeService : IRecipeService
{
    private readonly IRepository<Recipe> _recipeRepository;
    private readonly IFileService _fileService;
    private readonly IRepository<Category> _categoryRepository;

    public RecipeService(IRepository<Recipe> recipeRepository, IFileService fileService, IRepository<Category> categoryRepository)
    {
        _recipeRepository = recipeRepository;
        _fileService = fileService;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<RecipeDto>> CreateRecipeAsync(int authorId, CreateRecipeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Result<RecipeDto>.Failure("Title is required");
        }

        var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
        if (!categoryExists)
        {
            return Result<RecipeDto>.Failure("Category not found");
        }

        string? imagePath = null;
        if (dto.Image != null && dto.Image.Length > 0)
        {
            imagePath = await _fileService.SaveFileAsync(dto.Image, "recipes");
        }

        var recipe = new Recipe
        {
            Title = dto.Title,
            Description = dto.Description,
            PrepTimeMin = dto.PrepTimeMin,
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            ImagePath = imagePath
        };

        await _recipeRepository.AddAsync(recipe);
        await _recipeRepository.SaveChangesAsync();

        var responseDto = new RecipeDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            ImagePath = recipe.ImagePath,
            PrepTimeMin = recipe.PrepTimeMin,
            CategoryId = recipe.CategoryId,
            AuthorId = recipe.AuthorId
        };

        return Result<RecipeDto>.Success(responseDto);
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetRecipesByAuthorIdAsync(int authorId)
    {
        var recipes = await _recipeRepository
            .FindAsync(r => r.AuthorId == authorId, r => r.Category, r => r.Author);

        var dtos = recipes
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RecipeDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                ImagePath = r.ImagePath,
                PrepTimeMin = r.PrepTimeMin,
                CategoryId = r.CategoryId,
                CategoryName = r.Category?.Name ?? string.Empty,
                AuthorId = r.AuthorId,
                AuthorName = r.Author?.UserName ?? string.Empty
            });

        return Result<IEnumerable<RecipeDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<RecipeDto>>> GetAllRecipesAsync()
    {
        var recipes = await _recipeRepository
            .FindAsync(r => true, r => r.Category, r => r.Author);

        var dtos = recipes
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RecipeDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                ImagePath = r.ImagePath,
                PrepTimeMin = r.PrepTimeMin,
                CategoryId = r.CategoryId,
                CategoryName = r.Category?.Name ?? string.Empty,
                AuthorId = r.AuthorId,
                AuthorName = r.Author?.UserName ?? string.Empty
            });

        return Result<IEnumerable<RecipeDto>>.Success(dtos);
    }

    public async Task<Result> DeleteRecipeAsync(int recipeId, int authorId)
    {
        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null)
        {
            return Result.Failure("Recipe not found");
        }

        if (recipe.AuthorId != authorId)
        {
            return Result.Failure("You can only delete your own recipes");
        }

        _recipeRepository.Remove(recipe);
        await _recipeRepository.SaveChangesAsync();

        return Result.Success();
    }
}
```

**Verification**: `dotnet build` — build must be green; `IRecipeService` is fully satisfied, `IRecipeService` is already registered as `RecipeService` in `LaconicAndIconic.BLL/ServiceCollectionExtensions.cs` so no DI change is needed.

---

## Step 2 — Inject `IRecipeService` into `HomeController` and make `Index` async

### `LaconicAndIconic.Web/Controllers/HomeController.cs` — **modify**

```csharp
using System.Diagnostics;
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaconicAndIconic.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRecipeService _recipeService;

    public HomeController(ILogger<HomeController> logger, IRecipeService recipeService)
    {
        _logger = logger;
        _recipeService = recipeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _recipeService.GetAllRecipesAsync();
        var model = result.IsSuccess && result.Value is not null
            ? result.Value
            : Enumerable.Empty<RecipeDto>();
        return View(model);
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult Dashboard()
    {
        return View();
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
```

**Verification**: `dotnet run --project LaconicAndIconic.Web` — navigate to `/`; page must load without a 500 error.

---

## Step 3 — Update `Home/Index.cshtml` with Bootstrap card feed

### `LaconicAndIconic.Web/Views/Home/Index.cshtml` — **modify**

The `@using` directive at the top of the file makes `RecipeDto` available without touching `_ViewImports.cshtml` (which imports only `LaconicAndIconic.Web.Models`).

```cshtml
@using LaconicAndIconic.BLL.Models
@model IEnumerable<RecipeDto>
@{
    ViewData["Title"] = "Recipes";
}

<h1 class="mb-4">Latest Recipes</h1>

@if (!Model.Any())
{
    <p class="text-muted">No recipes yet. Be the first to share one!</p>
}
else
{
    <div class="row row-cols-1 row-cols-md-3 g-4">
        @foreach (var recipe in Model)
        {
            <div class="col">
                <div class="card h-100">
                    @if (!string.IsNullOrEmpty(recipe.ImagePath))
                    {
                        <img src="@recipe.ImagePath"
                             class="card-img-top"
                             alt="@recipe.Title"
                             style="max-height: 200px; object-fit: cover;" />
                    }
                    else
                    {
                        <div class="card-img-top bg-secondary" style="height: 200px;"></div>
                    }
                    <div class="card-body d-flex flex-column">
                        <h5 class="card-title">@recipe.Title</h5>
                        <p class="card-text flex-grow-1">@recipe.Description</p>
                        <p class="card-text text-muted small">
                            @recipe.CategoryName · @recipe.AuthorName · @recipe.PrepTimeMin min
                        </p>
                        <a href="#" class="btn btn-outline-primary btn-sm mt-2">View Recipe</a>
                    </div>
                </div>
            </div>
        }
    </div>
}
```

**Verification**: With a seeded or manually inserted recipe, navigate to `/`; the card renders with title, description, category, author, and prep time. With an empty database, the empty-state `<p>` is shown.

---

## Step 4 — Add unit tests for `GetAllRecipesAsync` in `RecipeServiceTests`

### `LaconicAndIconic.Tests/Services/RecipeServiceTests.cs` — **modify** (append three new test methods)

Full file including all existing tests plus the three new ones:

```csharp
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.BLL.Services;
using LaconicAndIconic.DAL.Entities;
using LaconicAndIconic.DAL.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace LaconicAndIconic.Tests.Services;

public class RecipeServiceTests
{
    private readonly Mock<IRepository<Recipe>> _recipeRepositoryMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly RecipeService _service;

    public RecipeServiceTests()
    {
        _recipeRepositoryMock = new Mock<IRepository<Recipe>>();
        _fileServiceMock = new Mock<IFileService>();
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _service = new RecipeService(_recipeRepositoryMock.Object, _fileServiceMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateRecipeAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateRecipeDto
        {
            Title = "Test Recipe",
            Description = "A delicious test recipe",
            PrepTimeMin = 30,
            CategoryId = 1
        };

        _categoryRepositoryMock.Setup(repo => repo.ExistsAsync(1)).ReturnsAsync(true);
        _recipeRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Recipe>())).Returns(Task.CompletedTask);
        _recipeRepositoryMock.Setup(repo => repo.GetByIdAsync(0)).ReturnsAsync(new Recipe { Id = 1, Title = "Test Recipe", CategoryId = 1, AuthorId = 1, PrepTimeMin = 30, Description = "A delicious test recipe" });

        // Act
        var result = await _service.CreateRecipeAsync(1, dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Test Recipe", result.Value.Title);
        _recipeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Recipe>()), Times.Once);
        _recipeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateRecipeAsync_MissingTitle_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateRecipeDto { Title = "  " };

        // Act
        var result = await _service.CreateRecipeAsync(1, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Title is required", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateRecipeAsync_CategoryNotFound_ReturnsFailure()
    {
        // Arrange
        var dto = new CreateRecipeDto { Title = "Title", CategoryId = 99 };
        _categoryRepositoryMock.Setup(repo => repo.ExistsAsync(99)).ReturnsAsync(false);

        // Act
        var result = await _service.CreateRecipeAsync(1, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found", result.ErrorMessage);
    }

    [Fact]
    public async Task GetRecipesByAuthorIdAsync_ReturnsRecipes()
    {
        // Arrange
        var authorId = 1;
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "R1", AuthorId = authorId, CreatedAt = DateTime.UtcNow },
            new Recipe { Id = 2, Title = "R2", AuthorId = authorId, CreatedAt = DateTime.UtcNow.AddMinutes(-10) }
        };

        _recipeRepositoryMock.Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync(recipes);

        // Act
        var result = await _service.GetRecipesByAuthorIdAsync(authorId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
        Assert.Equal("R1", result.Value!.First().Title);
    }

    [Fact]
    public async Task DeleteRecipeAsync_ExistingRecipeAndValidAuthor_ReturnsSuccess()
    {
        // Arrange
        var recipeId = 1;
        var authorId = 1;
        var recipe = new Recipe { Id = recipeId, AuthorId = authorId };

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(recipeId)).ReturnsAsync(recipe);

        // Act
        var result = await _service.DeleteRecipeAsync(recipeId, authorId);

        // Assert
        Assert.True(result.IsSuccess);
        _recipeRepositoryMock.Verify(r => r.Remove(recipe), Times.Once);
        _recipeRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteRecipeAsync_RecipeNotFound_ReturnsFailure()
    {
        // Arrange
        var recipeId = 99;
        var authorId = 1;

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(recipeId)).ReturnsAsync((Recipe?)null);

        // Act
        var result = await _service.DeleteRecipeAsync(recipeId, authorId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Recipe not found", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteRecipeAsync_WrongAuthor_ReturnsFailure()
    {
        // Arrange
        var recipeId = 1;
        var authorId = 2; // the requester
        var recipe = new Recipe { Id = recipeId, AuthorId = 1 }; // original author is 1

        _recipeRepositoryMock.Setup(r => r.GetByIdAsync(recipeId)).ReturnsAsync(recipe);

        // Act
        var result = await _service.DeleteRecipeAsync(recipeId, authorId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("You can only delete your own recipes", result.ErrorMessage);
        _recipeRepositoryMock.Verify(r => r.Remove(It.IsAny<Recipe>()), Times.Never);
    }

    // --- GetAllRecipesAsync tests ---

    [Fact]
    public async Task GetAllRecipesAsync_RecipesExist_ReturnsMappedDtosOrderedByDateDescending()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Title = "Older", AuthorId = 1, CreatedAt = now.AddHours(-1) },
            new Recipe { Id = 2, Title = "Newer", AuthorId = 1, CreatedAt = now }
        };

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync(recipes);

        // Act
        var result = await _service.GetAllRecipesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
        Assert.Equal("Newer", result.Value!.First().Title);
    }

    [Fact]
    public async Task GetAllRecipesAsync_NoRecipes_ReturnsSuccessWithEmptyCollection()
    {
        // Arrange
        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync([]);

        // Act
        var result = await _service.GetAllRecipesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task GetAllRecipesAsync_RecipesExist_MapsCategoryNameAndAuthorName()
    {
        // Arrange
        var recipe = new Recipe
        {
            Id = 1,
            Title = "Pasta",
            Category = new Category { Name = "Italian" },
            Author = new ApplicationUser { UserName = "chef" }
        };

        _recipeRepositoryMock
            .Setup(r => r.FindAsync(
                It.IsAny<Expression<Func<Recipe, bool>>>(),
                It.IsAny<Expression<Func<Recipe, object>>[]>()))
            .ReturnsAsync([recipe]);

        // Act
        var result = await _service.GetAllRecipesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal("Italian", dto.CategoryName);
        Assert.Equal("chef", dto.AuthorName);
    }
}
```

**Verification**: `dotnet test --filter "FullyQualifiedName~RecipeServiceTests"` — all tests pass (existing + 3 new).

---

## Step 5 — Add `HomeControllerTests` for the updated `Index` action

### `LaconicAndIconic.Tests/Controllers/HomeControllerTests.cs` — **create new**

Follows the `sealed class + IDisposable` pattern of `AccountControllerTests`. Uses `NullLogger<HomeController>.Instance` for the logger dependency (project convention for test logger injection).

```csharp
using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LaconicAndIconic.Tests.Controllers;

public sealed class HomeControllerTests : IDisposable
{
    private readonly Mock<IRecipeService> _recipeServiceMock;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _recipeServiceMock = new Mock<IRecipeService>();
        _controller = new HomeController(NullLogger<HomeController>.Instance, _recipeServiceMock.Object);
    }

    public void Dispose()
    {
        _controller.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Index_RecipesExist_ReturnsViewWithRecipeDtoCollection()
    {
        // Arrange
        var recipeDtoStub = new RecipeDto { Id = 1, Title = "Pasta", AuthorName = "chef" };
        _recipeServiceMock
            .Setup(s => s.GetAllRecipesAsync())
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Success([recipeDtoStub]));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(viewResult.Model);
        Assert.Single(model);
        Assert.Equal(recipeDtoStub.Title, model.First().Title);
    }

    [Fact]
    public async Task Index_NoRecipes_ReturnsViewWithEmptyCollection()
    {
        // Arrange
        _recipeServiceMock
            .Setup(s => s.GetAllRecipesAsync())
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Success(Enumerable.Empty<RecipeDto>()));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(viewResult.Model);
        Assert.Empty(model);
    }

    [Fact]
    public async Task Index_ServiceReturnsFailure_ReturnsViewWithEmptyCollection()
    {
        // Arrange
        _recipeServiceMock
            .Setup(s => s.GetAllRecipesAsync())
            .ReturnsAsync(Result<IEnumerable<RecipeDto>>.Failure("error"));

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(viewResult.Model);
        Assert.Empty(model);
    }
}
```

**Verification**: `dotnet test --filter "FullyQualifiedName~HomeControllerTests"` — all 3 tests pass.

---

## Full verification sequence

```powershell
# Build entire solution — must be green before running tests
dotnet build

# Run service tests
dotnet test --filter "FullyQualifiedName~RecipeServiceTests"

# Run controller tests
dotnet test --filter "FullyQualifiedName~HomeControllerTests"

# Run all tests
dotnet test
```

## Commit sequence (matches PLAN.md)

```
feat(bll): add GetAllRecipesAsync to IRecipeService and RecipeService
feat(web): inject IRecipeService into HomeController and make Index async
feat(views): render recipe feed cards on Home/Index
test(bll): add unit tests for GetAllRecipesAsync in RecipeServiceTests
test(web): add HomeControllerTests for Index action
```
