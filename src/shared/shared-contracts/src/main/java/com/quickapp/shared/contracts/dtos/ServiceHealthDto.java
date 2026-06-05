package com.quickapp.shared.contracts.dtos;

import java.time.Instant;

public record ServiceHealthDto(
        String serviceName,
        String status,
        Instant checkedAt) {
}
