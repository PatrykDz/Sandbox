using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using UserService.Domain.Errors;
using UserService.Domain.Repositories;

namespace UserService.Application.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateUserCommand>
{
    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return UserErrors.NotFound(request.UserId);

        var result = user.Deactivate();
        if (result.IsFailure) return result.Error;

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
