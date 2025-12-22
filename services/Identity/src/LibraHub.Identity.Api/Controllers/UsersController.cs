using LibraHub.Identity.Application.Admin.Commands.AssignRole;
using LibraHub.Identity.Application.Admin.Commands.DisableUser;
using LibraHub.Identity.Api.Dtos.Common;
using LibraHub.Identity.Api.Dtos.Users;
using LibraHub.Identity.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraHub.Identity.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize(Roles = "Admin")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpPost("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(
        [FromRoute] Guid id,
        [FromBody] AssignRoleRequestDto request,
        CancellationToken cancellationToken)
    {
        var role = Enum.Parse<Role>(request.Role, ignoreCase: true);
        var command = new AssignRoleCommand(id, role, true);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Code switch
            {
                "NOT_FOUND" => NotFound(new ErrorResponse(result.Error.Code, result.Error.Message)),
                _ => BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message))
            };
        }

        return Ok();
    }

    [HttpDelete("{id}/roles/{role}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(
        [FromRoute] Guid id,
        [FromRoute] string role,
        CancellationToken cancellationToken)
    {
        var roleEnum = Enum.Parse<Role>(role, ignoreCase: true);
        var command = new AssignRoleCommand(id, roleEnum, false);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Code switch
            {
                "NOT_FOUND" => NotFound(new ErrorResponse(result.Error.Code, result.Error.Message)),
                _ => BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message))
            };
        }

        return Ok();
    }

    [HttpPost("{id}/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableUser(
        [FromRoute] Guid id,
        [FromBody] DisableUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new DisableUserCommand(id, request.Reason, true);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Code switch
            {
                "NOT_FOUND" => NotFound(new ErrorResponse(result.Error.Code, result.Error.Message)),
                _ => BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message))
            };
        }

        return Ok();
    }

    [HttpPost("{id}/enable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableUser(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DisableUserCommand(id, string.Empty, false);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Code switch
            {
                "NOT_FOUND" => NotFound(new ErrorResponse(result.Error.Code, result.Error.Message)),
                _ => BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message))
            };
        }

        return Ok();
    }
}
