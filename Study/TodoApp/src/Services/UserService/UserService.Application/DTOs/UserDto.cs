namespace UserService.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    string Status,
    DateTime RegisteredAt,
    DateTime? UpdatedAt,
    DateTime? DeactivatedAt);
