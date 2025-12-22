using LibraHub.Identity.Api.Dtos.Common;
using LibraHub.Identity.Api.Dtos.Me;
using LibraHub.Identity.Application.Me.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraHub.Identity.Api.Controllers;

[ApiController]
[Route("me")]
[Authorize]
public class MeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(MeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var query = new GetMeQuery();
        var result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Code switch
            {
                "UNAUTHORIZED" => Unauthorized(new ErrorResponse(result.Error.Code, result.Error.Message)),
                "FORBIDDEN" => StatusCode(403, new ErrorResponse(result.Error.Code, result.Error.Message)),
                _ => BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message))
            };
        }

        var response = new MeResponseDto
        {
            UserId = result.Value.UserId,
            Email = result.Value.Email,
            Roles = result.Value.Roles,
            EmailVerified = result.Value.EmailVerified,
            Status = result.Value.Status
        };

        return Ok(response);
    }
}
