using LibraHub.BuildingBlocks.Results;
using MediatR;

namespace LibraHub.Identity.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password) : IRequest<Result<Guid>>;
