using Shared.BuildingBlocks.CQRS;

namespace UserService.Application.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(Guid UserId) : ICommand;
