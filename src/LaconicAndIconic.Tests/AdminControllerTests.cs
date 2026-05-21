using LaconicAndIconic.BLL.Interfaces;
using LaconicAndIconic.BLL.Models;
using LaconicAndIconic.Web.Controllers;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LaconicAndIconic.Tests;

public class AdminControllerTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IReportService> _reportServiceMock = new();
    private readonly Mock<IRecipeService> _recipeServiceMock = new();

    [Fact]
    public async Task ManageRecipes_ReturnsReportsFromService()
    {
        var reports = new List<ReportDto>
        {
            new ReportDto { Id = 1, RecipeId = 10, RecipeTitle = "Soup", ReporterName = "reader", ReportedUserName = "chef" }
        };

        _reportServiceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(Result<IEnumerable<ReportDto>>.Success(reports));

        using var controller = CreateController();

        var result = await controller.ManageRecipes();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdminRecipeReportsViewModel>(viewResult.Model);
        var report = Assert.Single(model.Reports);
        Assert.Equal("Soup", report.RecipeTitle);
    }

    [Fact]
    public async Task DismissReport_WhenServiceSucceeds_RedirectsAndShowsSuccess()
    {
        _reportServiceMock.Setup(s => s.DismissAsync(5)).ReturnsAsync(Result.Success());
        using var controller = CreateController();

        var result = await controller.DismissReport(5);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.ManageRecipes), redirect.ActionName);
        Assert.Equal("Скаргу відхилено.", controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public async Task DeleteReportedRecipe_WhenReportExists_DeletesRecipeByReportedUser()
    {
        var report = new ReportDto { Id = 6, RecipeId = 20, ReportedUserId = 3 };
        _reportServiceMock
            .Setup(s => s.GetByIdAsync(6))
            .ReturnsAsync(Result<ReportDto>.Success(report));
        _recipeServiceMock
            .Setup(s => s.DeleteRecipeAsync(20, 3))
            .ReturnsAsync(Result.Success());

        using var controller = CreateController();

        var result = await controller.DeleteReportedRecipe(6);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.ManageRecipes), redirect.ActionName);
        Assert.Equal("Рецепт видалено. Пов'язані скарги прибрано автоматично.", controller.TempData["SuccessMessage"]);
        _recipeServiceMock.Verify(s => s.DeleteRecipeAsync(20, 3), Times.Once);
    }

    [Fact]
    public async Task DeleteReportedRecipe_WhenReportMissing_DoesNotDeleteRecipe()
    {
        _reportServiceMock
            .Setup(s => s.GetByIdAsync(404))
            .ReturnsAsync(Result<ReportDto>.Failure("Скаргу не знайдено"));

        using var controller = CreateController();

        var result = await controller.DeleteReportedRecipe(404);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AdminController.ManageRecipes), redirect.ActionName);
        Assert.Equal("Скаргу не знайдено", controller.TempData["ErrorMessage"]);
        _recipeServiceMock.Verify(s => s.DeleteRecipeAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    private AdminController CreateController()
    {
        var controller = new AdminController(
            _categoryServiceMock.Object,
            _userServiceMock.Object,
            _reportServiceMock.Object,
            _recipeServiceMock.Object);

        controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            new Microsoft.AspNetCore.Http.DefaultHttpContext(),
            Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

        return controller;
    }
}
