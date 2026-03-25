using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.BuildingBlocks.Result;
using UserService.Application.Commands.DeactivateUser;
using UserService.Application.Commands.RegisterUser;
using UserService.Application.Commands.UpdateProfile;
using UserService.Application.DTOs;
using UserService.Application.Queries.GetUser;
using UserService.Application.Queries.GetUsers;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public sealed class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetUsersQuery(page, pageSize), cancellationToken);
        return result.Match<IActionResult>(Ok, e => e.ToActionResult(this));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserQuery(id), cancellationToken);
        return result.Match<IActionResult>(Ok, e => e.ToActionResult(this));
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RegisterUserCommand(request.Email, request.FirstName, request.LastName), cancellationToken);
        return result.Match<IActionResult>(
            dto => CreatedAtAction(nameof(GetUser), new { id = dto.Id }, dto),
            e => e.ToActionResult(this));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateProfileCommand(id, request.Email, request.FirstName, request.LastName), cancellationToken);
        return result.Match<IActionResult>(Ok, e => e.ToActionResult(this));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeactivateUserCommand(id), cancellationToken);
        return result.Match<IActionResult>(() => NoContent(), e => e.ToActionResult(this));
    }
}

public sealed record RegisterUserRequest(string Email, string FirstName, string LastName);
public sealed record UpdateProfileRequest(string Email, string FirstName, string LastName);

internal static class ErrorExtensions
{
    public static IActionResult ToActionResult(this Error error, ControllerBase controller)
        => error.Type switch
        {
            ErrorType.NotFound => controller.NotFound(new { error.Code, error.Description }),
            ErrorType.Validation => controller.BadRequest(new { error.Code, error.Description }),
            ErrorType.Conflict => controller.UnprocessableEntity(new { error.Code, error.Description }),
            _ => controller.StatusCode(500, new { error.Code, error.Description })
        };
}
