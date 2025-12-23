using LibraHub.BuildingBlocks.Results;
using LibraHub.Library.Api.Dtos.Entitlements;
using LibraHub.Library.Application.Entitlements.Commands.AdminGrantEntitlement;
using LibraHub.Library.Application.Entitlements.Commands.RevokeEntitlement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraHub.Library.Api.Controllers;

[ApiController]
[Route("api/admin/entitlements")]
[Authorize(Policy = "Admin")]
public class AdminEntitlementsController(IMediator mediator) : ControllerBase
{
    [HttpPost("grant")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GrantEntitlement(
        [FromBody] GrantEntitlementRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new AdminGrantEntitlementCommand
        {
            UserId = request.UserId,
            BookId = request.BookId
        };

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GrantEntitlement), new { id = result.Value }, new { id = result.Value });
        }

        return result.ToActionResult(this);
    }

    [HttpPost("{id}/revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeEntitlement(
        Guid id,
        [FromBody] RevokeEntitlementRequestDto? request = null,
        CancellationToken cancellationToken = default)
    {
        var command = new RevokeEntitlementCommand
        {
            EntitlementId = id,
            Reason = request?.Reason
        };

        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
}

