using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Pagination;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.GetUsers;

public sealed record GetUsersQuery(int Page = 1, int PageSize = 20) : IQuery<PagedResult<UserDto>>;
