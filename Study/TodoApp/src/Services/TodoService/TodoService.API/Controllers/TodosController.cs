using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.BuildingBlocks.Pagination;
using TodoService.Application.Commands.CompleteTodo;
using TodoService.Application.Commands.CreateTodo;
using TodoService.Application.Commands.DeleteTodo;
using TodoService.Application.Commands.UpdateTodo;
using TodoService.Application.DTOs;
using TodoService.Application.Queries.GetTodo;
using TodoService.Application.Queries.GetTodos;
using TodoService.Domain.Enums;

namespace TodoService.API.Controllers;

[ApiController]
[Route("api/v1/todos")]
[Produces("application/json")]
public sealed class TodosController(IMediator mediator) : ControllerBase
{
    /// <summary>Get a paged list of todos</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TodoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TodoDto>>> GetTodos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] TodoStatus? status = null,
        [FromQuery] TodoPriority? priority = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetTodosQuery(page, pageSize, status, priority, assignedToUserId, searchTerm),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a todo by ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TodoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoDto>> GetTodo(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTodoQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new todo</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TodoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoDto>> CreateTodo(
        [FromBody] CreateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTodoCommand(
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.AssignedToUserId);

        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTodo), new { id = result.Id }, result);
    }

    /// <summary>Update an existing todo</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TodoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoDto>> UpdateTodo(
        Guid id,
        [FromBody] UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTodoCommand(
            id,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Mark a todo as completed</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(TodoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TodoDto>> CompleteTodo(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CompleteTodoCommand(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a todo</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTodo(
        Guid id,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteTodoCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateTodoRequest(
    string Title,
    string? Description,
    TodoPriority Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId);

public sealed record UpdateTodoRequest(
    string Title,
    string? Description,
    TodoPriority Priority,
    DateTime? DueDate);
