using Shared.BuildingBlocks.Domain;
using Shared.BuildingBlocks.Result;
using UserService.Domain.Enums;
using UserService.Domain.Errors;
using UserService.Domain.Events;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class User : AggregateRoot<Guid>
{
    public Email Email { get; private set; } = default!;
    public FullName Name { get; private set; } = default!;
    public UserStatus Status { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }

    private User() { }

    public static Result<User> Register(Email email, FullName name)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            Status = UserStatus.Active,
            RegisteredAt = DateTime.UtcNow
        };

        user.RaiseDomainEvent(new UserRegisteredDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow,
            user.Id, user.Email.Value, user.Name.FirstName, user.Name.LastName));

        return user;
    }

    public Result UpdateProfile(Email email, FullName name)
    {
        if (Status == UserStatus.Deactivated)
            return Error.Conflict("User.Deactivated", "Cannot update a deactivated user.");

        Email = email;
        Name = name;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserProfileUpdatedDomainEvent(
            Guid.NewGuid(), DateTime.UtcNow,
            Id, Email.Value, Name.FirstName, Name.LastName));

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == UserStatus.Deactivated)
            return UserErrors.AlreadyDeactivated;

        Status = UserStatus.Deactivated;
        DeactivatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserDeactivatedDomainEvent(Guid.NewGuid(), DateTime.UtcNow, Id));

        return Result.Success();
    }
}
