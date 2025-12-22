using LibraHub.BuildingBlocks.Results;
using LibraHub.Identity.Application.Auth.Dtos;
using MediatR;

namespace LibraHub.Identity.Application.Auth.Commands.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<Result<AuthTokensDto>>;
