using System.Net;
using LaconicAndIconic.Web.Models;
using LaconicAndIconic.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LaconicAndIconic.Tests.Services;

public sealed class TheMealDbClientTests : IDisposable
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly List<HttpClient> _httpClients = [];

    [Fact]
    public async Task SearchRecipesAsync_MapsMealDbResponse()
    {
        // Arrange
        const string responseJson = """
        {
          "meals": [
            {
              "idMeal": "52772",
              "strMeal": "Teriyaki Chicken Casserole",
              "strCategory": "Chicken",
              "strArea": "Japanese",
              "strInstructions": "Bake it.",
              "strMealThumb": "https://example.com/meal.jpg",
              "strSource": "https://example.com/source",
              "strIngredient1": "Chicken",
              "strMeasure1": "2 pieces",
              "strIngredient2": "Soy sauce",
              "strMeasure2": "3 tbsp",
              "strIngredient3": "",
              "strMeasure3": ""
            }
          ]
        }
        """;
        using var handler = new StubHttpMessageHandler(responseJson);
        var client = CreateClient(handler);

        // Act
        var recipes = await client.SearchRecipesAsync(" chicken ");

        // Assert
        var recipe = Assert.Single(recipes);
        Assert.Equal("52772", recipe.ExternalId);
        Assert.Equal("Teriyaki Chicken Casserole", recipe.Title);
        Assert.Equal("Chicken", recipe.Category);
        Assert.Equal("Japanese", recipe.Area);
        Assert.Equal("Bake it.", recipe.Instructions);
        Assert.Equal($"2 pieces Chicken{Environment.NewLine}3 tbsp Soy sauce", recipe.Ingredients);
        Assert.Equal(new Uri("https://example.com/meal.jpg"), recipe.ThumbnailUri);
        Assert.Equal(new Uri("https://example.com/source"), recipe.SourceUri);
        Assert.Equal("/api/json/v1/1/search.php?s=chicken", handler.RequestUris.Single().PathAndQuery);
    }

    [Fact]
    public async Task SearchRecipesAsync_UsesCacheForSameSearchTermIgnoringCase()
    {
        // Arrange
        const string responseJson = """
        {
          "meals": [
            {
              "idMeal": "1",
              "strMeal": "Arrabiata",
              "strCategory": "Vegetarian",
              "strArea": "Italian",
              "strInstructions": "Cook pasta.",
              "strMealThumb": null,
              "strSource": null
            }
          ]
        }
        """;
        using var handler = new StubHttpMessageHandler(responseJson);
        var client = CreateClient(handler);

        // Act
        var firstResult = await client.SearchRecipesAsync("pasta");
        var secondResult = await client.SearchRecipesAsync(" PASTA ");

        // Assert
        Assert.Same(firstResult, secondResult);
        Assert.Single(handler.RequestUris);
    }

    [Fact]
    public async Task GetRecipeByExternalIdAsync_ReturnsNullWithoutCallingApi_WhenIdIsBlank()
    {
        // Arrange
        using var handler = new StubHttpMessageHandler("""{ "meals": [] }""");
        var client = CreateClient(handler);

        // Act
        var recipe = await client.GetRecipeByExternalIdAsync(" ");

        // Assert
        Assert.Null(recipe);
        Assert.Empty(handler.RequestUris);
    }

    [Fact]
    public async Task GetRecipeByExternalIdAsync_CachesRecipeLookup()
    {
        // Arrange
        const string responseJson = """
        {
          "meals": [
            {
              "idMeal": "42",
              "strMeal": "Soup",
              "strCategory": "Starter",
              "strArea": "French",
              "strInstructions": "Simmer.",
              "strMealThumb": null,
              "strSource": null
            }
          ]
        }
        """;
        using var handler = new StubHttpMessageHandler(responseJson);
        var client = CreateClient(handler);

        // Act
        var firstResult = await client.GetRecipeByExternalIdAsync("42");
        var secondResult = await client.GetRecipeByExternalIdAsync("42");

        // Assert
        Assert.Same(firstResult, secondResult);
        Assert.Single(handler.RequestUris);
        Assert.Equal("/api/json/v1/1/lookup.php?i=42", handler.RequestUris.Single().PathAndQuery);
    }

    public void Dispose()
    {
        foreach (var httpClient in _httpClients)
        {
            httpClient.Dispose();
        }

        _memoryCache.Dispose();
    }

    private TheMealDbClient CreateClient(StubHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.themealdb.com/api/json/v1/1/")
        };
        _httpClients.Add(httpClient);

        return new TheMealDbClient(
            httpClient,
            _memoryCache,
            Options.Create(new TheMealDbOptions { CacheMinutes = 10 }),
            NullLogger<TheMealDbClient>.Instance);
    }

    private sealed class StubHttpMessageHandler(string responseJson) : HttpMessageHandler
    {
        public List<Uri> RequestUris { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUris.Add(request.RequestUri!);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
