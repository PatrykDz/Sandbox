using MassTransit;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using Shared.Contracts.Events;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Errors;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;

namespace UserService.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<RegisterUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure) return emailResult.Error;

        var nameResult = FullName.Create(request.FirstName, request.LastName);
        if (nameResult.IsFailure) return nameResult.Error;

        if (await userRepository.EmailExistsAsync(emailResult.Value, cancellationToken))
            return UserErrors.EmailAlreadyInUse;

        var userResult = User.Register(emailResult.Value, nameResult.Value);
        if (userResult.IsFailure) return userResult.Error;

        var user = userResult.Value;
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Domain event handler will also publish via outbox, but we can publish directly too
        // The domain event dispatcher interceptor handles this via MediatR
        return user.ToDto();
    }
}
