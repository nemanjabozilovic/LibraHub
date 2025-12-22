using LibraHub.BuildingBlocks.Results;
using MediatR;

namespace LibraHub.Catalog.Application.Promotions.Commands.EndCampaign;

public record EndCampaignCommand(
    Guid CampaignId,
    Guid ActorUserId) : IRequest<Result>;
