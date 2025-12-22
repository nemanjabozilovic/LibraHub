using LibraHub.Identity.Application.Auth.Commands.Login;
using LibraHub.Identity.Application.Auth.Commands.Refresh;
using LibraHub.Identity.Application.Auth.Commands.Register;
using LibraHub.Identity.Application.Auth.Commands.VerifyEmail;
using LibraHub.Identity.Application.Auth.Dtos;
using LibraHub.Identity.Api.Dtos.Auth;
using LibraHub.Identity.Api.Dtos.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraHub.Identity.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.Email, request.Password);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Code switch
            {
                "CONFLICT" => Conflict(new ErrorResponse(result.Error.Code, result.Error.Message)),
                _ => BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message))
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error!.Code switch
            {
                "UNAUTHORIZED" => Unauthorized(new ErrorResponse(result.Error.Code, result.Error.Message)),
                "FORBIDDEN" => StatusCode(403, new ErrorResponse(result.Error.Code, result.Error.Message)),
                _ => BadRequest(new ErrorResponse(result.Error.Code, result.Error.Message))
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request, CancellationToken cancellationToken)
    {
        var command = new RefreshCommand(request.RefreshToken);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new ErrorResponse(result.Error!.Code, result.Error.Message));
        }

        return Ok(result.Value);
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request, CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(request.Token);
        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ErrorResponse(result.Error!.Code, result.Error.Message));
        }

        return Ok();
    }
}
