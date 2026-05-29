using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Customer.API.Tests;

public class CustomerApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CustomerApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/customer");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        // First create a customer
        var createResponse = await _client.PostAsJsonAsync("/api/customer", new
        {
            Name = "Test Customer",
            Email = "test@example.com",
            PhoneNumber = "555-0100",
            Address = "123 Test St",
            City = "TestCity",
            Gender = "Male"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/customer/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/customer/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_ValidCustomer_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/customer", new
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            PhoneNumber = "555-0101",
            Address = "456 Elm St",
            City = "Springfield",
            Gender = "Female"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(customer);
        Assert.Equal("Jane Doe", customer!.Name);
    }

    [Fact]
    public async Task Put_ExistingCustomer_ReturnsOk()
    {
        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/customer", new
        {
            Name = "Update Me",
            Email = "update@example.com",
            PhoneNumber = "555-0102",
            Address = "789 Oak Ave",
            City = "Shelbyville",
            Gender = "Male"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(created);

        // Update
        var response = await _client.PutAsJsonAsync($"/api/customer/{created!.Id}", new
        {
            Name = "Updated Name",
            Email = "updated@example.com",
            PhoneNumber = "555-0103",
            Address = "789 Oak Ave",
            City = "Shelbyville",
            Gender = "Male"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingCustomer_ReturnsNoContent()
    {
        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/customer", new
        {
            Name = "Delete Me",
            Email = "delete@example.com",
            PhoneNumber = "555-0104",
            Address = "321 Pine Rd",
            City = "Capital City",
            Gender = "Female"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(created);

        // Delete
        var response = await _client.DeleteAsync($"/api/customer/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AfterCreating_ReturnsCustomers()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/customer", new
        {
            Name = "List Test",
            Email = "list@example.com",
            PhoneNumber = "555-0105",
            Address = "999 Maple Dr",
            City = "Ogdenville",
            Gender = "Male"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var response = await _client.GetAsync("/api/customer");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var customers = await response.Content.ReadFromJsonAsync<List<CustomerResponse>>();
        Assert.NotNull(customers);
        Assert.NotEmpty(customers!);
    }

    private record CustomerResponse(int Id, string Name, string Email, string? PhoneNumber, string? Address, string? City, string? Gender);
}
