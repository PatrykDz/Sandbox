using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;

namespace TodoService.Application.Commands.RemoveTag;

public sealed record RemoveTagCommand(Guid TodoId, string TagName) : ICommand<TodoDto>;
