using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.BuildingBlocks.Pagination;
using Shared.BuildingBlocks.Result;
using TodoService.Application.Commands.AddTag;
using TodoService.Application.Commands.AssignTodo;
using TodoService.Application.Commands.CancelTodo;
using TodoService.Application.Commands.CompleteTodo;
using TodoService.Application.Commands.CreateTodo;
using TodoService.Application.Commands.DeleteTodo;
using TodoService.Application.Commands.RemoveTag;
using TodoService.Application.Commands.StartProgress;
using TodoService.Application.Commands.UpdateTodo;
using TodoService.Application.DTOs;
using TodoService.Application.Queries.GetTodo;
using TodoService.Application.Queries.GetTodoHistory;
using TodoService.Application.Queries.GetTodos;
using TodoService.Domain.Enums;

namespace TodoService.API.Controllers;

[ApiController]
[Route("api/v1/todos")]
[Produces("application/json")]
public sealed class TodosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTodos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TodoStatus? status = null,
        [FromQuery] TodoPriority? priority = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool overdueOnly = false,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetTodosQuery(page, pageSize, status, priority, assignedToUserId, searchTerm, overdueOnly),
            cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTodo(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTodoQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    /// <summary>Returns the paginated state-change history for a specific todo.</summary>
    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetTodoHistory(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetTodoHistoryQuery(id, page, pageSize), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTodo(
        [FromBody] CreateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTodoCommand(request.Title, request.Description, request.Priority, request.DueDate, request.AssignedToUserId),
            cancellationToken);

        return result.Match<IActionResult>(
            onSuccess: dto => CreatedAtAction(nameof(GetTodo), new { id = dto.Id }, dto),
            onFailure: error => error.ToActionResult(this));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTodo(
        Guid id,
        [FromBody] UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateTodoCommand(id, request.Title, request.Description, request.Priority, request.DueDate),
            cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTodo(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CompleteTodoCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/start-progress")]
    public async Task<IActionResult> StartProgress(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new StartProgressCommand(id), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelTodo(
        Guid id,
        [FromBody] CancelTodoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CancelTodoCommand(id, request.Reason), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> AssignTodo(
        Guid id,
        [FromBody] AssignTodoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AssignTodoCommand(id, request.UserId), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/tags")]
    public async Task<IActionResult> AddTag(
        Guid id,
        [FromBody] TagRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AddTagCommand(id, request.Name), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}/tags/{tagName}")]
    public async Task<IActionResult> RemoveTag(
        Guid id,
        string tagName,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveTagCommand(id, tagName), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTodo(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteTodoCommand(id), cancellationToken);
        return result.Match<IActionResult>(
            onSuccess: () => NoContent(),
            onFailure: error => error.ToActionResult(this));
    }
}

// ─── Request models ────────────────────────────────────────────────────────────

public sealed record CreateTodoRequest(
    string Title, string? Description, TodoPriority Priority, DateTime? DueDate, Guid? AssignedToUserId);

public sealed record UpdateTodoRequest(
    string Title, string? Description, TodoPriority Priority, DateTime? DueDate);

public sealed record CancelTodoRequest(string Reason);

public sealed record AssignTodoRequest(Guid UserId);

public sealed record TagRequest(string Name);

// ─── Result extension helpers ──────────────────────────────────────────────────

internal static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        => result.Match<IActionResult>(
            onSuccess: value => controller.Ok(value),
            onFailure: error => error.ToActionResult(controller));

    public static IActionResult ToActionResult(this Error error, ControllerBase controller)
        => error.Type switch
        {
            ErrorType.NotFound => controller.NotFound(new { error.Code, error.Description }),
            ErrorType.Validation => controller.BadRequest(new { error.Code, error.Description }),
            ErrorType.Conflict => controller.UnprocessableEntity(new { error.Code, error.Description }),
            ErrorType.Unauthorized => controller.Unauthorized(new { error.Code, error.Description }),
            _ => controller.StatusCode(500, new { error.Code, error.Description })
        };
}
