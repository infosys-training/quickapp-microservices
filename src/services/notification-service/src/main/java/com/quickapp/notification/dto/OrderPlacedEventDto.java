package com.quickapp.notification.dto;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.UUID;

/**
 * DTO for receiving order events via HTTP (local testing).
 * Maps to the shared OrderPlacedEvent contract.
 */
public record OrderPlacedEventDto(
        UUID orderId,
        UUID customerId,
        BigDecimal totalAmount,
        Instant placedAt) {
}
