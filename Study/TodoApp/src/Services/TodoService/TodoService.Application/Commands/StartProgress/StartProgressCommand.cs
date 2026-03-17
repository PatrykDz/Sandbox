using Shared.BuildingBlocks.CQRS;
using TodoService.Application.DTOs;

namespace TodoService.Application.Commands.StartProgress;

public sealed record StartProgressCommand(Guid Id) : ICommand<TodoDto>;
