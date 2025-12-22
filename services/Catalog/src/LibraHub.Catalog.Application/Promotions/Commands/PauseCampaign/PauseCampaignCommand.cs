using LibraHub.BuildingBlocks.Results;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Commands.PauseCampaign;

public record PauseCampaignCommand(
    Guid CampaignId,
    Guid ActorUserId) : IRequest<Result>;
