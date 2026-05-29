namespace Shared.Contracts.DTOs;

public record CustomerDto(
    int Id,
    string Name,
    string Email,
    string? PhoneNumber,
    string? Address,
    string? City,
    string? Gender
);

public record CreateCustomerDto(
    string Name,
    string Email,
    string? PhoneNumber,
    string? Address,
    string? City,
    string? Gender
);

public record UpdateCustomerDto(
    string Name,
    string Email,
    string? PhoneNumber,
    string? Address,
    string? City,
    string? Gender
);
