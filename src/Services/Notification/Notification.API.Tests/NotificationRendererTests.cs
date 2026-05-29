using System.Globalization;
using Notification.API.Services;
using Notification.Domain.Entities;
using Xunit;

namespace Notification.API.Tests;

public class NotificationRendererTests
{
    private readonly NotificationRenderer _renderer = new();

    private OrderNotification CreateNotification(decimal orderTotal) => new()
    {
        Id = Guid.NewGuid(),
        OrderId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        OrderTotal = orderTotal,
        CustomerEmail = "test@example.com",
        CustomerName = "Test Customer",
        Type = NotificationType.OrderConfirmation,
        Status = NotificationStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public void RenderNotification_DollarAmount_NotDividedBy100()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        try
        {
            var notification = CreateNotification(149.99m);

            var (subject, body) = _renderer.RenderNotification(notification);

            Assert.Contains("$149.99", body);
            Assert.DoesNotContain("$1.50", body);
            Assert.Contains("$149.99", subject);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Theory]
    [InlineData(0.00, "$0.00")]
    [InlineData(0.01, "$0.01")]
    [InlineData(999999.99, "$999,999.99")]
    [InlineData(50.00, "$50.00")]
    public void RenderNotification_FormatsAmountCorrectly(decimal amount, string expected)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        try
        {
            var notification = CreateNotification(amount);

            var (_, body) = _renderer.RenderNotification(notification);

            Assert.Contains(expected, body);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }
}
