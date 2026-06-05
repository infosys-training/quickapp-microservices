using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Infrastructure.Data;
using Shared.Contracts.DTOs;
using Xunit;

namespace Order.IntegrationTests;

public class OrderApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<OrderDbContext>(options =>
                    options.UseSqlite("Data Source=integration_test.db"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
    public async Task CreateOrder_ReturnsCreatedOrder()
    {
        var createDto = new CreateOrderDto(
            Discount: 5.0m,
            Comments: "Integration test order",
            CashierId: "test-cashier",
            CustomerId: 1,
            OrderDetails:
            [
                new CreateOrderDetailDto(
                    UnitPrice: 29.99m,
                    Quantity: 2,
                    Discount: 0m,
                    ProductId: 1
                )
            ]
        );

        var response = await _client.PostAsJsonAsync("/api/order", createDto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
        Assert.Equal(5.0m, order.Discount);
        Assert.Equal("Integration test order", order.Comments);
        Assert.Equal(1, order.CustomerId);
        Assert.Single(order.OrderDetails);
        Assert.Equal(29.99m, order.OrderDetails[0].UnitPrice);
        Assert.Equal(2, order.OrderDetails[0].Quantity);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForNonExistent()
    {
        var response = await _client.GetAsync("/api/order/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetById_RoundTrip()
    {
        var createDto = new CreateOrderDto(
            Discount: 10.0m,
            Comments: "Roundtrip test",
            CashierId: null,
            CustomerId: 2,
            OrderDetails:
            [
                new CreateOrderDetailDto(UnitPrice: 15.50m, Quantity: 1, Discount: 1.0m, ProductId: 3)
            ]
        );

        var createResponse = await _client.PostAsJsonAsync("/api/order", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/order/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal("Roundtrip test", fetched.Comments);
    }

    [Fact]
    public async Task DeleteOrder_ReturnsNoContent()
    {
        var createDto = new CreateOrderDto(
            Discount: 0m,
            Comments: "To be deleted",
            CashierId: null,
            CustomerId: 3,
            OrderDetails: []
        );

        var createResponse = await _client.PostAsJsonAsync("/api/order", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/order/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/order/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
