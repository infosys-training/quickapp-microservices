using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Shared.Contracts.DTOs;
using Xunit;

namespace Inventory.API.Tests;

public class InventoryApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InventoryApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/inventory");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/inventory/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreatesItem_Returns201()
    {
        var dto = new CreateInventoryItemDto(
            ProductId: 1,
            UnitsInStock: 100,
            ReorderLevel: 10,
            IsActive: true,
            IsDiscontinued: false,
            BuyingPrice: 9.99m
        );

        var response = await _client.PostAsJsonAsync("/api/inventory", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<InventoryItemDto>();
        Assert.NotNull(created);
        Assert.Equal(dto.ProductId, created.ProductId);
        Assert.Equal(dto.UnitsInStock, created.UnitsInStock);
        Assert.Equal(dto.ReorderLevel, created.ReorderLevel);
        Assert.Equal(dto.IsActive, created.IsActive);
        Assert.Equal(dto.IsDiscontinued, created.IsDiscontinued);
        Assert.Equal(dto.BuyingPrice, created.BuyingPrice);
    }

    [Fact]
    public async Task GetById_ExistingItem_ReturnsOk()
    {
        var dto = new CreateInventoryItemDto(
            ProductId: 2,
            UnitsInStock: 50,
            ReorderLevel: 5,
            IsActive: true,
            IsDiscontinued: false,
            BuyingPrice: 19.99m
        );

        var createResponse = await _client.PostAsJsonAsync("/api/inventory", dto);
        var created = await createResponse.Content.ReadFromJsonAsync<InventoryItemDto>();

        var response = await _client.GetAsync($"/api/inventory/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<InventoryItemDto>();
        Assert.NotNull(item);
        Assert.Equal(created.Id, item.Id);
        Assert.Equal(dto.ProductId, item.ProductId);
    }

    [Fact]
    public async Task Put_UpdatesItem_ReturnsOk()
    {
        var createDto = new CreateInventoryItemDto(
            ProductId: 3,
            UnitsInStock: 30,
            ReorderLevel: 5,
            IsActive: true,
            IsDiscontinued: false,
            BuyingPrice: 15.00m
        );

        var createResponse = await _client.PostAsJsonAsync("/api/inventory", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<InventoryItemDto>();

        var updateDto = new UpdateInventoryItemDto(
            ProductId: 3,
            UnitsInStock: 60,
            ReorderLevel: 10,
            IsActive: true,
            IsDiscontinued: false,
            BuyingPrice: 12.00m
        );

        var response = await _client.PutAsJsonAsync($"/api/inventory/{created!.Id}", updateDto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<InventoryItemDto>();
        Assert.NotNull(updated);
        Assert.Equal(60, updated.UnitsInStock);
        Assert.Equal(12.00m, updated.BuyingPrice);
    }

    [Fact]
    public async Task Delete_ExistingItem_ReturnsNoContent()
    {
        var createDto = new CreateInventoryItemDto(
            ProductId: 4,
            UnitsInStock: 20,
            ReorderLevel: 5,
            IsActive: true,
            IsDiscontinued: false,
            BuyingPrice: 5.00m
        );

        var createResponse = await _client.PostAsJsonAsync("/api/inventory", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<InventoryItemDto>();

        var response = await _client.DeleteAsync($"/api/inventory/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/inventory/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistent_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/inventory/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_NonExistent_ReturnsNotFound()
    {
        var updateDto = new UpdateInventoryItemDto(
            ProductId: 1,
            UnitsInStock: 10,
            ReorderLevel: 5,
            IsActive: true,
            IsDiscontinued: false,
            BuyingPrice: 1.00m
        );

        var response = await _client.PutAsJsonAsync("/api/inventory/99999", updateDto);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
