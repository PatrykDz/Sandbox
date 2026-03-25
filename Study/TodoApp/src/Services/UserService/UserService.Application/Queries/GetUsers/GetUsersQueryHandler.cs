using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Pagination;
using Shared.BuildingBlocks.Result;
using UserService.Application.DTOs;
using UserService.Domain.Repositories;

namespace UserService.Application.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await userRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
        var dtos = items.Select(u => u.ToDto()).ToList().AsReadOnly();
        return PagedResult<UserDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
