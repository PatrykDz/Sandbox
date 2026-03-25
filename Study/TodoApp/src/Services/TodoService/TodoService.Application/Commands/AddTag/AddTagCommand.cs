using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;

namespace TodoService.Application.Commands.AddTag;

public sealed record AddTagCommand(Guid TodoId, string TagName) : ICommand<TodoDto>;
