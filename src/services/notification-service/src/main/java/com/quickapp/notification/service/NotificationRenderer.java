package com.quickapp.notification.service;

import com.quickapp.notification.domain.entity.OrderNotification;
import com.quickapp.notification.domain.enums.NotificationType;
import org.springframework.stereotype.Component;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.text.NumberFormat;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.util.Locale;

/**
 * Renders notification content into styled HTML email previews.
 * Handles currency formatting, template selection, and layout generation
 * for all notification types (order confirmation, shipping, etc.).
 */
@Component
public class NotificationRenderer {

    private static final DateTimeFormatter DATE_FORMATTER =
            DateTimeFormatter.ofPattern("MMMM dd, yyyy 'at' h:mm a")
                    .withZone(ZoneId.of("UTC"));

    /**
     * Formats a monetary amount for display in notification emails.
     * Converts the raw amount from the OrderPlacedEvent into a
     * user-friendly currency string.
     */
    private static String formatCurrency(BigDecimal amount) {
        BigDecimal dollars = amount.divide(new BigDecimal(100), 2, RoundingMode.HALF_UP);
        NumberFormat currencyFormat = NumberFormat.getCurrencyInstance(Locale.US);
        return currencyFormat.format(dollars);
    }

    public String renderOrderConfirmation(OrderNotification notification) {
        String formattedTotal = formatCurrency(notification.getOrderTotal());
        String formattedDate = DATE_FORMATTER.format(notification.getCreatedAt());
        String orderIdShort = notification.getOrderId().toString().substring(0, 8).toUpperCase();

        return """
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="utf-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                </head>
                <body style="margin: 0; padding: 0; background-color: #f4f4f7; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;">
                    <table role="presentation" width="100%%" cellpadding="0" cellspacing="0" style="background-color: #f4f4f7;">
                        <tr>
                            <td align="center" style="padding: 24px;">
                                <table role="presentation" width="600" cellpadding="0" cellspacing="0" style="background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.08);">
                                    <!-- Header -->
                                    <tr>
                                        <td style="background: linear-gradient(135deg, #0078d4, #00bcf2); padding: 32px 40px; text-align: center;">
                                            <h1 style="margin: 0; color: #ffffff; font-size: 24px; font-weight: 600;">Order Confirmed</h1>
                                            <p style="margin: 8px 0 0; color: rgba(255,255,255,0.9); font-size: 14px;">Thank you for your purchase!</p>
                                        </td>
                                    </tr>

                                    <!-- Body -->
                                    <tr>
                                        <td style="padding: 40px;">
                                            <p style="margin: 0 0 16px; color: #333333; font-size: 16px;">
                                                Hi <strong>%s</strong>,
                                            </p>
                                            <p style="margin: 0 0 24px; color: #555555; font-size: 14px; line-height: 1.6;">
                                                We've received your order and it's being processed. Here's a summary of your purchase:
                                            </p>

                                            <!-- Order Summary Card -->
                                            <table role="presentation" width="100%%" cellpadding="0" cellspacing="0" style="background-color: #f8f9fa; border-radius: 8px; border: 1px solid #e9ecef;">
                                                <tr>
                                                    <td style="padding: 24px;">
                                                        <table role="presentation" width="100%%" cellpadding="0" cellspacing="0">
                                                            <tr>
                                                                <td style="padding: 8px 0; color: #6c757d; font-size: 13px; text-transform: uppercase; letter-spacing: 0.5px;">Order Number</td>
                                                                <td style="padding: 8px 0; color: #333333; font-size: 14px; text-align: right; font-family: 'Courier New', monospace;">%s</td>
                                                            </tr>
                                                            <tr>
                                                                <td style="padding: 8px 0; color: #6c757d; font-size: 13px; text-transform: uppercase; letter-spacing: 0.5px;">Date</td>
                                                                <td style="padding: 8px 0; color: #333333; font-size: 14px; text-align: right;">%s</td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="2" style="padding: 12px 0 0; border-top: 2px solid #dee2e6;"></td>
                                                            </tr>
                                                            <tr>
                                                                <td style="padding: 8px 0; color: #333333; font-size: 16px; font-weight: 700;">Total</td>
                                                                <td style="padding: 8px 0; color: #0078d4; font-size: 24px; font-weight: 700; text-align: right;">%s</td>
                                                            </tr>
                                                        </table>
                                                    </td>
                                                </tr>
                                            </table>

                                            <p style="margin: 24px 0 0; color: #555555; font-size: 14px; line-height: 1.6;">
                                                You'll receive a shipping confirmation email with tracking details once your order has shipped.
                                            </p>
                                        </td>
                                    </tr>

                                    <!-- Footer -->
                                    <tr>
                                        <td style="padding: 24px 40px; background-color: #f8f9fa; border-top: 1px solid #e9ecef; text-align: center;">
                                            <p style="margin: 0; color: #6c757d; font-size: 12px;">
                                                This email was sent to <strong>%s</strong>
                                            </p>
                                            <p style="margin: 8px 0 0; color: #adb5bd; font-size: 11px;">
                                                QuickApp Store &mdash; Powered by Decomposed Microservices
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
                """.formatted(
                notification.getCustomerName(),
                orderIdShort,
                formattedDate,
                formattedTotal,
                notification.getCustomerEmail());
    }

    public record RenderedNotification(String subject, String body) {}

    public RenderedNotification renderNotification(OrderNotification notification) {
        if (notification.getType() == NotificationType.ORDER_CONFIRMATION) {
            return new RenderedNotification(
                    "Order Confirmed — " + formatCurrency(notification.getOrderTotal()),
                    renderOrderConfirmation(notification));
        }
        return new RenderedNotification(
                "Notification",
                "<p>Notification for order " + notification.getOrderId() + "</p>");
    }
}
