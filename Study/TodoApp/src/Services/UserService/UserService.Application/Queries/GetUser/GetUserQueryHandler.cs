using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Result;
using UserService.Application.DTOs;
using UserService.Domain.Errors;
using UserService.Domain.Repositories;

namespace UserService.Application.Queries.GetUser;

public sealed class GetUserQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        return user is null ? UserErrors.NotFound(request.UserId) : user.ToDto();
    }
}
