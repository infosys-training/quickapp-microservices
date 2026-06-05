namespace Shared.Contracts.DTOs;

/// <summary>
/// Standardized customer data contract used for inter-service communication.
/// Consumed by Order and Notification services when resolving customer details.
/// </summary>
public record CustomerDto(
    Guid Id,
    string FullName,
    string Email
);
