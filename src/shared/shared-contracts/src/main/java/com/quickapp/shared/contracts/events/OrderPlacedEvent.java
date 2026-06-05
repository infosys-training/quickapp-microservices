package com.quickapp.shared.contracts.events;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.UUID;

/**
 * Integration event published when a new order is placed.
 * Consumed by Notification service to trigger confirmation emails.
 */
public record OrderPlacedEvent(
        UUID orderId,
        UUID customerId,
        BigDecimal totalAmount,
        Instant placedAt) {
}
