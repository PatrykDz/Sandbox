using UserService.Domain.Entities;

namespace UserService.Application.DTOs;

public static class UserMappings
{
    public static UserDto ToDto(this User user) => new(
        user.Id,
        user.Email.Value,
        user.Name.FirstName,
        user.Name.LastName,
        user.Name.DisplayName,
        user.Status.ToString(),
        user.RegisteredAt,
        user.UpdatedAt,
        user.DeactivatedAt);
}
