using Shared.BuildingBlocks.CQRS;
using UserService.Application.DTOs;

namespace UserService.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Email, string FirstName, string LastName) : ICommand<UserDto>;
