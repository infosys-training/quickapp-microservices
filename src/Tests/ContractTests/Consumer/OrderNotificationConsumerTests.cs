using System.Net;
using System.Net.Http.Json;
using PactNet;
using Xunit;
using Xunit.Abstractions;

namespace ContractTests.Consumer;

/// <summary>
/// Pact consumer tests for the Order → Notification service boundary.
///
/// Contract: Order service publishes an OrderPlacedEvent to the Notification service
/// via POST /api/notification/events/order-placed. The Notification service processes
/// the event, renders an email notification, and returns a 201 Created response
/// containing the notification ID and a preview URL.
///
/// Shared contract type: Shared.Contracts.Events.OrderPlacedEvent
/// </summary>
public class OrderNotificationConsumerTests
{
    private readonly ITestOutputHelper _output;

    public OrderNotificationConsumerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task OrderService_SendsOrderPlacedEvent_NotificationServiceCreatesNotification()
    {
        var config = new PactConfig
        {
            PactDir = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "..", "..", "pacts")),
            LogLevel = PactLogLevel.Information
        };

        var pact = Pact.V3("OrderService", "NotificationService", config).WithHttpInteractions();
        var orderId = "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d";
        var customerId = "c1d2e3f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f";

        pact
            .UponReceiving("a request to process an order-placed event")
                .Given("the notification service is available")
                .WithRequest(HttpMethod.Post, "/api/notification/events/order-placed")
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(new
                {
                    orderId,
                    customerId,
                    totalAmount = 4999m,
                    placedAt = "2025-06-01T12:00:00Z"
                })
            .WillRespond()
                .WithStatus(HttpStatusCode.Created)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = PactNet.Matchers.Match.Type("00000000-0000-0000-0000-000000000000"),
                    previewUrl = PactNet.Matchers.Match.Regex(
                        "/api/notification/00000000-0000-0000-0000-000000000000/preview",
                        "^/api/notification/.+/preview$")
                });

        await pact.VerifyAsync(async ctx =>
        {
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };

            var response = await client.PostAsJsonAsync("/api/notification/events/order-placed", new
            {
                orderId,
                customerId,
                totalAmount = 4999m,
                placedAt = "2025-06-01T12:00:00Z"
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<NotificationCreatedResponse>();
            Assert.NotNull(body);
            Assert.NotNull(body!.Id);
            Assert.Contains("/preview", body.PreviewUrl);
        });
    }

    [Fact]
    public async Task OrderService_GetsNotification_ReturnsNotificationDetails()
    {
        var config = new PactConfig
        {
            PactDir = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "..", "..", "pacts")),
            LogLevel = PactLogLevel.Information
        };

        var pact = Pact.V3("OrderService", "NotificationService", config).WithHttpInteractions();
        var notificationId = "b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e";

        pact
            .UponReceiving("a request to get notification details")
                .Given("a notification with the specified ID exists")
                .WithRequest(HttpMethod.Get, $"/api/notification/{notificationId}")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = PactNet.Matchers.Match.Type("00000000-0000-0000-0000-000000000000"),
                    orderId = PactNet.Matchers.Match.Type("00000000-0000-0000-0000-000000000000"),
                    customerId = PactNet.Matchers.Match.Type("00000000-0000-0000-0000-000000000000"),
                    orderTotal = PactNet.Matchers.Match.Number(4999),
                    customerEmail = PactNet.Matchers.Match.Type("customer@example.com"),
                    customerName = PactNet.Matchers.Match.Type("Valued Customer"),
                    type = PactNet.Matchers.Match.Integer(0),
                    status = PactNet.Matchers.Match.Integer(1),
                    renderedSubject = PactNet.Matchers.Match.Type("Order Confirmation"),
                    createdAt = PactNet.Matchers.Match.Type("2025-06-01T12:00:00Z")
                });

        await pact.VerifyAsync(async ctx =>
        {
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };

            var response = await client.GetAsync($"/api/notification/{notificationId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<NotificationDetailResponse>();
            Assert.NotNull(body);
            Assert.NotNull(body!.Id);
            Assert.NotNull(body.OrderId);
        });
    }

    private record NotificationCreatedResponse(string Id, string PreviewUrl);

    private record NotificationDetailResponse(
        string Id,
        string OrderId,
        string CustomerId,
        decimal OrderTotal,
        string CustomerEmail,
        string CustomerName,
        int Type,
        int Status,
        string RenderedSubject,
        string CreatedAt);
}
