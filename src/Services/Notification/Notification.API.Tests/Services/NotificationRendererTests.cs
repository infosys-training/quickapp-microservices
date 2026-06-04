using Notification.API.Services;
using Notification.Domain.Entities;
using Xunit;

namespace Notification.API.Tests.Services;

public class NotificationRendererTests
{
    [Fact]
    public void RenderNotification_WithDollarAmount_FormatsCorrectly()
    {
        // Arrange
        var renderer = new NotificationRenderer();
        var notification = new OrderNotification
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            OrderTotal = 149.99m,  // dollars
            CustomerEmail = "test@example.com",
            CustomerName = "Test User",
            Type = NotificationType.OrderConfirmation,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var (subject, body) = renderer.RenderNotification(notification);

        // Assert — $149.99 should render as "$149.99", NOT "$1.50"
        Assert.Contains("$149.99", subject);
        Assert.DoesNotContain("$1.50", subject);
        Assert.Contains("$149.99", body);
        Assert.DoesNotContain("$1.50", body);
    }
}
