using LibraHub.BuildingBlocks.Results;
using LibraHub.Identity.Domain.Users;
using MediatR;

namespace LibraHub.Identity.Application.Admin.Commands.AssignRole;

public record AssignRoleCommand(Guid UserId, Role Role, bool Assign) : IRequest<Result>;
