using Shared.BuildingBlocks.CQRS;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserDto>;
