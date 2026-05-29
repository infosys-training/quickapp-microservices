using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Infrastructure.Data;
using Shared.Contracts.DTOs;
using Xunit;

namespace Order.API.Tests;

public class OrderApiContractTests : IClassFixture<OrderApiFactory>
{
    private readonly HttpClient _client;

    public OrderApiContractTests(OrderApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<OrderDto> CreateTestOrder()
    {
        var createDto = new CreateOrderDto(
            Discount: 10.0m,
            Comments: "Test order",
            CashierId: "cashier-1",
            CustomerId: 1,
            OrderDetails: new List<CreateOrderDetailDto>
            {
                new(UnitPrice: 25.50m, Quantity: 2, Discount: 0m, ProductId: 1),
                new(UnitPrice: 15.00m, Quantity: 1, Discount: 5m, ProductId: 2)
            }
        );
        var response = await _client.PostAsJsonAsync("/api/order", createDto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/order");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        Assert.NotNull(orders);
    }

    [Fact]
    public async Task GetById_ExistingOrder_ReturnsOk()
    {
        var created = await CreateTestOrder();

        var response = await _client.GetAsync($"/api/order/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal(created.Id, order.Id);
        Assert.Equal(10.0m, order.Discount);
        Assert.Equal("Test order", order.Comments);
        Assert.Equal("cashier-1", order.CashierId);
        Assert.Equal(1, order.CustomerId);
    }

    [Fact]
    public async Task GetById_NonExistingOrder_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/order/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreatesOrder_ReturnsCreated()
    {
        var createDto = new CreateOrderDto(
            Discount: 5.0m,
            Comments: "New order",
            CashierId: "cashier-2",
            CustomerId: 2,
            OrderDetails: new List<CreateOrderDetailDto>
            {
                new(UnitPrice: 100.00m, Quantity: 3, Discount: 10m, ProductId: 5)
            }
        );

        var response = await _client.PostAsJsonAsync("/api/order", createDto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.True(order.Id > 0);
        Assert.Equal(5.0m, order.Discount);
        Assert.Equal("New order", order.Comments);
        Assert.Equal("cashier-2", order.CashierId);
        Assert.Equal(2, order.CustomerId);
        Assert.Single(order.OrderDetails);
        Assert.Equal(100.00m, order.OrderDetails.First().UnitPrice);
        Assert.Equal(3, order.OrderDetails.First().Quantity);
    }

    [Fact]
    public async Task Put_ExistingOrder_ReturnsOk()
    {
        var created = await CreateTestOrder();

        var updateDto = new UpdateOrderDto(
            Discount: 20.0m,
            Comments: "Updated order",
            CashierId: "cashier-3",
            CustomerId: 1,
            OrderDetails: new List<CreateOrderDetailDto>
            {
                new(UnitPrice: 50.00m, Quantity: 5, Discount: 2m, ProductId: 3)
            }
        );

        var response = await _client.PutAsJsonAsync($"/api/order/{created.Id}", updateDto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal(created.Id, order.Id);
        Assert.Equal(20.0m, order.Discount);
        Assert.Equal("Updated order", order.Comments);
    }

    [Fact]
    public async Task Put_NonExistingOrder_ReturnsNotFound()
    {
        var updateDto = new UpdateOrderDto(
            Discount: 0m,
            Comments: "Does not exist",
            CashierId: null,
            CustomerId: 1,
            OrderDetails: new List<CreateOrderDetailDto>()
        );

        var response = await _client.PutAsJsonAsync("/api/order/99999", updateDto);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingOrder_ReturnsNoContent()
    {
        var created = await CreateTestOrder();

        var response = await _client.DeleteAsync($"/api/order/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await _client.GetAsync($"/api/order/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistingOrder_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/order/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByCustomerId_ReturnsOrders()
    {
        await CreateTestOrder();

        var response = await _client.GetAsync("/api/order/customer/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        Assert.NotNull(orders);
        Assert.All(orders, o => Assert.Equal(1, o.CustomerId));
    }

    [Fact]
    public async Task Post_OrderDto_MatchesSharedContract()
    {
        var created = await CreateTestOrder();

        Assert.True(created.Id > 0);
        Assert.IsType<decimal>(created.Discount);
        Assert.IsType<int>(created.CustomerId);
        Assert.NotEqual(default, created.CreatedDate);
        Assert.NotEqual(default, created.UpdatedDate);
        Assert.NotEmpty(created.OrderDetails);

        var detail = created.OrderDetails.First();
        Assert.True(detail.Id > 0);
        Assert.IsType<decimal>(detail.UnitPrice);
        Assert.IsType<int>(detail.Quantity);
        Assert.IsType<decimal>(detail.Discount);
        Assert.IsType<int>(detail.ProductId);
        Assert.Equal(created.Id, detail.OrderId);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
