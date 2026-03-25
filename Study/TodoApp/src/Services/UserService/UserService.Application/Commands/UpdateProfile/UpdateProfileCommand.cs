using Shared.BuildingBlocks.CQRS;
using UserService.Application.DTOs;

namespace UserService.Application.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(Guid UserId, string Email, string FirstName, string LastName) : ICommand<UserDto>;
