using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using UserService.Application.DTOs;
using UserService.Domain.Errors;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateProfileCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return UserErrors.NotFound(request.UserId);

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure) return emailResult.Error;

        var nameResult = FullName.Create(request.FirstName, request.LastName);
        if (nameResult.IsFailure) return nameResult.Error;

        // Check email uniqueness if changed
        if (emailResult.Value != user.Email)
        {
            if (await userRepository.EmailExistsAsync(emailResult.Value, cancellationToken))
                return UserErrors.EmailAlreadyInUse;
        }

        var result = user.UpdateProfile(emailResult.Value, nameResult.Value);
        if (result.IsFailure) return result.Error;

        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
