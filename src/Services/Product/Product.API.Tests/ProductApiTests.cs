using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Product.Infrastructure.Data;
using Shared.Contracts.DTOs;
using Xunit;

namespace Product.API.Tests;

public class ProductApiTests : IClassFixture<ProductApiTests.ProductApiFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly ProductApiFactory _factory;

    public ProductApiTests(ProductApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/product");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var category = await CreateCategoryAsync();
        var product = await CreateProductAsync(category.Id);

        var response = await _client.GetAsync($"/api/product/{product.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(dto);
        Assert.Equal(product.Name, dto.Name);
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/product/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreatesProduct_Returns201()
    {
        var category = await CreateCategoryAsync();
        var create = new CreateProductDto(
            "Test Product", "A test product", null,
            10.00m, 15.00m, 100, true, false, category.Id, null);

        var response = await _client.PostAsJsonAsync("/api/product", create);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(dto);
        Assert.Equal("Test Product", dto.Name);
        Assert.True(dto.Id > 0);
    }

    [Fact]
    public async Task Put_UpdatesProduct_ReturnsOk()
    {
        var category = await CreateCategoryAsync();
        var product = await CreateProductAsync(category.Id);

        var update = new UpdateProductDto(
            "Updated Product", "Updated description", null,
            12.00m, 18.00m, 50, true, false, category.Id, null);

        var response = await _client.PutAsJsonAsync($"/api/product/{product.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(dto);
        Assert.Equal("Updated Product", dto.Name);
    }

    [Fact]
    public async Task Delete_RemovesProduct_Returns204()
    {
        var category = await CreateCategoryAsync();
        var product = await CreateProductAsync(category.Id);

        var response = await _client.DeleteAsync($"/api/product/{product.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/product/{product.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetAllCategories_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/product/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_Existing_ReturnsOk()
    {
        var category = await CreateCategoryAsync();

        var response = await _client.GetAsync($"/api/product/categories/{category.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<ProductCategoryDto>();
        Assert.NotNull(dto);
        Assert.Equal(category.Name, dto.Name);
    }

    [Fact]
    public async Task GetCategoryById_Missing_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/product/categories/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostCategory_CreatesCategory_Returns201()
    {
        var create = new CreateProductCategoryDto("Electronics", "Electronic items", null);

        var response = await _client.PostAsJsonAsync("/api/product/categories", create);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<ProductCategoryDto>();
        Assert.NotNull(dto);
        Assert.Equal("Electronics", dto.Name);
        Assert.True(dto.Id > 0);
    }

    [Fact]
    public async Task PutCategory_UpdatesCategory_ReturnsOk()
    {
        var category = await CreateCategoryAsync();
        var update = new UpdateProductCategoryDto("Updated Category", "Updated desc", null);

        var response = await _client.PutAsJsonAsync($"/api/product/categories/{category.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dto = await response.Content.ReadFromJsonAsync<ProductCategoryDto>();
        Assert.NotNull(dto);
        Assert.Equal("Updated Category", dto.Name);
    }

    [Fact]
    public async Task DeleteCategory_RemovesCategory_Returns204()
    {
        var category = await CreateCategoryAsync();

        var response = await _client.DeleteAsync($"/api/product/categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/product/categories/{category.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task ProductDto_MatchesExpectedShape()
    {
        var category = await CreateCategoryAsync();
        var product = await CreateProductAsync(category.Id);

        var response = await _client.GetAsync($"/api/product/{product.Id}");
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();

        Assert.NotNull(dto);
        Assert.IsType<int>(dto.Id);
        Assert.IsType<string>(dto.Name);
        Assert.IsType<decimal>(dto.BuyingPrice);
        Assert.IsType<decimal>(dto.SellingPrice);
        Assert.IsType<int>(dto.UnitsInStock);
        Assert.IsType<bool>(dto.IsActive);
        Assert.IsType<bool>(dto.IsDiscontinued);
        Assert.IsType<int>(dto.ProductCategoryId);
    }

    private async Task<ProductCategoryDto> CreateCategoryAsync()
    {
        var create = new CreateProductCategoryDto($"Category-{Guid.NewGuid():N}", "Test category", null);
        var response = await _client.PostAsJsonAsync("/api/product/categories", create);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductCategoryDto>())!;
    }

    private async Task<ProductDto> CreateProductAsync(int categoryId)
    {
        var create = new CreateProductDto(
            $"Product-{Guid.NewGuid():N}", "Test product", null,
            5.00m, 10.00m, 50, true, false, categoryId, null);
        var response = await _client.PostAsJsonAsync("/api/product", create);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }

    public class ProductApiFactory : WebApplicationFactory<Program>, IDisposable
    {
        private readonly SqliteConnection _connection;

        public ProductApiFactory()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ProductDbContext>(options =>
                    options.UseSqlite(_connection));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}
