using LibraHub.BuildingBlocks.Results;
using MediatR;

namespace LibraHub.Identity.Application.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : IRequest<Result>;
