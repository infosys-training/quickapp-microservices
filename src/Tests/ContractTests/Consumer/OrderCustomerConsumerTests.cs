using System.Net;
using System.Net.Http.Json;
using PactNet;
using Xunit;
using Xunit.Abstractions;

namespace ContractTests.Consumer;

/// <summary>
/// Pact consumer tests for the Order → Customer service boundary.
///
/// Contract: Order service queries the Customer service to resolve customer
/// details (name, email) when processing a new order. This data is needed
/// to populate the OrderPlacedEvent with customer information before
/// publishing to the Notification service.
///
/// Shared contract type: Shared.Contracts.DTOs.CustomerDto
/// </summary>
public class OrderCustomerConsumerTests
{
    private readonly ITestOutputHelper _output;

    public OrderCustomerConsumerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task OrderService_GetsCustomerById_ReturnsCustomerDetails()
    {
        var config = new PactConfig
        {
            PactDir = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "..", "..", "pacts")),
            LogLevel = PactLogLevel.Information
        };

        var pact = Pact.V3("OrderService", "CustomerService", config).WithHttpInteractions();
        var customerId = "c1d2e3f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f";

        pact
            .UponReceiving("a request for customer details by ID")
                .Given("a customer with the specified ID exists")
                .WithRequest(HttpMethod.Get, $"/api/customer/{customerId}")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = PactNet.Matchers.Match.Type("c1d2e3f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"),
                    fullName = PactNet.Matchers.Match.Type("Alice Smith"),
                    email = PactNet.Matchers.Match.Type("alice@example.com")
                });

        await pact.VerifyAsync(async ctx =>
        {
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };

            var response = await client.GetAsync($"/api/customer/{customerId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            Assert.NotNull(customer);
            Assert.Equal("Alice Smith", customer!.FullName);
            Assert.Equal("alice@example.com", customer.Email);
        });
    }

    [Fact]
    public async Task OrderService_GetsCustomerById_ReturnsNotFoundForMissingCustomer()
    {
        var config = new PactConfig
        {
            PactDir = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "..", "..", "pacts")),
            LogLevel = PactLogLevel.Information
        };

        var pact = Pact.V3("OrderService", "CustomerService", config).WithHttpInteractions();
        var missingCustomerId = "00000000-0000-0000-0000-000000000000";

        pact
            .UponReceiving("a request for a non-existent customer")
                .Given("no customer with the specified ID exists")
                .WithRequest(HttpMethod.Get, $"/api/customer/{missingCustomerId}")
            .WillRespond()
                .WithStatus(HttpStatusCode.NotFound);

        await pact.VerifyAsync(async ctx =>
        {
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };

            var response = await client.GetAsync($"/api/customer/{missingCustomerId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        });
    }

    [Fact]
    public async Task OrderService_GetsAllCustomers_ReturnsCustomerList()
    {
        var config = new PactConfig
        {
            PactDir = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "..", "..", "pacts")),
            LogLevel = PactLogLevel.Information
        };

        var pact = Pact.V3("OrderService", "CustomerService", config).WithHttpInteractions();

        pact
            .UponReceiving("a request for all customers")
                .Given("customers exist in the system")
                .WithRequest(HttpMethod.Get, "/api/customer")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(PactNet.Matchers.Match.MinType(new
                {
                    id = PactNet.Matchers.Match.Type("c1d2e3f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"),
                    fullName = PactNet.Matchers.Match.Type("Alice Smith"),
                    email = PactNet.Matchers.Match.Type("alice@example.com")
                }, 1));

        await pact.VerifyAsync(async ctx =>
        {
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };

            var response = await client.GetAsync("/api/customer");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var customers = await response.Content.ReadFromJsonAsync<CustomerResponse[]>();
            Assert.NotNull(customers);
            Assert.NotEmpty(customers!);
            Assert.All(customers, c =>
            {
                Assert.NotNull(c.FullName);
                Assert.NotNull(c.Email);
            });
        });
    }

    private record CustomerResponse(string Id, string FullName, string Email);
}
